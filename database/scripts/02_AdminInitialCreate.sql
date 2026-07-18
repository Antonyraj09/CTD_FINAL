-- sqlcmd's default session has QUOTED_IDENTIFIER OFF, which breaks the filtered
-- unique indexes below (WHERE [X] IS NOT NULL). dotnet ef database update is
-- unaffected (Microsoft.Data.SqlClient defaults it ON) but running this script
-- directly via sqlcmd requires it explicitly.
SET QUOTED_IDENTIFIER ON;
GO

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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718181256_InitialCreate'
)
BEGIN
    CREATE TABLE [Companies] (
        [Id] int NOT NULL IDENTITY,
        [CompanyName] nvarchar(200) NOT NULL,
        [CompanyCode] nvarchar(20) NOT NULL,
        [Address] nvarchar(500) NULL,
        [Country] nvarchar(100) NULL,
        [State] nvarchar(100) NULL,
        [City] nvarchar(100) NULL,
        [GstNumber] nvarchar(20) NULL,
        [ContactPerson] nvarchar(150) NULL,
        [Email] nvarchar(200) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [InstallationLocation] nvarchar(200) NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718181256_InitialCreate'
)
BEGIN
    CREATE TABLE [NumberSequences] (
        [Id] int NOT NULL IDENTITY,
        [Key] nvarchar(30) NOT NULL,
        [CurrentValue] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_NumberSequences] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718181256_InitialCreate'
)
BEGIN
    CREATE TABLE [ClientDatabases] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [DatabaseName] nvarchar(128) NOT NULL,
        [ServerName] nvarchar(200) NOT NULL,
        [DatabaseUsername] nvarchar(128) NOT NULL,
        [EncryptedPassword] nvarchar(500) NOT NULL,
        [EncryptedConnectionString] nvarchar(1000) NOT NULL,
        [DatabaseVersion] nvarchar(50) NULL,
        [ApplicationVersion] nvarchar(20) NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ClientDatabases] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClientDatabases_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718181256_InitialCreate'
)
BEGIN
    CREATE TABLE [InstallationHistories] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NULL,
        [InstallationDate] datetime2 NOT NULL,
        [InstalledBy] nvarchar(150) NULL,
        [MachineName] nvarchar(150) NULL,
        [ApplicationVersion] nvarchar(20) NULL,
        [DatabaseVersion] nvarchar(50) NULL,
        [InstallationStatus] int NOT NULL,
        [ErrorLog] nvarchar(4000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_InstallationHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InstallationHistories_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718181256_InitialCreate'
)
BEGIN
    CREATE TABLE [Licenses] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [LicenseNumber] nvarchar(20) NOT NULL,
        [LicenseType] int NOT NULL,
        [IssueDate] datetime2 NOT NULL,
        [ExpiryDate] datetime2 NOT NULL,
        [MachineIdentifier] nvarchar(200) NULL,
        [InstallationDate] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [Activated] bit NOT NULL,
        [LastValidation] datetime2 NULL,
        [LicenseKey] nvarchar(1000) NULL,
        [EncryptedLicense] nvarchar(2000) NULL,
        [ApplicationVersion] nvarchar(20) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Licenses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Licenses_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718181256_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ClientDatabases_CompanyId] ON [ClientDatabases] ([CompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718181256_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Companies_CompanyCode] ON [Companies] ([CompanyCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718181256_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Companies_Email] ON [Companies] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718181256_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InstallationHistories_CompanyId] ON [InstallationHistories] ([CompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718181256_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Licenses_CompanyId] ON [Licenses] ([CompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718181256_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Licenses_LicenseNumber] ON [Licenses] ([LicenseNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718181256_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NumberSequences_Key] ON [NumberSequences] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718181256_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260718181256_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

