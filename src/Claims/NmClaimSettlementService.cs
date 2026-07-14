using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
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
                            continue;
                        }

                        if (!string.Equals(approvalResponse.stCode, "S", StringComparison.OrdinalIgnoreCase))
                        {
                            failures.Add(
                                $"Estimation {selectedRisk.EST_ID}: " +
                                $"{approvalResponse.stCode ?? "F"} - " +
                                $"{approvalResponse.stDesc ?? "Approval failed."}");
                            continue;
                        }

                        // Reserve was approved ("Reserve Approved Successfully..!!"),
                        // so pull the beneficiary data and persist it automatically.
                        var beneficiaryResponse = await SubmitBeneficiaryDetailsAsync(claimProcessor, selectedRisk);

                        if (beneficiaryResponse == null
                            || !string.Equals(beneficiaryResponse.stCode, "S", StringComparison.OrdinalIgnoreCase))
                        {
                            failures.Add(
                                $"Estimation {selectedRisk.EST_ID}: reserve approved but beneficiary update failed - " +
                                $"{beneficiaryResponse?.stDesc ?? "no response"}");
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

        /// <summary>
        /// After a reserve is approved, loads the existing beneficiary details for
        /// the estimation and writes them back through
        /// <see cref="UpdateBeneficiaryDetailsAsync"/>. Any failure is returned as a
        /// non-success <see cref="ResponseModel"/> rather than being thrown, so the
        /// surrounding settlement batch keeps running.
        /// </summary>
        private async Task<ResponseModel> SubmitBeneficiaryDetailsAsync(TBLCLAIM_PROCESSOR claimProcessor, ClaimRisk selectedRisk)
        {
            try
            {
                var bindRequest = new BeneficiaryBindRequest
                {
                    CLAIM_NO = claimProcessor.CLAIM_NO,
                    SUB_CLAIM_NO = claimProcessor.SUB_CLAIM_NO,
                    PARTICIPANT_ID = claimProcessor.PARTICIPANT_ID,
                    EST_ID = selectedRisk.EST_ID
                };

                var beneficiaryData = await GetExistingBeneficiaryDetailsAsync(bindRequest);

                if (beneficiaryData == null)
                {
                    return new ResponseModel
                    {
                        stCode = "F",
                        stDesc = "Beneficiary details could not be loaded."
                    };
                }

                var updateRequest = BuildBeneficiaryUpdateRequest(beneficiaryData, claimProcessor, selectedRisk);

                return await UpdateBeneficiaryDetailsAsync(updateRequest);
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    stCode = "F",
                    stDesc = ex.ToString()
                };
            }
        }

        /// <summary>
        /// Maps the loaded beneficiary details onto an update request.
        ///
        /// NOTE: the source (<see cref="GetExistingBeneficiaryDetailsAsync"/>) does
        /// not expose address fields (ADDR_1/ADDR_2/CITY_CODE/STATE_CODE) or a scalar
        /// share-holder name, so those are left null. Verify this is acceptable for
        /// your automatic-settlement flow before relying on it in production.
        /// PAYEE_NAME is populated from PAYEE_CODENAME ("PREMIA_CODE~CUST_NAME"),
        /// which matches the "~"-split lookup performed by the update method.
        /// </summary>
        private static UpdateBeneficiaryRequest BuildBeneficiaryUpdateRequest(BindBenfResponse beneficiary, TBLCLAIM_PROCESSOR claimProcessor, ClaimRisk selectedRisk)
        {
            return new UpdateBeneficiaryRequest
            {
                CLAIM_NO = claimProcessor.CLAIM_NO,
                PARTICIPANT_ID = claimProcessor.PARTICIPANT_ID,
                EST_ID = selectedRisk.EST_ID,
                USERID = SystemUserId,

                CUST_CODE = beneficiary.CUST_CODE,
                PAYEE_NAME = beneficiary.PAYEE_CODENAME,
                PAYMENT_MODE = beneficiary.PAYMENT_MODE,
                COVER_CODE = beneficiary.COVER_CODE,
                RISK_ID = beneficiary.RISK_ID,
                EST_AMOUNT = beneficiary.EST_AMOUNT,
                EST_DATE = beneficiary.ESTIMATION_DATE,

                BENF_NAME = beneficiary.BENF_NAME,
                BENF_MOBILE = beneficiary.BENF_MOBILENO,
                BENF_REMARKS = beneficiary.BENF_REMARKS,
                RESIDENCE_ID = beneficiary.RESIDENCE_ID,
                COUNTRY_CODE = beneficiary.COUNTRY_CODE,
                PIN_CODE = beneficiary.PIN_CODE,
                NATIONALITY_ID = beneficiary.NAT_CODE,
                ID_CRNO = beneficiary.CR_NO,

                CHEQUE_CITY = beneficiary.CHEQUECITY_CODE,
                BANK_NAME = beneficiary.BANK_CODE,
                IBAN_NO = beneficiary.IBAN,

                IS_VAT_APPLICABLE = beneficiary.IS_VAT_APPLICABLE,
                VAT_PERCENT = beneficiary.VAT_PERCENT,
                VAT_AMOUNT = beneficiary.VAT_AMOUNT,
                INVOICE_NO = beneficiary.INVOICE_NO,
                INVOICE_DATE = beneficiary.INVOICE_DATE,
                VAT_RES_NO = beneficiary.VAT_RES_NO,

                IS_BENF_INDIVIDUAL = beneficiary.IS_BENF_INDIVIDUAL,
                IS_BENF_SAUDI = beneficiary.IS_BENF_SAUDI,
                IS_OWNED_BY_SAUDI = beneficiary.IS_OWNED_BY_SAUDI,
                NATIVE_ID = beneficiary.NATIVE_ID,
                NATIVE_TAX_NO = beneficiary.NATIVE_TAX_NO,
                VAT_REQ_NO = beneficiary.VAT_REQ_NO,
                PERC_OF_SHARE = beneficiary.PERC_OF_SHARE,
                HOME_TAX_REQ_NO = beneficiary.HOME_TAX_REQ_NO,
                HOME_CR_NO = beneficiary.HOME_CR_NO,

                corporatebenfdetail = beneficiary.corporatebenfdetail ?? new List<CorporateBenf>()
            };
        }

        /// <summary>
        /// Persists the beneficiary details for a claim estimation (renamed from
        /// <c>UpdateBenfDetails</c>). Now declares its own <see cref="ResponseModel"/>,
        /// guards against a missing payee/estimation and writes correctly to the new
        /// beneficiary entity when inserting rows.
        /// </summary>
        public async Task<ResponseModel> UpdateBeneficiaryDetailsAsync(UpdateBeneficiaryRequest request)
        {
            // FIX: the original referenced `response` without declaring it.
            var response = new ResponseModel();

            if (request == null)
            {
                response.stCode = "F";
                response.stDesc = "No beneficiary request supplied.";
                return response;
            }

            // Only parse the string date when one was provided; otherwise keep
            // whatever INVOICE_DATE the caller already set.
            if (!string.IsNullOrWhiteSpace(request.STRINVOICEDATE))
            {
                string[] formats =
                {
                    "dd/MM/yyyy", "dd/MM/yyyy h:mm:ss tt", "dd-MM-yyyy", "d/M/yyyy", "d-M-yyyy",
                    "d-MMM-yy", "d-MMMM-yyyy", "M/d/yyyy", "M-d-yyyy", "MM/dd/yyyy", "MM-dd-yyyy", "yyyy-dd-MM"
                };

                if (DateTime.TryParseExact(request.STRINVOICEDATE, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var invoiceDate))
                {
                    request.INVOICE_DATE = invoiceDate;
                }
            }

            long? payeeCode = 0;

            using (var db = new _DBContext())
            {
                // FIX: null-safe access to PAYEE_NAME.
                var payeeName = request.PAYEE_NAME ?? string.Empty;

                if (payeeName.Contains("~"))
                {
                    var payee = payeeName.Split('~')[0];
                    payeeCode = await db.TBLACSUBACCCODESMASTER
                        .Where(x => x.SUBACCCODE == payee)
                        .Select(s => s.COMPANY)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    payeeCode = await db.TBLACSUBACCCODESMASTER
                        .Where(x => x.SUBACCNAME == payeeName || x.SUBACCNAME_BL == payeeName)
                        .Select(s => s.COMPANY)
                        .FirstOrDefaultAsync();
                }

                if (payeeCode == null)
                {
                    response.stCode = "F";
                    response.stDesc = "Could not add Beneficiary Details.";
                    return response;
                }

                var claimEstimation = await db.TBLCLAIMESTIMATION
                    .FirstOrDefaultAsync(e => e.CLAIM_NO == request.CLAIM_NO
                                              && e.PARTICIPANT_ID == request.PARTICIPANT_ID
                                              && e.EST_ID == request.EST_ID);

                if (claimEstimation == null)
                {
                    response.stCode = "F";
                    response.stDesc = "Claim estimation not found for beneficiary update.";
                    return response;
                }

                claimEstimation.CUSTOMER_CODE = request.CUST_CODE;
                claimEstimation.PAYEECODE = payeeCode == 0 ? request.CUST_CODE : payeeCode;
                claimEstimation.PAYMENT_MODE = request.PAYMENT_MODE;
                claimEstimation.IS_BENF_SUBMITTED = true;

                var serverDate = db.GetServerDate();
                var corporateDetails = request.corporatebenfdetail ?? new List<CorporateBenf>();

                var existingBenf = await db.NMCLAIMBENFDETAIL
                    .FirstOrDefaultAsync(e => e.CLAIM_NO == request.CLAIM_NO
                                              && e.PARTICIPANT_ID == request.PARTICIPANT_ID
                                              && e.EST_ID == request.EST_ID);

                if (existingBenf != null)
                {
                    if (corporateDetails.Any())
                    {
                        // Preserves the original behaviour of updating the single
                        // existing row per corporate entry (last entry wins).
                        foreach (var corporate in corporateDetails)
                        {
                            PopulateBeneficiary(existingBenf, request, corporate, payeeCode, serverDate);
                        }
                    }
                    else
                    {
                        PopulateBeneficiary(existingBenf, request, null, payeeCode, serverDate);
                    }

                    response.stDesc = "Beneficiary Details Updated Successfully..!!";
                }
                else
                {
                    var nextBenfId = 1;
                    if (await db.NMCLAIMBENFDETAIL.AnyAsync())
                    {
                        nextBenfId = await db.NMCLAIMBENFDETAIL.MaxAsync(m => m.BENF_ID) + 1;
                    }

                    if (corporateDetails.Any())
                    {
                        foreach (var corporate in corporateDetails)
                        {
                            var newBenf = CreateBeneficiary(request, corporate, payeeCode, serverDate, nextBenfId);
                            db.NMCLAIMBENFDETAIL.Add(newBenf);
                            nextBenfId++;
                        }
                    }
                    else
                    {
                        var newBenf = CreateBeneficiary(request, null, payeeCode, serverDate, nextBenfId);
                        db.NMCLAIMBENFDETAIL.Add(newBenf);
                    }

                    response.stDesc = "Beneficiary Details Saved Successfully..!!";
                }

                var settlements = await db.TBLCLAIMSETTLEMENT
                    .Where(x => x.CLAIM_NO == request.CLAIM_NO
                                && x.PARTICIPANT_ID == request.PARTICIPANT_ID
                                && x.EST_ID == request.EST_ID)
                    .ToListAsync();

                foreach (var settlement in settlements)
                {
                    settlement.IS_BENF_SUBMITTED = true;
                    settlement.PAYEECODE = payeeCode;
                }

                try
                {
                    db.TBLCLAIMEVENT.Add(new TBLCLAIMEVENT
                    {
                        CLAIM_NO = request.CLAIM_NO,
                        PARTICIPANT_ID = request.PARTICIPANT_ID,
                        EVENT_TYPE = " Benificary Added",
                        EVENT_DATE = serverDate,
                        TRANSDATE = serverDate,
                        USERID = request.USERID
                    });

                    await db.SaveChangesAsync();
                    response.stCode = "S";
                }
                catch (Exception dbex)
                {
                    response.stCode = "F";
                    response.stDesc = dbex.Message;
                }

                response.mdlobj = claimEstimation;
            }

            return response;
        }

        /// <summary>
        /// Loads the existing beneficiary details plus supporting dropdowns for a
        /// claim estimation (renamed from <c>BindExistBenfDetails</c>). Hardened
        /// against missing processor/estimation/beneficiary rows.
        /// </summary>
        public async Task<BindBenfResponse> GetExistingBeneficiaryDetailsAsync(BeneficiaryBindRequest request)
        {
            var response = new BindBenfResponse();

            using (var db = new _DBContext())
            {
                response.losslocation.cities.AddRange(await db.TBLCITY.AsNoTracking()
                    .Select(x => new CityDropDown { citycode = x.CIT_CODE, citydesc = x.CIT_NAME_EN, citydesc_bl = x.CIT_SHORT_NAME_BL })
                    .ToListAsync());
                response.losslocation.countries.AddRange(await db.TBLCOUNTRY.AsNoTracking()
                    .Select(x => new CountryDropDown { COUNTRY_CODE = x.COUNTRY_CODE, COUNTRY_DESC_EN = x.COUNTRY_DESC_EN, COUNTRY_DESC_BL = x.COUNTRY_DESC_BL })
                    .ToListAsync());
                response.losslocation.states = await db.TBLSTREET.AsNoTracking()
                    .Select(s => new StateDropDown { statecode = s.STREET_CODE, statedesc = s.STREET_DESC_EN, statedesc_bl = s.STREET_DESC_BL })
                    .ToListAsync();

                response.paymentmodes.Add(new PaymentMode { PYMT_CODE = 1, PYMT_DESC_EN = "TRANSFER" });
                response.paymentmodes.Add(new PaymentMode { PYMT_CODE = 2, PYMT_DESC_EN = "Cheque" });

                response.chequecities.AddRange(await db.TBLCITY.AsNoTracking()
                    .Where(x => x.CHEQUE_CITY == "1")
                    .Select(x => new ChequeCity { CIT_CODE = x.CIT_CODE, CIT_NAME_EN = x.CIT_NAME_EN, CIT_NAME_BL = x.CIT_SHORT_NAME_BL })
                    .ToListAsync());

                response.banks = await db.TBLBANK.AsNoTracking()
                    .Select(s => new Bank { BANK_CODE = s.BANK_CODE, BANK_NAME_EN = s.BANK_NAME_EN, BANK_NAME_BL = s.BANK_NAME_BL })
                    .ToListAsync();

                var processor = await db.TBLCLAIM_PROCESSOR.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.SUB_CLAIM_NO == request.SUB_CLAIM_NO && x.PARTICIPANT_ID == request.PARTICIPANT_ID);

                if (processor == null)
                {
                    // No processor -> nothing meaningful to bind.
                    return response;
                }

                var estimation = await db.TBLCLAIMESTIMATION.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.SUB_CLAIM_NO == request.SUB_CLAIM_NO && x.PARTICIPANT_ID == request.PARTICIPANT_ID && x.EST_ID == request.EST_ID);

                response.settlelist = await db.TBLCLAIMSETTLEMENT.AsNoTracking()
                    .Where(x => x.SUB_CLAIM_NO == request.SUB_CLAIM_NO && x.PARTICIPANT_ID == request.PARTICIPANT_ID && x.EST_ID == request.EST_ID)
                    .Select(s => new settless { SETTLED_STATUS = s.SETTLED_STATUS })
                    .ToListAsync();

                response.IS_BENF_SETTLED = response.settlelist.Any(item => item.SETTLED_STATUS == "settled");

                var vatPercent = await db.TBLPROD_VATMASTER.AsNoTracking()
                    .Where(x => x.PRODUCT_CODE == processor.PRODUCT_CODE)
                    .OrderByDescending(s => s.TRANSDATE)
                    .FirstOrDefaultAsync();

                response.nationalities.AddRange(await db.TBLNATIONALITY.AsNoTracking()
                    .Select(x => new NationalDropDown { NAT_CODE = x.NAT_CODE, NAT_DESC_EN = x.NAT_DESC_EN, NAT_DESC_BL = x.NAT_DESC_BL })
                    .ToListAsync());

                var estimationDetails = await db.TBLCLAIMESTIMATION.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.EST_ID == request.EST_ID && x.SUB_CLAIM_NO == request.SUB_CLAIM_NO);

                var benfdetail = await db.NMCLAIMBENFDETAIL.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.SUB_CLAIM_NO == request.SUB_CLAIM_NO && e.PARTICIPANT_ID == request.PARTICIPANT_ID && e.EST_ID == request.EST_ID);

                // FIX: without a beneficiary row there is nothing to bind, and the
                // original code threw a NullReferenceException here.
                if (benfdetail == null || estimation == null || estimationDetails == null)
                {
                    return response;
                }

                response.pol_BankDetailsList.Add(new Bank { BANK_CODE = benfdetail.BANK_CODE, BANK_NAME_EN = benfdetail.BENF_NAME_EN });

                var nmClaimRiskDetail = await db.TBLNMCLAIMRISKDETAIL.AsNoTracking()
                    .Where(e => e.SUB_CLAIM_NO == request.SUB_CLAIM_NO && e.PARTICIPANT_ID == request.PARTICIPANT_ID && e.EST_ID == request.EST_ID)
                    .Select(s => s.RISK_NO)
                    .FirstOrDefaultAsync();

                var deductible = estimationDetails.DEDUCTIBLE_AMOUNT ?? 0;
                var netAmount = (estimation.EST_AMOUNT - deductible).RoundUp();

                decimal? vatAmount = 0M;
                if (vatPercent != null)
                {
                    vatAmount = (Convert.ToDecimal(vatPercent.TAX_VALUE) * netAmount) / 100;
                }

                response.SUB_CLAIM_NO = request.SUB_CLAIM_NO;
                response.PARTICIPANT_ID = request.PARTICIPANT_ID;
                response.EST_ID = request.EST_ID;
                response.EST_TYPE = estimationDetails.EST_TYPE;
                response.EST_AMOUNT = netAmount;
                response.ESTIMATION_DATE = estimation.ESTIMATION_DATE;

                var surveyCustCode = await db.TBLCLAIM_SURVEY
                    .Where(x => x.CLAIM_NO == request.CLAIM_NO)
                    .Select(x => x.SURVEY_CUST_CODE)
                    .FirstOrDefaultAsync();

                if (surveyCustCode != null && estimationDetails.ESTIMATION_CODE == 27)
                {
                    response.CUST_CODE = Convert.ToInt64(surveyCustCode);
                }
                else
                {
                    response.CUST_CODE = benfdetail.CUST_CODE;
                }

                response.IS_BENF_SUBMITTED = estimation.IS_BENF_SUBMITTED;
                response.CUST_CODENAME = await db.TBLCUSTOMER.AsNoTracking()
                    .Where(x => x.CUST_CODE == response.CUST_CODE)
                    .Select(s => s.CUST_NAME)
                    .FirstOrDefaultAsync();
                response.PAYEE_CODE = benfdetail.PAYEE_CODE;

                if (response.PAYEE_CODE > 0)
                {
                    response.PAYEE_CODENAME = await db.TBLCUSTOMER.AsNoTracking()
                        .Where(x => x.CUST_CODE == response.PAYEE_CODE)
                        .Select(s => s.PREMIA_CODE + "~" + s.CUST_NAME)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    response.PAYEE_CODENAME = await db.TBLCUSTOMER.AsNoTracking()
                        .Where(x => x.CUST_CODE == response.CUST_CODE)
                        .Select(s => s.PREMIA_CODE + "~" + s.CUST_NAME)
                        .FirstOrDefaultAsync();
                }

                if (response.PAYEE_CODENAME.HasValue())
                {
                    response.pol_BankDetailsList.Add(new Bank { BANK_CODE = response.PAYEE_CODENAME, BANK_NAME_EN = response.PAYEE_CODENAME });
                }
                if (benfdetail.BANK_CODE.HasValue())
                {
                    response.pol_BankDetailsList.Add(new Bank { BANK_CODE = benfdetail.BANK_CODE, BANK_NAME_EN = benfdetail.BENF_NAME_EN });
                }

                response.RISK_ID = nmClaimRiskDetail != 0 ? nmClaimRiskDetail : 0;
                response.COVER_CODE = estimation.COVERCODE ?? "";

                if (benfdetail.VAT_AMOUNT == null)
                {
                    response.VAT_PERCENT = vatPercent?.TAX_VALUE;
                    response.VAT_AMOUNT = vatAmount;
                    response.IS_VAT_APPLICABLE = false;
                }
                else
                {
                    response.VAT_PERCENT = benfdetail.VAT_PERCENT;
                    response.VAT_AMOUNT = benfdetail.VAT_AMOUNT;
                    response.INVOICE_NO = benfdetail.INVOICE_NO;
                    response.INVOICE_DATE = benfdetail.INVOICE_DATE;
                    response.VAT_RES_NO = benfdetail.VAT_REQ_NO;
                    response.IS_VAT_APPLICABLE = true;
                }

                response.COUNTRY_CODE = benfdetail.COUNTRY_CODE;
                response.PIN_CODE = benfdetail.PIN_CODE;
                response.NAT_CODE = benfdetail.NAT_CODE ?? 0;
                response.CR_NO = benfdetail.CR_NO;

                if (benfdetail.BENF_NAME_EN == null)
                {
                    response.BENF_NAME = await (from pp in db.TBLPOLICY.AsNoTracking().Where(x => x.POLICY_NO == processor.POLICY_NO)
                                                join ass in db.TBLASSURED.AsNoTracking() on pp.ASSURED_ID equals ass.CODE
                                                select ass.AS_NAME).FirstOrDefaultAsync();
                }
                else
                {
                    response.BENF_NAME = benfdetail.BENF_NAME_EN;
                }

                if (benfdetail.BENF_MOBILENO == null)
                {
                    response.BENF_MOBILENO = await db.TBLCUSTOMER.AsNoTracking()
                        .Where(x => x.CUST_CODE == response.CUST_CODE)
                        .Select(s => s.MOBILE)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    response.BENF_MOBILENO = benfdetail.BENF_MOBILENO;
                }

                response.BENF_REMARKS = benfdetail.BENF_REMARKS;
                response.RESIDENCE_ID = benfdetail.RESIDENCE_ID;
                response.PAYMENT_MODE = benfdetail.PAYMENT_MODE;
                response.CHEQUECITY_CODE = benfdetail.CHEQUECITY_CODE;

                response.IBAN = benfdetail.IBAN ?? await db.TBLCUSTOMER_BANK.AsNoTracking()
                    .Where(x => x.CUST_CODE == response.CUST_CODE)
                    .Select(s => s.IBAN_NO)
                    .FirstOrDefaultAsync();

                response.BANK_CODE = benfdetail.BANK_CODE ?? await db.TBLCUSTOMER_BANK.AsNoTracking()
                    .Where(x => x.CUST_CODE == response.CUST_CODE)
                    .Select(s => s.BANK_CODE)
                    .FirstOrDefaultAsync();

                response.IS_BENF_INDIVIDUAL = benfdetail.IS_BENF_INDIVIDUAL;
                response.IS_BENF_SAUDI = benfdetail.IS_BENF_SAUDI;
                response.IS_OWNED_BY_SAUDI = benfdetail.IS_OWNED_BY_SAUDI;
                response.NATIVE_ID = benfdetail.NATIVE_ID;
                response.NATIVE_TAX_NO = benfdetail.NATIVE_TAX_NO;
                response.VAT_REQ_NO = benfdetail.VAT_REQ_NO;
                response.PERC_OF_SHARE = benfdetail.PERC_OF_SHARE;
                response.HOME_TAX_REQ_NO = benfdetail.HOME_TAX_REQ_NO ?? "";
                response.HOME_CR_NO = benfdetail.HOME_CR_NO ?? "";

                response.corporatebenfdetail.AddRange(await db.NMCLAIMBENFDETAIL.AsNoTracking()
                    .Where(x => x.SUB_CLAIM_NO == request.SUB_CLAIM_NO && x.PARTICIPANT_ID == request.PARTICIPANT_ID && x.EST_ID == request.EST_ID)
                    .Select(s => new CorporateBenf
                    {
                        CR_NO = s.CR_NO,
                        NAT_CODE = s.NAT_CODE,
                        HOME_CR_NO = s.HOME_CR_NO == "null" ? null : s.HOME_CR_NO,
                        HOME_TAX_REQ_NO = s.HOME_TAX_REQ_NO == "null" ? null : s.HOME_TAX_REQ_NO,
                        PERC_OF_SHARE = s.PERC_OF_SHARE,
                        HOME_SHARE_HOLDER = s.NAME_SHARE_HOLDER == "null" ? null : s.NAME_SHARE_HOLDER
                    })
                    .ToListAsync());

                var parameters = new List<IDataParameter>
                {
                    _DatabaseUtil.GetParameter("P_VIEWNAME", "VW_GETCLAIMCUSTOMERS")
                };

                response.PolicyList = await db._ExecuteQuery("PKG_NMCLAIMS.SP_GETNMCLAIMSVIEWS", parameters.ToArray(), CommandType.StoredProcedure)
                    .GetItems<PolicyList>();
            }

            return response;
        }

        // ---------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------

        private static void PopulateBeneficiary(NMCLAIMBENFDETAIL benf, UpdateBeneficiaryRequest request, CorporateBenf corporate, long? payeeCode, DateTime serverDate)
        {
            benf.CUST_CODE = request.CUST_CODE;
            benf.PAYEE_CODE = payeeCode == 0 ? request.CUST_CODE : payeeCode;
            benf.ADDRESS1 = request.ADDR_1;
            benf.ADDRESS2 = request.ADDR_2;
            benf.CIT_CODE = request.CITY_CODE;
            benf.STATE_CODE = request.STATE_CODE;
            benf.COUNTRY_CODE = request.COUNTRY_CODE;
            benf.PIN_CODE = request.PIN_CODE;
            benf.NAT_CODE = request.NATIONALITY_ID;
            benf.CR_NO = corporate != null ? corporate.CR_NO : request.ID_CRNO;
            benf.BENF_NAME_EN = request.BENF_NAME;
            benf.BENF_NAME_BL = request.BENF_NAME;
            benf.BENF_MOBILENO = request.BENF_MOBILE;
            benf.RESIDENCE_ID = request.RESIDENCE_ID;
            benf.PAYMENT_MODE = request.PAYMENT_MODE;
            benf.BENF_REMARKS = request.BENF_REMARKS;

            if (request.PAYMENT_MODE == "CHEQUE")
            {
                benf.CHEQUECITY_CODE = request.CHEQUE_CITY;
                benf.BANK_CODE = string.Empty;
                benf.IBAN = string.Empty;
            }
            else
            {
                benf.BANK_CODE = request.BANK_NAME;
                benf.IBAN = request.IBAN_NO;
                benf.CHEQUECITY_CODE = string.Empty;
            }

            benf.TRANSDATE = serverDate;
            benf.IS_BENF_INDIVIDUAL = request.IS_BENF_INDIVIDUAL;
            benf.IS_BENF_SAUDI = request.IS_BENF_SAUDI;
            benf.IS_OWNED_BY_SAUDI = request.IS_OWNED_BY_SAUDI;
            benf.NATIVE_ID = request.NATIVE_ID;
            benf.NATIVE_TAX_NO = request.NATIVE_TAX_NO;

            if (request.IS_VAT_APPLICABLE == true)
            {
                benf.VAT_PERCENT = request.VAT_PERCENT;
                benf.VAT_AMOUNT = request.VAT_AMOUNT;
                benf.INVOICE_NO = request.INVOICE_NO;
                benf.INVOICE_DATE = request.INVOICE_DATE;
                benf.VAT_REQ_NO = request.VAT_RES_NO;
            }
            else
            {
                benf.VAT_PERCENT = null;
                benf.VAT_AMOUNT = null;
                benf.INVOICE_NO = null;
                benf.INVOICE_DATE = null;
                benf.VAT_REQ_NO = null;
            }

            benf.PERC_OF_SHARE = corporate != null ? corporate.PERC_OF_SHARE : request.PERC_OF_SHARE;
            benf.HOME_TAX_REQ_NO = (corporate != null ? corporate.HOME_TAX_REQ_NO : request.HOME_TAX_REQ_NO) ?? "";
            benf.HOME_CR_NO = (corporate != null ? corporate.HOME_CR_NO : request.HOME_CR_NO) ?? "";
            benf.NAME_SHARE_HOLDER = corporate != null ? corporate.HOME_SHARE_HOLDER : request.NAME_SHARE_HOLDER;
        }

        private static NMCLAIMBENFDETAIL CreateBeneficiary(UpdateBeneficiaryRequest request, CorporateBenf corporate, long? payeeCode, DateTime serverDate, int benfId)
        {
            // FIX: the original insert paths wrote several fields to the wrong
            // (null) `benfdetail` instance instead of the new row, which threw a
            // NullReferenceException. All fields now populate the new entity.
            var benf = new NMCLAIMBENFDETAIL
            {
                BENF_ID = benfId,
                CLAIM_NO = request.CLAIM_NO,
                PARTICIPANT_ID = request.PARTICIPANT_ID,
                EST_ID = request.EST_ID,
                COVER_CODE = request.COVER_CODE,
                EST_AMOUNT = request.EST_AMOUNT,
                EST_DATE = request.EST_DATE,
                RISK_ID = request.RISK_ID,
                USERID = request.USERID
            };

            PopulateBeneficiary(benf, request, corporate, payeeCode, serverDate);

            return benf;
        }

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
