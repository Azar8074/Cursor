CREATE OR ALTER PROCEDURE [PKG_NMCLAIMS].[CREDIT_LIFE_PAID_CLAIMS]
--> EXEC [PKG_NMCLAIMS].[CREDIT_LIFE_PAID_CLAIMS] '01/10/2025','31/10/2025'
    @P_FROMDATE  VARCHAR(20),
    @P_TODATE    VARCHAR(20),
    @LOB         VARCHAR(200)  = NULL,
    @P_PRODUCTS  VARCHAR(MAX)  = NULL,
    @P_CUST_CODE VARCHAR(50)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        RC.LOB_CODE                                       AS [Class Code],
        RC.LOB                                            AS [Class],
        RC.PRODUCT                                        AS [Sub Class],
        RC.CLAIM_NO                                       AS [Claim No],
        RC.POLICY_NO                                      AS [Policy No],
        RC.POLICY_FROMDATE                                AS [Policy FromDate],
        RC.POLICY_TODATE                                  AS [Policy ToDate],
        RC.INSURED_NAME                                   AS [Customer Name],
        RC.INSURED_CUST_NAME                              AS [Customer Name Ar],
        RC.ASSURED_NAME                                   AS [Contributor Name],
        RC.LOAN_REF_NO                                    AS [Loan Ref No],
        FORMAT(RC.DECLARATION_DATE,'dd/MM/yyyy')          AS [Declaration Date],
        FORMAT(RC.LOAN_START_DATE,'dd/MM/yyyy')           AS [Loan Start Date],
        RC.RISK_NAME                                      AS [Risk Name],
        RC.RISK_NAME_AR                                   AS [Risk Name Ar],
        FORMAT(RC.DOB,'dd/MM/yyyy')                       AS [DOB],
        FORMAT(RC.ACCIDENTDATE,'dd/MM/yyyy')              AS [Accident Date],
        FORMAT(RC.CLAIM_INTIMATION_DATE,'dd/MM/yyyy')     AS [Incident Date],
        RC.NATURE_OF_LOSS_DESC                            AS [Claim Type],
        YEAR(RC.POLICY_FROMDATE)                          AS [UW Year],
        RC.LOSS_DESCRIPTION                               AS [Loss Description],
        RC.NATURE_OF_LOSS_DESC                            AS [Nature of Loss],
        RC.COVERCODE                                      AS [Cover Code],
        FORMAT(RC.DR_CAL_APPROVAL,'dd/MM/yyyy')           AS [DR/CAL Approval Date],
        RC.CUSTOMER_CODE                                  AS [Customer Code],
        RC.CIVIL_ID                                       AS [Civil ID],
        RC.OCCUPATION                                     AS [Occupation],
        RC.NATIONALITY                                    AS [Nationality],
        RC.FAC_CUSTOMERS                                  AS [Reinsurance Name],
        CASE
            WHEN ISNULL(SUM(RC.FAC_PAID),0) <> 0 THEN 'FAC'
            ELSE 'Treaty'
        END                                               AS [Type of RI],
        FORMAT(RC.SETTLED_DATE,'dd/MM/yyyy')              AS [Paid Date],

        ISNULL(SUM(RC.PAID_AMT),0)                        AS [Paid Amt],

        CASE
            WHEN ISNULL(SUM(RC.PAID_AMT),0) = 0 THEN 0
            ELSE (ISNULL(SUM(RC.RETN_PAID),0) + ISNULL(SUM(RC.RETN_EXCESS_PAID),0)) * 100.0 / SUM(RC.PAID_AMT)
        END                                               AS [BTIC Share%],

        (ISNULL(SUM(RC.RETN_PAID),0) + ISNULL(SUM(RC.RETN_EXCESS_PAID),0)) AS [BTIC Share],

        CASE
            WHEN ISNULL(SUM(RC.PAID_AMT),0) = 0 THEN 0
            ELSE ISNULL(SUM(RC.QS_PAID + RC.SUR_PLUS_PAID),0) * 100.0 / SUM(RC.PAID_AMT)
        END                                               AS [Treaty Share%],

        ISNULL(SUM(RC.QS_PAID),0)                         AS [QS Share],
        ISNULL(SUM(RC.SUR_PLUS_PAID),0)                   AS [SUR Share],

        CASE
            WHEN ISNULL(SUM(RC.PAID_AMT),0) = 0 THEN 0
            ELSE ISNULL(SUM(RC.FAC_PAID),0) * 100.0 / SUM(RC.PAID_AMT)
        END                                               AS [Fac Share%],

        ISNULL(SUM(RC.FAC_PAID),0)                        AS [FAC Paid],
        RC.LOAN_TYPE                                      AS [Loan Type]
    INTO #CREDIT_REPORT
    FROM VW_NM_CLAIM_DATA RC
    WHERE RC.IS_RECOVERY = 'NO'
      AND CAST(RC.SETTLED_DATE AS DATE) BETWEEN CONVERT(DATE,@P_FROMDATE,103) AND CONVERT(DATE,@P_TODATE,103)
      AND RC.LOB_CODE = '30'
      AND RC.PRODUCT_CODE IN (3002,3009)
      AND RC.LOB_CODE IN (
            SELECT ITEM
            FROM DBO.FN_SPLITSTRING(ISNULL(@LOB, RC.LOB_CODE), ',')
          )
      AND RC.PRODUCT_CODE IN (
            SELECT ITEM
            FROM DBO.FN_SPLITSTRING(ISNULL(@P_PRODUCTS, RC.PRODUCT_CODE), ',')
          )
      AND RC.INSURED_CUST_CODE = ISNULL(@P_CUST_CODE, RC.INSURED_CUST_CODE)
    GROUP BY
        RC.LOB_CODE, RC.LOB, RC.PRODUCT, RC.CLAIM_NO, RC.POLICY_NO, RC.INSURED_NAME, RC.INSURED_CUST_NAME, RC.ASSURED_NAME, RC.LOAN_REF_NO,
        FORMAT(RC.DECLARATION_DATE,'dd/MM/yyyy'), FORMAT(RC.LOAN_START_DATE,'dd/MM/yyyy'), RC.RISK_NAME, RC.RISK_NAME_AR,
        FORMAT(RC.DOB,'dd/MM/yyyy'), FORMAT(RC.ACCIDENTDATE,'dd/MM/yyyy'), FORMAT(RC.CLAIM_INTIMATION_DATE,'dd/MM/yyyy'),
        RC.NATURE_OF_LOSS_DESC, YEAR(RC.POLICY_FROMDATE), FORMAT(RC.SETTLED_DATE,'dd/MM/yyyy'), RC.LOAN_TYPE, RC.FAC_CUSTOMERS, RC.POLICY_FROMDATE,
        RC.POLICY_TODATE,
        RC.LOSS_DESCRIPTION,
        RC.COVERCODE,
        RC.DECLARATION_DATE,
        RC.DR_CAL_APPROVAL,
        RC.CUSTOMER_CODE,
        RC.CIVIL_ID,
        RC.OCCUPATION,
        RC.NATIONALITY;

    /*
        Detail rows followed by a TOTAL row appended via UNION ALL.
        The TOTAL SELECT must expose exactly the same 40 columns, in the same
        order, as #CREDIT_REPORT. ORDER BY is applied once, after the union,
        and may only reference columns that are in the select list.
    */
    SELECT *
    FROM #CREDIT_REPORT
    WHERE [Paid Amt] <> 0

    UNION ALL

    SELECT
        'TOTAL',                 -- [Class Code]
        '',                      -- [Class]
        '',                      -- [Sub Class]
        '',                      -- [Claim No]
        '',                      -- [Policy No]
        NULL,                    -- [Policy FromDate]
        NULL,                    -- [Policy ToDate]
        '',                      -- [Customer Name]
        '',                      -- [Customer Name Ar]
        '',                      -- [Contributor Name]
        '',                      -- [Loan Ref No]
        '',                      -- [Declaration Date]
        '',                      -- [Loan Start Date]
        '',                      -- [Risk Name]
        '',                      -- [Risk Name Ar]
        '',                      -- [DOB]
        '',                      -- [Accident Date]
        '',                      -- [Incident Date]
        '',                      -- [Claim Type]
        NULL,                    -- [UW Year]
        '',                      -- [Loss Description]
        '',                      -- [Nature of Loss]
        '',                      -- [Cover Code]
        '',                      -- [DR/CAL Approval Date]
        '',                      -- [Customer Code]
        '',                      -- [Civil ID]
        '',                      -- [Occupation]
        '',                      -- [Nationality]
        '',                      -- [Reinsurance Name]
        '',                      -- [Type of RI]
        '',                      -- [Paid Date]
        SUM([Paid Amt]),         -- [Paid Amt]
        0,                       -- [BTIC Share%]
        SUM([BTIC Share]),       -- [BTIC Share]
        0,                       -- [Treaty Share%]
        SUM([QS Share]),         -- [QS Share]
        SUM([SUR Share]),        -- [SUR Share]
        0,                       -- [Fac Share%]
        SUM([FAC Paid]),         -- [FAC Paid]
        ''                       -- [Loan Type]
    FROM #CREDIT_REPORT
    WHERE [Paid Amt] <> 0
    ORDER BY
        [Class Code],   -- detail rows are '30', the TOTAL row ('TOTAL') sorts last
        [Claim No];

END;
