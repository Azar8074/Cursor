CREATE PROCEDURE [PKG_NMCLAIMS].[CREDIT_LIFE_OS_CLAIMS]
--> EXEC [PKG_NMCLAIMS].[CREDIT_LIFE_OS_CLAIMS] '30/10/2025',NULL,NULL,NULL
    @P_TODATE     VARCHAR(20),
    @LOB          VARCHAR(200) = NULL,
    @P_PRODUCTS   VARCHAR(MAX) = NULL,
    @P_CUST_CODE  VARCHAR(50)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        A.LOB_CODE        AS [Class Code],
        A.PRODUCT_CODE    AS [Sub Class Code],
        A.CLAIM_NO        AS [Claim Number],
        A.POLICY_NO       AS [Policy Number],
        A.POLICY_FROMDATE AS [Policy From Date],
        A.POLICY_TODATE   AS [Policy To Date],
        A.CIVIL_ID        AS [Civil ID],
        A.CUSTOMER_CODE   AS [Customer Code],
        A.INSURED_NAME    AS [Company Name],
        A.INSURED_CUST_NAME AS [Company Name Ar],
        A.ASSURED_NAME    AS [Assured Name],
        A.CAUSE_OF_LOSS_DESC  AS [Cause Of Loss],
        A.NATURE_OF_LOSS_DESC AS [Nature Of Loss],
        A.ACCIDENTDATE    AS [Loss Date],
        A.OS_DATE         AS [OS Date],
        A.CLAIM_INTIMATION_DATE AS [Date of Intimation],
        A.NATURE_OF_LOSS_DESC   AS [Type of claim],
        A.UW_YEAR         AS [UW Year],
        A.FAC_CUSTOMERS   AS [Reins Name],
        -- Per-row evaluation: FAC if this claim carries any FAC outstanding, otherwise Treaty.
        -- (Previously used SUM(...) without a GROUP BY, which is invalid in this row-level SELECT.)
        CASE WHEN ISNULL(A.FAC_OS, 0) <> 0 THEN 'FAC' ELSE 'Treaty' END AS [Type of RI],
        ISNULL(A.EST_AMOUNT, 0) AS [O/S Amount],
        ISNULL((ISNULL(A.RETN_OS, 0) + ISNULL(A.RETN_EXCESS_OS, 0)), 0) AS [Retn OS],
        ISNULL((ISNULL(A.RETN_OS_PERCENT, 0) + ISNULL(A.RETN_EXCESS_OS_PERCENT, 0)), 0) AS [BTIC Share(%)],
        ISNULL(A.QS_OS, 0)              AS [QS OS],
        ISNULL(A.QS_OS_PERCENT, 0)      AS [QS Share(%)],
        ISNULL(A.SUR_PLUS_OS, 0)        AS [Sur Plus OS],
        ISNULL(A.SUR_PLUS_OS_PERCENT, 0) AS [Sur Plus Share(%)],
        ISNULL(A.FAC_OS, 0)             AS [Fac OS],
        ISNULL(A.FAC_OS_PERCENT, 0)     AS [Fac Share(%)],
        A.LOAN_REF_NO     AS [Loan Number],
        A.LOAN_START_DATE AS [Loan Start Date],
        A.DECLARATION_DATE AS [Declaration Date],
        A.LOAN_TYPE       AS [Loan Type],
        A.DR_CAL_APPROVAL AS [DR/CAL Approval Date],
        A.COVERCODE       AS [CoverCode],
        A.RISK_NAME       AS [Risk Name],
        A.DOB             AS [DOB],
        A.LOSS_DESCRIPTION AS [Loss Description],
        A.POLICY_TYPE     AS [Policy Type],
        A.ClaimYear       AS [Claim Year],
        A.OCCUPATION      AS [Occupation],
        A.NATIONALITY     AS [Nationality]
    INTO #OS_REPORT
    FROM VW_NM_CLAIM_DATA A
    WHERE A.PRODUCT_CODE IN ('3001', '3002', '3009')
      AND (A.SETTLED_DATE IS NULL OR CAST(A.SETTLED_DATE AS DATE) > CONVERT(DATE, @P_TODATE, 103))
      AND CONVERT(DATE, A.OS_DATE, 103) <= CONVERT(DATE, @P_TODATE, 103)
      AND A.IS_RECOVERY = 'No'
      AND (A.EST_CLOSED_DATE IS NULL OR CAST(A.EST_CLOSED_DATE AS DATE) > CONVERT(DATE, @P_TODATE, 103))
      AND A.LOB_CODE IN (SELECT ITEM FROM DBO.FN_SPLITSTRING(ISNULL(@LOB, A.LOB_CODE), ','))
      AND A.PRODUCT_CODE IN (SELECT ITEM FROM DBO.FN_SPLITSTRING(ISNULL(@P_PRODUCTS, A.PRODUCT_CODE), ','))
      AND A.INSURED_CUST_CODE = ISNULL(@P_CUST_CODE, A.INSURED_CUST_CODE);
      --AND A.CUSTOMER_CODE = ISNULL(@P_CUST_CODE, A.CUSTOMER_CODE);

    INSERT INTO #OS_REPORT
    (
        [Class Code], [Sub Class Code], [Claim Number], [Policy Number],
        [Company Name], [Company Name Ar], [Assured Name], [Cause Of Loss],
        [Loss Date], [OS Date], [Date of Intimation], [Type of claim],
        [UW Year],
        [O/S Amount], [Retn OS], --[Retn Excess OS],
        [QS OS], [Sur Plus OS], [Fac OS],
        [Loan Number], [Loan Start Date], [DR/CAL Approval Date], [CoverCode],
        [Risk Name], [DOB], [Loss Description],
        [Declaration Date], [Policy Type], [Claim Year], [Loan Type]
    )
    SELECT
        'TOTAL', '', '', '',
        '', '', '', '',
        NULL, NULL, NULL, '',
        NULL,
        SUM([O/S Amount]),
        SUM([Retn OS]),
        --SUM([Retn Excess OS]),
        SUM([QS OS]),
        SUM([Sur Plus OS]),
        SUM([Fac OS]),
        '', NULL, NULL, '',
        '', NULL, '',
        NULL, '', NULL, NULL
    FROM #OS_REPORT;

    SELECT *
    FROM #OS_REPORT
    ORDER BY
        CASE WHEN [Class Code] = 'TOTAL' THEN 1 ELSE 0 END,
        [OS Date];
END;
