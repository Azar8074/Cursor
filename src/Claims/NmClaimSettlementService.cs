using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Claims.Services
{
    /// <summary>
    /// Handles automatic settlement of "under process" non-marine (NM) claims:
    /// estimation retrieval, approval-authority evaluation and reserve approval.
    ///
    /// This is a refactor of the original <c>AutomaticSettlement</c> /
    /// <c>GettingClaimEstimation</c> / <c>SendForEstimationApproval</c> methods.
    /// The behaviour is preserved but a number of latent bugs have been fixed
    /// (see comments prefixed with "FIX:") and the code has been made async,
    /// null-safe and easier to maintain.
    /// </summary>
    public class NmClaimSettlementService
    {
        // The user asked us not to worry about the hard-coded "1" user id, so we
        // keep it in one place instead of scattering the literal across the code.
        private const string SystemUserId = "1";

        private const string StatusUnderProcess = "Under Process";
        private const string RoleClaimProcessor = "nmclaimprocessor";

        private static readonly HashSet<string> HigherRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "nm supervisor",
            "nm claimmanager",
            "nm coo"
        };

        /// <summary>
        /// Settles every claim that is currently "Under Process" and has a sub
        /// claim number. Each claim is processed independently: a failure on one
        /// claim is captured in its own <see cref="SettlementResult"/> and does
        /// not abort the whole batch.
        /// </summary>
        public async Task<List<SettlementResult>> RunAutomaticSettlementAsync()
        {
            var settlementResults = new List<SettlementResult>();

            using (var dbContext = new _DBContext())
            {
                // FIX: AsNoTracking - these rows are only read, never updated here.
                var pendingClaims = await dbContext.TBLCLAIM_PROCESSOR
                    .AsNoTracking()
                    .Where(x => x.CLAIM_STATUS == StatusUnderProcess && x.SUB_CLAIM_NO != null)
                    .ToListAsync();

                if (pendingClaims.Count == 0)
                {
                    settlementResults.Add(new SettlementResult
                    {
                        IsSuccess = true,
                        ClaimNo = string.Empty,
                        Message = "No claims found for automatic settlement."
                    });

                    return settlementResults;
                }

                foreach (var claimProcessor in pendingClaims)
                {
                    settlementResults.Add(await SettleSingleClaimAsync(claimProcessor));
                }
            }

            return settlementResults;
        }

        private async Task<SettlementResult> SettleSingleClaimAsync(TBLCLAIM_PROCESSOR claimProcessor)
        {
            var failures = new List<string>();

            try
            {
                var claimRequest = new SelectedClaimRequest
                {
                    ClaimNumber = claimProcessor.CLAIM_NO,
                    PARTICIPANT_ID = claimProcessor.PARTICIPANT_ID,
                    subclaimno = claimProcessor.SUB_CLAIM_NO,
                    USERID = SystemUserId
                };

                var claimEstimation = await GetClaimEstimationForProcessorAsync(claimRequest);

                if (claimEstimation == null)
                {
                    return CreateFailedSettlementResult(
                        claimProcessor.CLAIM_NO,
                        "Claim estimation returned no response.");
                }

                if (string.Equals(claimEstimation.ResultCode, "F", StringComparison.OrdinalIgnoreCase))
                {
                    return CreateFailedSettlementResult(
                        claimProcessor.CLAIM_NO,
                        claimEstimation.ResultDescription ?? "Unable to retrieve claim estimation.");
                }

                var selectedRisks = claimEstimation.claimestimation?.selectedrisks;
                if (selectedRisks == null || !selectedRisks.Any())
                {
                    return CreateFailedSettlementResult(
                        claimProcessor.CLAIM_NO,
                        "No selected risks were found for settlement.");
                }

                foreach (var selectedRisk in selectedRisks)
                {
                    try
                    {
                        var riskRequest = new SelectedRiskEstRequest
                        {
                            EST_ID = selectedRisk.EST_ID,
                            EST_CODE = (int)selectedRisk.EST_CODE,
                            SETTLE_ID = selectedRisk.SETTLE_ID,
                            USERID = SystemUserId
                        };

                        var approvalResponse = await ApproveEstimationAsync(riskRequest);

                        if (approvalResponse == null)
                        {
                            failures.Add(
                                $"Estimation {selectedRisk.EST_ID}: approval returned no response.");
                        }
                        else if (!string.Equals(approvalResponse.stCode, "S", StringComparison.OrdinalIgnoreCase))
                        {
                            failures.Add(
                                $"Estimation {selectedRisk.EST_ID}: " +
                                $"{approvalResponse.stCode ?? "F"} - " +
                                $"{approvalResponse.stDesc ?? "Approval failed."}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // A single risk failure must not prevent the remaining
                        // risks or claims from being processed.
                        failures.Add(
                            $"Estimation {selectedRisk.EST_ID}: {ex}");
                    }
                }

                if (failures.Count > 0)
                {
                    return new SettlementResult
                    {
                        IsSuccess = false,
                        ClaimNo = claimProcessor.CLAIM_NO,
                        Message = $"Settlement completed with {failures.Count} failed estimation(s).",
                        ExceptionDetails = string.Join(Environment.NewLine, failures)
                    };
                }

                return new SettlementResult
                {
                    IsSuccess = true,
                    ClaimNo = claimProcessor.CLAIM_NO,
                    Message = "Settlement completed successfully."
                };
            }
            catch (Exception ex)
            {
                return CreateFailedSettlementResult(claimProcessor.CLAIM_NO, ex.ToString());
            }
        }

        private static SettlementResult CreateFailedSettlementResult(string claimNo, string details)
        {
            return new SettlementResult
            {
                IsSuccess = false,
                ClaimNo = claimNo,
                Message = "Settlement failed.",
                ExceptionDetails = details
            };
        }

        /// <summary>
        /// Builds the estimation view model for a claim processor record:
        /// selected risks, policy risks, estimation dropdowns, the approval
        /// authority for the current user and the various claim amount views.
        /// </summary>
        public async Task<ClaimDetailsForProcessorResponse> GetClaimEstimationForProcessorAsync(
            SelectedClaimRequest request)
        {
            var response = new ClaimDetailsForProcessorResponse();
            var claimEstimations = new ClaimEstimation();

            using (var db = new _DBContext())
            {
                var estimationDate = db.GetServerDate();

                try
                {
                    claimEstimations.selectedrisks = await GetClaimRisksAsync(db, request) ?? new List<ClaimRisk>();

                    // FIX: resolve the processor row first and bail out early if it
                    // does not exist. The original code dereferenced `processor`
                    // (switch on PRODUCT_CODE) before its null check, which threw a
                    // NullReferenceException for a missing/invalid claim.
                    var processor = await db.TBLCLAIM_PROCESSOR
                        .Where(p => p.SUB_CLAIM_NO == request.subclaimno && p.PARTICIPANT_ID == request.PARTICIPANT_ID)
                        .FirstOrDefaultAsync();

                    if (processor == null)
                    {
                        response.ResultCode = "F";
                        response.ResultDescription = "Claim processor record not found for the supplied sub claim / participant.";
                        response.claimestimation = claimEstimations;
                        return response;
                    }

                    var higherRole = await ResolveUserRolesAsync(db, request);

                    claimEstimations.policyRisks.AddRange(await GetPolicyRisksAsync(db, processor));
                    claimEstimations.estimates = await GetEstimationDropdownsAsync(db, processor);

                    await ApplyCustomerDetailsAsync(db, processor, claimEstimations);

                    // FIX: always give the response an ApprovalManager. The original
                    // only created it when an approver row existed, yet later code
                    // unconditionally wrote to response.approvalmanager -> NRE.
                    response.approvalmanager = await BuildApprovalManagerAsync(db, request);

                    ApplyRiskApprovalFlags(claimEstimations, response.approvalmanager, higherRole, estimationDate);

                    claimEstimations.NET_CLAIM_AMOUNT =
                        await GetClaimViewScalarAsync<string>(db, "VW_GETNETCLAIMAMOUNT", request);
                    claimEstimations.NET_OUTSTANDING_AMOUNT =
                        await GetClaimViewScalarAsync<string>(db, "VW_GETNETOUTSTANDINGAMOUNT", request);
                    claimEstimations.NET_SETTLED_AMOUNT =
                        await GetClaimViewScalarAsync<string>(db, "VW_GETTOTALPAIDAMT", request);

                    claimEstimations.CLAIM_NO = processor.CLAIM_NO;
                    claimEstimations.PARTICIPANT_ID = processor.PARTICIPANT_ID;
                }
                catch (Exception ex)
                {
                    var stackTrace = new StackTrace(ex, true);
                    response.ResultCode = "F";
                    response.ResultDescription = ex.GetAllInnerExceptionMessages() + stackTrace;
                }
            }

            response.claimestimation = claimEstimations;
            return response;
        }

        /// <summary>
        /// Approves an estimation (reserve) for a risk. Performs RI (reinsurance)
        /// allocation/validation, records a claim event and calls the approval
        /// stored procedure. Missing estimation/processor rows are now handled
        /// gracefully instead of throwing a NullReferenceException.
        /// </summary>
        public async Task<ResponseModel> ApproveEstimationAsync(
            SelectedRiskEstRequest request)
        {
            // FIX: the original method referenced `response` without ever
            // declaring it, so it could not compile. Declare it up front.
            var response = new ResponseModel();

            var isRiValidationHappened = false;

            using (var db = new _DBContext())
            {
                var estimation = await db.TBLCLAIMESTIMATION
                    .FirstOrDefaultAsync(e => e.EST_ID == request.EST_ID);

                // FIX: guard against a missing estimation before dereferencing it.
                if (estimation == null)
                {
                    response.stCode = "F";
                    response.stDesc = $"Estimation {request.EST_ID} was not found.";
                    return response;
                }

                var previousStatus = estimation.EST_STATUS ?? string.Empty;

                var claimRiskDetail = await db.TBLNMCLAIMRISKDETAIL
                    .Where(rp => rp.EST_ID == request.EST_ID)
                    .Select(s => s.RISK_NO)
                    .FirstOrDefaultAsync();

                var processor = await db.TBLCLAIM_PROCESSOR
                    .FirstOrDefaultAsync(x => x.CLAIM_NO == estimation.CLAIM_NO && x.PARTICIPANT_ID == estimation.PARTICIPANT_ID);

                // FIX: guard against a missing processor before using POLICY_NO.
                if (processor == null)
                {
                    response.stCode = "F";
                    response.stDesc = $"Claim processor not found for claim {estimation.CLAIM_NO}.";
                    return response;
                }

                var policyAllocation = await db.TBLFACT_RIPREM_ALLOC.AsNoTracking()
                    .Where(x => x.POLICY_NO == processor.POLICY_NO)
                    .Select(s => s.ALLOCATION_BASIS)
                    .FirstOrDefaultAsync();

                estimation.EST_STATUS = "Approved";
                estimation.IS_AML_PASSED = false;
                estimation.EST_APPROVAL_DATE = db.GetServerDate();
                // FIX: removed the redundant db.Entry(estimation).CurrentValues.SetValues(estimation);
                // the entity is already tracked, SaveChanges alone persists the changes.
                await db.SaveChangesAsync();

                if (string.Equals(previousStatus, "ri approved", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var riAllocationService = new ClaimAllocationLogic();
                        await riAllocationService.ApproveClaimAllocation(
                            claimId: 0,
                            transId: 0,
                            estimation.CLAIM_NO,
                            estimation.EST_ID,
                            request.USERID);

                        estimation.IS_RI_ALLOCATED = true;
                    }
                    catch (Exception riExp)
                    {
                        // FIX: do not continue to the stored procedure/event block,
                        // where this failure would otherwise be overwritten with
                        // an "S" response.
                        isRiValidationHappened = true;
                        response.stCode = "F";
                        response.stDesc = new StackTrace(riExp, true) + " " + riExp.Message;

                        estimation.IS_RI_ALLOCATED = false;
                        estimation.EST_STATUS = "OS";
                        estimation.EST_APPROVAL_DATE = null;
                        await db.SaveChangesAsync();
                    }
                }
                else
                {
                    try
                    {
                        var riAllocationService = new ClaimAllocationLogic();

                        if (policyAllocation == "POLICY")
                        {
                            claimRiskDetail = 1;
                        }

                        await riAllocationService.GetClaimEstimationDetail(
                            processor.POLICY_NO,
                            estimation.CLAIM_NO,
                            request.SETTLE_ID,
                            claimRiskDetail,
                            estimation.COVERCODE,
                            request.USERID);

                        estimation.IS_RI_ALLOCATED = true;
                    }
                    catch (Exception riExp)
                    {
                        isRiValidationHappened = true;
                        ApplyReinsuranceValidationFailure(estimation, response, riExp);
                        await db.SaveChangesAsync();
                    }
                }

                if (!isRiValidationHappened)
                {
                    response.mdlobj = await HasSalvageRecoveryEstimationAsync(db, estimation);

                    var parameters = new List<IDataParameter>
                    {
                        _DatabaseUtil.GetParameter("P_ESTID", estimation.EST_ID),
                        _DatabaseUtil.GetParameter("P_CLAIMNO", estimation.CLAIM_NO),
                        _DatabaseUtil.GetParameter("P_PARTICIPANT_ID", estimation.PARTICIPANT_ID),
                        _DatabaseUtil.GetParameter("P_USERID", request.USERID)
                    };

                    var procedureResponse = await db._ExecuteQuery("PKG_NMCLAIMS.SP_NMCLAIMESTAPPROVE", parameters.ToArray(), CommandType.StoredProcedure)
                        .GetItem<ResponseModel>();

                    if (procedureResponse != null
                        && !string.IsNullOrWhiteSpace(procedureResponse.stCode)
                        && !string.Equals(procedureResponse.stCode, "S", StringComparison.OrdinalIgnoreCase))
                    {
                        return procedureResponse;
                    }

                    var riAllocationAudit = await db.TBLRICLAIM_ALLOCATION_AUDIT
                        .Where(x => x.POLICY_NO == processor.POLICY_NO
                                    && x.RISK_ID == claimRiskDetail
                                    && x.COVER_CODE == estimation.COVERCODE
                                    && x.CLAIM_NO == processor.CLAIM_NO
                                    && x.ESTIMATION_CODE == request.SETTLE_ID)
                        .OrderByDescending(s => s.TRANS_ID)
                        .FirstOrDefaultAsync();

                    if (riAllocationAudit != null)
                    {
                        riAllocationAudit.STATUS = "APPROVED";
                        riAllocationAudit.APPROVED_DATE = db.GetServerDate();
                    }

                    try
                    {
                        db.TBLCLAIMEVENT.Add(new TBLCLAIMEVENT
                        {
                            CLAIM_NO = estimation.CLAIM_NO,
                            PARTICIPANT_ID = estimation.PARTICIPANT_ID,
                            EVENT_DATE = db.GetServerDate(),
                            TRANSDATE = db.GetServerDate(),
                            EVENT_TYPE = "Reserve Approved",
                            USERID = request.USERID
                        });

                        await db.SaveChangesAsync();

                        response.stCode = "S";
                        response.stDesc = "Reserve Approved Successfully..!!";
                    }
                    catch (Exception exp)
                    {
                        response.stCode = "F";
                        response.stDesc = new StackTrace(exp, true) + " " + exp.Message;
                    }
                }
            }

            return response;
        }

        // ---------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------

        private static async Task<List<ClaimRisk>> GetClaimRisksAsync(_DBContext db, SelectedClaimRequest request)
        {
            var parameters = new List<IDataParameter>
            {
                _DatabaseUtil.GetParameter("P_VIEWNAME", "VW_NMCLAIMESTGRID"),
                _DatabaseUtil.GetParameter("P_CLAIM_NO", request.subclaimno),
                _DatabaseUtil.GetParameter("P_PARTICIPANT_ID", request.PARTICIPANT_ID),
            };

            return await db._ExecuteQuery("PKG_NMCLAIMS.SP_GETNMCLAIMSVIEWS", parameters.ToArray(), CommandType.StoredProcedure)
                .GetItems<ClaimRisk>();
        }

        /// <summary>
        /// Sets the processor role id on the request and returns the highest
        /// "supervisory" role the user holds (empty when none).
        /// </summary>
        private static async Task<string> ResolveUserRolesAsync(_DBContext db, SelectedClaimRequest request)
        {
            var higherRole = string.Empty;

            // FIX: filter roles in the database instead of pulling the whole
            // TBLUSERROLE table into memory with AsEnumerable(). Also removed the
            // completely unused `users` query that loaded all of TBLUSERMASTER.
            var roleIds = await db.TBLUSERROLE
                .Where(x => x.USERID == request.USERID)
                .Select(s => s.ROLEID)
                .ToListAsync();

            var roleNames = await db.TBLROLEMASTER
                .Where(r => roleIds.Contains(r.ROLEID))
                .Select(r => new { r.ROLEID, r.ROLENAME })
                .ToListAsync();

            foreach (var role in roleNames)
            {
                var roleName = role.ROLENAME ?? string.Empty; // FIX: null-safe ToLower/compare

                if (string.Equals(roleName, RoleClaimProcessor, StringComparison.OrdinalIgnoreCase))
                {
                    request.ROLEID = role.ROLEID;
                }

                if (HigherRoles.Contains(roleName))
                {
                    higherRole = roleName;
                }
            }

            return higherRole;
        }

        private static async Task<List<RiskDropdown>> GetPolicyRisksAsync(_DBContext db, TBLCLAIM_PROCESSOR processor)
        {
            // Only product 5030 sourced policy risks in the original code.
            if (processor.PRODUCT_CODE != "5030")
            {
                return new List<RiskDropdown>();
            }

            var risks = await (
                from s in db.TBLPOL_RISK.Where(x => x.POLICY_NO == processor.POLICY_NO
                                                    && x.POLICY_FROMDATE <= processor.ACCIDENTDATE
                                                    && x.POLICY_TODATE >= processor.ACCIDENTDATE)
                join i in db.TBLCLAIM_INTIMATOR on s.POLICY_NO equals i.POLICY_NO
                join ii in db.TBLCLAIM_INTIMATOR.Where(x => x.SUB_CLAIM_NO == processor.SUB_CLAIM_NO) on s.RISK_ID equals ii.RISKNO
                select new RiskDropdown
                {
                    RISK_ID = s.RISK_ID,
                    TOTAL_PREMIUM = s.TOTAL_PREMIUM ?? 0,
                    RiskDescription = s.VESSEL_NAME,
                    RISK_STATUS = s.RISK_STATUS,
                    RISK_LOCATION = s.ADDRESS,
                    RISK_EFF_FROM = s.EFFECTIVE_FROM,
                    RISK_EFFF_TO = s.EFFECTIVE_TO
                }).Distinct().ToListAsync();

            return risks
                .Select(s => new RiskDropdown
                {
                    RISK_ID = s.RISK_ID,
                    RiskDescription = Convert.ToString(s.RISK_ID) + "~" + s.RiskDescription,
                    RISK_STATUS = s.RISK_STATUS,
                    RISK_LOCATION = s.RISK_LOCATION,
                    RISK_EFF_FROM = s.RISK_EFF_FROM,
                    RISK_EFFF_TO = s.RISK_EFFF_TO
                })
                .ToList();
        }

        private static Task<List<EstimationDropDown>> GetEstimationDropdownsAsync(_DBContext db, TBLCLAIM_PROCESSOR processor)
        {
            return (
                from es in db.TBLESTIMATION.AsNoTracking()
                    .Where(x => ((x.ESTIMATION_TYPE != "Reversal" && x.ESTIMATION_TYPE != "Recovery Reversal") || x.IS_DEDUCTIBLE == true)
                                && x.ACTIVE_FOR_PROCESSOR == true)
                join ed in db.TBLESTIMATION_DTLS.AsNoTracking().Where(x => x.PRODUCT_CODE == processor.PRODUCT_CODE)
                    on es.ESTIMATION_CODE equals ed.ESTIMATION_CODE
                select new EstimationDropDown
                {
                    ESTIMATION_CODE = es.ESTIMATION_CODE,
                    ESTIMATION_DESC_EN = es.ESTIMATION_DESC_EN,
                    ESTIMATION_DESC_AR = es.ESTIMATION_DESC_AR,
                    IS_DEDUCT_ALLOWED = es.ESTIMATION_TYPE == "Payment" && es.ISSURVEY != true,
                    EST_TYPE = es.ESTIMATION_TYPE,
                    ISSURVEY = es.ISSURVEY
                }).ToListAsync();
        }

        private static async Task ApplyCustomerDetailsAsync(_DBContext db, TBLCLAIM_PROCESSOR processor, ClaimEstimation claimEstimations)
        {
            var customer = await db.TBLCUSTOMER
                .FirstOrDefaultAsync(c => c.CUST_CODE == processor.CUSTOMER_CODE);

            if (customer != null)
            {
                claimEstimations.CUSTOMER_CODE = customer.ASSURED_ID;
                claimEstimations.CUSTOMER_CODENAME = customer.CUST_NAME;
            }
        }

        private static async Task<ApprovalManager> BuildApprovalManagerAsync(_DBContext db, SelectedClaimRequest request)
        {
            var approverCheck = await db.TBLCLAIM_APPROVALAUTHORITY
                .FirstOrDefaultAsync(a => a.APPROVAL_FOR == "NMEstimation" && a.APPROVER_ROLEID == request.ROLEID);

            if (approverCheck == null)
            {
                // A neutral, non-null approval manager so downstream logic is safe.
                return new ApprovalManager
                {
                    EST_AMOUNT = 0M,
                    FROM_AMOUNT = 0M,
                    TO_AMOUNT = 0M,
                    PAYMENT_AMOUNT = 0M,
                    IS_AUTHORISED = false,
                    TYPE = "ESTIMATION"
                };
            }

            return new ApprovalManager
            {
                EST_AMOUNT = 0M,
                FROM_AMOUNT = approverCheck.APPROVAL_AMOUNT ?? 0M,
                IS_AUTHORISED = true,
                PAYMENT_AMOUNT = 0M,
                TO_AMOUNT = approverCheck.APPROVAL_MAX_AMOUNT ?? 0M,
                TYPE = "ESTIMATION",
                ROLEID = approverCheck.APPROVER_ROLEID,
                USERID = approverCheck.APPROVER_USERID
            };
        }

        private static void ApplyRiskApprovalFlags(ClaimEstimation claimEstimations, ApprovalManager approvalManager, string higherRole, DateTime estimationDate)
        {
            var isRejectVisible = HigherRoles.Contains(higherRole ?? string.Empty);
            var checkAllEstimationClosed = true;

            foreach (var risk in claimEstimations.selectedrisks)
            {
                risk.IsRejectVisible = isRejectVisible;
            }

            foreach (var estimation in claimEstimations.selectedrisks)
            {
                if (estimation.EST_STATUS == "OS")
                {
                    checkAllEstimationClosed = false;
                }

                claimEstimations.EST_ID = estimation.EST_ID;
                claimEstimations.ESTIMATION_CODE = estimation.EST_CODE;
                claimEstimations.COVER_CODE = estimation.COVER_CODE;
                claimEstimations.ESTIMATION_DATE = estimationDate;
                claimEstimations.IS_ALL_ESTIMATION_CLOSED = checkAllEstimationClosed;

                approvalManager.EST_AMOUNT = estimation.EST_AMOUNT;
                approvalManager.PAYMENT_AMOUNT = estimation.EST_AMOUNT;

                bool isAuthorised;
                if (estimation.EST_TYPE != "Recovery")
                {
                    isAuthorised = approvalManager.EST_AMOUNT <= approvalManager.TO_AMOUNT
                                   && approvalManager.EST_AMOUNT >= approvalManager.FROM_AMOUNT;
                }
                else
                {
                    isAuthorised = true;
                }

                approvalManager.IS_AUTHORISED = isAuthorised;
                claimEstimations.IS_AUTHORISED = isAuthorised;

                if (isAuthorised)
                {
                    estimation.Approve = "Approve";
                    estimation.SendForApproval = null;
                    estimation.IS_AUTHORISED = true;
                }
                else
                {
                    estimation.Approve = null;
                    estimation.SendForApproval = "Send for Approval";
                    estimation.IS_AUTHORISED = false;
                }

                claimEstimations.ESTIMATION_STATUS = estimation.EST_STATUS;
            }
        }

        private static async Task<T> GetClaimViewScalarAsync<T>(_DBContext db, string viewName, SelectedClaimRequest request)
        {
            var parameters = new List<IDataParameter>
            {
                _DatabaseUtil.GetParameter("P_VIEWNAME", viewName),
                _DatabaseUtil.GetParameter("P_CLAIM_NO", request.ClaimNumber),
                _DatabaseUtil.GetParameter("P_PARTICIPANT_ID", request.PARTICIPANT_ID),
            };

            return await db._ExecuteQuery("PKG_NMCLAIMS.SP_GETNMCLAIMSVIEWS", parameters.ToArray(), CommandType.StoredProcedure)
                .GetItem<T>();
        }

        private static void ApplyReinsuranceValidationFailure(TBLCLAIMESTIMATION estimation, ResponseModel response, Exception riExp)
        {
            var message = riExp.Message ?? string.Empty;

            estimation.EST_STATUS = "OS";
            estimation.EST_APPROVAL_DATE = null;

            if (message.Contains("CLA Limit"))
            {
                response.stCode = "RI";
                response.stDesc = message;
                estimation.IS_CLA = "CLA";
            }
            else if (message.Contains("PLA Limit"))
            {
                response.stCode = "RI";
                response.stDesc = message;
                estimation.IS_CLA = "PLA";
            }
            else if (message.Contains("100"))
            {
                response.stCode = "RIS";
                response.stDesc = message;
            }
            else if (message.Contains("FAC does not exists") || message.Contains("Treaty allocation is not approved yet"))
            {
                response.stCode = "FAC";
                response.stDesc = message;
            }
            else
            {
                response.stCode = "F";
                response.stDesc = new StackTrace(riExp, true) + " " + message;
                estimation.IS_RI_ALLOCATED = false;
            }
        }

        private static async Task<bool> HasSalvageRecoveryEstimationAsync(_DBContext db, TBLCLAIMESTIMATION estimation)
        {
            // FIX: evaluate directly in the database rather than loading all rows
            // and looping in memory.
            return await db.TBLESTIMATION
                .AnyAsync(e => e.ESTIMATION_CODE == estimation.ESTIMATION_CODE
                               && e.ESTIMATION_TYPE == "Recovery"
                               && e.IS_SALVAGE == true);
        }
    }
}
