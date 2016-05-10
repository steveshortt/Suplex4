/*
renamed all the following 'dal_' -> 'api_'

splx_dal_sel_users
splx_dal_sel_groups
splx_dal_sel_uielementbyparent
splx_dal_sel_uielementbyid_composite
splx_dal_sel_vrbyuieparent_composite
splx_dal_sel_fmbyparent_composite
splx_dal_sel_vrbyparent_composite
splx_dal_sel_vr
splx_dal_sel_fmbyid_composite
splx_dal_sel_groupmembbygroup
splx_dal_sel_groupnestmembbygroup
splx_dal_sel_groupmembbyuser

splx_dal_ins_groupmemb
splx_dal_ins_groupnest
splx_dal_ins_groupmemb
splx_dal_ins_groupnest

splx_dal_del_user
splx_dal_del_group
splx_dal_del_groupmemb
splx_dal_del_groupnest
splx_dal_del_rightrole
splx_dal_del_ace
*/


--Deleted all the following:


USE [Suplex]
GO

/****** Object:  StoredProcedure [dbo].[splx_ruleed_ins_fmecopy]    Script Date: 10/24/2010 09:43:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[splx_ruleed_ins_fmecopy]

	@SPLX_FILLMAP_EXPRESSION_ID_SOURCE	int,
	@SPLX_UIE_VR_PARENT_ID_TARGET		uniqueidentifier,
	@SPLX_FILLMAP_EXPRESSION_ID_NEW		int = 0 OUTPUT

AS
BEGIN

	INSERT INTO SPLX_FILLMAP_EXPRESSIONS
		SELECT
			FME_NAME,
			FME_EVENT_BINDING,
			FME_EXPRESSION,
			FME_EXPRESSION_TYPE,
			FME_SORT_ORDER,
			@SPLX_UIE_VR_PARENT_ID_TARGET
		FROM
			SPLX_FILLMAP_EXPRESSIONS
		WHERE
			SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID_SOURCE


	SELECT @SPLX_FILLMAP_EXPRESSION_ID_NEW = scope_identity()


	INSERT INTO SPLX_FILLMAP_DATABINDINGS
		SELECT
			FMB_UIE_UNIQUE_NAME,
			FMB_PROPERTY_NAME,
			FMB_VALUE,
			FMB_TYPECAST_VALUE,
			FMB_OVERRIDE_VALUE,
			@SPLX_FILLMAP_EXPRESSION_ID_NEW
		FROM
			SPLX_FILLMAP_DATABINDINGS
		WHERE
			SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID_SOURCE

END

GO

/****** Object:  StoredProcedure [dbo].[splx_ruleed_ins_uiecopy]    Script Date: 10/24/2010 09:43:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_ruleed_ins_uiecopy]

	@SPLX_UI_ELEMENT_ID_SOURCE	uniqueidentifier,
	@SPLX_UI_ELEMENT_ID_TARGET	uniqueidentifier,
	@SPLX_UI_ELEMENT_ID_NEW		uniqueidentifier = NULL OUTPUT

AS
BEGIN

	SELECT @SPLX_UI_ELEMENT_ID_NEW = newid()


	DECLARE @uniqueName		VARCHAR(50)
	DECLARE @sourceParent	uniqueidentifier

	SELECT
		@uniqueName = UIE_UNIQUE_NAME,
		@sourceParent = UIE_PARENT_ID
	FROM
		SPLX_UI_ELEMENTS
	WHERE
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID_SOURCE

	--this avoids a unique_key violation on UIE_PARENT_ID + UIE_UNIQUE_NAME
	IF @sourceParent = @SPLX_UI_ELEMENT_ID_TARGET
	BEGIN
		SET @uniqueName = @SPLX_UI_ELEMENT_ID_NEW
	END


	INSERT INTO SPLX_UI_ELEMENTS
		SELECT
			@SPLX_UI_ELEMENT_ID_NEW,
			UIE_NAME,
			UIE_CONTROL_TYPE,
			UIE_DESC,
			UIE_DESC_TOOLTIP,
			@uniqueName,	--(UIE_UNIQUE_NAME)
			UIE_DATA_TYPE,
			UIE_DATA_TYPE_ERR_MSG,
			UIE_FORMAT_STRING,
			UIE_ALLOW_UNDECLARED,
			@SPLX_UI_ELEMENT_ID_TARGET,
			NULL,
			NULL,
			NULL
		FROM
			SPLX_UI_ELEMENTS
		WHERE
			SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID_SOURCE

END

GO

/****** Object:  StoredProcedure [dbo].[splx_ruleed_sel_validation]    Script Date: 10/24/2010 09:43:00 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_ruleed_sel_validation]

AS
BEGIN

	SELECT * FROM SPLX_UI_ELEMENTS
	SELECT * FROM SPLX_VALIDATION_RULES
	SELECT * FROM SPLX_FILLMAP_EXPRESSIONS
	SELECT * FROM SPLX_FILLMAP_DATABINDINGS

	--UI Support
	SELECT
		SPLX_FILLMAP_DATABINDING_ID		ID,
		FMB_UIE_UNIQUE_NAME				Control,
		FMB_PROPERTY_NAME				Property,
		FMB_VALUE						Value,
		FMB_TYPECAST_VALUE				Typecast,
		FMB_OVERRIDE_VALUE				Override
	FROM
		SPLX_FILLMAP_DATABINDINGS
	WHERE
		SPLX_FILLMAP_DATABINDING_ID = 0



	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		'UIElements,ValidationRules,FillMapExpressions,FillMapDataBindings,FillMapDataBindingsPretty' AS Tables

END
GO

/****** Object:  StoredProcedure [dbo].[splx_ruleed_upd_fmbsync]    Script Date: 10/24/2010 09:43:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_ruleed_upd_fmbsync]

	@SPLX_FILLMAP_DATABINDING_ID	int,
	@FMB_UIE_UNIQUE_NAME			varchar(500),
	@FMB_PROPERTY_NAME				varchar(50),
	@FMB_VALUE						varchar(50),
	@FMB_TYPECAST_VALUE				bit,
	@FMB_OVERRIDE_VALUE				bit,
	@SPLX_FILLMAP_EXPRESSION_ID		int

AS
BEGIN

	IF NOT EXISTS ( SELECT SPLX_FILLMAP_DATABINDING_ID FROM SPLX_FILLMAP_DATABINDINGS WHERE SPLX_FILLMAP_DATABINDING_ID = @SPLX_FILLMAP_DATABINDING_ID )
	BEGIN

		INSERT INTO SPLX_FILLMAP_DATABINDINGS
			(
			FMB_UIE_UNIQUE_NAME,
			FMB_PROPERTY_NAME,
			FMB_VALUE,
			FMB_TYPECAST_VALUE,
			FMB_OVERRIDE_VALUE,
			SPLX_FILLMAP_EXPRESSION_ID
			)
			VALUES
			(
			@FMB_UIE_UNIQUE_NAME,
			@FMB_PROPERTY_NAME,
			@FMB_VALUE,
			@FMB_TYPECAST_VALUE,
			@FMB_OVERRIDE_VALUE,
			@SPLX_FILLMAP_EXPRESSION_ID
			)

--		SET @SPLX_FILLMAP_DATABINDING_ID = scope_identity()

	END

	ELSE
	BEGIN

		UPDATE
			SPLX_FILLMAP_DATABINDINGS
		SET
			FMB_UIE_UNIQUE_NAME = @FMB_UIE_UNIQUE_NAME,
			FMB_PROPERTY_NAME = @FMB_PROPERTY_NAME,
			FMB_VALUE = @FMB_VALUE,
			FMB_TYPECAST_VALUE = @FMB_TYPECAST_VALUE,
			FMB_OVERRIDE_VALUE = @FMB_OVERRIDE_VALUE,
			SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID
		WHERE
			SPLX_FILLMAP_DATABINDING_ID = @SPLX_FILLMAP_DATABINDING_ID

	END

END

GO

/****** Object:  StoredProcedure [dbo].[splx_ruleed_upd_fmeparent]    Script Date: 10/24/2010 09:43:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_ruleed_upd_fmeparent]

	@SPLX_FILLMAP_EXPRESSION_ID	int,
	@SPLX_UIE_VR_PARENT_ID		uniqueidentifier

AS
BEGIN

	UPDATE
		SPLX_FILLMAP_EXPRESSIONS
	SET
		SPLX_UIE_VR_PARENT_ID = @SPLX_UIE_VR_PARENT_ID
	WHERE
		SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID

END

GO

/****** Object:  StoredProcedure [dbo].[splx_ruleed_upd_fmesync]    Script Date: 10/24/2010 09:43:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_ruleed_upd_fmesync]

	@SPLX_FILLMAP_EXPRESSION_ID	int = 0 OUTPUT,
	@FME_NAME					varchar(50),
	@FME_EVENT_BINDING			varchar(50),
	@FME_EXPRESSION				varchar(1000),
	@FME_EXPRESSION_TYPE		varchar(50),
	@FME_SORT_ORDER				int = 0,
	@SPLX_UIE_VR_PARENT_ID		uniqueidentifier

AS
BEGIN

	IF NOT EXISTS ( SELECT SPLX_FILLMAP_EXPRESSION_ID FROM SPLX_FILLMAP_EXPRESSIONS WHERE SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID )
	BEGIN

		INSERT INTO SPLX_FILLMAP_EXPRESSIONS
			(
			FME_NAME,
			FME_EVENT_BINDING,
			FME_EXPRESSION,
			FME_EXPRESSION_TYPE,
			FME_SORT_ORDER,
			SPLX_UIE_VR_PARENT_ID
			)
			VALUES
			(
			@FME_NAME,
			@FME_EVENT_BINDING,
			@FME_EXPRESSION,
			@FME_EXPRESSION_TYPE,
			@FME_SORT_ORDER,
			@SPLX_UIE_VR_PARENT_ID
			)

		SELECT @SPLX_FILLMAP_EXPRESSION_ID = scope_identity()

	END

	ELSE
	BEGIN

		UPDATE
			SPLX_FILLMAP_EXPRESSIONS
		SET
			FME_NAME = @FME_NAME,
			FME_EVENT_BINDING = @FME_EVENT_BINDING,
			FME_EXPRESSION = @FME_EXPRESSION,
			FME_EXPRESSION_TYPE = @FME_EXPRESSION_TYPE,
			FME_SORT_ORDER = @FME_SORT_ORDER,
			SPLX_UIE_VR_PARENT_ID = @SPLX_UIE_VR_PARENT_ID
		WHERE
			SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID

	END

END
GO

/****** Object:  StoredProcedure [dbo].[splx_ruleed_upd_uieparent]    Script Date: 10/24/2010 09:43:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[splx_ruleed_upd_uieparent]

	@SPLX_UI_ELEMENT_ID	uniqueidentifier,
	@UIE_PARENT_ID		uniqueidentifier

AS
BEGIN

	UPDATE
		SPLX_UI_ELEMENTS
	SET
		UIE_PARENT_ID = @UIE_PARENT_ID
	WHERE
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID

END

GO

/****** Object:  StoredProcedure [dbo].[splx_ruleed_upd_uiesync]    Script Date: 10/24/2010 09:43:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[splx_ruleed_upd_uiesync]

	@SPLX_UI_ELEMENT_ID		uniqueidentifier,
	@UIE_NAME				varchar(50),
	@UIE_CONTROL_TYPE		varchar(50),
	@UIE_DESC				varchar(255),
	@UIE_DESC_TOOLTIP		bit,
	@UIE_UNIQUE_NAME		varchar(50),
	@UIE_DATA_TYPE			varchar(50),
	@UIE_DATA_TYPE_ERR_MSG	varchar(255),
	@UIE_FORMAT_STRING		varchar(50),
	@UIE_ALLOW_UNDECLARED	bit,
	@UIE_PARENT_ID			uniqueidentifier

AS
BEGIN

	IF NOT EXISTS( SELECT SPLX_UI_ELEMENT_ID FROM SPLX_UI_ELEMENTS WHERE SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID )
	BEGIN

		INSERT INTO SPLX_UI_ELEMENTS
			(
			SPLX_UI_ELEMENT_ID,
			UIE_NAME,
			UIE_CONTROL_TYPE,
			UIE_DESC,
			UIE_DESC_TOOLTIP,
			UIE_UNIQUE_NAME,
			UIE_DATA_TYPE,
			UIE_DATA_TYPE_ERR_MSG,
			UIE_FORMAT_STRING,
			UIE_ALLOW_UNDECLARED,
			UIE_PARENT_ID
			)
			VALUES
			(
			@SPLX_UI_ELEMENT_ID,
			@UIE_NAME,
			@UIE_CONTROL_TYPE,
			@UIE_DESC,
			@UIE_DESC_TOOLTIP,
			@UIE_UNIQUE_NAME,
			@UIE_DATA_TYPE,
			@UIE_DATA_TYPE_ERR_MSG,
			@UIE_FORMAT_STRING,
			@UIE_ALLOW_UNDECLARED,
			@UIE_PARENT_ID
			)

	END

	ELSE
	BEGIN

		UPDATE
			SPLX_UI_ELEMENTS
		SET
			UIE_NAME = @UIE_NAME,
			UIE_CONTROL_TYPE = @UIE_CONTROL_TYPE,
			UIE_DESC = @UIE_DESC,
			UIE_DESC_TOOLTIP = @UIE_DESC_TOOLTIP,
			UIE_UNIQUE_NAME = @UIE_UNIQUE_NAME,
			UIE_DATA_TYPE = @UIE_DATA_TYPE,
			UIE_DATA_TYPE_ERR_MSG = @UIE_DATA_TYPE_ERR_MSG,
			UIE_FORMAT_STRING = @UIE_FORMAT_STRING,
			UIE_ALLOW_UNDECLARED = @UIE_ALLOW_UNDECLARED,
			UIE_PARENT_ID = @UIE_PARENT_ID
		WHERE
			SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID

	END

END

GO

/****** Object:  StoredProcedure [dbo].[splx_ruleed_upd_vrparent]    Script Date: 10/24/2010 09:43:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[splx_ruleed_upd_vrparent]

	@SPLX_VAILDATION_RULE_ID	uniqueidentifier,
	@VR_PARENT_ID				uniqueidentifier,
	@SPLX_UI_ELEMENT_ID			uniqueidentifier

AS
BEGIN

	UPDATE
		SPLX_VALIDATION_RULES
	SET
		VR_PARENT_ID = @VR_PARENT_ID,
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID
	WHERE
		SPLX_VAILDATION_RULE_ID = @SPLX_VAILDATION_RULE_ID

END
GO

/****** Object:  StoredProcedure [dbo].[splx_ruleed_upd_vrsync]    Script Date: 10/24/2010 09:43:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[splx_ruleed_upd_vrsync]

	@SPLX_VAILDATION_RULE_ID	uniqueidentifier,
	@VR_NAME					varchar(50),
	@VR_EVENT_BINDING			varchar(50),
	@VR_COMPARE_VALUE1			varchar(255),
	@VR_EXPRESSION_TYPE1		varchar(50),
	@VR_VALUE_TYPE1				varchar(50),
	@VR_COMPARE_VALUE2			varchar(255),
	@VR_EXPRESSION_TYPE2		varchar(50),
	@VR_VALUE_TYPE2				varchar(50),
	@VR_COMPARE_DATA_TYPE		varchar(50),
	@VR_OPERATOR				varchar(50),
	@VR_ERROR_MESSAGE			varchar(255),
	@VR_FAIL_PARENT				bit,
	@VR_PARENT_ID				uniqueidentifier,
	@SPLX_UI_ELEMENT_ID			uniqueidentifier,
	@VR_SORT_ORDER				int = 0

AS
BEGIN

	IF NOT EXISTS ( SELECT SPLX_VAILDATION_RULE_ID FROM SPLX_VALIDATION_RULES WHERE SPLX_VAILDATION_RULE_ID = @SPLX_VAILDATION_RULE_ID)
	BEGIN

		INSERT INTO SPLX_VALIDATION_RULES
			(
			SPLX_VAILDATION_RULE_ID,
			VR_NAME,
			VR_EVENT_BINDING,
			VR_COMPARE_VALUE1,
			VR_EXPRESSION_TYPE1,
			VR_VALUE_TYPE1,
			VR_COMPARE_VALUE2,
			VR_EXPRESSION_TYPE2,
			VR_VALUE_TYPE2,
			VR_COMPARE_DATA_TYPE,
			VR_OPERATOR,
			VR_ERROR_MESSAGE,
			VR_FAIL_PARENT,
			VR_PARENT_ID,
			SPLX_UI_ELEMENT_ID,
			VR_SORT_ORDER
			)
			VALUES
			(
			@SPLX_VAILDATION_RULE_ID,
			@VR_NAME,
			@VR_EVENT_BINDING,
			@VR_COMPARE_VALUE1,
			@VR_EXPRESSION_TYPE1,
			@VR_VALUE_TYPE1,
			@VR_COMPARE_VALUE2,
			@VR_EXPRESSION_TYPE2,
			@VR_VALUE_TYPE2,
			@VR_COMPARE_DATA_TYPE,
			@VR_OPERATOR,
			@VR_ERROR_MESSAGE,
			@VR_FAIL_PARENT,
			@VR_PARENT_ID,
			@SPLX_UI_ELEMENT_ID,
			@VR_SORT_ORDER
			)

	END

	ELSE
	BEGIN

		UPDATE
			SPLX_VALIDATION_RULES
		SET
			VR_NAME = @VR_NAME,
			VR_EVENT_BINDING = @VR_EVENT_BINDING,
			VR_SORT_ORDER = @VR_SORT_ORDER,
			VR_COMPARE_VALUE1 = @VR_COMPARE_VALUE1,
			VR_EXPRESSION_TYPE1 = @VR_EXPRESSION_TYPE1,
			VR_VALUE_TYPE1 = @VR_VALUE_TYPE1,
			VR_COMPARE_VALUE2 = @VR_COMPARE_VALUE2,
			VR_EXPRESSION_TYPE2 = @VR_EXPRESSION_TYPE2,
			VR_VALUE_TYPE2 = @VR_VALUE_TYPE2,
			VR_COMPARE_DATA_TYPE = @VR_COMPARE_DATA_TYPE,
			VR_OPERATOR = @VR_OPERATOR,
			VR_ERROR_MESSAGE = @VR_ERROR_MESSAGE,
			VR_FAIL_PARENT = @VR_FAIL_PARENT,
			VR_PARENT_ID = @VR_PARENT_ID,
			SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID
		WHERE
			SPLX_VAILDATION_RULE_ID = @SPLX_VAILDATION_RULE_ID

	END

END
GO

/****** Object:  StoredProcedure [dbo].[splx_usrgrp_sel_groupnestbygroup]    Script Date: 10/24/2010 09:43:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Steve Shortt
-- Create date: 10/17/2007
-- Description:	Select Nested Group Membership by Group_ID
-- =============================================
CREATE PROCEDURE [dbo].[splx_usrgrp_sel_groupnestbygroup]

	@SPLX_GROUP_ID		uniqueidentifier,
	@includeDisabled	bit = 0

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @SPLX_GROUP_NAME VARCHAR(300)
	SELECT @SPLX_GROUP_NAME = GROUP_NAME FROM SPLX_GROUPS WHERE SPLX_GROUP_ID = @SPLX_GROUP_ID


	--recurse down from @SPLX_GROUP_ID (find child-nested-groups)
	DECLARE @children TABLE (
		[PARENT_GROUP_ID] [uniqueidentifier] NOT NULL,
		[PARENT_GROUP_NAME] [varchar](300) NOT NULL,
		[CHILD_GROUP_ID] [uniqueidentifier] NOT NULL,
		[CHILD_GROUP_NAME] [varchar](300) NOT NULL,
		[GROUP_LEVEL] [int] NOT NULL
	);
	WITH nested ( PARENT_GROUP_ID, PARENT_GROUP_NAME, CHILD_GROUP_ID, CHILD_GROUP_NAME, Group_Level )
	AS
	(
		SELECT @SPLX_GROUP_ID 'PARENT_GROUP_ID', @SPLX_GROUP_NAME 'PARENT_GROUP_NAME', ng.CHILD_GROUP_ID, g.group_name 'CHILD_GROUP_NAME', -1 Group_Level FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.CHILD_GROUP_ID
			INNER JOIN SPLX_GROUPS p ON p.SPLX_GROUP_ID = ng.PARENT_GROUP_ID
			WHERE ng.PARENT_GROUP_ID = @SPLX_GROUP_ID
		UNION ALL
		SELECT ng.PARENT_GROUP_ID, p.GROUP_NAME 'PARENT_GROUP_NAME', ng.CHILD_GROUP_ID, g.group_name 'CHILD_GROUP_NAME', Group_Level-1 FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.CHILD_GROUP_ID
			INNER JOIN SPLX_GROUPS p ON p.SPLX_GROUP_ID = ng.PARENT_GROUP_ID
			INNER JOIN nested ON ng.PARENT_GROUP_ID = nested.CHILD_GROUP_ID
	)
	INSERT INTO @children
		SELECT DISTINCT * FROM nested ORDER BY Group_Level, CHILD_GROUP_NAME;


	--recurse up from @SPLX_GROUP_ID (find parent-nested-groups)
	WITH nested ( PARENT_GROUP_ID, PARENT_GROUP_NAME, CHILD_GROUP_ID, CHILD_GROUP_NAME, Group_Level )
	AS
	(
		SELECT ng.PARENT_GROUP_ID, g.group_name, ng.CHILD_GROUP_ID, c.group_name, 0 Group_Level FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.PARENT_GROUP_ID
			INNER JOIN SPLX_GROUPS c ON c.SPLX_GROUP_ID = ng.CHILD_GROUP_ID
			WHERE ng.CHILD_GROUP_ID = @SPLX_GROUP_ID
		UNION ALL
		SELECT ng.PARENT_GROUP_ID, g.group_name, ng.CHILD_GROUP_ID, c.group_name, Group_Level+1 FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.PARENT_GROUP_ID
			INNER JOIN SPLX_GROUPS c ON c.SPLX_GROUP_ID = ng.CHILD_GROUP_ID
			INNER JOIN nested ON ng.CHILD_GROUP_ID = nested.PARENT_GROUP_ID
	)
	SELECT PARENT_GROUP_ID, PARENT_GROUP_NAME, CHILD_GROUP_ID, CHILD_GROUP_NAME, Group_Level FROM nested
		UNION
	SELECT PARENT_GROUP_ID, PARENT_GROUP_NAME, CHILD_GROUP_ID, CHILD_GROUP_NAME, Group_Level FROM @children
		ORDER BY Group_Level DESC, CHILD_GROUP_NAME ASC

END
GO

/****** Object:  StoredProcedure [dbo].[splx_usrgrp_sel_groupnestbyuser]    Script Date: 10/24/2010 09:43:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Steve Shortt
-- Create date: 10/17/2007
-- Description:	Select Nested Group Membership by User_ID
-- =============================================
CREATE PROCEDURE [dbo].[splx_usrgrp_sel_groupnestbyuser]

	@SPLX_USER_ID		uniqueidentifier,
	@includeDisabled	bit = 0

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--recurse down from groups of @SPLX_USER_ID (find child nested-groups of user's groupmembership-based groups)
	DECLARE @children TABLE (
		[PARENT_GROUP_ID] [uniqueidentifier] NULL,
		[PARENT_GROUP_NAME] [varchar](300) NULL,
		[CHILD_GROUP_ID] [uniqueidentifier] NOT NULL,
		[CHILD_GROUP_NAME] [varchar](300) NOT NULL,
		[GROUP_LEVEL] [int] NOT NULL
	);
	WITH nested ( PARENT_GROUP_ID, PARENT_GROUP_NAME, CHILD_GROUP_ID, CHILD_GROUP_NAME, Group_Level )
	AS
	(
		SELECT ng.PARENT_GROUP_ID, p.group_name, ng.CHILD_GROUP_ID, g.group_name 'CHILD_GROUP_NAME', -1 Group_Level FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.CHILD_GROUP_ID
			INNER JOIN SPLX_GROUPS p ON p.SPLX_GROUP_ID = ng.PARENT_GROUP_ID
			INNER JOIN
				(
					SELECT SPLX_GROUP_ID
					FROM SPLX_GROUP_MEMBERSHIP
					WHERE SPLX_USER_ID = @SPLX_USER_ID
				) gm
				ON ng.PARENT_GROUP_ID = gm.SPLX_GROUP_ID
		UNION ALL
		SELECT ng.PARENT_GROUP_ID, p.GROUP_NAME 'PARENT_GROUP_NAME', ng.CHILD_GROUP_ID, g.group_name 'CHILD_GROUP_NAME', Group_Level-1 FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.CHILD_GROUP_ID
			INNER JOIN SPLX_GROUPS p ON p.SPLX_GROUP_ID = ng.PARENT_GROUP_ID
			INNER JOIN nested ON ng.PARENT_GROUP_ID = nested.CHILD_GROUP_ID
	)
	INSERT INTO @children
		SELECT DISTINCT * FROM nested ORDER BY Group_Level, CHILD_GROUP_NAME;


	--recurse up from groups of @SPLX_USER_ID (find parent nested-groups of user's groupmembership-based groups)
	WITH nested ( PARENT_GROUP_ID, PARENT_GROUP_NAME, CHILD_GROUP_ID, CHILD_GROUP_NAME, Group_Level )
	AS
	(
		SELECT ng.PARENT_GROUP_ID, g.group_name, ng.CHILD_GROUP_ID, c.group_name, 0 Group_Level FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.PARENT_GROUP_ID
			INNER JOIN SPLX_GROUPS c ON c.SPLX_GROUP_ID = ng.CHILD_GROUP_ID
			INNER JOIN
				(
					SELECT SPLX_GROUP_ID
					FROM SPLX_GROUP_MEMBERSHIP
					WHERE SPLX_USER_ID = @SPLX_USER_ID
				) gm
				ON ng.CHILD_GROUP_ID = gm.SPLX_GROUP_ID
		UNION ALL
		SELECT ng.PARENT_GROUP_ID, g.group_name, ng.CHILD_GROUP_ID, c.group_name, Group_Level+1 FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.PARENT_GROUP_ID
			INNER JOIN SPLX_GROUPS c ON c.SPLX_GROUP_ID = ng.CHILD_GROUP_ID
			INNER JOIN nested ON ng.CHILD_GROUP_ID = nested.PARENT_GROUP_ID
	)
	SELECT PARENT_GROUP_ID, PARENT_GROUP_NAME, CHILD_GROUP_ID, CHILD_GROUP_NAME, Group_Level FROM nested
		UNION
	SELECT PARENT_GROUP_ID, PARENT_GROUP_NAME, CHILD_GROUP_ID, CHILD_GROUP_NAME, Group_Level FROM @children
		ORDER BY Group_Level DESC, CHILD_GROUP_NAME ASC

END
GO

/****** Object:  StoredProcedure [dbo].[x_splx_dal_sel_groupnestbygroup]    Script Date: 10/24/2010 09:43:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Steve Shortt
-- Create date: 10/16/2007
-- Description:	Select Nested Group Membership by Group_ID
-- =============================================
CREATE PROCEDURE [dbo].[x_splx_dal_sel_groupnestbygroup]

	@SPLX_GROUP_ID		uniqueidentifier,
	@includeDisabled	bit = 0

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @SPLX_GROUP_NAME VARCHAR(50)
	SELECT @SPLX_GROUP_NAME = GROUP_NAME FROM SPLX_GROUPS WHERE SPLX_GROUP_ID = @SPLX_GROUP_ID

	DECLARE @children TABLE (
		[CHILD_GROUP_ID] uniqueidentifier,
		[CHILD_GROUP_NAME] varchar(50)
	);

	WITH nested ( CHILD_GROUP_ID, CHILD_GROUP_NAME )
	AS
	(
		SELECT ng.CHILD_GROUP_ID, g.group_name FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.CHILD_GROUP_ID
			WHERE ng.PARENT_GROUP_ID = @SPLX_GROUP_ID
		UNION ALL
		SELECT ng.CHILD_GROUP_ID, g.group_name FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.CHILD_GROUP_ID
			INNER JOIN nested ON ng.PARENT_GROUP_ID = nested.CHILD_GROUP_ID
	)
	INSERT INTO @children
		SELECT DISTINCT CHILD_GROUP_ID, CHILD_GROUP_NAME FROM nested;


	WITH nested ( PARENT_GROUP_ID, PARENT_GROUP_NAME, CHILD_GROUP_ID )
	AS
	(
		SELECT ng.PARENT_GROUP_ID, g.group_name, ng.CHILD_GROUP_ID FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.PARENT_GROUP_ID
			WHERE ng.CHILD_GROUP_ID = @SPLX_GROUP_ID
		UNION ALL
		SELECT ng.PARENT_GROUP_ID, g.group_name, ng.CHILD_GROUP_ID FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.PARENT_GROUP_ID
			INNER JOIN nested ON ng.CHILD_GROUP_ID = nested.PARENT_GROUP_ID
	)
	SELECT DISTINCT n.PARENT_GROUP_ID, n.PARENT_GROUP_NAME, n.CHILD_GROUP_ID FROM nested n
		UNION
	SELECT DISTINCT ng.PARENT_GROUP_ID, n.PARENT_GROUP_NAME, n.CHILD_GROUP_ID FROM nested n
		FULL OUTER JOIN SPLX_NESTED_GROUPS ng ON n.PARENT_GROUP_ID = ng.CHILD_GROUP_ID
		WHERE ng.PARENT_GROUP_ID IS NULL
		UNION
	SELECT CHILD_GROUP_ID, CHILD_GROUP_NAME, CAST(NULL AS uniqueidentifier) FROM @children
		UNION
	SELECT @SPLX_GROUP_ID, @SPLX_GROUP_NAME, CHILD_GROUP_ID FROM @children

END
GO

/****** Object:  StoredProcedure [dbo].[x_splx_dal_sel_groupnestbyuser]    Script Date: 10/24/2010 09:43:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Steve Shortt
-- Create date: 10/16/2007
-- Description:	Select Nested Group Membership by User_ID
-- =============================================
CREATE PROCEDURE [dbo].[x_splx_dal_sel_groupnestbyuser]

	@SPLX_USER_ID		uniqueidentifier,
	@includeDisabled	bit = 0

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	WITH nested ( GROUP_ID, GROUP_NAME, CHILD_GROUP_ID )
	AS
	(
		SELECT ng.PARENT_GROUP_ID 'GROUP_ID', g.group_name, ng.CHILD_GROUP_ID FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.PARENT_GROUP_ID
			INNER JOIN
				(
					SELECT SPLX_GROUP_ID
					FROM SPLX_GROUP_MEMBERSHIP
					WHERE SPLX_USER_ID = @SPLX_USER_ID
				) gm
				ON ng.CHILD_GROUP_ID = gm.SPLX_GROUP_ID
		UNION ALL
		SELECT ng.PARENT_GROUP_ID 'GROUP_ID', g.group_name, ng.CHILD_GROUP_ID FROM SPLX_GROUPS g
			INNER JOIN SPLX_NESTED_GROUPS ng ON g.SPLX_GROUP_ID = ng.PARENT_GROUP_ID
			INNER JOIN nested ON ng.CHILD_GROUP_ID = nested.GROUP_ID
	)
	SELECT
		n.GROUP_ID, n.GROUP_NAME, n.CHILD_GROUP_ID, g.GROUP_NAME 'CHILD_GROUP_NAME'
	FROM
	(
		SELECT DISTINCT GROUP_ID, GROUP_NAME, CHILD_GROUP_ID FROM nested
			UNION
		SELECT gm.SPLX_GROUP_ID 'GROUP_ID', g.GROUP_NAME , ng.CHILD_GROUP_ID
			FROM SPLX_GROUP_MEMBERSHIP gm
				FULL OUTER JOIN SPLX_NESTED_GROUPS ng ON gm.SPLX_GROUP_ID = ng.PARENT_GROUP_ID
				INNER JOIN SPLX_GROUPS g ON gm.SPLX_GROUP_ID = g.SPLX_GROUP_ID
			WHERE SPLX_USER_ID = @SPLX_USER_ID --AND ng.CHILD_GROUP_ID IS NULL
				UNION
		SELECT DISTINCT ng.PARENT_GROUP_ID 'GROUP_ID', n.GROUP_NAME, n.CHILD_GROUP_ID FROM nested n
			FULL OUTER JOIN SPLX_NESTED_GROUPS ng ON n.GROUP_ID = ng.CHILD_GROUP_ID
			WHERE ng.PARENT_GROUP_ID IS NULL
	) AS n
		INNER JOIN SPLX_GROUPS g ON n.CHILD_GROUP_ID = g.SPLX_GROUP_ID
END
GO


USE [Suplex]
GO

/****** Object:  StoredProcedure [dbo].[splx_dal_ins_ace]    Script Date: 10/24/2010 09:49:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[splx_dal_ins_ace]

	@SPLX_ACE_ID				int = 0 OUTPUT,
	@ACE_TRUSTEE_USER_GROUP_ID	uniqueidentifier,
	@SPLX_UI_ELEMENT_ID			uniqueidentifier,
	@ACE_ACCESS_MASK			int,
	@ACE_ACCESS_TYPE1			bit,
	@ACE_ACCESS_TYPE2			bit = NULL,
	@ACE_INHERIT				bit,
	@ACE_TYPE					varchar(50)

AS

	INSERT INTO SPLX_ACES
	(
		ACE_TRUSTEE_USER_GROUP_ID,
		SPLX_UI_ELEMENT_ID,
		ACE_ACCESS_MASK,
		ACE_ACCESS_TYPE1,
		ACE_ACCESS_TYPE2,
		ACE_INHERIT,
		ACE_TYPE
	)
	VALUES
	(
		@ACE_TRUSTEE_USER_GROUP_ID,
		@SPLX_UI_ELEMENT_ID,
		@ACE_ACCESS_MASK,
		@ACE_ACCESS_TYPE1,
		@ACE_ACCESS_TYPE2,
		@ACE_INHERIT,
		@ACE_TYPE
	)

	SELECT @SPLX_ACE_ID = SCOPE_IDENTITY()


GO

/****** Object:  StoredProcedure [dbo].[splx_dal_ins_extgroupscache]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Create date: 11/26/2008
-- Description:	Insert SPLX_EXTERNAL_GROUPS_CACHE
-- =============================================
CREATE PROCEDURE [dbo].[splx_dal_ins_extgroupscache]

	@splx_external_groups_cache_id	int = null OUTPUT
	,@splx_user_id	uniqueidentifier
	,@expiration_date	datetime
	,@group_list	varchar(MAX)

AS
BEGIN

	INSERT INTO SPLX_EXTERNAL_GROUPS_CACHE
		(
		[SPLX_USER_ID]
		,[CACHE_DATE]
		,[EXPIRATION_DATE]
		,[GROUP_LIST]
		)
		VALUES
		(
		@splx_user_id
		,GETDATE()
		,@expiration_date
		,@group_list
		)


	SELECT @splx_external_groups_cache_id = SCOPE_IDENTITY()

END

GO

/****** Object:  StoredProcedure [dbo].[splx_dal_ins_fmb]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_ins_fmb]

	@SPLX_FILLMAP_DATABINDING_ID	int = 0 OUTPUT,
	@FMB_UIE_UNIQUE_NAME			varchar(500),
	@FMB_PROPERTY_NAME				varchar(50),
	@FMB_VALUE						varchar(50),
	@FMB_TYPECAST_VALUE				bit,
	@FMB_OVERRIDE_VALUE				bit,
	@SPLX_FILLMAP_EXPRESSION_ID		int

AS
BEGIN

	INSERT INTO SPLX_FILLMAP_DATABINDINGS
		(
		FMB_UIE_UNIQUE_NAME,
		FMB_PROPERTY_NAME,
		FMB_VALUE,
		FMB_TYPECAST_VALUE,
		FMB_OVERRIDE_VALUE,
		SPLX_FILLMAP_EXPRESSION_ID
		)
		VALUES
		(
		@FMB_UIE_UNIQUE_NAME,
		@FMB_PROPERTY_NAME,
		@FMB_VALUE,
		@FMB_TYPECAST_VALUE,
		@FMB_OVERRIDE_VALUE,
		@SPLX_FILLMAP_EXPRESSION_ID
		)


	SELECT @SPLX_FILLMAP_DATABINDING_ID = scope_identity()

END
GO

/****** Object:  StoredProcedure [dbo].[splx_dal_ins_fme]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_ins_fme]

	@SPLX_FILLMAP_EXPRESSION_ID	int = NULL OUTPUT,
	@FME_NAME					varchar(50),
	@FME_EVENT_BINDING			varchar(50) = NULL,
	@FME_EXPRESSION				varchar(1000) = NULL,
	@FME_EXPRESSION_TYPE		varchar(50) = NULL,
	@FME_SORT_ORDER				int = 0,
	@SPLX_UIE_VR_PARENT_ID		uniqueidentifier

AS
BEGIN

	INSERT INTO SPLX_FILLMAP_EXPRESSIONS
		(
		FME_NAME,
		FME_EVENT_BINDING,
		FME_EXPRESSION,
		FME_EXPRESSION_TYPE,
		FME_SORT_ORDER,
		SPLX_UIE_VR_PARENT_ID
		)
		VALUES
		(
		@FME_NAME,
		@FME_EVENT_BINDING,
		@FME_EXPRESSION,
		@FME_EXPRESSION_TYPE,
		@FME_SORT_ORDER,
		@SPLX_UIE_VR_PARENT_ID
		)


	SELECT @SPLX_FILLMAP_EXPRESSION_ID = scope_identity()

END

GO

/****** Object:  StoredProcedure [dbo].[splx_dal_ins_group]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_ins_group]

	@SPLX_GROUP_ID				uniqueidentifier = NULL OUTPUT,
	@GROUP_NAME					varchar(300),
	@GROUP_DESC					varchar(255),
	@GROUP_LOCAL				bit,
	@GROUP_ENABLED				bit,
	@GROUP_BUILT_IN				bit = 0

AS

BEGIN

	SET @SPLX_GROUP_ID = newid()

	INSERT INTO SPLX_GROUPS
	(
		SPLX_GROUP_ID,
		GROUP_NAME,
		GROUP_DESC,
		GROUP_LOCAL,
		GROUP_ENABLED,
		GROUP_BUILT_IN
	)
	VALUES
	(
		@SPLX_GROUP_ID,
		@GROUP_NAME,
		@GROUP_DESC,
		@GROUP_LOCAL,
		@GROUP_ENABLED,
		@GROUP_BUILT_IN
	)


	SELECT @SPLX_GROUP_ID 'SPLX_GROUP_ID'

END
GO

/****** Object:  StoredProcedure [dbo].[splx_dal_ins_rightrole]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_ins_rightrole]

	@SPLX_RIGHT_ROLE_ID			int = 0 OUTPUT,
	@SPLX_UI_ELEMENT_RULE_ID	uniqueidentifier,
	@RR_UIE_UNIQUE_NAME			varchar(500),
	@RR_ACE_TYPE				varchar(50),
	@RR_RIGHT_NAME				varchar(50),
	@RR_UI_RIGHT				varchar(50),
	@RR_ROLE_TYPE				varchar(50)

AS

	INSERT INTO SPLX_RIGHT_ROLES
	(
		SPLX_UI_ELEMENT_RULE_ID,
		RR_UIE_UNIQUE_NAME,
		RR_ACE_TYPE,
		RR_RIGHT_NAME,
		RR_UI_RIGHT,
		RR_ROLE_TYPE
	)
	VALUES
	(
		@SPLX_UI_ELEMENT_RULE_ID,
		@RR_UIE_UNIQUE_NAME,
		@RR_ACE_TYPE,
		@RR_RIGHT_NAME,
		@RR_UI_RIGHT,
		@RR_ROLE_TYPE
	)


	SELECT @SPLX_RIGHT_ROLE_ID = SCOPE_IDENTITY()


GO

/****** Object:  StoredProcedure [dbo].[splx_dal_ins_uie]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_ins_uie]

	@SPLX_UI_ELEMENT_ID	uniqueidentifier = NULL OUTPUT,
	@UIE_NAME	varchar(50),
	@UIE_CONTROL_TYPE	varchar(50),
	@UIE_DESC	varchar(255) = NULL,
	@UIE_DESC_TOOLTIP	bit = 0,
	@UIE_UNIQUE_NAME	varchar(50),
	@UIE_DATA_TYPE	varchar(50) = NULL,
	@UIE_DATA_TYPE_ERR_MSG	varchar(255) = NULL,
	@UIE_FORMAT_STRING	varchar(50) = NULL,
	@UIE_ALLOW_UNDECLARED	bit = 0,
	@UIE_PARENT_ID	uniqueidentifier = NULL,
	@UIE_DACL_INHERIT	bit = NULL,
	@UIE_SACL_INHERIT	bit = NULL,
	@UIE_SACL_AUDIT_TYPE_FILTER	int = NULL

AS
BEGIN

	SET @SPLX_UI_ELEMENT_ID = newid()

	INSERT INTO SPLX_UI_ELEMENTS
		(
		SPLX_UI_ELEMENT_ID,
		UIE_NAME,
		UIE_CONTROL_TYPE,
		UIE_DESC,
		UIE_DESC_TOOLTIP,
		UIE_UNIQUE_NAME,
		UIE_DATA_TYPE,
		UIE_DATA_TYPE_ERR_MSG,
		UIE_FORMAT_STRING,
		UIE_ALLOW_UNDECLARED,
		UIE_PARENT_ID,
		UIE_DACL_INHERIT,
		UIE_SACL_INHERIT,
		UIE_SACL_AUDIT_TYPE_FILTER
		)
		VALUES
		(
		@SPLX_UI_ELEMENT_ID,
		@UIE_NAME,
		@UIE_CONTROL_TYPE,
		@UIE_DESC,
		@UIE_DESC_TOOLTIP,
		@UIE_UNIQUE_NAME,
		@UIE_DATA_TYPE,
		@UIE_DATA_TYPE_ERR_MSG,
		@UIE_FORMAT_STRING,
		@UIE_ALLOW_UNDECLARED,
		@UIE_PARENT_ID,
		@UIE_DACL_INHERIT,
		@UIE_SACL_INHERIT,
		@UIE_SACL_AUDIT_TYPE_FILTER
		)

END
GO

/****** Object:  StoredProcedure [dbo].[splx_dal_ins_user]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_ins_user]

	@SPLX_USER_ID				uniqueidentifier = NULL OUTPUT,
	@USER_NAME					varchar(50),
	@USER_DESC					varchar(255),
	@USER_LOCAL					bit,
	@USER_PASSWORD				nvarchar(50) = NULL,
	@USER_ENABLED				bit,
	@USER_BUILT_IN				bit = 0

AS

BEGIN

	DECLARE @pswd varbinary(50)
	IF @USER_LOCAL = 1
		SELECT @pswd = CONVERT(varbinary(50), @USER_PASSWORD)

	SET @SPLX_USER_ID = newid()

	INSERT INTO SPLX_USERS
	(
		SPLX_USER_ID,
		USER_NAME,
		USER_DESC,
		USER_LOCAL,
		USER_PASSWORD,
		USER_ENABLED,
		USER_BUILT_IN,
		USER_LOGIN_TRIES
	)
	VALUES
	(
		@SPLX_USER_ID,
		@USER_NAME,
		@USER_DESC,
		@USER_LOCAL,
		@pswd,
		@USER_ENABLED,
		@USER_BUILT_IN,
		0
	)


	SELECT @SPLX_USER_ID 'SPLX_USER_ID'

END
GO

/****** Object:  StoredProcedure [dbo].[splx_dal_ins_vr]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_ins_vr]

	@SPLX_VAILDATION_RULE_ID	uniqueidentifier = NULL OUTPUT,
	@VR_NAME					varchar(50) = NULL,
	@VR_EVENT_BINDING			varchar(50) = NULL,
	@VR_COMPARE_VALUE1			varchar(255) = NULL,
	@VR_EXPRESSION_TYPE1		varchar(50) = NULL,
	@VR_VALUE_TYPE1				varchar(50) = NULL,
	@VR_COMPARE_VALUE2			varchar(255) = NULL,
	@VR_EXPRESSION_TYPE2		varchar(50) = NULL,
	@VR_VALUE_TYPE2				varchar(50) = NULL,
	@VR_COMPARE_DATA_TYPE		varchar(50) = NULL,
	@VR_OPERATOR				varchar(50) = NULL,
	@VR_ERROR_MESSAGE			varchar(255) = NULL,
	@VR_FAIL_PARENT				bit = 0,
	@VR_PARENT_ID				uniqueidentifier = NULL,
	@SPLX_UI_ELEMENT_ID			uniqueidentifier,
	@VR_SORT_ORDER				int = 0,
	@VR_RULE_TYPE				varchar(50),
	@VR_ERROR_UIE_UNIQUE_NAME	varchar(50) = NULL

AS
BEGIN

	SET @SPLX_VAILDATION_RULE_ID = newid()

	INSERT INTO SPLX_VALIDATION_RULES
		(
		SPLX_VAILDATION_RULE_ID,
		VR_NAME,
		VR_EVENT_BINDING,
		VR_COMPARE_VALUE1,
		VR_EXPRESSION_TYPE1,
		VR_VALUE_TYPE1,
		VR_COMPARE_VALUE2,
		VR_EXPRESSION_TYPE2,
		VR_VALUE_TYPE2,
		VR_COMPARE_DATA_TYPE,
		VR_OPERATOR,
		VR_ERROR_MESSAGE,
		VR_FAIL_PARENT,
		VR_PARENT_ID,
		SPLX_UI_ELEMENT_ID,
		VR_SORT_ORDER,
		VR_RULE_TYPE,
		VR_ERROR_UIE_UNIQUE_NAME
		)
		VALUES
		(
		@SPLX_VAILDATION_RULE_ID,
		@VR_NAME,
		@VR_EVENT_BINDING,
		@VR_COMPARE_VALUE1,
		@VR_EXPRESSION_TYPE1,
		@VR_VALUE_TYPE1,
		@VR_COMPARE_VALUE2,
		@VR_EXPRESSION_TYPE2,
		@VR_VALUE_TYPE2,
		@VR_COMPARE_DATA_TYPE,
		@VR_OPERATOR,
		@VR_ERROR_MESSAGE,
		@VR_FAIL_PARENT,
		@VR_PARENT_ID,
		@SPLX_UI_ELEMENT_ID,
		@VR_SORT_ORDER,
		@VR_RULE_TYPE,
		@VR_ERROR_UIE_UNIQUE_NAME
		)

END
GO

/****** Object:  StoredProcedure [dbo].[splx_dal_upd_ace]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[splx_dal_upd_ace]

	@SPLX_ACE_ID				int,
	@ACE_TRUSTEE_USER_GROUP_ID	uniqueidentifier,
	@SPLX_UI_ELEMENT_ID			uniqueidentifier,
	@ACE_ACCESS_MASK			int,
	@ACE_ACCESS_TYPE1			bit,
	@ACE_ACCESS_TYPE2			bit = NULL,
	@ACE_INHERIT				bit,
	@ACE_TYPE					varchar(50)

AS

	UPDATE
		SPLX_ACES
	SET
		ACE_TRUSTEE_USER_GROUP_ID = @ACE_TRUSTEE_USER_GROUP_ID,
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID,
		ACE_ACCESS_MASK = @ACE_ACCESS_MASK,
		ACE_ACCESS_TYPE1 = @ACE_ACCESS_TYPE1,
		ACE_ACCESS_TYPE2 = @ACE_ACCESS_TYPE2,
		ACE_INHERIT = @ACE_INHERIT,
		ACE_TYPE = @ACE_TYPE
	WHERE
		SPLX_ACE_ID = @SPLX_ACE_ID


GO

/****** Object:  StoredProcedure [dbo].[splx_dal_upd_extgroupscachebyuser]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Create date: 11/26/2008
-- Description:	Update SPLX_EXTERNAL_GROUPS_CACHE
-- =============================================
CREATE PROCEDURE [dbo].[splx_dal_upd_extgroupscachebyuser]

	@splx_user_id	uniqueidentifier
	,@expiration_date	datetime
	,@group_list	varchar(MAX)

AS
BEGIN

	UPDATE
		SPLX_EXTERNAL_GROUPS_CACHE
	SET
		[CACHE_DATE] = GETDATE()
		,[EXPIRATION_DATE] = @expiration_date
		,[GROUP_LIST] = @group_list
	WHERE
		[SPLX_USER_ID] = @splx_user_id

END

GO

/****** Object:  StoredProcedure [dbo].[splx_dal_upd_fmb]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_upd_fmb]

	@SPLX_FILLMAP_DATABINDING_ID	int,
	@FMB_UIE_UNIQUE_NAME	varchar(500),
	@FMB_PROPERTY_NAME	varchar(50),
	@FMB_VALUE	varchar(50),
	@FMB_TYPECAST_VALUE	bit,
	@FMB_OVERRIDE_VALUE	bit,
	@SPLX_FILLMAP_EXPRESSION_ID	int

AS
BEGIN

	UPDATE
		SPLX_FILLMAP_DATABINDINGS
	SET
		FMB_UIE_UNIQUE_NAME = @FMB_UIE_UNIQUE_NAME,
		FMB_PROPERTY_NAME = @FMB_PROPERTY_NAME,
		FMB_VALUE = @FMB_VALUE,
		FMB_TYPECAST_VALUE = @FMB_TYPECAST_VALUE,
		FMB_OVERRIDE_VALUE = @FMB_OVERRIDE_VALUE,
		SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID
	WHERE
		SPLX_FILLMAP_DATABINDING_ID = @SPLX_FILLMAP_DATABINDING_ID

END

GO

/****** Object:  StoredProcedure [dbo].[splx_dal_upd_fme]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_upd_fme]

	@SPLX_FILLMAP_EXPRESSION_ID	int,
	@FME_NAME					varchar(50),
	@FME_EVENT_BINDING			varchar(50),
	@FME_EXPRESSION				varchar(1000),
	@FME_EXPRESSION_TYPE		varchar(50),
	@FME_SORT_ORDER				int,
	@SPLX_UIE_VR_PARENT_ID		uniqueidentifier

AS
BEGIN

	UPDATE
		SPLX_FILLMAP_EXPRESSIONS
	SET
		FME_NAME = @FME_NAME,
		FME_EVENT_BINDING = @FME_EVENT_BINDING,
		FME_EXPRESSION = @FME_EXPRESSION,
		FME_EXPRESSION_TYPE = @FME_EXPRESSION_TYPE,
		FME_SORT_ORDER = @FME_SORT_ORDER,
		SPLX_UIE_VR_PARENT_ID = @SPLX_UIE_VR_PARENT_ID
	WHERE
		SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID

END

GO

/****** Object:  StoredProcedure [dbo].[splx_dal_upd_group]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_upd_group]

	@SPLX_GROUP_ID				uniqueidentifier,
	@GROUP_NAME					varchar(50),
	@GROUP_DESC					varchar(255),
	@GROUP_LOCAL				bit,
	@GROUP_ENABLED				bit

AS

BEGIN

	UPDATE
		SPLX_GROUPS
	SET
		GROUP_NAME = @GROUP_NAME,
		GROUP_DESC = @GROUP_DESC,
		GROUP_LOCAL = @GROUP_LOCAL,
		GROUP_ENABLED = @GROUP_ENABLED
	WHERE
		SPLX_GROUP_ID = @SPLX_GROUP_ID

END
GO

/****** Object:  StoredProcedure [dbo].[splx_dal_upd_rightrole]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_upd_rightrole]

	@SPLX_RIGHT_ROLE_ID	int,
	@SPLX_UI_ELEMENT_ID	uniqueidentifier,
	@RR_UIE_UNIQUE_NAME	varchar(500),
	@RR_ACE_TYPE		varchar(50),
	@RR_RIGHT_NAME		varchar(50),
	@RR_UI_RIGHT		varchar(50)

AS

	UPDATE
		SPLX_RIGHT_ROLES
	SET
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID,
		RR_UIE_UNIQUE_NAME = @RR_UIE_UNIQUE_NAME,
		RR_ACE_TYPE = @RR_ACE_TYPE,
		RR_RIGHT_NAME = @RR_RIGHT_NAME,
		RR_UI_RIGHT = @RR_UI_RIGHT
	WHERE
		SPLX_RIGHT_ROLE_ID = @SPLX_RIGHT_ROLE_ID


GO

/****** Object:  StoredProcedure [dbo].[splx_dal_upd_uie]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_upd_uie]

	@SPLX_UI_ELEMENT_ID	uniqueidentifier,
	@UIE_NAME	varchar(50),
	@UIE_CONTROL_TYPE	varchar(50),
	@UIE_DESC	varchar(255),
	@UIE_DESC_TOOLTIP	bit,
	@UIE_UNIQUE_NAME	varchar(50),
	@UIE_DATA_TYPE	varchar(50),
	@UIE_DATA_TYPE_ERR_MSG	varchar(255),
	@UIE_FORMAT_STRING	varchar(50),
	@UIE_ALLOW_UNDECLARED	bit
--	@UIE_PARENT_ID	uniqueidentifier
--	@UIE_DACL_INHERIT	bit,
--	@UIE_SACL_INHERIT	bit,
--	@UIE_SACL_AUDIT_TYPE_FILTER	int

AS
BEGIN

	UPDATE
		SPLX_UI_ELEMENTS
	SET
		UIE_NAME = @UIE_NAME,
		UIE_CONTROL_TYPE = @UIE_CONTROL_TYPE,
		UIE_DESC = @UIE_DESC,
		UIE_DESC_TOOLTIP = @UIE_DESC_TOOLTIP,
		UIE_UNIQUE_NAME = @UIE_UNIQUE_NAME,
		UIE_DATA_TYPE = @UIE_DATA_TYPE,
		UIE_DATA_TYPE_ERR_MSG = @UIE_DATA_TYPE_ERR_MSG,
		UIE_FORMAT_STRING = @UIE_FORMAT_STRING,
		UIE_ALLOW_UNDECLARED = @UIE_ALLOW_UNDECLARED
--		UIE_PARENT_ID = @UIE_PARENT_ID
--		UIE_DACL_INHERIT = @UIE_DACL_INHERIT,
--		UIE_SACL_INHERIT = @UIE_SACL_INHERIT,
--		UIE_SACL_AUDIT_TYPE_FILTER = @UIE_SACL_AUDIT_TYPE_FILTER
	WHERE
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID

END
GO

/****** Object:  StoredProcedure [dbo].[splx_dal_upd_uieaclinfo]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_upd_uieaclinfo]

	@UIE_DACL_INHERIT			bit,
	@UIE_SACL_INHERIT			bit,
	@UIE_SACL_AUDIT_TYPE_FILTER	int,
	@SPLX_UI_ELEMENT_ID			uniqueidentifier

AS

	UPDATE
		SPLX_UI_ELEMENTS
	SET
		UIE_DACL_INHERIT = @UIE_DACL_INHERIT,
		UIE_SACL_INHERIT = @UIE_SACL_INHERIT,
		UIE_SACL_AUDIT_TYPE_FILTER = @UIE_SACL_AUDIT_TYPE_FILTER
	WHERE
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID


GO

/****** Object:  StoredProcedure [dbo].[splx_dal_upd_user]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_upd_user]

	@SPLX_USER_ID				uniqueidentifier,
	@USER_NAME					varchar(50),
	@USER_DESC					varchar(255),
	@USER_PASSWORD				nvarchar(50) = NULL,
	@USER_ENABLED				bit

AS

BEGIN

	DECLARE @pswd varbinary(50)
	SELECT @pswd = CONVERT(varbinary(50), @USER_PASSWORD)

	DECLARE @local bit
	SELECT @local = USER_LOCAL
		FROM SPLX_USERS
		WHERE SPLX_USER_ID = @SPLX_USER_ID

	IF @local = 1
		BEGIN
			IF @pswd IS NULL
				SELECT
					@pswd = USER_PASSWORD
				FROM
					SPLX_USERS
				WHERE
					SPLX_USER_ID = @SPLX_USER_ID
		END
	ELSE
		SET @pswd = NULL

	UPDATE
		SPLX_USERS
	SET
		USER_NAME = @USER_NAME,
		USER_DESC = @USER_DESC,
		USER_PASSWORD = @pswd,
		USER_ENABLED = @USER_ENABLED
	WHERE
		SPLX_USER_ID = @SPLX_USER_ID

END
GO

/****** Object:  StoredProcedure [dbo].[splx_dal_upd_vr]    Script Date: 10/24/2010 09:49:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_upd_vr]

	@SPLX_VAILDATION_RULE_ID	uniqueidentifier,
	@VR_NAME					varchar(50),
	@VR_EVENT_BINDING			varchar(50),
	@VR_SORT_ORDER				int,
	@VR_COMPARE_VALUE1			varchar(255),
	@VR_EXPRESSION_TYPE1		varchar(50),
	@VR_VALUE_TYPE1				varchar(50),
	@VR_COMPARE_VALUE2			varchar(255),
	@VR_EXPRESSION_TYPE2		varchar(50),
	@VR_VALUE_TYPE2				varchar(50),
	@VR_COMPARE_DATA_TYPE		varchar(50),
	@VR_OPERATOR				varchar(50),
	@VR_ERROR_MESSAGE			varchar(255),
	@VR_FAIL_PARENT				bit,
	@VR_RULE_TYPE				varchar(50),
	@VR_ERROR_UIE_UNIQUE_NAME	varchar(50),
	@VR_PARENT_ID				uniqueidentifier,
	@SPLX_UI_ELEMENT_ID			uniqueidentifier

AS
BEGIN

	UPDATE
		SPLX_VALIDATION_RULES
	SET
		VR_NAME = @VR_NAME,
		VR_EVENT_BINDING = @VR_EVENT_BINDING,
		VR_SORT_ORDER = @VR_SORT_ORDER,
		VR_COMPARE_VALUE1 = @VR_COMPARE_VALUE1,
		VR_EXPRESSION_TYPE1 = @VR_EXPRESSION_TYPE1,
		VR_VALUE_TYPE1 = @VR_VALUE_TYPE1,
		VR_COMPARE_VALUE2 = @VR_COMPARE_VALUE2,
		VR_EXPRESSION_TYPE2 = @VR_EXPRESSION_TYPE2,
		VR_VALUE_TYPE2 = @VR_VALUE_TYPE2,
		VR_COMPARE_DATA_TYPE = @VR_COMPARE_DATA_TYPE,
		VR_OPERATOR = @VR_OPERATOR,
		VR_ERROR_MESSAGE = @VR_ERROR_MESSAGE,
		VR_FAIL_PARENT = @VR_FAIL_PARENT,
		VR_RULE_TYPE = @VR_RULE_TYPE,
		VR_ERROR_UIE_UNIQUE_NAME = @VR_ERROR_UIE_UNIQUE_NAME,
		VR_PARENT_ID = @VR_PARENT_ID,
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID
	WHERE
		SPLX_VAILDATION_RULE_ID = @SPLX_VAILDATION_RULE_ID

END
GO


USE [Suplex]
GO

/****** Object:  StoredProcedure [dbo].[splx_dal_del_extgroupscache]    Script Date: 10/24/2010 09:57:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Create date: 11/26/2008
-- Description:	Delete SPLX_EXTERNAL_GROUPS_CACHE
-- =============================================
CREATE PROCEDURE [dbo].[splx_dal_del_extgroupscache]

	@splx_external_groups_cache_id	int
AS
BEGIN

	DELETE
	FROM
		SPLX_EXTERNAL_GROUPS_CACHE
	WHERE
		[SPLX_EXTERNAL_GROUPS_CACHE_ID] = @splx_external_groups_cache_id

END

GO

/****** Object:  StoredProcedure [dbo].[splx_dal_ins_auditlog]    Script Date: 10/24/2010 09:57:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_ins_auditlog]

	@SPLX_AUDIT_LOG_ID	int = 0 OUTPUT,
	@AUDIT_DTTM			datetime,
	@SPLX_USER_ID		uniqueidentifier,
	@USER_NAME			varchar(50) = NULL,
	@COMPUTER_NAME		varchar(50) = NULL,
	@AUDIT_TYPE			varchar(50),
	@AUDIT_SOURCE		varchar(255),
	@AUDIT_CATEGORY		varchar(50),
	@AUDIT_DESC			varchar(2000)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	INSERT INTO SPLX_AUDIT_LOG
		(
			AUDIT_DTTM,
			SPLX_USER_ID,
			USER_NAME,
			COMPUTER_NAME,
			AUDIT_TYPE,
			AUDIT_SOURCE,
			AUDIT_CATEGORY,
			AUDIT_DESC
		)
	VALUES
		(
			@AUDIT_DTTM,
			@SPLX_USER_ID,
			@USER_NAME,
			@COMPUTER_NAME,
			@AUDIT_TYPE,
			@AUDIT_SOURCE,
			@AUDIT_CATEGORY,
			@AUDIT_DESC
		)


	SELECT @SPLX_AUDIT_LOG_ID = SCOPE_IDENTITY()

END

GO

/****** Object:  StoredProcedure [dbo].[splx_dal_sel_ace]    Script Date: 10/24/2010 09:57:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_sel_ace]

	@SPLX_ACE_ID		int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		*,
		CAST( CASE ISNULL(CAST(ACE_ACCESS_TYPE2 as INT), -1)
			WHEN -1 THEN 0
			ELSE 1
		END AS BIT ) IS_AUDIT_ACE
	FROM
		SPLX_ACES
	WHERE
		SPLX_ACE_ID = @SPLX_ACE_ID

END

GO

/****** Object:  StoredProcedure [dbo].[splx_dal_sel_authuser]    Script Date: 10/24/2010 09:57:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[splx_dal_sel_authuser]

	@USER_NAME		varchar(50),
	@USER_PASSWORD	nvarchar(50),
	@USER_LOCAL		bit

AS

DECLARE @userPswd1			varbinary(50)
DECLARE @userPswd2			varbinary(50)
DECLARE @userExists			BIT
DECLARE @splxUserId			uniqueidentifier
DECLARE @maxAttempts		INT
DECLARE @lastAttempt		DATETIME
DECLARE @loginTries			INT
DECLARE @userLocked			BIT
DECLARE @lockoutTime		INT
DECLARE @lockoutDuration	INT
DECLARE @lockoutEnd			DATETIME
DECLARE @allowLoginRetry	BIT
DECLARE @errorMessage		VARCHAR(150)


SET @errorMessage = 'The account does not exist, is disabled, or the password is incorrect.'
SET @maxAttempts = 3
SET @lockoutTime = 0	--in seconds
SET @userExists = 0
SET @allowLoginRetry = 0
SET @lockoutEnd = GETDATE()
SET @userLocked = 0



IF @USER_LOCAL = 0
	BEGIN
		SELECT
			@splxUserId = SPLX_USER_ID,
			@userExists = Count(USER_NAME)
		FROM
			SPLX_USERS
		WHERE
			USER_NAME = @USER_NAME
			AND
			USER_LOCAL = 0
			AND
			USER_ENABLED = 1
		GROUP BY
			SPLX_USER_ID
	END
ELSE
	BEGIN
		SELECT @userPswd1 = CONVERT(varbinary(50), @USER_PASSWORD)

		SELECT
			@splxUserId = SPLX_USER_ID,
			@userExists = Count(USER_NAME),
			@userPswd2 = USER_PASSWORD,
			@lastAttempt = USER_LAST_LOGIN_ATTEMPT,
			@loginTries = USER_LOGIN_TRIES
		FROM
			SPLX_USERS
		WHERE
			USER_NAME = @USER_NAME
			AND
			USER_LOCAL = 1
			AND
			USER_ENABLED = 1
		GROUP BY
			SPLX_USER_ID, USER_PASSWORD, USER_LAST_LOGIN_ATTEMPT, USER_LOGIN_TRIES

		IF @userExists = 1
			BEGIN

				IF @loginTries >= @maxAttempts
					BEGIN
						SELECT @lockoutDuration = DATEDIFF(second, @lastAttempt, GETDATE())

						IF @lockoutDuration > @lockoutTime OR @lockoutTime = 0
							BEGIN
								--SELECT 'Account NOT Locked', @lockoutDuration
								SET @allowLoginRetry = 1
								SET @loginTries = 0
							END
						ELSE
							BEGIN
								--SELECT 'Account Locked', @lockoutDuration
								SET @userLocked = 1
								SET	@lockoutEnd = DATEADD(second, @lockoutTime, @lastAttempt)
								SET @errorMessage = 'The account is locked until ' + CAST( @lockoutEnd AS VARCHAR(25) )
								SET @userExists = 0

								UPDATE	SPLX_USERS
								SET		USER_LOGIN_TRIES = @loginTries+1
								WHERE	SPLX_USER_ID = @splxUserId
							END
					END
				ELSE
					BEGIN
						SET @allowLoginRetry = 1
					END


				IF @allowLoginRetry = 1
					BEGIN
						IF @userPswd1 != @userPswd2
							OR ISNULL(@userPswd1, 0) = 0
							OR ISNULL(@userPswd2, 0) = 0
							BEGIN
								SET @userExists = 0
							
								UPDATE
									SPLX_USERS
								SET
									USER_LAST_LOGIN_ATTEMPT = GETDATE(),
									USER_LOGIN_TRIES = @loginTries+1
								WHERE
									SPLX_USER_ID = @splxUserId
							END
					END
			END
		ELSE
			SET @allowLoginRetry = 1
	END


IF @userExists = 0 OR @userExists IS NULL
	BEGIN
		--RAISERROR (@errorMessage, 16, 1)
		SET @errorMessage = @errorMessage
	END
ELSE
	UPDATE
		SPLX_USERS
	SET
		USER_LAST_LOGIN = GETDATE(),
		USER_LOGIN_TRIES = 0
	WHERE
		SPLX_USER_ID = @splxUserId


SELECT
	@userExists AS USER_EXISTS,
	@lockoutEnd AS LOCKOUT_END_TIME,
	@userLocked AS USER_LOCKED,
	@errorMessage AS ERROR_MESSAGE,
	@allowLoginRetry AS ALLOW_LOGIN_RETRY

GO

/****** Object:  StoredProcedure [dbo].[splx_dal_sel_extgroupscachebyuser]    Script Date: 10/24/2010 09:57:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Create date: 11/26/2008
-- Description:	Select SPLX_EXTERNAL_GROUPS_CACHE by UserID
-- =============================================
CREATE PROCEDURE [dbo].[splx_dal_sel_extgroupscachebyuser]

		@splx_user_id	uniqueidentifier
AS
BEGIN

	SELECT
		[SPLX_EXTERNAL_GROUPS_CACHE_ID]
		,[SPLX_USER_ID]
		,[CACHE_DATE]
		,[EXPIRATION_DATE]
		,[GROUP_LIST]
	FROM
		SPLX_EXTERNAL_GROUPS_CACHE
	WHERE
		[SPLX_USER_ID] = @splx_user_id

END

GO

/****** Object:  StoredProcedure [dbo].[splx_dal_sel_rightrole]    Script Date: 10/24/2010 09:57:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_sel_rightrole]

	@SPLX_RIGHT_ROLE_ID		int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		*
	FROM
		SPLX_RIGHT_ROLES
	WHERE
		SPLX_RIGHT_ROLE_ID = @SPLX_RIGHT_ROLE_ID

END
GO

/****** Object:  StoredProcedure [dbo].[splx_dal_sel_uie]    Script Date: 10/24/2010 09:57:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_sel_uie]

	@SPLX_UI_ELEMENT_ID	uniqueidentifier

AS
BEGIN

	SELECT
		*
	FROM
		SPLX_UI_ELEMENTS
	WHERE
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID

END

GO

/****** Object:  StoredProcedure [dbo].[splx_dal_sel_uieaclinfo]    Script Date: 10/24/2010 09:57:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[splx_dal_sel_uieaclinfo]

	@SPLX_UI_ELEMENT_ID		uniqueidentifier

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		ISNULL(UIE_DACL_INHERIT, 1) AS 'UIE_DACL_INHERIT',
		ISNULL(UIE_SACL_INHERIT, 1) AS 'UIE_SACL_INHERIT',
		UIE_SACL_AUDIT_TYPE_FILTER
	FROM
		SPLX_UI_ELEMENTS
	WHERE
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID

END
GO

/****** Object:  StoredProcedure [dbo].[splx_dal_sel_uielements]    Script Date: 10/24/2010 09:57:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[splx_dal_sel_uielements]

AS

	SELECT
		*
	FROM
		SPLX_UI_ELEMENTS


GO

