--IF NOT EXISTS(SELECT 1 FROM sys.schemas WHERE sys.schemas.name = 'Field')
--BEGIN
--	PRINT 'Creating [Field] schema...';
--	EXEC ('CREATE SCHEMA [Field] AUTHORIZATION [dbo];');
--END
--ELSE
--BEGIN
--	PRINT 'Schema [Field] already exists.';
--END


IF EXISTS(SELECT 1 WHERE OBJECT_ID('[dbo].[Field_Authorization_Rule_Resource]') IS NOT NULL)
BEGIN
	PRINT 'Dropping [dbo].[Field_Authorization_Rule_Resource] table...'
	DROP TABLE [dbo].[Field_Authorization_Rule_Resource];
END

IF EXISTS(SELECT 1 WHERE OBJECT_ID('[dbo].[Field_Authorization_Rule_Identity]') IS NOT NULL)
BEGIN
	PRINT 'Dropping [dbo].[Field_Authorization_Rule_Identity] table...'
	DROP TABLE [dbo].[Field_Authorization_Rule_Identity];
END

IF EXISTS(SELECT 1 WHERE OBJECT_ID('[dbo].[Field_Authorization_Resource]') IS NOT NULL)
BEGIN
	PRINT 'Dropping [dbo].[Field_Authorization_Resource] table...'
	DROP TABLE [dbo].[Field_Authorization_Resource];
END

IF EXISTS(SELECT 1 WHERE OBJECT_ID('[dbo].[Field_Authorization_Identity]') IS NOT NULL)
BEGIN
	PRINT 'Dropping [dbo].[Field_Authorization_Identity] table...'
	DROP TABLE [dbo].[Field_Authorization_Identity];
END

IF EXISTS(SELECT 1 WHERE OBJECT_ID('[dbo].[Field_Authorization_Rule]') IS NOT NULL)
BEGIN
	PRINT 'Dropping [dbo].[Field_Authorization_Rule] table...'
	DROP TABLE [dbo].[Field_Authorization_Rule];
END

IF EXISTS(SELECT 1 WHERE OBJECT_ID('[dbo].[Field_Authorization_Log]') IS NOT NULL)
BEGIN
	PRINT 'Dropping [dbo].[Field_Authorization_Log] table...'
	DROP TABLE [dbo].[Field_Authorization_Log];
END

PRINT 'Creating [dbo].[Field_Authorization_Rule] table...'
CREATE TABLE [dbo].[Field_Authorization_Rule]
(
	[ID] INT IDENTITY(1,1),
	[Type] CHAR(1) DEFAULT 'A', -- 'A' allow | 'D' deny

	CONSTRAINT [PK_Field_Authorization_Rule] PRIMARY KEY ([ID])
)

PRINT 'Creating [dbo].[Field_Authorization_Resource] table...'
CREATE TABLE [dbo].[Field_Authorization_Resource]
(
	[ID] INT IDENTITY(1,1),
	[Type] CHAR(1), -- 'F' Form | 'V' View | 'S' SmartObject
	[Name] NVARCHAR(448),
	[Guid] UNIQUEIDENTIFIER,

	CONSTRAINT [PK_Field_Authorization_Resource] PRIMARY KEY NONCLUSTERED ([ID])
)

PRINT 'Creating [dbo].[Field_Authorization_Identity] table...'
CREATE TABLE [dbo].[Field_Authorization_Identity]
(
	[ID] INT IDENTITY(1,1) ,
	[FQN] NVARCHAR(448),

	CONSTRAINT [PK_Field_AuthorizationIdentity] PRIMARY KEY ([ID])
)


PRINT 'Creating [dbo].[Field_Authorization_Rule_Resource] table...'
CREATE TABLE [dbo].[Field_Authorization_Rule_Resource]
(
	[RuleID] INT NOT NULL,
	[ResourceID] INT NOT NULL,

	CONSTRAINT [PK_Field_Authorization_Rule_Resource]
		PRIMARY KEY ([RuleID], [ResourceID]),
	CONSTRAINT [FK_Field_Authorization_Rule_Resource__Field_Authorization_Rule]
		FOREIGN KEY ([RuleID]) REFERENCES [Field_Authorization_Rule]([ID]),
	CONSTRAINT [FK_Field_Authorization_Rule_Resource__Field_Authorization_Resource]
		FOREIGN KEY ([ResourceID]) REFERENCES [Field_Authorization_Resource]([ID])
)

PRINT 'Creating [dbo].[Field_Authorization_Rule_Identity] table...'
CREATE TABLE [dbo].[Field_Authorization_Rule_Identity]
(
	[RuleID] INT NOT NULL,
	[IdentityID] INT NOT NULL, 

	CONSTRAINT [PK_Field_Authorization_Rule_Identity]
		PRIMARY KEY ([RuleID], [IdentityID]),
	CONSTRAINT [FK_Field_Authorization_Rule_Identity__Field_Authorization_Rule]
		FOREIGN KEY ([RuleID]) REFERENCES [Field_Authorization_Rule]([ID]),
	CONSTRAINT [FK_Field_Authorization_Rule_Identity__Field_Authorization_Identity]
		FOREIGN KEY ([IdentityID]) REFERENCES [Field_Authorization_Identity]([ID])
)


PRINT 'Creating [dbo].[Field_Authorization_Log] table...'
CREATE TABLE [dbo].[Field_Authorization_Log]
(
	[DateTime] DATETIME2 DEFAULT SYSDATETIME(),
	[Level] CHAR(1), -- 'E' error, 'W' warning, 'I' info, 'D' debug
	[Type] NVARCHAR(200),
	[Message] NVARCHAR(MAX)
)
