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

CREATE TABLE [Orders] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(32) NOT NULL,
    [StatusCode] nvarchar(32) NOT NULL DEFAULT N'Pending',
    [CustomerId] uniqueidentifier NOT NULL,
    [CustomerName] nvarchar(200) NOT NULL,
    [CustomerSurname] nvarchar(200) NOT NULL,
    [CustomerMobileNumber] nvarchar(32) NULL,
    [CustomerEmail] nvarchar(256) NULL,
    [RowVersion] rowversion NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [ModifiedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [LastModifiedBy] nvarchar(max) NOT NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Sequences] (
    [Type] nvarchar(64) NOT NULL,
    [SeqNumber] bigint NOT NULL,
    CONSTRAINT [PK_Sequences] PRIMARY KEY ([Type])
);
GO

CREATE TABLE [OrderItems] (
    [Id] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [ProductCode] nvarchar(32) NOT NULL,
    [ProductName] nvarchar(200) NOT NULL,
    [ProductUnitPrice] decimal(18,2) NOT NULL,
    [Quantity] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [ModifiedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [LastModifiedBy] nvarchar(max) NOT NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
GO

CREATE UNIQUE INDEX [IX_Orders_Code] ON [Orders] ([Code]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260415105334_Initial_Migration', N'8.0.25');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Orders] ADD [TotalAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [Orders] ADD [TotalItems] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260416060838_AddOrdersTotals', N'8.0.25');
GO

COMMIT;
GO

