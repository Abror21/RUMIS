CREATE PROCEDURE [dbo].[Generate_UpdateModifTable]
	@TableName varchar(128),
	@Owner varchar(128) = 'dbo',
	@AuditNameExtention varchar(128) = '_Modif',
	@DropAuditTable bit = 0
AS
-- 2019-03-12 | Dmitrijs | Globālas izmaiņās, tagad metode ne tikai pārģenerē trigerus, bet ari pieliec/izmaina/dzēš modif tabulas kolonas ja galvena tabula bija mainīta
BEGIN
	DECLARE @MajorVersion nvarchar(max)
	SET @MajorVersion = ''

	DECLARE @ModifIdInitColumnName nvarchar(max) = '_Id'
	DECLARE @ModifActionInitColumnName nvarchar(max) = '_Action'
	DECLARE @ModifUserInitColumnName nvarchar(max) = '_Author'
	DECLARE @ModifDateInitColumnName nvarchar(max) = '_Date'

	SELECT @MajorVersion = 
		CASE 
			WHEN CONVERT(VARCHAR(128), SERVERPROPERTY('productversion')) like '8%' THEN 'NOT OK'
			WHEN CONVERT(VARCHAR(128), SERVERPROPERTY('productversion')) like '9%' THEN 'NOT OK'
			WHEN CONVERT(VARCHAR(128), SERVERPROPERTY('productversion')) like '10.0%' THEN 'NOT OK'
			WHEN CONVERT(VARCHAR(128), SERVERPROPERTY('productversion')) like '10.5%' THEN 'NOT OK'
			ELSE 'OK'
		END

	-- Check if table exists
	IF not exists (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[' + @Owner + '].[' + @TableName + ']') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	BEGIN
		PRINT 'ERROR: Table ' + @TableName + ' does not exist'
		RETURN
	END

	-- Drop audit table if it exists and drop should be forced
	IF (exists (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[' + @Owner + '].[' + @TableName + @AuditNameExtention + ']') and OBJECTPROPERTY(id, N'IsUserTable') = 1) and @DropAuditTable = 1)
	BEGIN
		PRINT 'Dropping audit table [' + @Owner + '].[' + @TableName + @AuditNameExtention + ']'
		EXEC ('drop table ' + @TableName + @AuditNameExtention)
	END

	-- IF SQL MajorVersion >= 2012
	DECLARE @MainDatatype nvarchar(max)
	DECLARE @ModifDatatype nvarchar(max)
	DECLARE @MainCollation nvarchar(max)
	DECLARE @ModifCollation nvarchar(max)

	-- IF SQL MajorVersion < 2012
	DECLARE @MainType nvarchar(max)
	DECLARE @ModifType nvarchar(max)
	DECLARE @MainLength smallint
	DECLARE @ModifLength smallint
	DECLARE @MainPrecision tinyint
	DECLARE @ModifPrecision tinyint
	DECLARE @MainScale tinyint
	DECLARE @ModifScale tinyint

	-- Visam versijām
	DECLARE @MainColumnName nvarchar(max)
	DECLARE @ModifColumnName nvarchar(max)
	DECLARE @MainIs_nullable nvarchar(max)
	DECLARE @ModifIs_nullable nvarchar(max)

	DECLARE @AlterStatement varchar(max) = ''
	DECLARE @CreateStatement varchar(max) = ''
	DECLARE @ListOfFields varchar(max) = ''
	DECLARE @ListOfFieldsNoModif varchar(max) = ''
	DECLARE @ListOfFieldsNoModif_D varchar(max) = ''
	DECLARE @ListOfFieldsModif varchar(max) = ''

	DECLARE @DeleteUserIdSql varchar(max) = 'NULL'

	IF exists (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[' + @Owner + '].[' + @TableName + @AuditNameExtention + ']') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	BEGIN
		IF (@MajorVersion = 'OK')
		BEGIN
			DECLARE TableColumnsCompare CURSOR Read_Only
			FOR 
			SELECT MAIN.name as MAIN_ColumnName,
				MODIF.name as MODIF_ColumnName,
				'NULL' AS MAIN_is_nullable,
				'NULL' AS MODIF_is_nullable,
				MAIN.system_type_name as MAIN_Datatype,
				MODIF.system_type_name as MODIF_Datatype,
				MAIN.collation_name as MAIN_Collation,
				MODIF.collation_name as MODIF_Collation
			FROM sys.dm_exec_describe_first_result_SET (N'SELECT * FROM [' + @Owner + '].[' + @TableName + ']', NULL, 0) MAIN 
				FULL OUTER JOIN sys.dm_exec_describe_first_result_SET (N'SELECT * FROM [' + @Owner + '].[' + @TableName + @AuditNameExtention + ']', NULL, 0) MODIF 
				ON MAIN.name = MODIF.name

			OPEN TableColumnsCompare

			FETCH Next FROM TableColumnsCompare
			INTO @MainColumnName, @ModifColumnName, @MainIs_nullable, @ModifIs_nullable, @MainDatatype, @ModifDatatype, @MainCollation, @ModifCollation

			WHILE @@FETCH_STATUS = 0
			BEGIN
				IF (@MainColumnName = @ModifDateInitColumnName)
				BEGIN
					SET @ListOfFieldsNoModif_D = @ListOfFieldsNoModif + ' getdate(),'
					SET @ListOfFieldsNoModif = @ListOfFieldsNoModif + ' CASE WHEN ' + @ModifDateInitColumnName+'=''1900-01-01'' or ' 
						+ @ModifDateInitColumnName + ' is null then getdate() else ' + @ModifDateInitColumnName + ' end,'
					SET @ListOfFieldsModif = @ListOfFieldsModif + '[' + @MainColumnName + '],'
				END
				ELSE IF (@MainColumnName = @ModifUserInitColumnName)
				BEGIN
					SET @ListOfFieldsNoModif_D = @ListOfFieldsNoModif_D + @DeleteUserIdSql + ' ,'
					SET @ListOfFieldsNoModif = @ListOfFieldsNoModif + ' '+@ModifUserInitColumnName+','
					SET @ListOfFieldsModif = @ListOfFieldsModif + '[' + @MainColumnName + '],'
				END
				ELSE IF isnull(@ModifColumnName, '') NOT IN (@ModifActionInitColumnName, @ModifIdInitColumnName) and @MainColumnName is not null
					SET @ListOfFields = @ListOfFields + '[' + @MainColumnName + '],'
				
				IF (@MainColumnName = @ModifColumnName)
					AND (@MainIs_nullable <> @ModifIs_nullable OR @MainDatatype <> @ModifDatatype OR @MainCollation <> @ModifCollation)
					AND @MainColumnName NOT IN (@ModifDateInitColumnName, @ModifUserInitColumnName)
					SET @AlterStatement = @AlterStatement + ' ALTER TABLE [' + @Owner + '].[' + @TableName + @AuditNameExtention + '] ALTER COLUMN [' + @MainColumnName + '] ' + @MainDatatype + ' ' + @MainIs_nullable + ';'
				ELSE IF (@MainColumnName is null AND @ModifColumnName NOT IN (@ModifActionInitColumnName, @ModifIdInitColumnName, @ModifDateInitColumnName, @ModifUserInitColumnName))
					SET @AlterStatement = @AlterStatement + ' ALTER TABLE [' + @Owner + '].[' + @TableName + @AuditNameExtention + '] DROP COLUMN [' + @ModifColumnName + '];'
				ELSE IF (@ModifColumnName is null)
					SET @AlterStatement = @AlterStatement + ' ALTER TABLE [' + @Owner + '].[' + @TableName + @AuditNameExtention + '] ADD [' + @MainColumnName + '] ' + @MainDatatype + ' ' + @MainIs_nullable + ';'
			
				FETCH Next FROM TableColumnsCompare
				INTO @MainColumnName, @ModifColumnName, @MainIs_nullable, @ModifIs_nullable, @MainDatatype, @ModifDatatype, @MainCollation, @ModifCollation
		
			END

			CLOSE TableColumnsCompare
			DEALLOCATE TableColumnsCompare

			IF (@AlterStatement <> '')
			BEGIN
				PRINT 'Updating Modif table'
				EXEC (@AlterStatement)
			END
		END
		ELSE IF (@MajorVersion = 'NOT OK')
		BEGIN
			DECLARE TableColumnsCompare CURSOR Read_Only
			FOR 
			SELECT MAIN.name as MAIN_ColumnName,
				MODIF.name as MODIF_ColumnName,
				'NULL' AS MAIN_is_nullable,
				'NULL' AS MODIF_is_nullable,
				MAIN.TypeName as MAIN_Type,
				MODIF.TypeName as MODIF_Type,
				MAIN.length as MAIN_Length,
				MODIF.length as MODIF_Length,
				MAIN.xprec as MAIN_Precision,
				MODIF.xprec as MODIF_Precision,
				MAIN.xscale as MAIN_Scale,
				MODIF.xscale as MODIF_Scale,
				MAIN.collation as MAIN_Collation,
				MODIF.collation as MODIF_Collation 
			FROM (
				SELECT b.name, c.name as TypeName, b.length, b.isnullable, b.collation, b.xprec, b.xscale
				FROM sysobjects a
					inner join syscolumns b on a.id = b.id 
					inner join systypes c on b.xtype = c.xtype and c.name <> 'sysname' 
				WHERE a.id = object_id(N'[' + @Owner + '].[' + @TableName + ']') 
					and OBJECTPROPERTY(a.id, N'IsUserTable') = 1
				) as MAIN FULL OUTER JOIN 
				(
				SELECT b.name, c.name as TypeName, b.length, b.isnullable, b.collation, b.xprec, b.xscale
				FROM sysobjects a
					inner join syscolumns b on a.id = b.id 
					inner join systypes c on b.xtype = c.xtype and c.name <> 'sysname' 
				WHERE a.id = object_id(N'[' + @Owner + '].[' + @TableName + @AuditNameExtention + ']') 
					and OBJECTPROPERTY(a.id, N'IsUserTable') = 1
				) as MODIF 
			ON MAIN.name = MODIF.name

			OPEN TableColumnsCompare

			FETCH Next FROM TableColumnsCompare
			INTO @MainColumnName, @ModifColumnName, @MainIs_nullable, @ModifIs_nullable, @MainType, @ModifType, @MainLength, @ModifLength, @MainPrecision, @ModifPrecision, @MainScale, @ModifScale, @MainCollation, @ModifCollation

			WHILE @@FETCH_STATUS = 0
			BEGIN
				IF (@MainColumnName = @ModifDateInitColumnName)
				BEGIN
					SET @ListOfFieldsNoModif_D = @ListOfFieldsNoModif + ' getdate(),'
					SET @ListOfFieldsNoModif = @ListOfFieldsNoModif + ' CASE WHEN '+@ModifDateInitColumnName+'=''1900-01-01'' or '+@ModifDateInitColumnName+' is null then getdate() else '+@ModifDateInitColumnName+' end,'
					SET @ListOfFieldsModif = @ListOfFieldsModif + '[' + @MainColumnName + '],'
				END
				ELSE IF (@MainColumnName = @ModifUserInitColumnName)
				BEGIN
					SET @ListOfFieldsNoModif_D = @ListOfFieldsNoModif_D + @DeleteUserIdSql + ' ,'
					SET @ListOfFieldsNoModif = @ListOfFieldsNoModif + ' '+@ModifUserInitColumnName+','
					SET @ListOfFieldsModif = @ListOfFieldsModif + '[' + @MainColumnName + '],'
				END
				ELSE IF isnull(@ModifColumnName, '') NOT IN (@ModifActionInitColumnName, @ModifIdInitColumnName) and @MainColumnName is not null
					SET @ListOfFields = @ListOfFields + '[' + @MainColumnName + '],'
				
				IF (@MainColumnName = @ModifColumnName)
					AND (@MainIs_nullable <> @ModifIs_nullable OR @MainType <> @ModifType OR @MainLength <> @ModifLength 
						OR @MainPrecision <> @ModifPrecision OR @MainScale <> @ModifScale OR @MainCollation <> @ModifCollation)
					AND @MainColumnName NOT IN (@ModifDateInitColumnName, @ModifUserInitColumnName)
				BEGIN
					SET @AlterStatement = @AlterStatement + ' ALTER TABLE [' + @Owner + '].[' + @TableName + @AuditNameExtention + '] ALTER COLUMN [' + @MainColumnName + '] [' + @MainType + '] '
				
					IF @MainType in ('binary', 'char', 'nchar', 'nvarchar', 'varbinary', 'varchar')
					BEGIN
						IF (@MainLength = -1)
							Set @AlterStatement = @AlterStatement + '(max) '
						ELSE
							SET @AlterStatement = @AlterStatement + '(' + cast(@MainLength as varchar(10)) + ') '
					END
					ELSE IF @MainType in ('decimal', 'numeric')
						SET @AlterStatement = @AlterStatement + '(' + cast(@MainPrecision as varchar(10)) + ',' + cast(@MainScale as varchar(10)) + ') '
					ELSE IF @MainType in ('char', 'nchar', 'nvarchar', 'varchar', 'text', 'ntext')
						SET @AlterStatement = @AlterStatement + 'COLLATE ' + @MainCollation + ' '
		
					SET @AlterStatement = @AlterStatement + @MainIs_nullable + '; '
				END
				ELSE IF (@MainColumnName is null AND @ModifColumnName NOT IN (@ModifActionInitColumnName, @ModifIdInitColumnName, @ModifDateInitColumnName, @ModifUserInitColumnName))
					SET @AlterStatement = @AlterStatement + ' ALTER TABLE [' + @Owner + '].[' + @TableName + @AuditNameExtention + '] DROP COLUMN [' + @ModifColumnName + '];'
				ELSE IF (@ModifColumnName is null)
				BEGIN
					SET @AlterStatement = @AlterStatement + ' ALTER TABLE [' + @Owner + '].[' + @TableName + @AuditNameExtention + '] ADD [' + @MainColumnName + '] [' + @MainType + '] '
				
					IF @MainType in ('binary', 'char', 'nchar', 'nvarchar', 'varbinary', 'varchar')
					BEGIN
						IF (@MainLength = -1)
							Set @AlterStatement = @AlterStatement + '(max) '
						ELSE
							SET @AlterStatement = @AlterStatement + '(' + cast(@MainLength as varchar(10)) + ') '
					END
					ELSE IF @MainType in ('decimal', 'numeric')
						SET @AlterStatement = @AlterStatement + '(' + cast(@MainPrecision as varchar(10)) + ',' + cast(@MainScale as varchar(10)) + ') '
					ELSE IF @MainType in ('char', 'nchar', 'nvarchar', 'varchar', 'text', 'ntext')
						SET @AlterStatement = @AlterStatement + 'COLLATE ' + @MainCollation + ' '
		
					SET @AlterStatement = @AlterStatement + @MainIs_nullable + '; '
				END
			
				FETCH Next FROM TableColumnsCompare
				INTO @MainColumnName, @ModifColumnName, @MainIs_nullable, @ModifIs_nullable, @MainType, @ModifType, @MainLength, @ModifLength, @MainPrecision, @ModifPrecision, @MainScale, @ModifScale, @MainCollation, @ModifCollation
		
			END

			CLOSE TableColumnsCompare
			DEALLOCATE TableColumnsCompare

			IF (@AlterStatement <> '')
			BEGIN
				PRINT 'Updating Modif table'
				EXEC (@AlterStatement)
			END
		END
	END
	ELSE
	BEGIN
		IF (@MajorVersion = 'OK')
		BEGIN
			DECLARE TableColumns CURSOR Read_Only
			FOR 
			SELECT MAIN.name as MAIN_ColumnName,
				'NULL' as MAIN_is_nullable,
				MAIN.system_type_name as MAIN_Datatype,
				MAIN.collation_name as MAIN_Collation
			FROM sys.dm_exec_describe_first_result_SET (N'SELECT * FROM [' + @Owner + '].[' + @TableName + ']', NULL, 0) MAIN

			OPEN TableColumns

			-- Start of create table
			SET @CreateStatement = 'CREATE TABLE [' + @Owner + '].[' + @TableName + @AuditNameExtention + '] ('
			SET @CreateStatement = @CreateStatement + '[' + @ModifIdInitColumnName + '] [bigint] IDENTITY (1, 1) NOT NULL,'

			FETCH Next FROM TableColumns
			INTO @MainColumnName, @MainIs_nullable, @MainDatatype, @MainCollation

			WHILE @@FETCH_STATUS = 0
			BEGIN
				IF (@MainColumnName = @ModifDateInitColumnName)
				BEGIN
					SET @ListOfFieldsNoModif_D = @ListOfFieldsNoModif + ' getdate(),'
					SET @ListOfFieldsNoModif = @ListOfFieldsNoModif + ' case when '+@ModifDateInitColumnName+'=''1900-01-01'' or '+@ModifDateInitColumnName+' is null then getdate() ELSE '+@ModifDateInitColumnName+' END,'
					SET @ListOfFieldsModif= @ListOfFieldsModif + '[' + @MainColumnName + '],'
				END
				ELSE IF (@MainColumnName = @ModifUserInitColumnName)
				BEGIN
					SET @ListOfFieldsNoModif_D = @ListOfFieldsNoModif_D + @DeleteUserIdSql + ' ,'
					SET @ListOfFieldsNoModif = @ListOfFieldsNoModif + ' '+@ModifUserInitColumnName+','
					SET @ListOfFieldsModif= @ListOfFieldsModif + '[' + @MainColumnName + '],'
				END
				ELSE
				BEGIN
					set @ListOfFields = @ListOfFields + '[' + @MainColumnName + '],'
			
					SET @CreateStatement = @CreateStatement + '[' + @MainColumnName + '] ' + @MainDatatype + ' '
		
					IF @MainCollation is not null
						SET @CreateStatement = @CreateStatement + 'COLLATE ' + @MainCollation + ' '
		
					SET @CreateStatement = @CreateStatement + @MainIs_nullable + ', '
				END

				FETCH Next FROM TableColumns
				INTO @MainColumnName, @MainIs_nullable, @MainDatatype, @MainCollation
			END

			CLOSE TableColumns
			DEALLOCATE TableColumns

			-- Add audit trail columns
			SET @CreateStatement = @CreateStatement + '[' + @ModifActionInitColumnName + '] [char] (1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,'
			SET @CreateStatement = @CreateStatement + '[' + @ModifDateInitColumnName + '] [datetime] NULL,'
			SET @CreateStatement = @CreateStatement + '[' + @ModifUserInitColumnName + '] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL)'

			-- Create audit table
			PRINT 'Creating audit table [' + @Owner + '].[' + @TableName + @AuditNameExtention + ']'
			EXEC (@CreateStatement)

			-- Set primary key and default values
			SET @CreateStatement = 'ALTER TABLE [' + @Owner + '].[' + @TableName + @AuditNameExtention + '] ADD '
			SET @CreateStatement = @CreateStatement + 'CONSTRAINT [DF_' + @TableName + @AuditNameExtention + '_' + @ModifDateInitColumnName + '] DEFAULT (getdate()) FOR ['+@ModifDateInitColumnName+'],'
			SET @CreateStatement = @CreateStatement + 'CONSTRAINT [DF_' + @TableName + @AuditNameExtention + '_' + @ModifUserInitColumnName + '] DEFAULT (suser_sname()) FOR ['+@ModifUserInitColumnName+'],'
			SET @CreateStatement = @CreateStatement + 'CONSTRAINT [PK_' + @TableName + @AuditNameExtention + '] PRIMARY KEY CLUSTERED '
			SET @CreateStatement = @CreateStatement + '([' + @ModifIdInitColumnName + '])  ON [PRIMARY] '

			EXEC (@CreateStatement)
		END
		ELSE IF (@MajorVersion = 'NOT OK')
		BEGIN
			DECLARE TableColumns CURSOR Read_Only
			FOR 
			SELECT b.name as MAIN_ColumnName,
				'NULL' AS MAIN_is_nullable,
				c.name as MAIN_Type,
				b.length as MAIN_Length,
				b.xprec as MAIN_Precision,
				b.xscale as MAIN_Scale,
				b.collation as MAIN_Collation
			FROM sysobjects a
				inner join syscolumns b on a.id = b.id 
				inner join systypes c on b.xtype = c.xtype and c.name <> 'sysname' 
			WHERE a.id = object_id(N'[' + @Owner + '].[' + @TableName + ']') 
				and OBJECTPROPERTY(a.id, N'IsUserTable') = 1

			OPEN TableColumns

			-- Start of create table
			SET @CreateStatement = 'CREATE TABLE [' + @Owner + '].[' + @TableName + @AuditNameExtention + '] ('
			SET @CreateStatement = @CreateStatement + '[' + @ModifIdInitColumnName + '] [bigint] IDENTITY (1, 1) NOT NULL,'

			FETCH Next FROM TableColumns
			INTO @MainColumnName, @MainIs_nullable, @MainType, @MainLength, @MainPrecision, @MainScale, @MainCollation

			WHILE @@FETCH_STATUS = 0
			BEGIN
				IF (@MainColumnName = @ModifDateInitColumnName)
				BEGIN
					SET @ListOfFieldsNoModif_D = @ListOfFieldsNoModif + ' getdate(),'
					SET @ListOfFieldsNoModif = @ListOfFieldsNoModif + ' case when '+@ModifDateInitColumnName+'=''1900-01-01'' or '+@ModifDateInitColumnName+' is null then getdate() ELSE '+@ModifDateInitColumnName+' END,'
					SET @ListOfFieldsModif= @ListOfFieldsModif + '[' + @MainColumnName + '],'
				END
				ELSE IF (@MainColumnName = @ModifUserInitColumnName)
				BEGIN
					SET @ListOfFieldsNoModif_D = @ListOfFieldsNoModif_D + @DeleteUserIdSql + ' ,'
					SET @ListOfFieldsNoModif = @ListOfFieldsNoModif + ' '+@ModifUserInitColumnName+','
					SET @ListOfFieldsModif= @ListOfFieldsModif + '[' + @MainColumnName + '],'
				END
				ELSE
				BEGIN
					SET @ListOfFields = @ListOfFields + '[' + @MainColumnName + '],'

					SET @CreateStatement = @CreateStatement + '[' + @MainColumnName + '] [' + @MainType + '] '
				
					IF @MainType in ('binary', 'char', 'nchar', 'nvarchar', 'varbinary', 'varchar')
					BEGIN
						IF (@MainLength = -1)
							Set @CreateStatement = @CreateStatement + '(max) '
						ELSE
							SET @CreateStatement = @CreateStatement + '(' + cast(@MainLength as varchar(10)) + ') '
					END
					ELSE IF @MainType in ('decimal', 'numeric')
						SET @CreateStatement = @CreateStatement + '(' + cast(@MainPrecision as varchar(10)) + ',' + cast(@MainScale as varchar(10)) + ') '
		
					ELSE IF @MainType in ('char', 'nchar', 'nvarchar', 'varchar', 'text', 'ntext')
						SET @CreateStatement = @CreateStatement + 'COLLATE ' + @MainCollation + ' '
		
					SET @CreateStatement = @CreateStatement + @MainIs_nullable + ', '
				END

				FETCH Next FROM TableColumns
				INTO @MainColumnName, @MainIs_nullable, @MainType, @MainLength, @MainPrecision, @MainScale, @MainCollation
			END

			CLOSE TableColumns
			DEALLOCATE TableColumns

			-- Add audit trail columns
			SET @CreateStatement = @CreateStatement + '[' + @ModifActionInitColumnName + '] [char] (1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,'
			SET @CreateStatement = @CreateStatement + '['+@ModifDateInitColumnName+'] [datetime] NULL,'
			SET @CreateStatement = @CreateStatement + '['+@ModifUserInitColumnName+'] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL)'

			-- Create audit table
			PRINT 'Creating audit table [' + @Owner + '].[' + @TableName + @AuditNameExtention + ']'
			EXEC (@CreateStatement)

			-- Set primary key and default values
			SET @CreateStatement = 'ALTER TABLE [' + @Owner + '].[' + @TableName + @AuditNameExtention + '] ADD '
			SET @CreateStatement = @CreateStatement + 'CONSTRAINT [DF_' + @TableName + @AuditNameExtention + '_' + @ModifDateInitColumnName + '] DEFAULT (getdate()) FOR ['+@ModifDateInitColumnName+'],'
			SET @CreateStatement = @CreateStatement + 'CONSTRAINT [DF_' + @TableName + @AuditNameExtention + '_' + @ModifUserInitColumnName + '] DEFAULT (suser_sname()) FOR ['+@ModifUserInitColumnName+'],'
			SET @CreateStatement = @CreateStatement + 'CONSTRAINT [PK_' + @TableName + @AuditNameExtention + '] PRIMARY KEY CLUSTERED '
			SET @CreateStatement = @CreateStatement + '([' + @ModifIdInitColumnName + '])  ON [PRIMARY] '

			EXEC (@CreateStatement)
		END
	END

	/* Drop Triggers, if they exist */
	PRINT 'Dropping triggers'
	IF exists (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[' + @Owner + '].[tr_' + @TableName + '_Insert]') and OBJECTPROPERTY(id, N'IsTrigger') = 1) 
		EXEC ('drop trigger [' + @Owner + '].[tr_' + @TableName + '_Insert]')

	IF exists (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[' + @Owner + '].[tr_' + @TableName + '_Update]') and OBJECTPROPERTY(id, N'IsTrigger') = 1) 
		EXEC ('drop trigger [' + @Owner + '].[tr_' + @TableName + '_Update]')

	IF exists (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[' + @Owner + '].[tr_' + @TableName + '_Delete]') and OBJECTPROPERTY(id, N'IsTrigger') = 1) 
		EXEC ('drop trigger [' + @Owner + '].[tr_' + @TableName + '_Delete]')

	/* Create triggers */
	DECLARE @triggerCmd nvarchar(max)

	PRINT 'Creating triggers' 

	PRINT 'Creating INSERT trigger'

	print '@ListOfFields'
	print @ListOfFields
	print '@ListOfFieldsNoModif'
	print @ListOfFieldsNoModif

	SET @triggerCmd = 'CREATE TRIGGER tr_' + @TableName + '_Insert ON ' + @Owner + '.' + @TableName + ' FOR INSERT AS INSERT INTO ' 
		+ @TableName + @AuditNameExtention + '(' +  @ListOfFields + @ListOfFieldsModif + @ModifActionInitColumnName + ') SELECT ' 
		+ @ListOfFields + @ListOfFieldsNoModif + '''I'' FROM Inserted'
	PRINT @triggerCmd
	EXEC (@triggerCmd)

	PRINT 'Creating UPDATE trigger'
	SET @triggerCmd = 'CREATE TRIGGER tr_' + @TableName + '_Update ON ' + @Owner + '.' + @TableName + ' FOR UPDATE AS INSERT INTO ' 
		+ @TableName + @AuditNameExtention + '(' +  @ListOfFields + @ListOfFieldsModif + @ModifActionInitColumnName + ') SELECT ' 
		+ @ListOfFields + @ListOfFieldsNoModif + '''U'' FROM Inserted'
	PRINT @triggerCmd
	EXEC (@triggerCmd)

	PRINT 'Creating DELETE trigger'
	SET @triggerCmd = 'CREATE TRIGGER tr_' + @TableName + '_Delete ON ' + @Owner + '.' + @TableName + ' FOR DELETE AS INSERT INTO ' 
		+ @TableName + @AuditNameExtention + '(' +  @ListOfFields + @ListOfFieldsModif + @ModifActionInitColumnName + ') SELECT ' 
		+ @ListOfFields + @ListOfFieldsNoModif_D + '''D'' FROM Deleted d'
	PRINT @triggerCmd
	EXEC (@triggerCmd)
END