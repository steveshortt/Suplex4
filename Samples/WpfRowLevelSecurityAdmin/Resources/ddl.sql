USE [Suplex_Example]
GO
/****** Object:  ForeignKey [FK_FooData_FooLookup]    Script Date: 11/07/2012 19:01:19 ******/
ALTER TABLE [dbo].[FooData] DROP CONSTRAINT [FK_FooData_FooLookup]
GO
/****** Object:  StoredProcedure [dbo].[del_foodata_trunc]    Script Date: 11/07/2012 19:01:20 ******/
DROP PROCEDURE [dbo].[del_foodata_trunc]
GO
/****** Object:  StoredProcedure [dbo].[ins_foodata]    Script Date: 11/07/2012 19:01:20 ******/
DROP PROCEDURE [dbo].[ins_foodata]
GO
/****** Object:  StoredProcedure [dbo].[sel_foodata_by_lookup_nostage_nomask]    Script Date: 11/07/2012 19:01:20 ******/
DROP PROCEDURE [dbo].[sel_foodata_by_lookup_nostage_nomask]
GO
/****** Object:  StoredProcedure [dbo].[sel_foodata_by_lookup_nostage_withmask]    Script Date: 11/07/2012 19:01:20 ******/
DROP PROCEDURE [dbo].[sel_foodata_by_lookup_nostage_withmask]
GO
/****** Object:  StoredProcedure [dbo].[sel_foodata_by_lookup_withstage_withmask]    Script Date: 11/07/2012 19:01:20 ******/
DROP PROCEDURE [dbo].[sel_foodata_by_lookup_withstage_withmask]
GO
/****** Object:  StoredProcedure [dbo].[sel_foodata_nomask]    Script Date: 11/07/2012 19:01:20 ******/
DROP PROCEDURE [dbo].[sel_foodata_nomask]
GO
/****** Object:  StoredProcedure [dbo].[sel_foodata_nomask_tvp]    Script Date: 11/07/2012 19:01:20 ******/
DROP PROCEDURE [dbo].[sel_foodata_nomask_tvp]
GO
/****** Object:  StoredProcedure [dbo].[sel_foodata_withmask]    Script Date: 11/07/2012 19:01:20 ******/
DROP PROCEDURE [dbo].[sel_foodata_withmask]
GO
/****** Object:  StoredProcedure [dbo].[sel_foolookup_row_permissions]    Script Date: 11/07/2012 19:01:20 ******/
DROP PROCEDURE [dbo].[sel_foolookup_row_permissions]
GO
/****** Object:  StoredProcedure [dbo].[upd_foolookup_row_permissions]    Script Date: 11/07/2012 19:01:20 ******/
DROP PROCEDURE [dbo].[upd_foolookup_row_permissions]
GO
/****** Object:  Table [dbo].[FooData]    Script Date: 11/07/2012 19:01:19 ******/
--ALTER TABLE [dbo].[FooData] DROP CONSTRAINT [FK_FooData_FooLookup]
--GO
DROP TABLE [dbo].[FooData]
GO
/****** Object:  UserDefinedTableType [dbo].[FooData_Tvp]    Script Date: 11/07/2012 19:01:20 ******/
DROP TYPE [dbo].[FooData_Tvp]
GO
/****** Object:  Table [dbo].[FooLookup]    Script Date: 11/07/2012 19:01:19 ******/
DROP TABLE [dbo].[FooLookup]
GO
/****** Object:  Table [dbo].[FooLookup]    Script Date: 11/07/2012 19:01:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[FooLookup](
	[foo_lookup_id] [int] IDENTITY(1,1) NOT NULL,
	[rls_mask] [binary](__masksize__) NULL,
	[lookup_data] [varchar](50) NULL,
 CONSTRAINT [PK_FooLookup] PRIMARY KEY CLUSTERED 
(
	[foo_lookup_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  UserDefinedTableType [dbo].[FooData_Tvp]    Script Date: 11/07/2012 19:01:20 ******/
