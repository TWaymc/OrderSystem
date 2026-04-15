IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Contacts] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(32) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Surname] nvarchar(200) NOT NULL,
    [MobileNumber] nvarchar(32) NULL,
    [Email] nvarchar(256) NULL,
    [RowVersion] rowversion NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [ModifiedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [LastModifiedBy] nvarchar(max) NOT NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Contacts] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Sequences] (
    [Type] nvarchar(64) NOT NULL,
    [SeqNumber] bigint NOT NULL,
    CONSTRAINT [PK_Sequences] PRIMARY KEY ([Type])
);
GO

CREATE UNIQUE INDEX [IX_Contacts_Code] ON [Contacts] ([Code]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260415083329_Initial_Migration', N'8.0.25');
GO

COMMIT;
GO

