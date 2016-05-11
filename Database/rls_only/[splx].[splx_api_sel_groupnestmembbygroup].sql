SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 10/16/2007
-- Modified:	10/07/2012, switched @includeDisabled default from 0 to 1
-- Modified:	05/10/2016, expanding default group bitmask
-- Description:	Select Group Membership by Group_ID
-- =============================================
ALTER PROCEDURE [splx].[splx_api_sel_groupnestmembbygroup]

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
		,[GROUP_MASK] [binary](256) NOT NULL
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