CREATE TYPE [dbo].[FooData_Tvp] AS TABLE(
	[foo_id] [int] NULL,
	[rls_mask] [binary](__masksize__) NULL,
	[data] [varchar](50) NULL,
	[foo_lookup_id] [int] NULL
)
GO
/****** Object:  Table [dbo].[FooData]    Script Date: 11/07/2012 19:01:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[FooData](
	[foo_id] [int] IDENTITY(1,1) NOT NULL,
	[rls_mask] [binary](__masksize__) NULL,
	[data] [varchar](50) NULL,
	[foo_lookup_id] [int] NULL,
 CONSTRAINT [PK_FooData] PRIMARY KEY CLUSTERED 
(
	[foo_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  StoredProcedure [dbo].[upd_foolookup_row_permissions]    Script Date: 11/07/2012 19:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		steve shortt
-- Create date: 11/14/2010
-- Description:	Update FooLookup Row Permissions
-- =============================================
CREATE PROCEDURE [dbo].[upd_foolookup_row_permissions]

	@foo_lookup_id	int
	,@rls_mask binary(__masksize__)

AS
BEGIN

	UPDATE [FooLookup]
	   SET [rls_mask] = @rls_mask
	 WHERE foo_lookup_id = @foo_lookup_id
 
END
GO
/****** Object:  StoredProcedure [dbo].[sel_foolookup_row_permissions]    Script Date: 11/07/2012 19:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		steve shortt
-- Create date: 11/14/2010
-- Description:	Select FooLookup Row Permissions
-- =============================================
CREATE PROCEDURE [dbo].[sel_foolookup_row_permissions]

	@foo_lookup_id int

AS
BEGIN

	DECLARE @rls_mask binary(__masksize__)
	SELECT @rls_mask = rls_Mask FROM FooLookup where foo_lookup_id = @foo_lookup_id

	if ISNULL(@rls_mask, NULL) = @rls_mask
	BEGIN
		CREATE TABLE #gm (
			[SPLX_GROUP_ID] [uniqueidentifier] NOT NULL
			,[GROUP_NAME] varchar(300) NOT NULL
			,[GROUP_ENABLED] bit NOT NULL
			,[GROUP_MASK] [binary](__masksize__) NOT NULL
		);

		INSERT INTO #gm	
			SELECT
				[SPLX_GROUP_ID]
				,[GROUP_NAME]
				,[GROUP_ENABLED]
				,[GROUP_MASK]
				--,cast(group_mask as varchar(__masksize__))
			FROM
				[splx].splx_groups
			WHERE
				splx.Splx_ContainsOne(group_mask, @rls_mask) > 0
				
		SELECT *
		FROM #gm
		ORDER BY
			[GROUP_NAME]


		SELECT
			[SPLX_GROUP_ID]
			,[GROUP_NAME]
			,[GROUP_ENABLED]
			,[GROUP_MASK]
			--,cast(group_mask as varchar(__masksize__))
		FROM
			[splx].splx_groups
		WHERE
			[SPLX_GROUP_ID] NOT IN
				(SELECT [SPLX_GROUP_ID] FROM #gm)
		ORDER BY
			[GROUP_NAME]


		--done with temp table, drop it
		IF OBJECT_ID(N'tempdb..#gm',N'U') IS NOT NULL
			DROP TABLE #gm
	END
	
	else
	BEGIN
			SELECT
				[SPLX_GROUP_ID]
				,[GROUP_NAME]
				,[GROUP_ENABLED]
				,[GROUP_MASK]
			FROM
				[splx].splx_groups
			WHERE
				[SPLX_GROUP_ID] = null
				
			SELECT
				[SPLX_GROUP_ID]
				,[GROUP_NAME]
				,[GROUP_ENABLED]
				,[GROUP_MASK]
			FROM
				[splx].splx_groups
			ORDER BY
				[GROUP_NAME]
	END

	--IMPORTANT:
	--	arrange this in order of the tables above so the UI layer can match to the DataSet.Tables collection
	SELECT
		'GroupMembers,GroupNonMembers' AS Tables
END
GO
/****** Object:  StoredProcedure [dbo].[sel_foodata_withmask]    Script Date: 11/07/2012 19:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		steve shortt
-- Create date: 11/14/2010
-- Description:	Select FooData Norm
-- =============================================
CREATE PROCEDURE [dbo].[sel_foodata_withmask]

	--@rls_mask binary(__masksize__)
	@rls_mask bigint

AS
BEGIN

	SELECT
		[foo_id]
		,[rls_mask]
		,[data]
		,[foo_lookup_id]
	FROM
		FooData
	WHERE
		--splx.Splx_ContainsOne(rls_mask, @rls_mask) > 0
		rls_mask & @rls_mask > 0

END
GO
/****** Object:  StoredProcedure [dbo].[sel_foodata_nomask_tvp]    Script Date: 11/07/2012 19:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		steve shortt
-- Create date: 11/06/2012
-- Description:	Select FooData Norm
-- =============================================
create PROCEDURE [dbo].[sel_foodata_nomask_tvp]

	@TVP FooData_Tvp READONLY

AS
BEGIN

	SELECT
		fd.[foo_id]
		,fd.[rls_mask]
		,fd.[data]
		,fd.[foo_lookup_id]
	FROM
		FooData fd
	INNER JOIN @TVP tvp ON fd.foo_id = tvp.foo_id

END
GO
/****** Object:  StoredProcedure [dbo].[sel_foodata_nomask]    Script Date: 11/07/2012 19:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		steve shortt
-- Create date: 11/14/2010
-- Description:	Select FooData Norm
-- =============================================
CREATE PROCEDURE [dbo].[sel_foodata_nomask]

	@topN int

AS
BEGIN

	SELECT TOP(@topN)
		[foo_id]
		,[rls_mask]
		--,[data]
		,[foo_lookup_id]
	FROM
		FooData

END
GO
/****** Object:  StoredProcedure [dbo].[sel_foodata_by_lookup_withstage_withmask]    Script Date: 11/07/2012 19:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		steve shortt
-- Create date: 11/17/2010
-- Description:	Select FooData Norm
-- =============================================
CREATE PROCEDURE [dbo].[sel_foodata_by_lookup_withstage_withmask]

	@foo_lookup_id_list	varchar(50)
	,@rls_mask binary(__masksize__)

AS
BEGIN

	CREATE TABLE #fl (
		foo_lookup_id int NOT NULL
	);

	INSERT INTO #fl
		SELECT
			fl.foo_lookup_id
		FROM
			FooLookup fl
		WHERE
			fl.foo_lookup_id IN
				(SELECT value from splx.ap_Split( @foo_lookup_id_list, ','))
			AND
			splx.Splx_ContainsOne(fl.rls_mask, @rls_mask) > 0


	SELECT
		fd.foo_id
		,fd.foo_lookup_id
	FROM
		FooData fd
	WHERE
		fd.foo_lookup_id IN
			(SELECT foo_lookup_id from #fl)

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#fl',N'U') IS NOT NULL
		DROP TABLE #fl
END
GO
/****** Object:  StoredProcedure [dbo].[sel_foodata_by_lookup_nostage_withmask]    Script Date: 11/07/2012 19:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		steve shortt
-- Create date: 11/17/2010
-- Description:	Select FooData Norm
-- =============================================
CREATE PROCEDURE [dbo].[sel_foodata_by_lookup_nostage_withmask]

	@foo_lookup_id_list	varchar(50)
	,@rls_mask binary(__masksize__)

AS
BEGIN

	SELECT
		fd.foo_id
		,fd.foo_lookup_id
	FROM
		FooData fd INNER JOIN FooLookup fl ON fd.foo_lookup_id = fl.foo_lookup_id
	WHERE
		fl.foo_lookup_id IN
			(SELECT value from splx.ap_Split( @foo_lookup_id_list, ','))
		AND
		splx.Splx_ContainsOne(fl.rls_mask, @rls_mask) > 0

END
GO
/****** Object:  StoredProcedure [dbo].[sel_foodata_by_lookup_nostage_nomask]    Script Date: 11/07/2012 19:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		steve shortt
-- Create date: 11/17/2010
-- Description:	Select FooData Norm
-- =============================================
CREATE PROCEDURE [dbo].[sel_foodata_by_lookup_nostage_nomask]

	@foo_lookup_id_list	varchar(50)

AS
BEGIN

	SELECT
		fd.foo_id
		,fd.foo_lookup_id
	FROM
		FooData fd INNER JOIN FooLookup fl ON fd.foo_lookup_id = fl.foo_lookup_id
	WHERE
		fl.foo_lookup_id IN
			(SELECT value from splx.ap_Split( @foo_lookup_id_list, ','))

END
GO
/****** Object:  StoredProcedure [dbo].[ins_foodata]    Script Date: 11/07/2012 19:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		steve shortt
-- Create date: 11/14/2010
-- Description:	Insert FooData
-- =============================================
CREATE PROCEDURE [dbo].[ins_foodata]

	@foo_id	int = null output
	,@rls_mask	binary(__masksize__)
	,@data	varchar(50)
	,@foo_lookup_id	int

AS
BEGIN

	INSERT INTO FooData
		(
		[rls_mask]
		,[data]
		,[foo_lookup_id]
		)
		VALUES
		(
		@rls_mask
		,@data
		,@foo_lookup_id
		)


	SELECT @foo_id = SCOPE_IDENTITY()

END
GO
/****** Object:  StoredProcedure [dbo].[del_foodata_trunc]    Script Date: 11/07/2012 19:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		steve shortt
-- Create date: 11/14/2010
-- Description:	truncate foodata
-- =============================================
CREATE PROCEDURE [dbo].[del_foodata_trunc]

AS
BEGIN

	truncate table foodata

END
GO
/****** Object:  ForeignKey [FK_FooData_FooLookup]    Script Date: 11/07/2012 19:01:19 ******/
ALTER TABLE [dbo].[FooData]  WITH CHECK ADD  CONSTRAINT [FK_FooData_FooLookup] FOREIGN KEY([foo_lookup_id])
REFERENCES [dbo].[FooLookup] ([foo_lookup_id])
GO
ALTER TABLE [dbo].[FooData] CHECK CONSTRAINT [FK_FooData_FooLookup]
GO
SET IDENTITY_INSERT [dbo].[FooLookup] ON
INSERT [dbo].[FooLookup] ([foo_lookup_id], [rls_mask], [lookup_data]) VALUES (1, 0x05, N'North')
INSERT [dbo].[FooLookup] ([foo_lookup_id], [rls_mask], [lookup_data]) VALUES (2, 0x30, N'East')
INSERT [dbo].[FooLookup] ([foo_lookup_id], [rls_mask], [lookup_data]) VALUES (3, 0xA0, N'South')
INSERT [dbo].[FooLookup] ([foo_lookup_id], [rls_mask], [lookup_data]) VALUES (4, 0x07, N'West')
INSERT [dbo].[FooLookup] ([foo_lookup_id], [rls_mask], [lookup_data]) VALUES (5, 0x05, N'Left')
INSERT [dbo].[FooLookup] ([foo_lookup_id], [rls_mask], [lookup_data]) VALUES (6, 0x52, N'Top')
INSERT [dbo].[FooLookup] ([foo_lookup_id], [rls_mask], [lookup_data]) VALUES (7, 0x8A, N'Right')
INSERT [dbo].[FooLookup] ([foo_lookup_id], [rls_mask], [lookup_data]) VALUES (8, 0x07, N'Bottom')
INSERT [dbo].[FooLookup] ([foo_lookup_id], [rls_mask], [lookup_data]) VALUES (9, 0x02, N'Inside')
INSERT [dbo].[FooLookup] ([foo_lookup_id], [rls_mask], [lookup_data]) VALUES (10, 0x8B, N'Outside')
SET IDENTITY_INSERT [dbo].[FooLookup] OFF
GO
