SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 09/15/2007
-- Modified:	05/10/2016, expanding default group bitmask
-- Description:	Select Group Membership by User_ID
-- =============================================
ALTER PROCEDURE [splx].[splx_api_upsert_group]

	@SPLX_GROUP_ID			uniqueidentifier = NULL OUTPUT
	,@GROUP_NAME			varchar(300) OUTPUT
	,@GROUP_DESC			varchar(255)
	,@GROUP_LOCAL			bit
	,@GROUP_ENABLED			bit
	,@GROUP_BUILT_IN		bit = 0
	,@GROUP_MASK			binary(256)

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
