IF OBJECT_ID('SPLX.splx_dal_sel_security_byuserbyuie_up', 'P') IS NOT NULL
	DROP PROCEDURE [SPLX].[splx_dal_sel_security_byuserbyuie_up]
GO

/****** Object:  StoredProcedure [SPLX].[splx_dal_sel_security_byuserbyuie_up]    Script Date: 6/23/2016 3:56:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Steve Shortt
-- Create date: 08/23/2007
-- Modified:	10/15/2007, Added support for nested groups
-- Description:	Select Security by User, UI_Element
-- =============================================
create PROCEDURE [SPLX].[splx_dal_sel_security_byuserbyuie_up]

	@UIE_UNIQUE_NAME		varchar(50)
	,@SPLX_USER_ID			uniqueidentifier = NULL
	,@EXTERNAL_GROUP_LIST	varchar(max)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--get the SPLX_UI_ELEMENT_ID for the @UIE_UNIQUE_NAME parameter
	DECLARE @SPLX_UI_ELEMENT_ID uniqueidentifier
	SELECT @SPLX_UI_ELEMENT_ID = SPLX_UI_ELEMENT_ID
		FROM splx.SPLX_UI_ELEMENTS
		WHERE UIE_UNIQUE_NAME = @UIE_UNIQUE_NAME;


	--- UIE_LIST --------------------------------------------
	CREATE TABLE #UIE_LIST (
		SPLX_UI_ELEMENT_ID [uniqueidentifier] NOT NULL
		,UIE_UNIQUE_NAME [varchar](50) NOT NULL
		,UIE_PARENT_ID [uniqueidentifier] NULL
		,UIE_LEVEL [int] NOT NULL
		,UIE_DACL_INHERIT [BIT] NULL
		,UIE_SACL_INHERIT [BIT] NULL
		,UIE_SACL_AUDIT_TYPE_FILTER [INT] NULL
	);

	--populate temp table: recurse the UI_ELEMENTS using @SPLX_UI_ELEMENT_ID as the topmost element
	WITH CHILD_ELEMENTS(SPLX_UI_ELEMENT_ID, UIE_UNIQUE_NAME, UIE_PARENT_ID, UIE_LEVEL, UIE_DACL_INHERIT, UIE_SACL_INHERIT, UIE_SACL_AUDIT_TYPE_FILTER) AS 
	(
		SELECT SPLX_UI_ELEMENT_ID, UIE_UNIQUE_NAME, UIE_PARENT_ID, 1 AS UIE_LEVEL, UIE_DACL_INHERIT, UIE_SACL_INHERIT, UIE_SACL_AUDIT_TYPE_FILTER
			FROM splx.SPLX_UI_ELEMENTS
			WHERE SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID
		UNION ALL
		SELECT p.SPLX_UI_ELEMENT_ID, p.UIE_UNIQUE_NAME, p.UIE_PARENT_ID, UIE_LEVEL + 1, p.UIE_DACL_INHERIT, p.UIE_SACL_INHERIT, p.UIE_SACL_AUDIT_TYPE_FILTER
			FROM splx.SPLX_UI_ELEMENTS p
				INNER JOIN CHILD_ELEMENTS c
				ON p.SPLX_UI_ELEMENT_ID = c.UIE_PARENT_ID
	)
	INSERT INTO #UIE_LIST
		SELECT SPLX_UI_ELEMENT_ID, UIE_UNIQUE_NAME, UIE_PARENT_ID, 0 AS UIE_LEVEL, UIE_DACL_INHERIT, UIE_SACL_INHERIT, UIE_SACL_AUDIT_TYPE_FILTER
			FROM splx.SPLX_UI_ELEMENTS
			WHERE SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID
		UNION ALL
		SELECT SPLX_UI_ELEMENT_ID, UIE_UNIQUE_NAME, UIE_PARENT_ID, UIE_LEVEL, UIE_DACL_INHERIT, UIE_SACL_INHERIT, UIE_SACL_AUDIT_TYPE_FILTER
			FROM CHILD_ELEMENTS
	--- UIE_LIST --------------------------------------------


	--- VR_LIST --------------------------------------------
	CREATE TABLE #VR_LIST (
			SPLX_VALIDATION_RULE_ID [uniqueidentifier] NOT NULL
	);

	WITH VR_LIST( SPLX_VALIDATION_RULE_ID )
	AS
	(
		SELECT
			SPLX_VALIDATION_RULE_ID
		FROM splx.SPLX_VALIDATION_RULES
		WHERE
			SPLX_UI_ELEMENT_ID in (SELECT SPLX_UI_ELEMENT_ID FROM #UIE_LIST) 
			AND
			VR_PARENT_ID IS NULL
			
		UNION ALL
		
		SELECT
			r.SPLX_VALIDATION_RULE_ID
		FROM splx.SPLX_VALIDATION_RULES r
		INNER JOIN VR_LIST
			ON r.VR_PARENT_ID = VR_LIST.SPLX_VALIDATION_RULE_ID
	)
	INSERT INTO #VR_LIST
		SELECT * FROM VR_LIST
	--- VR_LIST --------------------------------------------


	--- group membership --------------------------------------------
	CREATE TABLE #gm (
		[SPLX_GROUP_ID] [uniqueidentifier] NOT NULL
		,[GROUP_MASK] varbinary(max) NOT NULL
	);

	INSERT INTO #gm
		SELECT
			gm.SPLX_GROUP_ID
			,g.GROUP_MASK
		FROM
			splx.SPLX_GROUP_MEMBERSHIP gm
				INNER JOIN
			splx.SPLX_GROUPS g ON gm.SPLX_GROUP_ID = g.SPLX_GROUP_ID
				INNER JOIN
			splx.SPLX_USERS u ON gm.SPLX_USER_ID = u.SPLX_USER_ID
		WHERE
			gm.SPLX_USER_ID = @SPLX_USER_ID
			AND
			g.GROUP_ENABLED = 1
			AND
			u.USER_ENABLED = 1

		UNION

		SELECT
			SPLX_GROUP_ID
			,GROUP_MASK
		FROM
			splx.SPLX_GROUPS g
				INNER JOIN
			(SELECT DISTINCT value AS GROUP_NAME FROM ap_Split( @EXTERNAL_GROUP_LIST, ',' )) eg
				ON g.GROUP_NAME = eg.GROUP_NAME
		WHERE
			g.GROUP_ENABLED = 1
			AND
			g.GROUP_LOCAL = 0
	--- group membership --------------------------------------------


	--- nested group membership --------------------------------------------
	CREATE TABLE #nestedgm (
		[SPLX_GROUP_ID] [uniqueidentifier] NOT NULL
		,[GROUP_MASK] varbinary(max) NOT NULL
	);

	--recurse up from user
	WITH nested ( PARENT_GROUP_ID, GROUP_MASK )
	AS
	(
		SELECT
			ng.PARENT_GROUP_ID
			,g.GROUP_MASK
		FROM splx.SPLX_NESTED_GROUPS ng
			INNER JOIN
				#gm ON ng.CHILD_GROUP_ID = #gm.SPLX_GROUP_ID
			INNER JOIN
				splx.SPLX_GROUPS g on ng.PARENT_GROUP_ID = g.SPLX_GROUP_ID
				
			UNION ALL
			
		SELECT
			ng.PARENT_GROUP_ID
			,g.GROUP_MASK
		FROM splx.SPLX_NESTED_GROUPS ng
			INNER JOIN
				nested ON ng.CHILD_GROUP_ID = nested.PARENT_GROUP_ID
			INNER JOIN
				splx.SPLX_GROUPS g on ng.PARENT_GROUP_ID = g.SPLX_GROUP_ID
	)
	INSERT INTO #nestedgm
		SELECT DISTINCT PARENT_GROUP_ID, GROUP_MASK FROM nested
	--- nested group membership --------------------------------------------


	--- total trustees --------------------------------------------
	CREATE TABLE #trustees_mask (
		[GROUP_MASK] varbinary(max) NOT NULL
	);
	INSERT INTO #trustees_mask
			SELECT GROUP_MASK FROM #gm
				UNION
			SELECT GROUP_MASK FROM #nestedgm
	--- total trustees --------------------------------------------


--	just for debugging:
--	SELECT * FROM #UIE_LIST


	SELECT
		@SPLX_USER_ID AS SPLX_USER_ID
		,splx.Splx_GetTableOr( '#trustees_mask', 'GROUP_MASK' ) AS 'GROUP_MEMBERSHIP_MASK'

	--select Aces
	SELECT DISTINCT
		#UIE_LIST.UIE_UNIQUE_NAME
		,@SPLX_USER_ID SPLX_USER_ID	--Need this for ID checking at UI: don't remove it
		,aces.SPLX_ACE_ID
		,aces.ACE_ACCESS_MASK
		,aces.ACE_ACCESS_TYPE1
		,aces.ACE_ACCESS_TYPE2
		,ISNULL(aces.ACE_ACCESS_TYPE2, 0) IS_AUDIT_ACE
		,aces.ACE_INHERIT
		,aces.ACE_TYPE
		,aces.SPLX_UI_ELEMENT_ID
	FROM
		splx.SPLX_ACES aces
			INNER JOIN
		(
			SELECT @SPLX_USER_ID as SPLX_USER_ID, SPLX_GROUP_ID FROM #gm
				UNION
			SELECT @SPLX_USER_ID as SPLX_USER_ID, SPLX_GROUP_ID FROM #nestedgm
		) TRUSTEES ON
				aces.ACE_TRUSTEE_USER_GROUP_ID = TRUSTEES.SPLX_GROUP_ID
					OR
				aces.ACE_TRUSTEE_USER_GROUP_ID = TRUSTEES.SPLX_USER_ID
			INNER JOIN
		#UIE_LIST ON aces.SPLX_UI_ELEMENT_ID = #UIE_LIST.SPLX_UI_ELEMENT_ID


	--select AclInfo
	SELECT
		UIE_UNIQUE_NAME
		,UIE_DACL_INHERIT
		,UIE_SACL_INHERIT
		,UIE_SACL_AUDIT_TYPE_FILTER
		,SPLX_UI_ELEMENT_ID
		,UIE_PARENT_ID
	FROM
		#UIE_LIST
	WHERE
		UIE_DACL_INHERIT IS NOT NULL


	--select RightRoles
	SELECT
		RR_UIE_UNIQUE_NAME 'UIE_UNIQUE_NAME',
		*
	FROM
		splx.SPLX_RIGHT_ROLES rr
	WHERE
		SPLX_UI_ELEMENT_RULE_ID IN
			(SELECT #UIE_LIST.SPLX_UI_ELEMENT_ID FROM #UIE_LIST)
		OR
		SPLX_UI_ELEMENT_RULE_ID IN
			(SELECT #VR_LIST.SPLX_VALIDATION_RULE_ID FROM #VR_LIST)


	SELECT * FROM splx.SPLX_VALIDATION_RULES
		WHERE SPLX_VALIDATION_RULE_ID IN
		(SELECT #VR_LIST.SPLX_VALIDATION_RULE_ID FROM #VR_LIST)
		AND (VR_RULE_TYPE = 'RightRoleIf' OR VR_RULE_TYPE = 'RightRoleElse')



	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#trustees_mask',N'U') IS NOT NULL
		DROP TABLE #trustees_mask

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#gm',N'U') IS NOT NULL
		DROP TABLE #gm

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#nestedgm',N'U') IS NOT NULL
		DROP TABLE #nestedgm

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#VR_LIST',N'U') IS NOT NULL
		DROP TABLE #VR_LIST

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#UIE_LIST',N'U') IS NOT NULL
		DROP TABLE #UIE_LIST


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		'UserInfo,Aces,AclInfo,RightRoles,RightRoleRules' AS Tables

END
GO