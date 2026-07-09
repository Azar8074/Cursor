using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace BYAN.Services.NonMotorClaims
{
    /// <summary>
    /// Refactored, de-duplicated version of the three "settlement" e-mail routines
    /// (ProcessorSendMailForApproval, ProcessorSendMailForReturn, EmailFromHigherApproval).
    ///
    /// All three originally repeated the same data loading, FGA resolution, recipient
    /// resolution, e-mail dispatch and error-logging logic. The common parts are now
    /// factored into small private helpers, so each public method only expresses what is
    /// actually different: how the amount is calculated, the subject, the body and the
    /// stored procedure used to resolve recipients.
    /// </summary>
    public partial class NonMotorClaimEmailService
    {
        // Reference to the response object returned by every public method (declared by the
        // enclosing class in the original code). Kept here so the file compiles standalone.
        private readonly ResponseModel response = new ResponseModel();

        private const string ApprovalPageUrl =
            "https://limra.boubyantakaful.com/NMClaims/ClaimProcessor/SettleApprovePending";

        private const string EmailModule = "Non Motor Claims";

        // Error tag preserved verbatim from the original methods so existing log filters keep working.
        private const string ErrorTag = "Email Failed From ProcessorSendMailForApproval";

        private static readonly string[] StandardBcc = { "akhil.kumar@amtpl.com" /*, "mohd.azaruddin@amtpl.com" */ };

        #region Public API

        public ResponseModel ProcessorSendMailForApproval(string CLAIM_NO, string PARTICIPANT_ID, long SETTLE_ID, string USERID)
        {
            using (var db = new _DBContext())
            {
                try
                {
                    var ctx = LoadContext(db, CLAIM_NO, PARTICIPANT_ID, SETTLE_ID);

                    if (!IsPaymentOrRecovery(ctx.Estimation))
                    {
                        LogEstimationTypeError();
                        return response;
                    }

                    var totalAmount = SumEstimationAmountsForApproval(db, CLAIM_NO, PARTICIPANT_ID, ctx.Settlement);
                    // EST_AMOUNT is used as the fallback (the DEDUCTIBLE_AMOUNT subtraction is commented out in the original).
                    var amount = ResolveAmount(totalAmount, ctx.Estimation.EST_AMOUNT);

                    var body = ctx.Estimation.EST_TYPE == "Payment"
                        ? BuildApprovalBody(ctx.Settlement.CLAIM_NO, amount, bold: true)
                        : BuildApprovalBody(ctx.Settlement.CLAIM_NO, amount, bold: false);

                    var parameters = new List<IDataParameter>
                    {
                        _DatabaseUtil.GetParameter("AMOUNT", ctx.Estimation.EST_AMOUNT),
                        _DatabaseUtil.GetParameter("FGA", ResolveFga(ctx.Processor)),
                        _DatabaseUtil.GetParameter("USERID", USERID)
                    };

                    var subject = $"\"{ctx.ProductShortDesc}\" - Claim Notification Send for Approval with claim No: {CLAIM_NO}"
                        + $", Policy No: {ctx.Processor.POLICY_NO}, Policy Holder: {ctx.Policy.INSURED_NAME}"
                        + $" and Risk Name: {ctx.PolicyRisk.RISK_ID}{FormatRiskName(ctx.PolicyRisk.NAME)}";

                    var to = ResolveRecipients(db, "PKG_NMCLAIMS.GETEMAILSBYROLEANDDEPARTMENT", parameters);

                    QueueClaimEmail(to, subject, body, ctx.Processor, GetUserEmails(db, USERID));
                }
                catch (Exception ex)
                {
                    LogEmailError(ex.Message);
                }
            }

            return response;
        }

        public ResponseModel ProcessorSendMailForReturn(string CLAIM_NO, string PARTICIPANT_ID, long SETTLE_ID, string USERID)
        {
            using (var db = new _DBContext())
            {
                try
                {
                    var ctx = LoadContext(db, CLAIM_NO, PARTICIPANT_ID, SETTLE_ID);

                    if (!IsPaymentOrRecovery(ctx.Estimation))
                    {
                        LogEstimationTypeError();
                        return response;
                    }

                    var totalAmount = SumEstimationAmountsForApproval(db, CLAIM_NO, PARTICIPANT_ID, ctx.Settlement);
                    var amount = ResolveAmount(totalAmount, ctx.Estimation.EST_AMOUNT);

                    var returnedByUser = db.TBLUSERMASTER.AsNoTracking()
                        .Where(x => x.USERID == USERID).Select(x => x.USERNAME).FirstOrDefault();

                    var body = BuildReturnBody(ctx.Settlement.CLAIM_NO, amount, returnedByUser);

                    var parameters = new List<IDataParameter>
                    {
                        _DatabaseUtil.GetParameter("FGA", ResolveFga(ctx.Processor))
                    };

                    var subject = $"\"{ctx.ProductShortDesc}\" - Claim Notification Returned from Approval with claim No: {CLAIM_NO}"
                        + $", Policy No: {ctx.Processor.POLICY_NO}, Policy Holder: {ctx.Policy.INSURED_NAME}"
                        + $" and Risk Name: {ctx.PolicyRisk.RISK_ID}{FormatRiskName(ctx.PolicyRisk.NAME)}";

                    var to = ResolveRecipients(db, "PKG_NMCLAIMS.GETEMAILSBYROLEANDDEPARTMENT_RETURN", parameters);

                    QueueClaimEmail(to, subject, body, ctx.Processor, GetUserEmails(db, USERID));
                }
                catch (Exception ex)
                {
                    LogEmailError(ex.Message);
                }
            }

            return response;
        }

        public ResponseModel EmailFromHigherApproval(string CLAIM_NO, string PARTICIPANT_ID, long SETTLE_ID, string USERID)
        {
            using (var db = new _DBContext())
            {
                try
                {
                    var ctx = LoadContext(db, CLAIM_NO, PARTICIPANT_ID, SETTLE_ID);

                    // Only notify once the settlement has actually been paid.
                    if (ctx.Settlement.PAID_AMOUNT == null || ctx.Settlement.PAID_AMOUNT == 0)
                    {
                        return response;
                    }

                    if (!IsPaymentOrRecovery(ctx.Estimation))
                    {
                        LogEstimationTypeError();
                        return response;
                    }

                    var totalAmount = SumPaidAmounts(db, CLAIM_NO, PARTICIPANT_ID, ctx.Settlement);
                    // Fallback here is EST_AMOUNT minus the deductible (matches the original higher-approval logic).
                    var fallback = (ctx.Estimation.EST_AMOUNT ?? 0) - (ctx.Estimation.DEDUCTIBLE_AMOUNT ?? 0);
                    var amount = ResolveAmount(totalAmount, fallback);

                    var body = BuildSettledBody(ctx.Settlement.CLAIM_NO, amount, ctx.Estimation.EST_TYPE == "Payment");

                    var parameters = new List<IDataParameter>
                    {
                        _DatabaseUtil.GetParameter("AMOUNT", ctx.Estimation.EST_AMOUNT),
                        _DatabaseUtil.GetParameter("FGA", ResolveFga(ctx.Processor)),
                        _DatabaseUtil.GetParameter("USERID", USERID)
                    };

                    var subject = $"Claim Notification Settlement Approval with Claim No: {CLAIM_NO}"
                        + $", Policy No: {ctx.Policy.POLICY_NO}, Policy Holder: {ctx.Policy.INSURED_NAME}"
                        + (string.IsNullOrWhiteSpace(ctx.PolicyRisk.NAME) ? "" : $" and Risk Name: {ctx.PolicyRisk.NAME}");

                    var to = ResolveRecipients(db, "PKG_NMCLAIMS.GET_LOWER_EMAILSBY_ROLEANDDEPARTMENT", parameters);

                    QueueClaimEmail(to, subject, body, ctx.Processor, GetUserEmails(db, USERID));
                }
                catch (Exception ex)
                {
                    LogEmailError(ex.Message);
                }
            }

            return response;
        }

        #endregion

        #region Shared data loading

        private sealed class ClaimEmailContext
        {
            public TBLCLAIMSETTLEMENT Settlement { get; set; }
            public TBLCLAIMESTIMATION Estimation { get; set; }
            public TBLCLAIM_PROCESSOR Processor { get; set; }
            public string ProductShortDesc { get; set; }
            public TBLPOLICY Policy { get; set; }
            public TBLPOL_RISK PolicyRisk { get; set; }
        }

        private static ClaimEmailContext LoadContext(_DBContext db, string claimNo, string participantId, long settleId)
        {
            var settlement = db.TBLCLAIMSETTLEMENT
                .FirstOrDefault(x => x.CLAIM_NO == claimNo && x.PARTICIPANT_ID == participantId && x.SETTLE_ID == settleId);

            var estimation = db.TBLCLAIMESTIMATION
                .FirstOrDefault(x => x.CLAIM_NO == claimNo && x.PARTICIPANT_ID == participantId && x.EST_ID == settlement.EST_ID);

            // NOTE: the original code had "x.CLAIM_NO == x.CLAIM_NO" (always true) which is a bug.
            // Fixed here to correctly filter by the requested claim number.
            var processor = db.TBLCLAIM_PROCESSOR
                .FirstOrDefault(x => x.CLAIM_NO == claimNo && x.PARTICIPANT_ID == participantId);

            var productShort = db.TBLPRODUCT
                .Where(x => x.PRODUCT_CODE == processor.PRODUCT_CODE)
                .Select(x => x.PRODSHORT_DESC_EN)
                .FirstOrDefault();

            var policy = db.TBLPOLICY.AsNoTracking()
                .FirstOrDefault(x => x.POLICY_NO == processor.POLICY_NO);

            var polDetails = db.TBLPOL_RISK.AsNoTracking()
                .FirstOrDefault(x => x.POLICY_NO == processor.POLICY_NO && x.RISK_ID == processor.RISKNO);

            return new ClaimEmailContext
            {
                Settlement = settlement,
                Estimation = estimation,
                Processor = processor,
                ProductShortDesc = productShort,
                Policy = policy,
                PolicyRisk = polDetails
            };
        }

        #endregion

        #region Amount helpers

        /// <summary>Uses the aggregated total when it is non-zero, otherwise the supplied fallback.</summary>
        private static decimal ResolveAmount(decimal total, decimal? fallback)
            => total != 0 ? total : (fallback ?? 0);

        /// <summary>
        /// Sums the estimation amounts across every settlement that shares the same
        /// NETPAY_SEND_FOR_APPROVAL batch as the current settlement.
        /// </summary>
        private static decimal SumEstimationAmountsForApproval(
            _DBContext db, string claimNo, string participantId, TBLCLAIMSETTLEMENT settlement)
        {
            if (settlement.NETPAY_SEND_FOR_APPROVAL == null)
            {
                return 0m;
            }

            var estIds = db.TBLCLAIMSETTLEMENT
                .Where(x => x.NETPAY_SEND_FOR_APPROVAL == settlement.NETPAY_SEND_FOR_APPROVAL)
                .Select(x => x.EST_ID)
                .ToList();

            var total = 0m;
            foreach (var estId in estIds)
            {
                var estAmount = db.TBLCLAIMESTIMATION
                    .Where(x => x.CLAIM_NO == claimNo && x.PARTICIPANT_ID == participantId && x.EST_ID == estId)
                    .Select(x => x.EST_AMOUNT)
                    .FirstOrDefault();

                total += estAmount ?? 0;
            }

            return total;
        }

        /// <summary>Sums the paid amounts across every settlement in the same NETPAY_ID batch.</summary>
        private static decimal SumPaidAmounts(
            _DBContext db, string claimNo, string participantId, TBLCLAIMSETTLEMENT settlement)
        {
            if (settlement.NETPAY_ID == null)
            {
                return 0m;
            }

            return db.TBLCLAIMSETTLEMENT
                .Where(x => x.CLAIM_NO == claimNo && x.PARTICIPANT_ID == participantId && x.NETPAY_ID == settlement.NETPAY_ID)
                .Sum(x => (decimal?)x.PAID_AMOUNT) ?? 0m;
        }

        #endregion

        #region Body builders

        private static string BuildApprovalBody(string claimNo, decimal amount, bool bold)
        {
            var claim = bold ? $"<b>{claimNo}</b>" : $"[{claimNo}]";
            var amt = bold ? $"<b>{amount}</b>" : amount.ToString();
            var status = bold ? "<b>Sent For Approval</b>" : "Sent For Approval";

            return "Dear Claim User,<br/><br/>" +
                   $"Claim Number {claim} is sent for the Settlement Approval with Claim Amount {amt} " +
                   $"and the current status of the claim is {status}.<br/><br/>" +
                   $"You can view the claim processing details here: <a href='{ApprovalPageUrl}'>Approval Page</a>.<br/><br/>" +
                   "Thanks,<br/>Claims Department";
        }

        private static string BuildReturnBody(string claimNo, decimal amount, string returnedByUser)
        {
            return "Dear Claim User,<br/><br/>" +
                   $"Claim Number <b>{claimNo}</b> has been <b>returned</b> for further review/correction." +
                   "<br/><br/>" +
                   $"<b>Claim Amount:</b> {amount}<br/>" +
                   "<b>Current Status:</b> Returned<br/>" +
                   $"<b>Returned By:</b> {returnedByUser}<br/>" +
                   "<br/>" +
                   "For more details, please refer to the claim processing system." +
                   "<br/><br/>" +
                   "Thanks,<br/>Claims Department";
        }

        private static string BuildSettledBody(string claimNo, decimal amount, bool bold)
        {
            var claim = bold ? $"<b> {claimNo} </b>" : $"[{claimNo}]";
            var amt = bold ? $"<b> {amount} </b>" : amount.ToString();
            var status = bold ? "<b> Settled </b>" : "Settled";

            return "Dear Claim Department,<br/><br/>" +
                   $"Claim Number {claim} is Approved with Claim Amount {amt} " +
                   $"and the current status of the claim is {status}. <br/><br/> Thanks, <br/>Claims Department";
        }

        private static string FormatRiskName(string riskName)
            => string.IsNullOrWhiteSpace(riskName) ? "" : $" ~ {riskName}";

        #endregion

        #region Recipients / dispatch / logging

        private static bool IsPaymentOrRecovery(TBLCLAIMESTIMATION estimation)
            => estimation.EST_TYPE == "Payment" || estimation.EST_TYPE == "Recovery";

        private static string ResolveFga(TBLCLAIM_PROCESSOR processor)
            => processor.LOB_CODE == "30" ? "LIFE" : "FGA";

        private static string[] ResolveRecipients(_DBContext db, string storedProcedure, List<IDataParameter> parameters)
            => db._ExecuteQuery(storedProcedure, parameters.ToArray(), CommandType.StoredProcedure)
                 .GetItemsSync<string>()
                 .ToArray();

        private static string[] GetUserEmails(_DBContext db, string userId)
            => db.TBLUSERMASTER.AsNoTracking()
                 .Where(x => x.USERID == userId)
                 .Select(x => x.EMAIL)
                 .ToArray();

        private static void QueueClaimEmail(string[] to, string subject, string body, TBLCLAIM_PROCESSOR processor, string[] cc)
        {
            Task.Factory.StartNew(
                () => EmailUtil.Send(to, subject, body, processor.USERID, true, cc, StandardBcc,
                                     0, "", "", "", "", null, false, null, EmailModule),
                TaskCreationOptions.LongRunning);
        }

        private static void LogEmailError(string detail)
            => BYAN.Context.Database.Configurations.ExceptionHandler.Logger
                   .LogErrorToDB(ErrorTag, "", "", detail, null, "10000");

        private static void LogEstimationTypeError()
            => LogEmailError(" estimation.EST_TYPE going other");

        #endregion
    }
}
