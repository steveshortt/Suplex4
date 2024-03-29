USE [Suplex]
GO
/****** Object:  StoredProcedure [splx].[splx_api_future_use_sel_auditlogbycat]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 09/08/2007
-- Description:	Select Audit Log entries by category
-- Modified:	11/14/2007 to handle addition of
--				USER_NAME column to SPLX_AUDIT_LOG table
-- =============================================
CREATE PROCEDURE [splx].[splx_api_future_use_sel_auditlogbycat]

	@CATEGORY varchar(50)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		l.SPLX_AUDIT_LOG_ID,
		l.AUDIT_DTTM,
		l.SPLX_USER_ID,
		COALESCE(l.USER_NAME,u.USER_NAME) 'USER_NAME',
		l.COMPUTER_NAME,
		l.AUDIT_TYPE,
		l.AUDIT_SOURCE,
		l.AUDIT_CATEGORY,
		l.AUDIT_DESC,
		u.USER_DESC
	FROM
		splx.SPLX_AUDIT_LOG l
			LEFT OUTER JOIN
		splx.SPLX_USERS u ON l.SPLX_USER_ID = u.SPLX_USER_ID
	WHERE
		AUDIT_CATEGORY = @CATEGORY
	ORDER BY
		AUDIT_DTTM DESC

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_future_use_del_auditlogbycat]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 09/08/2007
-- Description:	Delete Audit Log entries by category
-- =============================================
CREATE PROCEDURE [splx].[splx_api_future_use_del_auditlogbycat]

	@CATEGORY varchar(50)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DELETE
	FROM
		splx.SPLX_AUDIT_LOG
	WHERE
		AUDIT_CATEGORY = @CATEGORY

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_groupbyname_valid]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 10/24/2012
-- Description:	Select Group by Name, for client validation
-- =============================================
CREATE PROCEDURE [splx].[splx_api_sel_groupbyname_valid]

	@SPLX_GROUP_ID			uniqueidentifier
	,@GROUP_NAME			varchar(50)
	,@success				bit = 0 OUTPUT
	,@UNIQUE_GROUP_NAME		varchar(50) = null OUTPUT

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @FOUND_SPLX_GROUP_ID			uniqueidentifier

	if @SPLX_GROUP_ID IS NULL
	BEGIN
	
		SELECT
			@success = COUNT(GROUP_NAME)
		FROM
			splx.SPLX_GROUPS
		WHERE
			GROUP_NAME = @GROUP_NAME

		set @success = ~@success

	END
	else
	BEGIN
	
		SELECT
			@FOUND_SPLX_GROUP_ID = SPLX_GROUP_ID
		FROM
			splx.SPLX_GROUPS
		WHERE
			GROUP_NAME = @GROUP_NAME

		set @success = 0
		if @FOUND_SPLX_GROUP_ID IS NULL
			set @success = 1
		if @FOUND_SPLX_GROUP_ID = @SPLX_GROUP_ID
			set @success = 1

	END


	set @UNIQUE_GROUP_NAME = @GROUP_NAME	
	if @success = 0
		set @UNIQUE_GROUP_NAME = @GROUP_NAME + '_' + CAST(newid() AS varchar(50))
		
END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_groupbyname_exists]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 02/13/2010
-- Description:	Select Group by Name
-- =============================================
CREATE PROCEDURE [splx].[splx_api_sel_groupbyname_exists]

	@GROUP_NAME		varchar(50)
	,@success		bit = 0 OUTPUT

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		@success = COUNT(GROUP_NAME)
	FROM
		splx.SPLX_GROUPS
	WHERE
		GROUP_NAME = @GROUP_NAME

	select ~@success as [ok]

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_groups]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [splx].[splx_api_sel_groups]

AS

	SELECT
		*
	FROM
		splx.SPLX_GROUPS
	ORDER BY
		GROUP_NAME
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_users]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 09/16/2007
-- Description:	Select User by ID
-- =============================================
CREATE PROCEDURE [splx].[splx_api_sel_users]

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		SPLX_USER_ID,
		USER_NAME,
		USER_DESC,
		USER_LOCAL,
		USER_BUILT_IN,
		USER_ENABLED,
		USER_LAST_LOGIN,
		USER_LAST_LOGIN_ATTEMPT,
		USER_LOGIN_TRIES
	FROM
		splx.SPLX_USERS
	ORDER BY
		USER_NAME, USER_DESC

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_userbyname_valid]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 10/24/2012
-- Description:	Select User by Name, for client validation
-- =============================================
CREATE PROCEDURE [splx].[splx_api_sel_userbyname_valid]

	@SPLX_USER_ID			uniqueidentifier
	,@USER_NAME				varchar(50)
	,@success				bit = 0 OUTPUT
	,@UNIQUE_USER_NAME		varchar(50) = null OUTPUT

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @FOUND_SPLX_USER_ID			uniqueidentifier

	if @SPLX_USER_ID IS NULL
	BEGIN
	
		SELECT
			@success = COUNT(USER_NAME)
		FROM
			splx.SPLX_USERS
		WHERE
			USER_NAME = @USER_NAME

		set @success = ~@success

	END
	else
	BEGIN
	
		SELECT
			@FOUND_SPLX_USER_ID = SPLX_USER_ID
		FROM
			splx.SPLX_USERS
		WHERE
			USER_NAME = @USER_NAME

		set @success = 0
		if @FOUND_SPLX_USER_ID IS NULL
			set @success = 1
		if @FOUND_SPLX_USER_ID = @SPLX_USER_ID
			set @success = 1

	END


	set @UNIQUE_USER_NAME = @USER_NAME	
	if @success = 0
		set @UNIQUE_USER_NAME = @USER_NAME + '_' + CAST(newid() AS varchar(50))
		
END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_userbyname_exists]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 02/13/2010
-- Description:	Select User by Name, for client validation
-- =============================================
CREATE PROCEDURE [splx].[splx_api_sel_userbyname_exists]

	@USER_NAME		varchar(50)
	,@success		bit = 0 OUTPUT

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		@success = COUNT(USER_NAME)
	FROM
		splx.SPLX_USERS
	WHERE
		USER_NAME = @USER_NAME

	select ~@success as [ok]

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_uielementbyparent]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_sel_uielementbyparent]

	@UIE_PARENT_ID uniqueidentifier

AS

	IF @UIE_PARENT_ID IS NULL
		SELECT
			*
		FROM
			splx.SPLX_UI_ELEMENTS
		WHERE
			UIE_PARENT_ID IS NULL
	ELSE
		SELECT
			*
		FROM
			splx.SPLX_UI_ELEMENTS
		WHERE
			UIE_PARENT_ID = @UIE_PARENT_ID
GO
/****** Object:  StoredProcedure [splx].[splx_api_upsert_uie]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [splx].[splx_api_upsert_uie]

	@SPLX_UI_ELEMENT_ID				uniqueidentifier = null output
	,@UIE_NAME						varchar(50)
	,@UIE_CONTROL_TYPE				varchar(50)
	,@UIE_DESC						varchar(255) = null
	,@UIE_DESC_TOOLTIP				bit = 0
	,@UIE_UNIQUE_NAME				varchar(50)
	,@UIE_DATA_TYPE					varchar(50) = null
	,@UIE_DATA_TYPE_ERR_MSG			varchar(255) = null
	,@UIE_FORMAT_STRING				varchar(50) = null
	,@UIE_ALLOW_UNDECLARED			bit = 0
	,@UIE_PARENT_ID					uniqueidentifier = null
	,@UIE_DACL_INHERIT				bit = null
	,@UIE_SACL_INHERIT				bit = null
	,@UIE_SACL_AUDIT_TYPE_FILTER	int = null

AS
BEGIN

	IF NOT EXISTS( SELECT SPLX_UI_ELEMENT_ID FROM splx.SPLX_UI_ELEMENTS WHERE SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID )
	BEGIN
	
		IF @SPLX_UI_ELEMENT_ID IS NULL
		BEGIN
			SET @SPLX_UI_ELEMENT_ID = newid()
		END

		INSERT INTO splx.SPLX_UI_ELEMENTS
		(
			SPLX_UI_ELEMENT_ID
			,UIE_NAME
			,UIE_CONTROL_TYPE
			,UIE_DESC
			,UIE_DESC_TOOLTIP
			,UIE_UNIQUE_NAME
			,UIE_DATA_TYPE
			,UIE_DATA_TYPE_ERR_MSG
			,UIE_FORMAT_STRING
			,UIE_ALLOW_UNDECLARED
			,UIE_PARENT_ID
			,UIE_DACL_INHERIT
			,UIE_SACL_INHERIT
			,UIE_SACL_AUDIT_TYPE_FILTER
		)
		VALUES
		(
			@SPLX_UI_ELEMENT_ID
			,@UIE_NAME
			,@UIE_CONTROL_TYPE
			,@UIE_DESC
			,@UIE_DESC_TOOLTIP
			,@UIE_UNIQUE_NAME
			,@UIE_DATA_TYPE
			,@UIE_DATA_TYPE_ERR_MSG
			,@UIE_FORMAT_STRING
			,@UIE_ALLOW_UNDECLARED
			,@UIE_PARENT_ID
			,@UIE_DACL_INHERIT
			,@UIE_SACL_INHERIT
			,@UIE_SACL_AUDIT_TYPE_FILTER
		)

	END

	ELSE
	BEGIN

		UPDATE
			splx.SPLX_UI_ELEMENTS
		SET
			UIE_NAME = @UIE_NAME
			,UIE_CONTROL_TYPE = @UIE_CONTROL_TYPE
			,UIE_DESC = @UIE_DESC
			,UIE_DESC_TOOLTIP = @UIE_DESC_TOOLTIP
			,UIE_UNIQUE_NAME = @UIE_UNIQUE_NAME
			,UIE_DATA_TYPE = @UIE_DATA_TYPE
			,UIE_DATA_TYPE_ERR_MSG = @UIE_DATA_TYPE_ERR_MSG
			,UIE_FORMAT_STRING = @UIE_FORMAT_STRING
			,UIE_ALLOW_UNDECLARED = @UIE_ALLOW_UNDECLARED
			,UIE_PARENT_ID = @UIE_PARENT_ID
			,UIE_DACL_INHERIT = @UIE_DACL_INHERIT
			,UIE_SACL_INHERIT = @UIE_SACL_INHERIT
			,UIE_SACL_AUDIT_TYPE_FILTER = @UIE_SACL_AUDIT_TYPE_FILTER
		WHERE
			SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID

	END

END
GO
/****** Object:  StoredProcedure [splx].[splx_dal_sel_groupbyname]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 09/07/2007
-- Description:	Select Group by Name
-- =============================================
CREATE PROCEDURE [splx].[splx_dal_sel_groupbyname]

	@GROUP_NAME		varchar(50)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		*
	FROM
		splx.SPLX_GROUPS
	WHERE
		GROUP_NAME = @GROUP_NAME

END
GO
/****** Object:  StoredProcedure [splx].[splx_dal_sel_groupbyid]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 09/07/2007
-- Description:	Select Group by ID
-- =============================================
CREATE PROCEDURE [splx].[splx_dal_sel_groupbyid]

	@SPLX_GROUP_ID		uniqueidentifier

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		*
	FROM
		splx.SPLX_GROUPS
	WHERE
		SPLX_GROUP_ID = @SPLX_GROUP_ID

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_upsert_vr]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_upsert_vr]

	@SPLX_VALIDATION_RULE_ID	uniqueidentifier = NULL output
	,@VR_NAME					varchar(50) = NULL
	,@VR_EVENT_BINDING			varchar(50) = NULL
	,@VR_COMPARE_VALUE1			varchar(255) = NULL
	,@VR_EXPRESSION_TYPE1		varchar(50) = NULL
	,@VR_VALUE_TYPE1			varchar(50) = NULL
	,@VR_COMPARE_VALUE2			varchar(255) = NULL
	,@VR_EXPRESSION_TYPE2		varchar(50) = NULL
	,@VR_VALUE_TYPE2			varchar(50) = NULL
	,@VR_COMPARE_DATA_TYPE		varchar(50) = NULL
	,@VR_OPERATOR				varchar(50) = NULL
	,@VR_ERROR_MESSAGE			varchar(255) = NULL
	,@VR_ERROR_UIE_UNIQUE_NAME	varchar(50) = NULL
	,@VR_FAIL_PARENT			bit = 0
	,@VR_RULE_TYPE				varchar(50)
	,@VR_PARENT_ID				uniqueidentifier = NULL
	,@SPLX_UI_ELEMENT_ID		uniqueidentifier
	,@VR_SORT_ORDER				int = 0

AS
BEGIN

	IF NOT EXISTS ( SELECT SPLX_VALIDATION_RULE_ID FROM splx.SPLX_VALIDATION_RULES WHERE SPLX_VALIDATION_RULE_ID = @SPLX_VALIDATION_RULE_ID)
	BEGIN

		IF @SPLX_VALIDATION_RULE_ID IS NULL
		BEGIN
			SET @SPLX_VALIDATION_RULE_ID = newid()
		END

		INSERT INTO splx.SPLX_VALIDATION_RULES
		(
			SPLX_VALIDATION_RULE_ID
			,VR_NAME
			,VR_EVENT_BINDING
			,VR_COMPARE_VALUE1
			,VR_EXPRESSION_TYPE1
			,VR_VALUE_TYPE1
			,VR_COMPARE_VALUE2
			,VR_EXPRESSION_TYPE2
			,VR_VALUE_TYPE2
			,VR_COMPARE_DATA_TYPE
			,VR_OPERATOR
			,VR_ERROR_MESSAGE
			,VR_ERROR_UIE_UNIQUE_NAME
			,VR_FAIL_PARENT
			,VR_RULE_TYPE
			,VR_PARENT_ID
			,SPLX_UI_ELEMENT_ID
			,VR_SORT_ORDER
		)
		VALUES
		(
			@SPLX_VALIDATION_RULE_ID
			,@VR_NAME
			,@VR_EVENT_BINDING
			,@VR_COMPARE_VALUE1
			,@VR_EXPRESSION_TYPE1
			,@VR_VALUE_TYPE1
			,@VR_COMPARE_VALUE2
			,@VR_EXPRESSION_TYPE2
			,@VR_VALUE_TYPE2
			,@VR_COMPARE_DATA_TYPE
			,@VR_OPERATOR
			,@VR_ERROR_MESSAGE
			,@VR_ERROR_UIE_UNIQUE_NAME
			,@VR_FAIL_PARENT
			,@VR_RULE_TYPE
			,@VR_PARENT_ID
			,@SPLX_UI_ELEMENT_ID
			,@VR_SORT_ORDER
		)

	END

	ELSE
	BEGIN

		UPDATE
			splx.SPLX_VALIDATION_RULES
		SET
			VR_NAME = @VR_NAME
			,VR_EVENT_BINDING = @VR_EVENT_BINDING
			,VR_SORT_ORDER = @VR_SORT_ORDER
			,VR_COMPARE_VALUE1 = @VR_COMPARE_VALUE1
			,VR_EXPRESSION_TYPE1 = @VR_EXPRESSION_TYPE1
			,VR_VALUE_TYPE1 = @VR_VALUE_TYPE1
			,VR_COMPARE_VALUE2 = @VR_COMPARE_VALUE2
			,VR_EXPRESSION_TYPE2 = @VR_EXPRESSION_TYPE2
			,VR_VALUE_TYPE2 = @VR_VALUE_TYPE2
			,VR_COMPARE_DATA_TYPE = @VR_COMPARE_DATA_TYPE
			,VR_OPERATOR = @VR_OPERATOR
			,VR_ERROR_MESSAGE = @VR_ERROR_MESSAGE
			,VR_ERROR_UIE_UNIQUE_NAME = @VR_ERROR_UIE_UNIQUE_NAME
			,VR_FAIL_PARENT = @VR_FAIL_PARENT
			,VR_RULE_TYPE = @VR_RULE_TYPE
			,VR_PARENT_ID = @VR_PARENT_ID
			,SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID
		WHERE
			SPLX_VALIDATION_RULE_ID = @SPLX_VALIDATION_RULE_ID

	END

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_upsert_user]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_upsert_user]

	@SPLX_USER_ID				uniqueidentifier = NULL OUTPUT
	,@USER_NAME					varchar(50) OUTPUT
	,@USER_DESC					varchar(255)
	,@USER_LOCAL				bit
	,@USER_PASSWORD				nvarchar(50) = NULL
	,@USER_ENABLED				bit
	,@USER_BUILT_IN				bit = 0

AS
BEGIN

	DECLARE @pswd varbinary(50)

	DECLARE	@UNIQUE_USER_NAME varchar(50)

	EXEC	[splx].[splx_api_sel_userbyname_valid]
			@SPLX_USER_ID = @SPLX_USER_ID,
			@USER_NAME = @USER_NAME,
			@UNIQUE_USER_NAME = @UNIQUE_USER_NAME OUTPUT
	
	SET @USER_NAME = @UNIQUE_USER_NAME


	IF NOT EXISTS ( SELECT SPLX_USER_ID FROM splx.SPLX_USERS WHERE SPLX_USER_ID = @SPLX_USER_ID )
	BEGIN

		IF @USER_LOCAL = 1
			SELECT @pswd = CONVERT(varbinary(50), @USER_PASSWORD)

		IF @SPLX_USER_ID IS NULL
		BEGIN
			SET @SPLX_USER_ID = newid()
		END

		INSERT INTO splx.SPLX_USERS
		(
			SPLX_USER_ID
			,USER_NAME
			,USER_DESC
			,USER_LOCAL
			,USER_PASSWORD
			,USER_ENABLED
			,USER_BUILT_IN
			,USER_LOGIN_TRIES
		)
		VALUES
		(
			@SPLX_USER_ID
			,@USER_NAME
			,@USER_DESC
			,@USER_LOCAL
			,@pswd
			,@USER_ENABLED
			,@USER_BUILT_IN
			,0
		)

	END

	ELSE
	BEGIN

		SELECT @pswd = CONVERT(varbinary(50), @USER_PASSWORD)

		DECLARE @local bit
		SELECT @local = USER_LOCAL
			FROM splx.SPLX_USERS
			WHERE SPLX_USER_ID = @SPLX_USER_ID

		IF @local = 1
			BEGIN
				IF @pswd IS NULL
					SELECT
						@pswd = USER_PASSWORD
					FROM
						splx.SPLX_USERS
					WHERE
						SPLX_USER_ID = @SPLX_USER_ID
			END
		ELSE
			SET @pswd = NULL

		UPDATE
			splx.SPLX_USERS
		SET
			USER_NAME = @USER_NAME
			,USER_DESC = @USER_DESC
			,USER_PASSWORD = @pswd
			,USER_ENABLED = @USER_ENABLED
		WHERE
			SPLX_USER_ID = @SPLX_USER_ID

	END

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_upsert_ace]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_upsert_ace]

	@SPLX_ACE_ID				int = 0 output
	,@ACE_TRUSTEE_USER_GROUP_ID	uniqueidentifier
	,@SPLX_UI_ELEMENT_ID		uniqueidentifier
	,@ACE_ACCESS_MASK			int
	,@ACE_ACCESS_TYPE1			bit
	,@ACE_ACCESS_TYPE2			bit = NULL
	,@ACE_INHERIT				bit
	,@ACE_TYPE					varchar(50)

AS
BEGIN

	IF NOT EXISTS ( SELECT SPLX_ACE_ID FROM splx.SPLX_ACES WHERE SPLX_ACE_ID = @SPLX_ACE_ID )
	BEGIN

		INSERT INTO splx.SPLX_ACES
		(
			ACE_TRUSTEE_USER_GROUP_ID
			,SPLX_UI_ELEMENT_ID
			,ACE_ACCESS_MASK
			,ACE_ACCESS_TYPE1
			,ACE_ACCESS_TYPE2
			,ACE_INHERIT
			,ACE_TYPE
		)
		VALUES
		(
			@ACE_TRUSTEE_USER_GROUP_ID
			,@SPLX_UI_ELEMENT_ID
			,@ACE_ACCESS_MASK
			,@ACE_ACCESS_TYPE1
			,@ACE_ACCESS_TYPE2
			,@ACE_INHERIT
			,@ACE_TYPE
		)

		SELECT @SPLX_ACE_ID = SCOPE_IDENTITY()

	END

	ELSE
	BEGIN

		UPDATE
			splx.SPLX_ACES
		SET
			ACE_TRUSTEE_USER_GROUP_ID = @ACE_TRUSTEE_USER_GROUP_ID
			,SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID
			,ACE_ACCESS_MASK = @ACE_ACCESS_MASK
			,ACE_ACCESS_TYPE1 = @ACE_ACCESS_TYPE1
			,ACE_ACCESS_TYPE2 = @ACE_ACCESS_TYPE2
			,ACE_INHERIT = @ACE_INHERIT
			,ACE_TYPE = @ACE_TYPE
		WHERE
			SPLX_ACE_ID = @SPLX_ACE_ID

	END

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_upsert_group]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_upsert_group]

	@SPLX_GROUP_ID			uniqueidentifier = NULL OUTPUT
	,@GROUP_NAME			varchar(300) OUTPUT
	,@GROUP_DESC			varchar(255)
	,@GROUP_LOCAL			bit
	,@GROUP_ENABLED			bit
	,@GROUP_BUILT_IN		bit = 0
	,@GROUP_MASK			binary(16)

AS
BEGIN

	DECLARE	@UNIQUE_GROUP_NAME varchar(50)

	EXEC	[splx].[splx_api_sel_groupbyname_valid]
			@SPLX_GROUP_ID = @SPLX_GROUP_ID,
			@GROUP_NAME = @GROUP_NAME,
			@UNIQUE_GROUP_NAME = @UNIQUE_GROUP_NAME OUTPUT

	SET @GROUP_NAME = @UNIQUE_GROUP_NAME


	IF NOT EXISTS ( SELECT SPLX_GROUP_ID FROM splx.SPLX_GROUPS WHERE SPLX_GROUP_ID = @SPLX_GROUP_ID )
	BEGIN

		IF @SPLX_GROUP_ID IS NULL
		BEGIN
			SET @SPLX_GROUP_ID = newid()
		END

		INSERT INTO splx.SPLX_GROUPS
		(
			SPLX_GROUP_ID
			,GROUP_NAME
			,GROUP_DESC
			,GROUP_LOCAL
			,GROUP_ENABLED
			,GROUP_BUILT_IN
			,GROUP_MASK
		)
		VALUES
		(
			@SPLX_GROUP_ID
			,@GROUP_NAME
			,@GROUP_DESC
			,@GROUP_LOCAL
			,@GROUP_ENABLED
			,@GROUP_BUILT_IN
			,@GROUP_MASK
		)

	END

	ELSE
	BEGIN

		UPDATE
			splx.SPLX_GROUPS
		SET
			GROUP_NAME = @GROUP_NAME
			,GROUP_DESC = @GROUP_DESC
			,GROUP_LOCAL = @GROUP_LOCAL
			,GROUP_ENABLED = @GROUP_ENABLED
			,GROUP_MASK = @GROUP_MASK
		WHERE
			SPLX_GROUP_ID = @SPLX_GROUP_ID

	END

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_vr]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_sel_vr]

	@SPLX_VALIDATION_RULE_ID	uniqueidentifier

AS
BEGIN

	SELECT
		*
	FROM
		splx.SPLX_VALIDATION_RULES
	WHERE
		SPLX_VALIDATION_RULE_ID = @SPLX_VALIDATION_RULE_ID

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_del_ace]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_del_ace]

	@SPLX_ACE_ID	int

AS

	DELETE
	FROM
		splx.SPLX_ACES
	WHERE
		SPLX_ACE_ID = @SPLX_ACE_ID
GO
/****** Object:  StoredProcedure [splx].[splx_dal_sel_userbyname]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 08/24/2007
-- Update:		07/20/2013, added Rls resolution
-- Description:	Select User by Name
-- =============================================
CREATE PROCEDURE [splx].[splx_dal_sel_userbyname]

	@USER_NAME				varchar(50)
	,@RESOLVE_RLS			bit = 0
	,@EXTERNAL_GROUP_LIST	varchar(max) = null

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	UPDATE
		splx.SPLX_USERS
	SET
		USER_LAST_LOGIN = GETDATE()
	WHERE
		USER_NAME = @USER_NAME

	IF @RESOLVE_RLS = 0
	BEGIN
		SELECT
			*
			,NULL AS 'GROUP_MEMBERSHIP_MASK'
		FROM
			splx.SPLX_USERS
		WHERE
			USER_NAME = @USER_NAME
	END
	
	ELSE
	BEGIN

		DECLARE @SPLX_USER_ID uniqueidentifier
		SELECT
			@SPLX_USER_ID = SPLX_USER_ID
		FROM
			splx.SPLX_USERS
		WHERE
			USER_NAME = @USER_NAME

		--- group membership --------------------------------------------
		CREATE TABLE #gm (
			[SPLX_GROUP_ID] [uniqueidentifier] NOT NULL
			,[GROUP_MASK] [binary](16) NOT NULL
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
			,[GROUP_MASK] [binary](16) NOT NULL
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
			[GROUP_MASK] [binary](16) NOT NULL
		);
		INSERT INTO #trustees_mask
				SELECT GROUP_MASK FROM #gm
					UNION
				SELECT GROUP_MASK FROM #nestedgm
		--- total trustees --------------------------------------------



		SELECT
			*
			,splx.Splx_GetTableOr( '#trustees_mask', 'GROUP_MASK', 128 ) AS 'GROUP_MEMBERSHIP_MASK'
		FROM
			splx.SPLX_USERS
		WHERE
			USER_NAME = @USER_NAME

	END

END
GO
/****** Object:  StoredProcedure [splx].[splx_dal_sel_userbyid]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 08/24/2007
-- Update:		07/20/2013, added Rls resolution
-- Description:	Select User by Id
-- =============================================
CREATE PROCEDURE [splx].[splx_dal_sel_userbyid]

	@SPLX_USER_ID		uniqueidentifier
	,@RESOLVE_RLS			bit = 0
	,@EXTERNAL_GROUP_LIST	varchar(max) = null

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE
		splx.SPLX_USERS
	SET
		USER_LAST_LOGIN = GETDATE()
	WHERE
		SPLX_USER_ID = @SPLX_USER_ID

	IF @RESOLVE_RLS = 0
	BEGIN
		SELECT
			*
			,NULL AS 'GROUP_MEMBERSHIP_MASK'
		FROM
			splx.SPLX_USERS
		WHERE
			SPLX_USER_ID = @SPLX_USER_ID
	END
	
	ELSE
	BEGIN

		--- group membership --------------------------------------------
		CREATE TABLE #gm (
			[SPLX_GROUP_ID] [uniqueidentifier] NOT NULL
			,[GROUP_MASK] [binary](16) NOT NULL
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
			,[GROUP_MASK] [binary](16) NOT NULL
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
			[GROUP_MASK] [binary](16) NOT NULL
		);
		INSERT INTO #trustees_mask
				SELECT GROUP_MASK FROM #gm
					UNION
				SELECT GROUP_MASK FROM #nestedgm
		--- total trustees --------------------------------------------



		SELECT
			*
			,splx.Splx_GetTableOr( '#trustees_mask', 'GROUP_MASK', 128 ) AS 'GROUP_MEMBERSHIP_MASK'
		FROM
			splx.SPLX_USERS
		WHERE
			SPLX_USER_ID = @SPLX_USER_ID

	END

END
GO
/****** Object:  StoredProcedure [splx].[splx_dal_sel_taskaccess]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_dal_sel_taskaccess]

	@TASK_NAME				VARCHAR(50),
	@SPLX_USER_ID			uniqueidentifier,
	@ALLOWED				BIT = 0 OUTPUT,
	@EXTERNAL_GROUP_LIST	varchar(max)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	--NOTE:this doesn't currently address NT Groups

	DECLARE @SPLX_TASK_ID INT
	SELECT
		@SPLX_TASK_ID = SPLX_TASK_ID
	FROM
		splx.SPLX_TASKS
	WHERE
		TASK_Enabled = 1
		AND
		TASK_NAME = @TASK_NAME

	SELECT
		@ALLOWED = Count(TRUSTEE_USER_GROUP_ID)
	FROM
		SPLX_TASK_TRUSTEE tt
			INNER JOIN
		(
			SELECT
				gm.SPLX_USER_ID, gm.SPLX_GROUP_ID
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
				@SPLX_USER_ID, SPLX_GROUP_ID
			FROM
				splx.SPLX_GROUPS g
					INNER JOIN
				(SELECT DISTINCT value AS GROUP_NAME FROM ap_Split( @EXTERNAL_GROUP_LIST, ',' )) eg
					ON g.GROUP_NAME = eg.GROUP_NAME
			WHERE
				g.GROUP_ENABLED = 1
		) TRUSTEES ON
			tt.TRUSTEE_USER_GROUP_ID = TRUSTEES.SPLX_GROUP_ID
				OR
			tt.TRUSTEE_USER_GROUP_ID = TRUSTEES.SPLX_USER_ID
	WHERE
		tt.SPLX_TASK_ID = @SPLX_TASK_ID

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_groupnestmembbygroup]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 10/16/2007
-- Modified:	10/07/2012, switched @includeDisabled default from 0 to 1
-- Description:	Select Group Membership by Group_ID
-- =============================================
CREATE PROCEDURE [splx].[splx_api_sel_groupnestmembbygroup]

	@SPLX_GROUP_ID		uniqueidentifier,
	@includeDisabled	bit = 1,
	@includeNonMemb		bit = 1

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @tables VARCHAR(50)
	SET @tables = 'GroupMembership'

	CREATE TABLE #GM (
		[SPLX_GROUP_ID] [uniqueidentifier] NOT NULL
		,[GROUP_NAME] [varchar](300) NOT NULL
		,[GROUP_DESC] [varchar](255) NULL
		,[GROUP_LOCAL] [bit] NOT NULL
		,[GROUP_BUILTIN] [bit] NOT NULL
		,[GROUP_ENABLED] [bit] NOT NULL
		,[GROUP_MASK] [binary](16) NOT NULL
	);
	INSERT INTO #GM
		SELECT
			g.*
		FROM
			splx.SPLX_NESTED_GROUPS ng
				INNER JOIN
			splx.SPLX_GROUPS g ON ng.CHILD_GROUP_ID = g.SPLX_GROUP_ID
		WHERE
			PARENT_GROUP_ID = @SPLX_GROUP_ID
		ORDER BY
			g.GROUP_NAME


	IF @includeDisabled IS NULL OR @includeDisabled = 0
	BEGIN
		--select membership
		SELECT
			#GM.*
		FROM
			#GM
		WHERE
			GROUP_ENABLED = 1

		IF @includeNonMemb = 1
		BEGIN
			--select non-membership
			SELECT
				g.*
			FROM
				splx.SPLX_GROUPS g
					LEFT OUTER JOIN
				#GM ON g.SPLX_GROUP_ID = #GM.SPLX_GROUP_ID
			WHERE
				#GM.SPLX_GROUP_ID IS NULL
				AND
				g.GROUP_ENABLED = 1
				AND
				g.SPLX_GROUP_ID != @SPLX_GROUP_ID
			ORDER BY
				g.GROUP_NAME

			SET @tables = 'GroupMembership,GroupNonMembership'
		END
	END

	ELSE
	BEGIN
		--select membership
		SELECT
			#GM.*
		FROM
			#GM

		IF @includeNonMemb = 1
		BEGIN
			--select non-membership
			SELECT
				g.*
			FROM
				splx.SPLX_GROUPS g
					LEFT OUTER JOIN
				#GM ON g.SPLX_GROUP_ID = #GM.SPLX_GROUP_ID
			WHERE
				#GM.SPLX_GROUP_ID IS NULL
				AND
				g.SPLX_GROUP_ID != @SPLX_GROUP_ID
			ORDER BY
				g.GROUP_NAME

			SET @tables = 'GroupMembership,GroupNonMembership'
		END
	END

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#GM',N'U') IS NOT NULL
		DROP TABLE #GM


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		@tables AS Tables

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_groupmembbyuser]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 09/15/2007
-- Modified:	10/07/2012, switched @includeDisabled default from 0 to 1
-- Description:	Select Group Membership by User_ID
-- =============================================
CREATE PROCEDURE [splx].[splx_api_sel_groupmembbyuser]

	@SPLX_USER_ID		uniqueidentifier,
	@includeDisabled	bit = 1,
	@includeNonMemb		bit = 1

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @tables VARCHAR(50)
	SET @tables = 'GroupMembership'

	CREATE TABLE #GM (
		[SPLX_GROUP_ID] [uniqueidentifier] NOT NULL
		,[GROUP_NAME] [varchar](300) NOT NULL
		,[GROUP_DESC] [varchar](255) NULL
		,[GROUP_LOCAL] [bit] NOT NULL
		,[GROUP_BUILTIN] [bit] NOT NULL
		,[GROUP_ENABLED] [bit] NOT NULL
		,[GROUP_MASK] [binary](16) NOT NULL
	);
	INSERT INTO #GM
		SELECT
			g.*
		FROM
			splx.SPLX_GROUP_MEMBERSHIP gm
				INNER JOIN
			splx.SPLX_GROUPS g ON gm.SPLX_GROUP_ID = g.SPLX_GROUP_ID
		WHERE
			gm.SPLX_USER_ID = @SPLX_USER_ID
			AND
			g.GROUP_LOCAL = 1
		ORDER BY
			g.GROUP_NAME


	IF @includeDisabled IS NULL OR @includeDisabled = 0
	BEGIN
		--select membership
		SELECT
			#GM.*
		FROM
			#GM
		WHERE
			GROUP_ENABLED = 1

		IF @includeNonMemb = 1
		BEGIN
			--select non-membership
			SELECT
				g.*
			FROM
				splx.SPLX_GROUPS g
					LEFT OUTER JOIN
				#GM ON g.SPLX_GROUP_ID = #GM.SPLX_GROUP_ID
			WHERE
				#GM.SPLX_GROUP_ID IS NULL
				AND
				g.GROUP_LOCAL = 1
				AND
				g.GROUP_ENABLED = 1
			ORDER BY
				g.GROUP_NAME

			SET @tables = 'GroupMembership,GroupNonMembership'
		END
	END

	ELSE
	BEGIN
		--select membership
		SELECT
			#GM.*
		FROM
			#GM

		IF @includeNonMemb = 1
		BEGIN
			--select non-membership
			SELECT
				g.*
			FROM
				splx.SPLX_GROUPS g
					LEFT OUTER JOIN
				#GM ON g.SPLX_GROUP_ID = #GM.SPLX_GROUP_ID
			WHERE
				#GM.SPLX_GROUP_ID IS NULL
				AND
				g.GROUP_LOCAL = 1
			ORDER BY
				g.GROUP_NAME

			SET @tables = 'GroupMembership,GroupNonMembership'
		END
	END

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#GM',N'U') IS NOT NULL
		DROP TABLE #GM


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		@tables AS Tables

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_groupmembbygroup]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 09/15/2007
-- Modified:	10/07/2012, switched @includeDisabled default from 0 to 1
-- Description:	Select Group Membership by Group_ID
-- =============================================
CREATE PROCEDURE [splx].[splx_api_sel_groupmembbygroup]

	@SPLX_GROUP_ID		uniqueidentifier,
	@includeDisabled	bit = 1,
	@includeNonMemb		bit = 1

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @tables VARCHAR(50)
	SET @tables = 'GroupMembership'

	CREATE TABLE #UM (
		[SPLX_USER_ID] [uniqueidentifier] NOT NULL,
		[USER_NAME] [varchar](50) NOT NULL,
		[USER_DESC] [varchar](255) NULL,
		[USER_LOCAL] [bit] NOT NULL,
		[USER_BUILT_IN] [bit] NOT NULL,
		[USER_ENABLED] [bit] NOT NULL,
	);
	INSERT INTO #UM
		SELECT
			u.SPLX_USER_ID,
			u.USER_NAME,
			u.USER_DESC,
			u.USER_LOCAL,
			u.USER_BUILT_IN,
			u.USER_ENABLED
		FROM
			splx.SPLX_GROUP_MEMBERSHIP gm
				INNER JOIN
			splx.SPLX_USERS u ON gm.SPLX_USER_ID = u.SPLX_USER_ID
		WHERE
			SPLX_GROUP_ID = @SPLX_GROUP_ID
		ORDER BY
			u.USER_NAME


	IF @includeDisabled IS NULL OR @includeDisabled = 0
	BEGIN
		--select membership
		SELECT
			#UM.*
		FROM
			#UM
		WHERE
			USER_ENABLED = 1

		IF @includeNonMemb = 1
		BEGIN
			--select non-membership
			SELECT
				u.SPLX_USER_ID,
				u.USER_NAME,
				u.USER_DESC,
				u.USER_LOCAL,
				u.USER_BUILT_IN,
				u.USER_ENABLED
			FROM
				splx.SPLX_USERS u
					LEFT OUTER JOIN
				#UM ON u.SPLX_USER_ID = #UM.SPLX_USER_ID
			WHERE
				#UM.SPLX_USER_ID IS NULL
				AND
				u.USER_ENABLED = 1
			ORDER BY
				u.USER_NAME

			SET @tables = 'GroupMembership,GroupNonMembership'
		END
	END

	ELSE
	BEGIN
		--select membership
		SELECT
			#UM.*
		FROM
			#UM

		IF @includeNonMemb = 1
		BEGIN
			--select non-membership
			SELECT
				u.SPLX_USER_ID,
				u.USER_NAME,
				u.USER_DESC,
				u.USER_LOCAL,
				u.USER_BUILT_IN,
				u.USER_ENABLED
			FROM
				splx.SPLX_USERS u
					LEFT OUTER JOIN
				#UM ON u.SPLX_USER_ID = #UM.SPLX_USER_ID
			WHERE
				#UM.SPLX_USER_ID IS NULL
			ORDER BY
				u.USER_NAME

			SET @tables = 'GroupMembership,GroupNonMembership'
		END
	END

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#UM',N'U') IS NOT NULL
		DROP TABLE #UM


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		@tables AS Tables

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_groupmemb_nested_composite]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_sel_groupmemb_nested_composite]
AS
	SELECT * FROM splx.SPLX_GROUP_MEMBERSHIP
	SELECT * FROM splx.SPLX_NESTED_GROUPS
	
	
	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		'GroupMembership,NestedGroups' AS Tables
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_grouphiermembbygroup]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 10/6/2012
-- Description:	Select Group Hierarchy Membership by Group_ID
-- =============================================
CREATE PROCEDURE [splx].[splx_api_sel_grouphiermembbygroup]

	@SPLX_GROUP_ID		uniqueidentifier

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	CREATE TABLE #nested_list(
		[PARENT_GROUP_ID] [uniqueidentifier] NOT NULL
		,[CHILD_GROUP_ID] [uniqueidentifier] NOT NULL
	);

	WITH nested ( PARENT_GROUP_ID, CHILD_GROUP_ID )
	AS
	(
		SELECT
			ng.PARENT_GROUP_ID
			,ng.CHILD_GROUP_ID
		FROM splx.SPLX_NESTED_GROUPS ng
		WHERE ng.CHILD_GROUP_ID = @SPLX_GROUP_ID
		UNION ALL
		SELECT
			ng.PARENT_GROUP_ID
			,ng.CHILD_GROUP_ID
		FROM splx.SPLX_NESTED_GROUPS ng
			INNER JOIN nested ON ng.CHILD_GROUP_ID = nested.PARENT_GROUP_ID
	)
	INSERT INTO #nested_list
		SELECT * FROM nested;

	WITH nested ( PARENT_GROUP_ID, CHILD_GROUP_ID )
	AS
	(
		SELECT
			ng.PARENT_GROUP_ID
			,ng.CHILD_GROUP_ID
		FROM splx.SPLX_NESTED_GROUPS ng
		WHERE ng.PARENT_GROUP_ID = @SPLX_GROUP_ID
		UNION ALL
		SELECT
			ng.PARENT_GROUP_ID
			,ng.CHILD_GROUP_ID
		FROM splx.SPLX_NESTED_GROUPS ng
			INNER JOIN nested ON ng.PARENT_GROUP_ID = nested.CHILD_GROUP_ID
	)
	INSERT INTO #nested_list
		SELECT * FROM nested;

	select
		par.[SPLX_GROUP_ID] as PARENT_SPLX_GROUP_ID
		,par.[GROUP_NAME] as PARENT_GROUP_NAME
		,par.[GROUP_DESC] as PARENT_GROUP_DESC
		,par.[GROUP_LOCAL] as PARENT_GROUP_LOCAL
		,par.[GROUP_BUILT_IN] as PARENT_GROUP_BUILT_IN
		,par.[GROUP_ENABLED] as PARENT_GROUP_ENABLED
		,par.[GROUP_MASK] as PARENT_GROUP_MASK
		,ch.[SPLX_GROUP_ID] as CHILD_SPLX_GROUP_ID
		,ch.[GROUP_NAME] as CHILD_GROUP_NAME
		,ch.[GROUP_DESC] as CHILD_GROUP_DESC
		,ch.[GROUP_LOCAL] as CHILD_GROUP_LOCAL
		,ch.[GROUP_BUILT_IN] as CHILD_GROUP_BUILT_IN
		,ch.[GROUP_ENABLED] as CHILD_GROUP_ENABLED
		,ch.[GROUP_MASK] as CHILD_GROUP_MASK
	from #nested_list
		inner join splx.SPLX_GROUPS par on par.SPLX_GROUP_ID = #nested_list.PARENT_GROUP_ID
		inner join splx.SPLX_GROUPS ch on ch.SPLX_GROUP_ID = #nested_list.CHILD_GROUP_ID

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#nested_list',N'U') IS NOT NULL
		DROP TABLE #nested_list

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_del_user]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_del_user]

	@SPLX_USER_ID	uniqueidentifier

AS

BEGIN

	DECLARE @builtIn BIT
	SELECT @builtIn = USER_BUILT_IN FROM splx.SPLX_USERS WHERE SPLX_USER_ID = @SPLX_USER_ID

	IF @builtIn=1
		BEGIN
			RAISERROR ('You cannot delete built-in user accounts.', 16, 1)
		END
	ELSE
		BEGIN

			DELETE
			FROM
				splx.SPLX_ACES
			WHERE
				ACE_TRUSTEE_USER_GROUP_ID = @SPLX_USER_ID

			DELETE
			FROM
				SPLX_TASK_TRUSTEE
			WHERE
				TRUSTEE_USER_GROUP_ID = @SPLX_USER_ID

			DELETE
			FROM
				splx.SPLX_GROUP_MEMBERSHIP
			WHERE
				SPLX_USER_ID = @SPLX_USER_ID

			DELETE
			FROM
				splx.SPLX_USERS
			WHERE
				SPLX_USER_ID = @SPLX_USER_ID

		END

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_future_use_sel_acesbytrustee]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_future_use_sel_acesbytrustee]

	@ACE_TRUSTEE_USER_GROUP_ID		uniqueidentifier

AS
BEGIN

	SELECT DISTINCT
		uie.UIE_UNIQUE_NAME,
		aces.SPLX_ACE_ID,
		aces.ACE_ACCESS_MASK,
		aces.ACE_ACCESS_TYPE1,
		aces.ACE_ACCESS_TYPE2,
		CAST( CASE ISNULL(CAST(aces.ACE_ACCESS_TYPE2 as INT), -1)
			WHEN -1 THEN 0
			ELSE 1
		END AS BIT ) IS_AUDIT_ACE,
		aces.ACE_INHERIT,
		aces.ACE_TYPE
	FROM
		splx.SPLX_ACES aces
			INNER JOIN
		splx.SPLX_UI_ELEMENTS uie ON aces.SPLX_UI_ELEMENT_ID = uie.SPLX_UI_ELEMENT_ID
	WHERE
		aces.ACE_TRUSTEE_USER_GROUP_ID = @ACE_TRUSTEE_USER_GROUP_ID
	ORDER BY
		uie.UIE_UNIQUE_NAME

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_del_group]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_del_group]

	@SPLX_GROUP_ID	uniqueidentifier

AS

BEGIN

	DECLARE @builtIn BIT
	SELECT @builtIn = GROUP_BUILT_IN FROM splx.SPLX_GROUPS WHERE SPLX_GROUP_ID = @SPLX_GROUP_ID

	IF @builtIn=1
		BEGIN
			RAISERROR ('You cannot delete built-in groups.', 16, 1)
		END
	ELSE
		BEGIN
			DELETE
			FROM
				splx.SPLX_ACES
			WHERE
				ACE_TRUSTEE_USER_GROUP_ID = @SPLX_GROUP_ID

			DELETE
			FROM
				SPLX_TASK_TRUSTEE
			WHERE
				TRUSTEE_USER_GROUP_ID = @SPLX_GROUP_ID

			DELETE
			FROM
				splx.SPLX_GROUP_MEMBERSHIP
			WHERE
				SPLX_GROUP_ID = @SPLX_GROUP_ID

			DELETE
			FROM
				splx.SPLX_NESTED_GROUPS
			WHERE
				PARENT_GROUP_ID = @SPLX_GROUP_ID

			DELETE
			FROM
				splx.SPLX_NESTED_GROUPS
			WHERE
				CHILD_GROUP_ID = @SPLX_GROUP_ID

			DELETE
			FROM
				splx.SPLX_GROUPS
			WHERE
				SPLX_GROUP_ID = @SPLX_GROUP_ID
		END
END
GO
/****** Object:  StoredProcedure [splx].[splx_api_del_rightrole]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_del_rightrole]

	@SPLX_RIGHT_ROLE_ID	int

AS

	DELETE
	FROM
		splx.SPLX_RIGHT_ROLES
	WHERE
		SPLX_RIGHT_ROLE_ID = @SPLX_RIGHT_ROLE_ID
GO
/****** Object:  StoredProcedure [splx].[splx_api_del_groupnest]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_del_groupnest]

	--NOTE: passing NULL to @CHILD_GROUP_ID removes ALL entries for the specified @PARENT_GROUP_ID (all parent and child entries)
	@PARENT_GROUP_ID	uniqueidentifier,
	@CHILD_GROUP_ID		uniqueidentifier,
	@CURR_USER_ID		uniqueidentifier

AS

BEGIN

	DECLARE @allowed BIT
	DECLARE @hasTask BIT
	SET @allowed = 0
	SET @hasTask = 0
	DECLARE @taskName VARCHAR(50)
	SET @taskName = CAST( @PARENT_GROUP_ID AS VARCHAR(50) )

	SELECT
		@hasTask = COUNT( SPLX_TASK_ID )
	FROM
		splx.SPLX_TASKS
	WHERE
		TASK_NAME = @taskName
		AND
		TASK_ENABLED = 1

	IF @hasTask = 1
	  BEGIN
		EXEC splx_dal_sel_taskaccess @taskName, @CURR_USER_ID, @allowed OUTPUT
	  END
	ELSE
	  BEGIN
		SET @allowed = 1
	  END


	IF @allowed = 1
		BEGIN
			IF @CHILD_GROUP_ID IS NULL
			BEGIN
				DELETE
				FROM
					splx.SPLX_NESTED_GROUPS
				WHERE
					PARENT_GROUP_ID = @PARENT_GROUP_ID

				DELETE
				FROM
					splx.SPLX_NESTED_GROUPS
				WHERE
					CHILD_GROUP_ID = @PARENT_GROUP_ID
			END

			ELSE
			BEGIN
				DELETE
				FROM
					splx.SPLX_NESTED_GROUPS
				WHERE
					PARENT_GROUP_ID = @PARENT_GROUP_ID
					AND
					CHILD_GROUP_ID = @CHILD_GROUP_ID
			END
		END

	ELSE
	BEGIN
		RAISERROR( 'Insufficient rights to edit group nesting.', 16 , 1 )
	END

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_del_groupmemb]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_del_groupmemb]

	--NOTE: passing NULL to @SPLX_USER_ID removes ALL members of the specified group (@SPLX_GROUP_ID)
	@SPLX_USER_ID	uniqueidentifier,
	@SPLX_GROUP_ID	uniqueidentifier,
	@CURR_USER_ID	uniqueidentifier

AS

BEGIN

	DECLARE @allowed BIT
	DECLARE @hasTask BIT
	SET @allowed = 0
	SET @hasTask = 0
	DECLARE @taskName VARCHAR(50)
	SET @taskName = CAST( @SPLX_GROUP_ID AS VARCHAR(50) )

	SELECT
		@hasTask = COUNT( SPLX_TASK_ID )
	FROM
		splx.SPLX_TASKS
	WHERE
		TASK_NAME = @taskName
		AND
		TASK_ENABLED = 1

	IF @hasTask = 1
	  BEGIN
		EXEC splx_dal_sel_taskaccess @taskName, @CURR_USER_ID, @allowed OUTPUT
	  END
	ELSE
	  BEGIN
		SET @allowed = 1
	  END


	IF @allowed = 1
		BEGIN
			IF @SPLX_USER_ID IS NULL
			  BEGIN
				DELETE
				FROM
					splx.SPLX_GROUP_MEMBERSHIP
				WHERE
					SPLX_GROUP_ID = @SPLX_GROUP_ID
			  END
			ELSE
			  BEGIN
				DELETE
				FROM
					splx.SPLX_GROUP_MEMBERSHIP
				WHERE
					SPLX_USER_ID = @SPLX_USER_ID
					AND
					SPLX_GROUP_ID = @SPLX_GROUP_ID
			  END
		END
	ELSE
	  BEGIN
		RAISERROR( 'Insufficient rights to edit group membership.', 16 , 1 )
	  END

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_ins_groupnest]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_ins_groupnest]

	@PARENT_GROUP_ID	uniqueidentifier,
	@CHILD_GROUP_ID		uniqueidentifier,
	@CURR_USER_ID		uniqueidentifier

AS

BEGIN

	DECLARE @allowed BIT
	DECLARE @hasTask BIT
	SET @allowed = 0
	SET @hasTask = 0
	DECLARE @taskName VARCHAR(50)
	SET @taskName = CAST( @PARENT_GROUP_ID AS VARCHAR(50) )

	DECLARE @count INT

	SELECT
		@hasTask = COUNT( SPLX_TASK_ID )
	FROM
		splx.SPLX_TASKS
	WHERE
		TASK_NAME = @taskName
		AND
		TASK_ENABLED = 1

	IF @hasTask = 1
	  BEGIN
		EXEC splx_dal_sel_taskaccess @taskName, @CURR_USER_ID, @allowed OUTPUT
	  END
	ELSE
	  BEGIN
		SET @allowed = 1
	  END


	IF @allowed = 1
	BEGIN

		--recurse up from group: child cannot be parent of it ancestors
		WITH nested ( PARENT_GROUP_ID )
		AS
		(
			SELECT ng.PARENT_GROUP_ID FROM splx.SPLX_NESTED_GROUPS ng
				WHERE ng.CHILD_GROUP_ID = @PARENT_GROUP_ID
			UNION ALL
			SELECT ng.PARENT_GROUP_ID FROM splx.SPLX_NESTED_GROUPS ng
				INNER JOIN nested ON ng.CHILD_GROUP_ID = nested.PARENT_GROUP_ID
		)
		SELECT DISTINCT @count = COUNT(PARENT_GROUP_ID) FROM nested WHERE PARENT_GROUP_ID = @CHILD_GROUP_ID
		IF @count > 0
		BEGIN
			RAISERROR( 'Child cannot be a Parent of it''s ancestors.', 16 , 1 )
		END

		ELSE
		BEGIN
			--recurse down from group: parent cannot be a child of it's children
			WITH nested ( CHILD_GROUP_ID )
			AS
			(
				SELECT ng.CHILD_GROUP_ID FROM splx.SPLX_NESTED_GROUPS ng
					WHERE ng.PARENT_GROUP_ID = @CHILD_GROUP_ID
				UNION ALL
				SELECT ng.CHILD_GROUP_ID FROM splx.SPLX_NESTED_GROUPS ng
					INNER JOIN nested ON ng.PARENT_GROUP_ID = nested.CHILD_GROUP_ID
			)
			SELECT DISTINCT @count = COUNT(CHILD_GROUP_ID) FROM nested WHERE CHILD_GROUP_ID = @PARENT_GROUP_ID
			IF @count > 0
			BEGIN
				RAISERROR( 'Parent cannot be a Child of it''s descendents.', 16 , 1 )
			END

			ELSE
			BEGIN
				INSERT INTO splx.SPLX_NESTED_GROUPS
				(
					PARENT_GROUP_ID,
					CHILD_GROUP_ID
				)
				VALUES
				(
					@PARENT_GROUP_ID,
					@CHILD_GROUP_ID
				)
			END
		END
	END

	ELSE
	BEGIN
		RAISERROR( 'Insufficient rights to edit group nesting.', 16 , 1 )
	END

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_ins_groupmemb]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_ins_groupmemb]

	@SPLX_USER_ID	uniqueidentifier,
	@SPLX_GROUP_ID	uniqueidentifier,
	@CURR_USER_ID	uniqueidentifier

AS

BEGIN

	DECLARE @allowed BIT
	DECLARE @hasTask BIT
	SET @allowed = 0
	SET @hasTask = 0
	DECLARE @taskName VARCHAR(50)
	SET @taskName = CAST( @SPLX_GROUP_ID AS VARCHAR(50) )

	SELECT
		@hasTask = COUNT( SPLX_TASK_ID )
	FROM
		splx.SPLX_TASKS
	WHERE
		TASK_NAME = @taskName
		AND
		TASK_ENABLED = 1

	IF @hasTask = 1
	  BEGIN
		EXEC splx_dal_sel_taskaccess @taskName, @CURR_USER_ID, @allowed OUTPUT
	  END
	ELSE
	  BEGIN
		SET @allowed = 1
	  END


	IF @allowed = 1
	  BEGIN
		INSERT INTO splx.SPLX_GROUP_MEMBERSHIP
		(
			SPLX_USER_ID,
			SPLX_GROUP_ID
		)
		VALUES
		(
			@SPLX_USER_ID,
			@SPLX_GROUP_ID
		)
	  END
	ELSE
	  BEGIN
		RAISERROR( 'Insufficient rights to edit group membership.', 16 , 1 )
	  END

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_future_use_sel_securitybyuie]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_future_use_sel_securitybyuie]

	@SPLX_UI_ELEMENT_ID		uniqueidentifier

AS
BEGIN

	SELECT
		g.GROUP_NAME,
		aces.SPLX_ACE_ID,
		aces.ACE_ACCESS_MASK,
		aces.ACE_ACCESS_TYPE1,
		aces.ACE_ACCESS_TYPE2,
		CAST( CASE ISNULL(CAST(aces.ACE_ACCESS_TYPE2 as INT), -1)
			WHEN -1 THEN 0
			ELSE 1
		END AS BIT ) IS_AUDIT_ACE,
		aces.ACE_INHERIT,
		aces.ACE_TYPE
	FROM
		splx.SPLX_ACES aces
			INNER JOIN
		splx.SPLX_GROUPS g ON aces.ACE_TRUSTEE_USER_GROUP_ID = g.SPLX_GROUP_ID
	WHERE
		aces.SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID


	SELECT
		u.USER_NAME,
		aces.SPLX_ACE_ID,
		aces.ACE_ACCESS_MASK,
		aces.ACE_ACCESS_TYPE1,
		aces.ACE_ACCESS_TYPE2,
		CAST( CASE ISNULL(CAST(aces.ACE_ACCESS_TYPE2 as INT), -1)
			WHEN -1 THEN 0
			ELSE 1
		END AS BIT ) IS_AUDIT_ACE,
		aces.ACE_INHERIT,
		aces.ACE_TYPE
	FROM
		splx.SPLX_ACES aces
			INNER JOIN
		splx.SPLX_USERS u ON aces.ACE_TRUSTEE_USER_GROUP_ID = u.SPLX_USER_ID
	WHERE
		aces.SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID


	SELECT
		ISNULL(UIE_DACL_INHERIT, 1) AS 'UIE_DACL_INHERIT',
		ISNULL(UIE_SACL_INHERIT, 1) AS 'UIE_SACL_INHERIT',
		UIE_SACL_AUDIT_TYPE_FILTER
	FROM
		splx.SPLX_UI_ELEMENTS
	WHERE
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID


	SELECT
		*
	FROM
		splx.SPLX_RIGHT_ROLES
	WHERE
		SPLX_UI_ELEMENT_RULE_ID = @SPLX_UI_ELEMENT_ID


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		'GroupAces,UserAces,AclInfo,RightRoles' AS Tables

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_del_uie_sd]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_del_uie_sd]

	@SPLX_ACE_ID_LIST				varchar(max)
	,@SPLX_RIGHT_ROLE_ID_LIST		varchar(max)
	,@SPLX_VALIDATION_RULE_ID_LIST	varchar(max)

AS
BEGIN

	CREATE TABLE #VR_LIST (
			SPLX_VALIDATION_RULE_ID [uniqueidentifier] NOT NULL
	);

	--- VR_LIST --------------------------------------------
	WITH VR_LIST( SPLX_VALIDATION_RULE_ID )
	AS
	(
		SELECT
			SPLX_VALIDATION_RULE_ID
		FROM splx.SPLX_VALIDATION_RULES
		WHERE
			SPLX_VALIDATION_RULE_ID IN
			(SELECT DISTINCT value AS SPLX_VALIDATION_RULE_ID FROM ap_Split( @SPLX_VALIDATION_RULE_ID_LIST, ',' ))
			
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


	BEGIN TRANSACTION

	DELETE FROM splx.SPLX_ACES
		WHERE SPLX_ACE_ID IN
		(SELECT DISTINCT value AS SPLX_ACE_ID FROM ap_Split( @SPLX_ACE_ID_LIST, ',' ))

	DELETE FROM splx.SPLX_RIGHT_ROLES
		WHERE SPLX_RIGHT_ROLE_ID IN
		(SELECT DISTINCT value AS SPLX_RIGHT_ROLE_ID FROM ap_Split( @SPLX_RIGHT_ROLE_ID_LIST, ',' ))

	DELETE FROM splx.SPLX_RIGHT_ROLES
		WHERE SPLX_UI_ELEMENT_RULE_ID IN
		(SELECT #VR_LIST.SPLX_VALIDATION_RULE_ID FROM #VR_LIST)

	DELETE FROM splx.SPLX_VALIDATION_RULES
		WHERE SPLX_VALIDATION_RULE_ID IN
		(SELECT #VR_LIST.SPLX_VALIDATION_RULE_ID FROM #VR_LIST)

	COMMIT TRANSACTION


	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#VR_LIST',N'U') IS NOT NULL
		DROP TABLE #VR_LIST

END
GO
/****** Object:  StoredProcedure [splx].[splx_dal_sel_security_byuserbyuie]    Script Date: 07/20/2013 23:22:43 ******/
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
CREATE PROCEDURE [splx].[splx_dal_sel_security_byuserbyuie]

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
			WHERE UIE_PARENT_ID = @SPLX_UI_ELEMENT_ID
		UNION ALL
		SELECT p.SPLX_UI_ELEMENT_ID, p.UIE_UNIQUE_NAME, p.UIE_PARENT_ID, UIE_LEVEL + 1, p.UIE_DACL_INHERIT, p.UIE_SACL_INHERIT, p.UIE_SACL_AUDIT_TYPE_FILTER
			FROM splx.SPLX_UI_ELEMENTS p
				INNER JOIN CHILD_ELEMENTS c
				ON p.UIE_PARENT_ID = c.SPLX_UI_ELEMENT_ID
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
		,[GROUP_MASK] [binary](16) NOT NULL
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
		,[GROUP_MASK] [binary](16) NOT NULL
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
		[GROUP_MASK] [binary](16) NOT NULL
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
		,splx.Splx_GetTableOr( '#trustees_mask', 'GROUP_MASK', 128 ) AS 'GROUP_MEMBERSHIP_MASK'

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
/****** Object:  StoredProcedure [splx].[splx_api_sel_uielementbyparent_composite]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_sel_uielementbyparent_composite]

	@UIE_PARENT_ID uniqueidentifier

AS

	CREATE TABLE #uie (
		[SPLX_UI_ELEMENT_ID] [uniqueidentifier] NOT NULL,
		[UIE_NAME] [varchar](50) NOT NULL,
		[UIE_CONTROL_TYPE] [varchar](50) NULL,
		[UIE_DESC] [varchar](255) NULL,
		[UIE_DESC_TOOLTIP] [bit] NOT NULL,
		[UIE_UNIQUE_NAME] [varchar](50) NULL,
		[UIE_DATA_TYPE] [varchar](50) NULL,
		[UIE_DATA_TYPE_ERR_MSG] [varchar](255) NULL,
		[UIE_FORMAT_STRING] [varchar](50) NULL,
		[UIE_ALLOW_UNDECLARED] [bit] NOT NULL,
		[UIE_PARENT_ID] [uniqueidentifier] NULL,
		[UIE_DACL_INHERIT] [bit] NULL,
		[UIE_SACL_INHERIT] [bit] NULL,
		[UIE_SACL_AUDIT_TYPE_FILTER] [int] NULL
	);

	CREATE TABLE #VR (
		[SPLX_VALIDATION_RULE_ID] [uniqueidentifier] NOT NULL,
		[VR_NAME] [varchar](50) NULL,
		[VR_EVENT_BINDING] [varchar](50) NULL,
		[VR_COMPARE_VALUE1] [varchar](255) NULL,
		[VR_EXPRESSION_TYPE1] [varchar](50) NULL,
		[VR_VALUE_TYPE1] [varchar](50) NULL,
		[VR_COMPARE_VALUE2] [varchar](255) NULL,
		[VR_EXPRESSION_TYPE2] [varchar](50) NULL,
		[VR_VALUE_TYPE2] [varchar](50) NULL,
		[VR_COMPARE_DATA_TYPE] [varchar](50) NULL,
		[VR_OPERATOR] [varchar](50) NULL,
		[VR_ERROR_MESSAGE] [varchar](255) NULL,
		[VR_ERROR_UIE_UNIQUE_NAME] [varchar](50) NULL,
		[VR_FAIL_PARENT] [bit] NOT NULL,
		[VR_RULE_TYPE] [varchar](50) NOT NULL,
		[VR_PARENT_ID] [uniqueidentifier] NULL,
		[SPLX_UI_ELEMENT_ID] [uniqueidentifier] NOT NULL,
		[VR_SORT_ORDER] [int] NOT NULL
	);

	IF @UIE_PARENT_ID IS NULL
		INSERT INTO #uie
			SELECT
				*
			FROM
				splx.SPLX_UI_ELEMENTS
			WHERE
				UIE_PARENT_ID IS NULL
	ELSE
		INSERT INTO #uie
			SELECT
				*
			FROM
				splx.SPLX_UI_ELEMENTS
			WHERE
				UIE_PARENT_ID = @UIE_PARENT_ID

	SELECT * FROM #uie


	SELECT
		*
		,IS_AUDIT_ACE =
			CASE 
				WHEN ACE_ACCESS_TYPE2 IS NULL THEN 0
				ELSE 1
			END
	FROM
		splx.SPLX_ACES
	WHERE
		SPLX_UI_ELEMENT_ID IN
			( SELECT SPLX_UI_ELEMENT_ID FROM #uie )


	SELECT
		*
	FROM
		splx.SPLX_RIGHT_ROLES
	WHERE
		SPLX_UI_ELEMENT_RULE_ID IN
			( SELECT SPLX_UI_ELEMENT_ID FROM #uie )


	INSERT INTO #VR
		SELECT
			*
		FROM
			splx.SPLX_VALIDATION_RULES
		WHERE
			SPLX_UI_ELEMENT_ID IN
				( SELECT SPLX_UI_ELEMENT_ID FROM #uie )
			AND
			( VR_RULE_TYPE = 'RightRoleIf' OR VR_RULE_TYPE = 'RightRoleElse' )


	SELECT * FROM #VR


	SELECT
		*
	FROM
		splx.SPLX_RIGHT_ROLES
	WHERE
		SPLX_UI_ELEMENT_RULE_ID IN
		(
			SELECT
				SPLX_VALIDATION_RULE_ID
			FROM
				#VR
		)


	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#uie',N'U') IS NOT NULL
		DROP TABLE #uie

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#VR',N'U') IS NOT NULL
		DROP TABLE #VR


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		'UIElements,Aces,UieRightRoles,RightRoleRules,RrrRightRoles' AS Tables
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_uielementbyid_composite]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_sel_uielementbyid_composite]

	@SPLX_UI_ELEMENT_ID uniqueidentifier

AS

	CREATE TABLE #VR (
		[SPLX_VALIDATION_RULE_ID] [uniqueidentifier] NOT NULL,
		[VR_NAME] [varchar](50) NULL,
		[VR_EVENT_BINDING] [varchar](50) NULL,
		[VR_COMPARE_VALUE1] [varchar](255) NULL,
		[VR_EXPRESSION_TYPE1] [varchar](50) NULL,
		[VR_VALUE_TYPE1] [varchar](50) NULL,
		[VR_COMPARE_VALUE2] [varchar](255) NULL,
		[VR_EXPRESSION_TYPE2] [varchar](50) NULL,
		[VR_VALUE_TYPE2] [varchar](50) NULL,
		[VR_COMPARE_DATA_TYPE] [varchar](50) NULL,
		[VR_OPERATOR] [varchar](50) NULL,
		[VR_ERROR_MESSAGE] [varchar](255) NULL,
		[VR_ERROR_UIE_UNIQUE_NAME] [varchar](50) NULL,
		[VR_FAIL_PARENT] [bit] NOT NULL,
		[VR_RULE_TYPE] [varchar](50) NOT NULL,
		[VR_PARENT_ID] [uniqueidentifier] NULL,
		[SPLX_UI_ELEMENT_ID] [uniqueidentifier] NOT NULL,
		[VR_SORT_ORDER] [int] NOT NULL
	);

	SELECT
		*
	FROM
		splx.SPLX_UI_ELEMENTS
	WHERE
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID


	SELECT
		*
		,IS_AUDIT_ACE =
			CASE 
				WHEN ACE_ACCESS_TYPE2 IS NULL THEN 0
				ELSE 1
			END
	FROM
		splx.SPLX_ACES
	WHERE
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID


	SELECT
		*
	FROM
		splx.SPLX_RIGHT_ROLES
	WHERE
		SPLX_UI_ELEMENT_RULE_ID = @SPLX_UI_ELEMENT_ID


	INSERT INTO #VR
		SELECT
			*
		FROM
			splx.SPLX_VALIDATION_RULES
		WHERE
			SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID
			AND
			( VR_RULE_TYPE = 'RightRoleIf' OR VR_RULE_TYPE = 'RightRoleElse' )


	SELECT * FROM #VR


	SELECT
		*
	FROM
		splx.SPLX_RIGHT_ROLES
	WHERE
		SPLX_UI_ELEMENT_RULE_ID IN
		(
			SELECT
				SPLX_VALIDATION_RULE_ID
			FROM
				#VR
		)


	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#VR',N'U') IS NOT NULL
		DROP TABLE #VR


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		'UIElements,Aces,UieRightRoles,RightRoleRules,RrrRightRoles' AS Tables
GO
/****** Object:  StoredProcedure [splx].[splx_api_upsert_rightrole]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_upsert_rightrole]

	@SPLX_RIGHT_ROLE_ID			int = 0 output,
	@SPLX_UI_ELEMENT_RULE_ID	uniqueidentifier,
	@RR_UIE_UNIQUE_NAME			varchar(500),
	@RR_ACE_TYPE				varchar(50),
	@RR_RIGHT_NAME				varchar(50),
	@RR_UI_RIGHT				varchar(50),
	@RR_ROLE_TYPE				varchar(50)

AS
BEGIN

	IF NOT EXISTS ( SELECT SPLX_RIGHT_ROLE_ID FROM splx.SPLX_RIGHT_ROLES WHERE SPLX_RIGHT_ROLE_ID = @SPLX_RIGHT_ROLE_ID )
	BEGIN

		INSERT INTO splx.SPLX_RIGHT_ROLES
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

	END

	ELSE
	BEGIN

		UPDATE
			splx.SPLX_RIGHT_ROLES
		SET
			SPLX_UI_ELEMENT_RULE_ID = @SPLX_UI_ELEMENT_RULE_ID,
			RR_UIE_UNIQUE_NAME = @RR_UIE_UNIQUE_NAME,
			RR_ACE_TYPE = @RR_ACE_TYPE,
			RR_RIGHT_NAME = @RR_RIGHT_NAME,
			RR_UI_RIGHT = @RR_UI_RIGHT
		WHERE
			SPLX_RIGHT_ROLE_ID = @SPLX_RIGHT_ROLE_ID

	END

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_upsert_fme]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_upsert_fme]

	@SPLX_FILLMAP_EXPRESSION_ID	int = NULL output
	,@FME_NAME					varchar(50)
	,@FME_EVENT_BINDING			varchar(50) = NULL
	,@FME_EXPRESSION			varchar(1000) = NULL
	,@FME_EXPRESSION_TYPE		varchar(50) = NULL
	,@FME_SORT_ORDER			int = 0
	,@FME_IF_CLAUSE				bit
	,@SPLX_UIE_VR_PARENT_ID		uniqueidentifier

AS
BEGIN

	IF NOT EXISTS ( SELECT SPLX_FILLMAP_EXPRESSION_ID FROM splx.SPLX_FILLMAP_EXPRESSIONS WHERE SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID )
	BEGIN

		INSERT INTO splx.SPLX_FILLMAP_EXPRESSIONS
		(
			FME_NAME
			,FME_EVENT_BINDING
			,FME_EXPRESSION
			,FME_EXPRESSION_TYPE
			,FME_SORT_ORDER
			,FME_IF_CLAUSE
			,SPLX_UIE_VR_PARENT_ID
		)
		VALUES
		(
			@FME_NAME
			,@FME_EVENT_BINDING
			,@FME_EXPRESSION
			,@FME_EXPRESSION_TYPE
			,@FME_SORT_ORDER
			,@FME_IF_CLAUSE
			,@SPLX_UIE_VR_PARENT_ID
		)

		SELECT @SPLX_FILLMAP_EXPRESSION_ID = scope_identity()

	END

	ELSE
	BEGIN

		UPDATE
			splx.SPLX_FILLMAP_EXPRESSIONS
		SET
			FME_NAME = @FME_NAME
			,FME_EVENT_BINDING = @FME_EVENT_BINDING
			,FME_EXPRESSION = @FME_EXPRESSION
			,FME_EXPRESSION_TYPE = @FME_EXPRESSION_TYPE
			,FME_SORT_ORDER = @FME_SORT_ORDER
			,FME_IF_CLAUSE = @FME_IF_CLAUSE
			,SPLX_UIE_VR_PARENT_ID = @SPLX_UIE_VR_PARENT_ID
		WHERE
			SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID

	END

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_upsert_fmb]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_upsert_fmb]

	@SPLX_FILLMAP_DATABINDING_ID	int = null output
	,@FMB_UIE_UNIQUE_NAME			varchar(500)
	,@FMB_PROPERTY_NAME				varchar(50)
	,@FMB_VALUE						varchar(50)
	,@FMB_TYPECAST_VALUE			bit
	,@FMB_OVERRIDE_VALUE			bit
	,@SPLX_FILLMAP_EXPRESSION_ID	int

AS
BEGIN

	IF NOT EXISTS ( SELECT SPLX_FILLMAP_DATABINDING_ID FROM splx.SPLX_FILLMAP_DATABINDINGS WHERE SPLX_FILLMAP_DATABINDING_ID = @SPLX_FILLMAP_DATABINDING_ID )
	BEGIN

		INSERT INTO splx.SPLX_FILLMAP_DATABINDINGS
		(
			FMB_UIE_UNIQUE_NAME
			,FMB_PROPERTY_NAME
			,FMB_VALUE
			,FMB_TYPECAST_VALUE
			,FMB_OVERRIDE_VALUE
			,SPLX_FILLMAP_EXPRESSION_ID
		)
		VALUES
		(
			@FMB_UIE_UNIQUE_NAME
			,@FMB_PROPERTY_NAME
			,@FMB_VALUE
			,@FMB_TYPECAST_VALUE
			,@FMB_OVERRIDE_VALUE
			,@SPLX_FILLMAP_EXPRESSION_ID
		)

		SET @SPLX_FILLMAP_DATABINDING_ID = scope_identity()

	END

	ELSE
	BEGIN

		UPDATE
			splx.SPLX_FILLMAP_DATABINDINGS
		SET
			FMB_UIE_UNIQUE_NAME = @FMB_UIE_UNIQUE_NAME
			,FMB_PROPERTY_NAME = @FMB_PROPERTY_NAME
			,FMB_VALUE = @FMB_VALUE
			,FMB_TYPECAST_VALUE = @FMB_TYPECAST_VALUE
			,FMB_OVERRIDE_VALUE = @FMB_OVERRIDE_VALUE
			,SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID
		WHERE
			SPLX_FILLMAP_DATABINDING_ID = @SPLX_FILLMAP_DATABINDING_ID

	END

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_vrbyuieparent_composite]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_sel_vrbyuieparent_composite]

	@SPLX_UI_ELEMENT_ID uniqueidentifier

AS

	CREATE TABLE #VR (
		[SPLX_VALIDATION_RULE_ID] [uniqueidentifier] NOT NULL,
		[VR_NAME] [varchar](50) NULL,
		[VR_EVENT_BINDING] [varchar](50) NULL,
		[VR_COMPARE_VALUE1] [varchar](255) NULL,
		[VR_EXPRESSION_TYPE1] [varchar](50) NULL,
		[VR_VALUE_TYPE1] [varchar](50) NULL,
		[VR_COMPARE_VALUE2] [varchar](255) NULL,
		[VR_EXPRESSION_TYPE2] [varchar](50) NULL,
		[VR_VALUE_TYPE2] [varchar](50) NULL,
		[VR_COMPARE_DATA_TYPE] [varchar](50) NULL,
		[VR_OPERATOR] [varchar](50) NULL,
		[VR_ERROR_MESSAGE] [varchar](255) NULL,
		[VR_ERROR_UIE_UNIQUE_NAME] [varchar](50) NULL,
		[VR_FAIL_PARENT] [bit] NOT NULL,
		[VR_RULE_TYPE] [varchar](50) NOT NULL,
		[VR_PARENT_ID] [uniqueidentifier] NULL,
		[SPLX_UI_ELEMENT_ID] [uniqueidentifier] NOT NULL,
		[VR_SORT_ORDER] [int] NOT NULL
	);

	CREATE TABLE #FME (
		[SPLX_FILLMAP_EXPRESSION_ID] [int] NOT NULL,
		[FME_NAME] [varchar](50) NULL,
		[FME_EVENT_BINDING] [varchar](50) NULL,
		[FME_EXPRESSION] [varchar](1000) NULL,
		[FME_EXPRESSION_TYPE] [varchar](50) NULL,
		[FME_IF_CLAUSE] [bit] NOT NULL,
		[FME_SORT_ORDER] [int] NOT NULL,
		[SPLX_UIE_VR_PARENT_ID] [uniqueidentifier] NOT NULL
	);

	INSERT INTO #VR
		SELECT
			*
		FROM
			splx.SPLX_VALIDATION_RULES
		WHERE
			SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID
			AND
			VR_PARENT_ID IS NULL
		ORDER BY
			VR_SORT_ORDER


	INSERT INTO #FME
		SELECT
			*
		FROM
			splx.SPLX_FILLMAP_EXPRESSIONS
		WHERE
			SPLX_UIE_VR_PARENT_ID IN
			(
				SELECT
					SPLX_VALIDATION_RULE_ID
				FROM
					#VR
			)
		ORDER BY
			FME_SORT_ORDER


		--return the ValidationRules
		SELECT * FROM #VR

		--return the FillMaps
		SELECT * FROM #FME

		--return the DataBindings
		SELECT
			*
		FROM
			splx.SPLX_FILLMAP_DATABINDINGS
		WHERE
			SPLX_FILLMAP_EXPRESSION_ID IN
			(
				SELECT
					SPLX_FILLMAP_EXPRESSION_ID
				FROM
					#FME
			)

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#VR',N'U') IS NOT NULL
		DROP TABLE #VR
	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#FME',N'U') IS NOT NULL
		DROP TABLE #FME


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		'ValidationRules,FillMaps,DataBindings' AS Tables
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_vrbyparent_composite]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_sel_vrbyparent_composite]

	@SPLX_VALIDATION_RULE_ID uniqueidentifier

AS

	CREATE TABLE #VR (
		[SPLX_VALIDATION_RULE_ID] [uniqueidentifier] NOT NULL,
		[VR_NAME] [varchar](50) NULL,
		[VR_EVENT_BINDING] [varchar](50) NULL,
		[VR_COMPARE_VALUE1] [varchar](255) NULL,
		[VR_EXPRESSION_TYPE1] [varchar](50) NULL,
		[VR_VALUE_TYPE1] [varchar](50) NULL,
		[VR_COMPARE_VALUE2] [varchar](255) NULL,
		[VR_EXPRESSION_TYPE2] [varchar](50) NULL,
		[VR_VALUE_TYPE2] [varchar](50) NULL,
		[VR_COMPARE_DATA_TYPE] [varchar](50) NULL,
		[VR_OPERATOR] [varchar](50) NULL,
		[VR_ERROR_MESSAGE] [varchar](255) NULL,
		[VR_ERROR_UIE_UNIQUE_NAME] [varchar](50) NULL,
		[VR_FAIL_PARENT] [bit] NOT NULL,
		[VR_RULE_TYPE] [varchar](50) NOT NULL,
		[VR_PARENT_ID] [uniqueidentifier] NULL,
		[SPLX_UI_ELEMENT_ID] [uniqueidentifier] NOT NULL,
		[VR_SORT_ORDER] [int] NOT NULL
	);

	CREATE TABLE #FME (
		[SPLX_FILLMAP_EXPRESSION_ID] [int] NOT NULL,
		[FME_NAME] [varchar](50) NULL,
		[FME_EVENT_BINDING] [varchar](50) NULL,
		[FME_EXPRESSION] [varchar](1000) NULL,
		[FME_EXPRESSION_TYPE] [varchar](50) NULL,
		[FME_IF_CLAUSE] [bit] NOT NULL,
		[FME_SORT_ORDER] [int] NOT NULL,
		[SPLX_UIE_VR_PARENT_ID] [uniqueidentifier] NOT NULL
	);

	INSERT INTO #VR
		SELECT
			*
		FROM
			splx.SPLX_VALIDATION_RULES
		WHERE
			VR_PARENT_ID = @SPLX_VALIDATION_RULE_ID
		ORDER BY
			VR_SORT_ORDER


	INSERT INTO #FME
		SELECT
			*
		FROM
			splx.SPLX_FILLMAP_EXPRESSIONS
		WHERE
			SPLX_UIE_VR_PARENT_ID IN
			(
				SELECT
					SPLX_VALIDATION_RULE_ID
				FROM
					#VR
			)
		ORDER BY
			FME_SORT_ORDER


		--return the ValidationRules
		SELECT * FROM #VR

		--return the FillMaps
		SELECT * FROM #FME

		--return the DataBindings
		SELECT
			*
		FROM
			splx.SPLX_FILLMAP_DATABINDINGS
		WHERE
			SPLX_FILLMAP_EXPRESSION_ID IN
			(
				SELECT
					SPLX_FILLMAP_EXPRESSION_ID
				FROM
					#FME
			)

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#VR',N'U') IS NOT NULL
		DROP TABLE #VR
	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#FME',N'U') IS NOT NULL
		DROP TABLE #FME


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		'ValidationRules,FillMaps,DataBindings' AS Tables
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_vr_withchildren_composite]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_sel_vr_withchildren_composite]

	@SPLX_VALIDATION_RULE_ID	uniqueidentifier = null

AS
BEGIN

	CREATE TABLE #VR_LIST (
			SPLX_VALIDATION_RULE_ID [uniqueidentifier] NOT NULL
	);

	CREATE TABLE #FME_LIST (
			SPLX_FILLMAP_EXPRESSION_ID [int] NOT NULL
	);

	--- VR_LIST --------------------------------------------
	WITH VR_LIST( SPLX_VALIDATION_RULE_ID )
	AS
	(
		SELECT
			SPLX_VALIDATION_RULE_ID
		FROM splx.SPLX_VALIDATION_RULES
		WHERE
			SPLX_VALIDATION_RULE_ID = @SPLX_VALIDATION_RULE_ID
			
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


	--- FME_LIST -------------------------------------------
	INSERT INTO #FME_LIST
		SELECT
			fme.SPLX_FILLMAP_EXPRESSION_ID
		FROM
			splx.SPLX_FILLMAP_EXPRESSIONS fme
			INNER JOIN
			#VR_LIST ON fme.SPLX_UIE_VR_PARENT_ID = #VR_LIST.SPLX_VALIDATION_RULE_ID
	--- FME_LIST -------------------------------------------


	SELECT * FROM splx.SPLX_FILLMAP_DATABINDINGS
		WHERE SPLX_FILLMAP_EXPRESSION_ID IN
		(SELECT #FME_LIST.SPLX_FILLMAP_EXPRESSION_ID FROM #FME_LIST)

	SELECT * FROM splx.SPLX_FILLMAP_EXPRESSIONS
		WHERE SPLX_FILLMAP_EXPRESSION_ID IN
		(SELECT #FME_LIST.SPLX_FILLMAP_EXPRESSION_ID FROM #FME_LIST)

	SELECT * FROM splx.SPLX_VALIDATION_RULES
		WHERE SPLX_VALIDATION_RULE_ID IN
		(SELECT #VR_LIST.SPLX_VALIDATION_RULE_ID FROM #VR_LIST)
		AND (VR_RULE_TYPE = 'ValidationIf' OR VR_RULE_TYPE = 'ValidationElse')


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		'DataBindings,FillMaps,ValidationRules' AS Tables


	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#VR_LIST',N'U') IS NOT NULL
		DROP TABLE #VR_LIST

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#UIE_LIST',N'U') IS NOT NULL
		DROP TABLE #UIE_LIST

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_uie_withchildren_composite]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_sel_uie_withchildren_composite]

	@UIE_UNIQUE_NAME		varchar(50) = null
	,@SPLX_UI_ELEMENT_ID	uniqueidentifier = null
	,@IncludeSecurity bit = 1

AS
BEGIN

	IF @SPLX_UI_ELEMENT_ID IS NULL AND @UIE_UNIQUE_NAME IS NOT NULL
		SELECT
			@SPLX_UI_ELEMENT_ID = SPLX_UI_ELEMENT_ID
		FROM splx.SPLX_UI_ELEMENTS
		WHERE
			UIE_UNIQUE_NAME = @UIE_UNIQUE_NAME


	CREATE TABLE #UIE_LIST (
			[SPLX_UI_ELEMENT_ID] [uniqueidentifier] NOT NULL
	);

	CREATE TABLE #VR_LIST (
			SPLX_VALIDATION_RULE_ID [uniqueidentifier] NOT NULL
	);

	CREATE TABLE #FME_LIST (
			SPLX_FILLMAP_EXPRESSION_ID [int] NOT NULL
	);

	--- UIE_LIST -------------------------------------------
	IF @SPLX_UI_ELEMENT_ID IS NULL
		WITH UIE_LIST( SPLX_UI_ELEMENT_ID )
		AS
		(
			SELECT
				SPLX_UI_ELEMENT_ID
			FROM splx.SPLX_UI_ELEMENTS
			WHERE
				UIE_PARENT_ID IS NULL
				
			UNION ALL
			
			SELECT
				u.SPLX_UI_ELEMENT_ID
			FROM splx.SPLX_UI_ELEMENTS u
			INNER JOIN UIE_LIST
				ON u.UIE_PARENT_ID = UIE_LIST.SPLX_UI_ELEMENT_ID
		)
		INSERT INTO #UIE_LIST
			SELECT * FROM UIE_LIST;
	ELSE
		WITH UIE_LIST( SPLX_UI_ELEMENT_ID )
		AS
		(
			SELECT
				SPLX_UI_ELEMENT_ID
			FROM splx.SPLX_UI_ELEMENTS
			WHERE
				SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID
				
			UNION ALL
			
			SELECT
				u.SPLX_UI_ELEMENT_ID
			FROM splx.SPLX_UI_ELEMENTS u
			INNER JOIN UIE_LIST
				ON u.UIE_PARENT_ID = UIE_LIST.SPLX_UI_ELEMENT_ID
		)
		INSERT INTO #UIE_LIST
			SELECT * FROM UIE_LIST;
	--- UIE_LIST -------------------------------------------


	--- VR_LIST --------------------------------------------
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


	--- FME_LIST -------------------------------------------
	INSERT INTO #FME_LIST
		SELECT
			fme.SPLX_FILLMAP_EXPRESSION_ID
		FROM
			splx.SPLX_FILLMAP_EXPRESSIONS fme
			INNER JOIN
			#UIE_LIST ON fme.SPLX_UIE_VR_PARENT_ID = #UIE_LIST.SPLX_UI_ELEMENT_ID

	INSERT INTO #FME_LIST
		SELECT
			fme.SPLX_FILLMAP_EXPRESSION_ID
		FROM
			splx.SPLX_FILLMAP_EXPRESSIONS fme
			INNER JOIN
			#VR_LIST ON fme.SPLX_UIE_VR_PARENT_ID = #VR_LIST.SPLX_VALIDATION_RULE_ID
	--- FME_LIST -------------------------------------------

	IF @IncludeSecurity = 1
	BEGIN
		SELECT
			*
			,IS_AUDIT_ACE =
				CASE 
					WHEN ACE_ACCESS_TYPE2 IS NULL THEN 0
					ELSE 1
				END
		FROM
			splx.SPLX_ACES
		WHERE SPLX_UI_ELEMENT_ID IN
			(SELECT #UIE_LIST.SPLX_UI_ELEMENT_ID FROM #UIE_LIST)

		SELECT * FROM splx.SPLX_RIGHT_ROLES
			WHERE SPLX_UI_ELEMENT_RULE_ID IN
			(SELECT #UIE_LIST.SPLX_UI_ELEMENT_ID FROM #UIE_LIST)

		SELECT * FROM splx.SPLX_RIGHT_ROLES
			WHERE SPLX_UI_ELEMENT_RULE_ID IN
			(SELECT #VR_LIST.SPLX_VALIDATION_RULE_ID FROM #VR_LIST)

		SELECT * FROM splx.SPLX_VALIDATION_RULES
			WHERE SPLX_VALIDATION_RULE_ID IN
			(SELECT #VR_LIST.SPLX_VALIDATION_RULE_ID FROM #VR_LIST)
			AND (VR_RULE_TYPE = 'RightRoleIf' OR VR_RULE_TYPE = 'RightRoleElse')
	END

	SELECT * FROM splx.SPLX_FILLMAP_DATABINDINGS
		WHERE SPLX_FILLMAP_EXPRESSION_ID IN
		(SELECT #FME_LIST.SPLX_FILLMAP_EXPRESSION_ID FROM #FME_LIST)

	SELECT * FROM splx.SPLX_FILLMAP_EXPRESSIONS
		WHERE SPLX_FILLMAP_EXPRESSION_ID IN
		(SELECT #FME_LIST.SPLX_FILLMAP_EXPRESSION_ID FROM #FME_LIST)

	SELECT * FROM splx.SPLX_VALIDATION_RULES
		WHERE SPLX_VALIDATION_RULE_ID IN
		(SELECT #VR_LIST.SPLX_VALIDATION_RULE_ID FROM #VR_LIST)
		AND (VR_RULE_TYPE = 'ValidationIf' OR VR_RULE_TYPE = 'ValidationElse')

	SELECT * FROM splx.SPLX_UI_ELEMENTS
		WHERE SPLX_UI_ELEMENT_ID IN
		(SELECT #UIE_LIST.SPLX_UI_ELEMENT_ID FROM #UIE_LIST)


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	IF @IncludeSecurity = 1
		SELECT
			'Aces,UieRightRoles,RrrRightRoles,RightRoleRules,DataBindings,FillMaps,ValidationRules,UIElements' AS Tables
	ELSE
		SELECT
			'DataBindings,FillMaps,ValidationRules,UIElements' AS Tables


	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#FME_LIST',N'U') IS NOT NULL
		DROP TABLE #FME_LIST

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#VR_LIST',N'U') IS NOT NULL
		DROP TABLE #VR_LIST

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#UIE_LIST',N'U') IS NOT NULL
		DROP TABLE #UIE_LIST

END
GO
/****** Object:  StoredProcedure [splx].[splx_dal_sel_validationbyuie]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 10/02/2007
-- Description:	Select Validation by UI_Element
-- =============================================
CREATE PROCEDURE [splx].[splx_dal_sel_validationbyuie]

	@UIE_UNIQUE_NAME		varchar(50)

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

	CREATE TABLE #UIEV (
		[SPLX_UI_ELEMENT_ID] [uniqueidentifier] NOT NULL,
		[UIE_NAME] [varchar](50) NOT NULL,
		[UIE_CONTROL_TYPE] [varchar](50) NULL,
		[UIE_DESC] [varchar](255) NULL,
		[UIE_DESC_TOOLTIP] [bit] NOT NULL,
		[UIE_UNIQUE_NAME] [varchar](50) NOT NULL,
		[UIE_DATA_TYPE] [varchar](50) NULL,
		[UIE_DATA_TYPE_ERR_MSG] [varchar](255) NULL,
		[UIE_FORMAT_STRING] [varchar](50) NULL,
		[UIE_ALLOW_UNDECLARED] [bit] NOT NULL,
		[UIE_PARENT_ID] [uniqueidentifier] NULL,
		[UIE_LEVEL] [int] NOT NULL
	);


	--populate temp table: recurse the UI_ELEMENTS using @SPLX_UI_ELEMENT_ID as the topmost element
	WITH CHILD_ELEMENTS(
		SPLX_UI_ELEMENT_ID, UIE_NAME, UIE_CONTROL_TYPE,
		UIE_DESC, UIE_DESC_TOOLTIP, UIE_UNIQUE_NAME,
		UIE_DATA_TYPE, UIE_DATA_TYPE_ERR_MSG, UIE_FORMAT_STRING,
		UIE_ALLOW_UNDECLARED, UIE_PARENT_ID, UIE_LEVEL) AS
	(
		SELECT
			SPLX_UI_ELEMENT_ID, UIE_NAME, UIE_CONTROL_TYPE,
			UIE_DESC, UIE_DESC_TOOLTIP, UIE_UNIQUE_NAME,
			UIE_DATA_TYPE, UIE_DATA_TYPE_ERR_MSG, UIE_FORMAT_STRING,
			UIE_ALLOW_UNDECLARED, UIE_PARENT_ID, 1 AS UIE_LEVEL
		FROM
			splx.SPLX_UI_ELEMENTS
		WHERE
			UIE_PARENT_ID = @SPLX_UI_ELEMENT_ID

		UNION ALL

		SELECT
			p.SPLX_UI_ELEMENT_ID, p.UIE_NAME, p.UIE_CONTROL_TYPE,
			p.UIE_DESC, p.UIE_DESC_TOOLTIP, p.UIE_UNIQUE_NAME,
			p.UIE_DATA_TYPE, p.UIE_DATA_TYPE_ERR_MSG, p.UIE_FORMAT_STRING,
			p.UIE_ALLOW_UNDECLARED, p.UIE_PARENT_ID, UIE_LEVEL + 1
		FROM
			splx.SPLX_UI_ELEMENTS p
				INNER JOIN
			CHILD_ELEMENTS c ON p.UIE_PARENT_ID = c.SPLX_UI_ELEMENT_ID
	)
	INSERT INTO #UIEV
		SELECT
			SPLX_UI_ELEMENT_ID, UIE_NAME, UIE_CONTROL_TYPE,
			UIE_DESC, UIE_DESC_TOOLTIP, UIE_UNIQUE_NAME,
			UIE_DATA_TYPE, UIE_DATA_TYPE_ERR_MSG, UIE_FORMAT_STRING,
			UIE_ALLOW_UNDECLARED, UIE_PARENT_ID, 0 AS UIE_LEVEL
		FROM
			splx.SPLX_UI_ELEMENTS
		WHERE
			SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID

		UNION ALL

		SELECT
			SPLX_UI_ELEMENT_ID, UIE_NAME, UIE_CONTROL_TYPE,
			UIE_DESC, UIE_DESC_TOOLTIP, UIE_UNIQUE_NAME,
			UIE_DATA_TYPE, UIE_DATA_TYPE_ERR_MSG, UIE_FORMAT_STRING,
			UIE_ALLOW_UNDECLARED, UIE_PARENT_ID, UIE_LEVEL
		FROM
			CHILD_ELEMENTS


--	just for debugging:
--	SELECT * FROM #UIEV

	CREATE TABLE #VR (
		[SPLX_VALIDATION_RULE_ID] [uniqueidentifier] NOT NULL,
		[VR_NAME] [varchar](50) NULL,
		[VR_EVENT_BINDING] [varchar](50) NULL,
		[VR_COMPARE_VALUE1] [varchar](255) NULL,
		[VR_EXPRESSION_TYPE1] [varchar](50) NULL,
		[VR_VALUE_TYPE1] [varchar](50) NULL,
		[VR_COMPARE_VALUE2] [varchar](255) NULL,
		[VR_EXPRESSION_TYPE2] [varchar](50) NULL,
		[VR_VALUE_TYPE2] [varchar](50) NULL,
		[VR_COMPARE_DATA_TYPE] [varchar](50) NULL,
		[VR_OPERATOR] [varchar](50) NULL,
		[VR_ERROR_MESSAGE] [varchar](255) NULL,
		[VR_FAIL_PARENT] [bit] NOT NULL,
		[VR_PARENT_ID] [uniqueidentifier] NULL,
		[SPLX_UI_ELEMENT_ID] [uniqueidentifier] NOT NULL,
		[VR_SORT_ORDER] [int] NOT NULL
	);

	--select validation_rules into temp table
	INSERT INTO #VR
		SELECT
			SPLX_VALIDATION_RULE_ID,
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
			vr.SPLX_UI_ELEMENT_ID,
			VR_SORT_ORDER
		FROM
			splx.SPLX_VALIDATION_RULES vr
				INNER JOIN
			#UIEV ON vr.SPLX_UI_ELEMENT_ID = #UIEV.SPLX_UI_ELEMENT_ID


	CREATE TABLE #FME (
		[SPLX_FILLMAP_EXPRESSION_ID] [int] NOT NULL,
		[FME_NAME] [varchar](50) NULL,
		[FME_EVENT_BINDING] [varchar](50) NULL,
		[FME_EXPRESSION] [varchar](1000) NULL,
		[FME_EXPRESSION_TYPE] [varchar](50) NULL,
		[FME_SORT_ORDER] [int] NOT NULL,
		[SPLX_UIE_VR_PARENT_ID] [uniqueidentifier] NOT NULL
	);

	INSERT INTO #FME
		SELECT
			SPLX_FILLMAP_EXPRESSION_ID,
			FME_NAME,
			FME_EVENT_BINDING,
			FME_EXPRESSION,
			FME_EXPRESSION_TYPE,
			FME_SORT_ORDER,
			SPLX_UIE_VR_PARENT_ID
		FROM
			splx.SPLX_FILLMAP_EXPRESSIONS fme
				INNER JOIN
			#UIEV ON fme.SPLX_UIE_VR_PARENT_ID = #UIEV.SPLX_UI_ELEMENT_ID

		UNION ALL

		SELECT
			SPLX_FILLMAP_EXPRESSION_ID,
			FME_NAME,
			FME_EVENT_BINDING,
			FME_EXPRESSION,
			FME_EXPRESSION_TYPE,
			FME_SORT_ORDER,
			SPLX_UIE_VR_PARENT_ID
		FROM
			splx.SPLX_FILLMAP_EXPRESSIONS fme
				INNER JOIN
			#VR ON fme.SPLX_UIE_VR_PARENT_ID = #VR.SPLX_VALIDATION_RULE_ID



	--select ui_elements
	SELECT
		SPLX_UI_ELEMENT_ID, UIE_NAME, UIE_CONTROL_TYPE,
		UIE_DESC, UIE_DESC_TOOLTIP, UIE_UNIQUE_NAME,
		UIE_DATA_TYPE, UIE_DATA_TYPE_ERR_MSG, UIE_FORMAT_STRING,
		UIE_ALLOW_UNDECLARED, UIE_PARENT_ID, UIE_LEVEL
	FROM
		#UIEV


	--select validation_rules
	SELECT
		SPLX_VALIDATION_RULE_ID, VR_NAME, VR_EVENT_BINDING,
		VR_COMPARE_VALUE1, VR_EXPRESSION_TYPE1, VR_VALUE_TYPE1,
		VR_COMPARE_VALUE2, VR_EXPRESSION_TYPE2, VR_VALUE_TYPE2,
		VR_COMPARE_DATA_TYPE, VR_OPERATOR, VR_ERROR_MESSAGE,
		VR_FAIL_PARENT, VR_PARENT_ID, SPLX_UI_ELEMENT_ID,
		VR_SORT_ORDER
	FROM
		#VR
	ORDER BY
		SPLX_UI_ELEMENT_ID, VR_SORT_ORDER


	--select fillmap_expressions
	SELECT
		SPLX_FILLMAP_EXPRESSION_ID, FME_NAME, FME_EVENT_BINDING,
		FME_EXPRESSION, FME_EXPRESSION_TYPE, FME_SORT_ORDER,
		SPLX_UIE_VR_PARENT_ID
	FROM
		#FME
	ORDER BY
		SPLX_UIE_VR_PARENT_ID, FME_SORT_ORDER


	--select fillmap_databindings
	SELECT
		SPLX_FILLMAP_DATABINDING_ID,
		FMB_UIE_UNIQUE_NAME,
		FMB_PROPERTY_NAME,
		FMB_VALUE,
		FMB_TYPECAST_VALUE,
		FMB_OVERRIDE_VALUE,
		fmb.SPLX_FILLMAP_EXPRESSION_ID
	FROM
		splx.SPLX_FILLMAP_DATABINDINGS fmb
				INNER JOIN
		#FME ON fmb.SPLX_FILLMAP_EXPRESSION_ID = #FME.SPLX_FILLMAP_EXPRESSION_ID
	ORDER BY
		fmb.SPLX_FILLMAP_EXPRESSION_ID


	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#FME',N'U') IS NOT NULL
		DROP TABLE #FME

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#VR',N'U') IS NOT NULL
		DROP TABLE #VR

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#UIEV',N'U') IS NOT NULL
		DROP TABLE #UIEV


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		'UIElements,ValidationRules,FillMapExpressions,FillMapDataBindings' AS Tables

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_fmbyparent_composite]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_sel_fmbyparent_composite]

	@SPLX_UIE_VR_PARENT_ID	uniqueidentifier

AS
BEGIN

	CREATE TABLE #fme (
		[SPLX_FILLMAP_EXPRESSION_ID] [int] NOT NULL,
		[FME_NAME] [varchar](50) NULL,
		[FME_EVENT_BINDING] [varchar](50) NULL,
		[FME_EXPRESSION] [varchar](1000) NULL,
		[FME_EXPRESSION_TYPE] [varchar](50) NULL,
		[FME_IF_CLAUSE] [bit] NOT NULL,
		[FME_SORT_ORDER] [int] NOT NULL,
		[SPLX_UIE_VR_PARENT_ID] [uniqueidentifier] NOT NULL,
	);

	INSERT INTO #fme
		SELECT
			*
		FROM
			splx.SPLX_FILLMAP_EXPRESSIONS
		WHERE
			SPLX_UIE_VR_PARENT_ID = @SPLX_UIE_VR_PARENT_ID
		ORDER BY
			FME_SORT_ORDER


	SELECT * FROM #fme


	SELECT
		*
	FROM
		splx.SPLX_FILLMAP_DATABINDINGS
	WHERE
		SPLX_FILLMAP_EXPRESSION_ID IN
		(
			SELECT SPLX_FILLMAP_EXPRESSION_ID FROM #fme
		)


	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#fme',N'U') IS NOT NULL
		DROP TABLE #fme


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		'FillMaps,DataBindings' AS Tables
END
GO
/****** Object:  StoredProcedure [splx].[splx_api_sel_fmbyid_composite]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_sel_fmbyid_composite]

	@SPLX_FILLMAP_EXPRESSION_ID	int

AS
BEGIN

	SELECT
		*
	FROM
		splx.SPLX_FILLMAP_EXPRESSIONS
	WHERE
		SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID

	SELECT
		*
	FROM
		splx.SPLX_FILLMAP_DATABINDINGS
	WHERE
		SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID


	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		'FillMaps,DataBindings' AS Tables
END
GO
/****** Object:  StoredProcedure [splx].[splx_api_del_vr]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_del_vr]

	@SPLX_VALIDATION_RULE_ID	uniqueidentifier

AS
BEGIN

	CREATE TABLE #VR_LIST (
			SPLX_VALIDATION_RULE_ID [uniqueidentifier] NOT NULL
	);

	CREATE TABLE #FME_LIST (
			SPLX_FILLMAP_EXPRESSION_ID [int] NOT NULL
	);

	--- VR_LIST --------------------------------------------
	WITH VR_LIST( SPLX_VALIDATION_RULE_ID )
	AS
	(
		SELECT
			SPLX_VALIDATION_RULE_ID
		FROM splx.SPLX_VALIDATION_RULES
		WHERE
			SPLX_VALIDATION_RULE_ID = @SPLX_VALIDATION_RULE_ID
			
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


	--- FME_LIST -------------------------------------------
	INSERT INTO #FME_LIST
		SELECT
			fme.SPLX_FILLMAP_EXPRESSION_ID
		FROM
			splx.SPLX_FILLMAP_EXPRESSIONS fme
			INNER JOIN
			#VR_LIST ON fme.SPLX_UIE_VR_PARENT_ID = #VR_LIST.SPLX_VALIDATION_RULE_ID
	--- FME_LIST -------------------------------------------


	BEGIN TRANSACTION

	DELETE FROM splx.SPLX_RIGHT_ROLES
		WHERE SPLX_UI_ELEMENT_RULE_ID IN
		(SELECT #VR_LIST.SPLX_VALIDATION_RULE_ID FROM #VR_LIST)

	DELETE FROM splx.SPLX_FILLMAP_DATABINDINGS
		WHERE SPLX_FILLMAP_EXPRESSION_ID IN
		(SELECT #FME_LIST.SPLX_FILLMAP_EXPRESSION_ID FROM #FME_LIST)

	DELETE FROM splx.SPLX_FILLMAP_EXPRESSIONS
		WHERE SPLX_FILLMAP_EXPRESSION_ID IN
		(SELECT #FME_LIST.SPLX_FILLMAP_EXPRESSION_ID FROM #FME_LIST)

	DELETE FROM splx.SPLX_VALIDATION_RULES
		WHERE SPLX_VALIDATION_RULE_ID IN
		(SELECT #VR_LIST.SPLX_VALIDATION_RULE_ID FROM #VR_LIST)

	COMMIT TRANSACTION


	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#VR_LIST',N'U') IS NOT NULL
		DROP TABLE #VR_LIST

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#UIE_LIST',N'U') IS NOT NULL
		DROP TABLE #UIE_LIST

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_del_uie]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_del_uie]

	@SPLX_UI_ELEMENT_ID	uniqueidentifier

AS
BEGIN

	CREATE TABLE #UIE_LIST (
			[SPLX_UI_ELEMENT_ID] [uniqueidentifier] NOT NULL
	);

	CREATE TABLE #VR_LIST (
			SPLX_VALIDATION_RULE_ID [uniqueidentifier] NOT NULL
	);

	CREATE TABLE #FME_LIST (
			SPLX_FILLMAP_EXPRESSION_ID [int] NOT NULL
	);

	--- UIE_LIST -------------------------------------------
	WITH UIE_LIST( SPLX_UI_ELEMENT_ID )
	AS
	(
		SELECT
			SPLX_UI_ELEMENT_ID
		FROM splx.SPLX_UI_ELEMENTS
		WHERE
			SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID
			
		UNION ALL
		
		SELECT
			u.SPLX_UI_ELEMENT_ID
		FROM splx.SPLX_UI_ELEMENTS u
		INNER JOIN UIE_LIST
			ON u.UIE_PARENT_ID = UIE_LIST.SPLX_UI_ELEMENT_ID
	)
	INSERT INTO #UIE_LIST
		SELECT * FROM UIE_LIST;
	--- UIE_LIST -------------------------------------------


	--- VR_LIST --------------------------------------------
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


	--- FME_LIST -------------------------------------------
	INSERT INTO #FME_LIST
		SELECT
			fme.SPLX_FILLMAP_EXPRESSION_ID
		FROM
			splx.SPLX_FILLMAP_EXPRESSIONS fme
			INNER JOIN
			#UIE_LIST ON fme.SPLX_UIE_VR_PARENT_ID = #UIE_LIST.SPLX_UI_ELEMENT_ID

	INSERT INTO #FME_LIST
		SELECT
			fme.SPLX_FILLMAP_EXPRESSION_ID
		FROM
			splx.SPLX_FILLMAP_EXPRESSIONS fme
			INNER JOIN
			#VR_LIST ON fme.SPLX_UIE_VR_PARENT_ID = #VR_LIST.SPLX_VALIDATION_RULE_ID
	--- FME_LIST -------------------------------------------


	BEGIN TRANSACTION

	DELETE FROM splx.SPLX_ACES
		WHERE SPLX_UI_ELEMENT_ID IN
		(SELECT #UIE_LIST.SPLX_UI_ELEMENT_ID FROM #UIE_LIST)

	DELETE FROM splx.SPLX_RIGHT_ROLES
		WHERE SPLX_UI_ELEMENT_RULE_ID IN
		(SELECT #UIE_LIST.SPLX_UI_ELEMENT_ID FROM #UIE_LIST)

	DELETE FROM splx.SPLX_RIGHT_ROLES
		WHERE SPLX_UI_ELEMENT_RULE_ID IN
		(SELECT #VR_LIST.SPLX_VALIDATION_RULE_ID FROM #VR_LIST)

	DELETE FROM splx.SPLX_FILLMAP_DATABINDINGS
		WHERE SPLX_FILLMAP_EXPRESSION_ID IN
		(SELECT #FME_LIST.SPLX_FILLMAP_EXPRESSION_ID FROM #FME_LIST)

	DELETE FROM splx.SPLX_FILLMAP_EXPRESSIONS
		WHERE SPLX_FILLMAP_EXPRESSION_ID IN
		(SELECT #FME_LIST.SPLX_FILLMAP_EXPRESSION_ID FROM #FME_LIST)

	DELETE FROM splx.SPLX_VALIDATION_RULES
		WHERE SPLX_VALIDATION_RULE_ID IN
		(SELECT #VR_LIST.SPLX_VALIDATION_RULE_ID FROM #VR_LIST)

	DELETE FROM splx.SPLX_UI_ELEMENTS
		WHERE SPLX_UI_ELEMENT_ID IN
		(SELECT #UIE_LIST.SPLX_UI_ELEMENT_ID FROM #UIE_LIST)

	COMMIT TRANSACTION


	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#FME_LIST',N'U') IS NOT NULL
		DROP TABLE #FME_LIST

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#VR_LIST',N'U') IS NOT NULL
		DROP TABLE #VR_LIST

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#UIE_LIST',N'U') IS NOT NULL
		DROP TABLE #UIE_LIST

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_del_fme]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_del_fme]

	@SPLX_FILLMAP_EXPRESSION_ID	int

AS
BEGIN

	DELETE
	FROM
		splx.SPLX_FILLMAP_DATABINDINGS
	WHERE
		SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID

	DELETE
	FROM
		splx.SPLX_FILLMAP_EXPRESSIONS
	WHERE
		SPLX_FILLMAP_EXPRESSION_ID = @SPLX_FILLMAP_EXPRESSION_ID

END
GO
/****** Object:  StoredProcedure [splx].[splx_api_del_fmb]    Script Date: 07/20/2013 23:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [splx].[splx_api_del_fmb]

	@SPLX_FILLMAP_DATABINDING_ID	int

AS
BEGIN

	DELETE
	FROM
		splx.SPLX_FILLMAP_DATABINDINGS
	WHERE
		SPLX_FILLMAP_DATABINDING_ID = @SPLX_FILLMAP_DATABINDING_ID

END
GO
