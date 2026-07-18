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
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [Agents] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [License] nvarchar(50) NOT NULL,
        [City] nvarchar(100) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [Email] nvarchar(150) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Agents] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [AlertRules] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [Channel] int NOT NULL,
        [Trigger] nvarchar(200) NOT NULL,
        [Audience] nvarchar(150) NOT NULL,
        [Active] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AlertRules] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [AppSettings] (
        [Id] int NOT NULL IDENTITY,
        [JobNumberPrefix] nvarchar(10) NOT NULL,
        [InvoicePrefix] nvarchar(10) NOT NULL,
        [DocumentPrefix] nvarchar(10) NOT NULL,
        [CompanyName] nvarchar(200) NOT NULL,
        [CompanyAddress] nvarchar(400) NOT NULL,
        [CompanyGstin] nvarchar(20) NOT NULL,
        [ChaLicenseNo] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AppSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] int NOT NULL IDENTITY,
        [FullName] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [LastLoginAt] datetime2 NULL,
        [ImporterId] int NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [JobNo] nvarchar(30) NULL,
        [JobId] int NULL,
        [Action] int NOT NULL,
        [User] nvarchar(150) NOT NULL,
        [Field] nvarchar(150) NULL,
        [FromValue] nvarchar(200) NULL,
        [ToValue] nvarchar(200) NULL,
        [Detail] nvarchar(500) NULL,
        [Timestamp] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [BorderPoints] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [State] nvarchar(150) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_BorderPoints] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [Commodities] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [HsCode] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Commodities] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [CustomsHouses] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CustomsHouses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [Importers] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Gstin] nvarchar(20) NOT NULL,
        [City] nvarchar(100) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [Email] nvarchar(150) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Importers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [JobIsnes] (
        [Id] int NOT NULL IDENTITY,
        [JobNumber] nvarchar(30) NOT NULL,
        [JobDate] datetime2 NOT NULL,
        [PartyCode] nvarchar(30) NOT NULL,
        [PartyName] nvarchar(200) NOT NULL,
        [Address] nvarchar(500) NULL,
        [SubAgentCode] nvarchar(30) NULL,
        [SubAgentName] nvarchar(200) NULL,
        [CtdNumber] nvarchar(40) NULL,
        [CtdDate] datetime2 NULL,
        [VesselName] nvarchar(100) NULL,
        [VoyageNo] nvarchar(30) NULL,
        [TsVessel] nvarchar(100) NULL,
        [TsVoyage] nvarchar(30) NULL,
        [CountryCgn] nvarchar(4) NULL,
        [CountryOrigin] nvarchar(4) NULL,
        [RouteOfTransit] nvarchar(100) NULL,
        [RotNo] nvarchar(40) NULL,
        [LineNo] nvarchar(40) NULL,
        [MblNo] nvarchar(40) NULL,
        [MblDate] datetime2 NULL,
        [HblNo] nvarchar(40) NULL,
        [HblDate] datetime2 NULL,
        [IlNo] nvarchar(40) NULL,
        [IlDate] datetime2 NULL,
        [LcNo] nvarchar(60) NULL,
        [LcDate] datetime2 NULL,
        [AccountName] nvarchar(100) NULL,
        [BankName] nvarchar(200) NULL,
        [RefNo] nvarchar(60) NULL,
        [RefDate] datetime2 NULL,
        [SteamerAgent] nvarchar(100) NULL,
        [ContainerAgent] nvarchar(100) NULL,
        [VesselArrival] datetime2 NULL,
        [CtdSentTo] nvarchar(150) NULL,
        [GreenCtd] bit NOT NULL,
        [DuePackingList] datetime2 NULL,
        [DueInvoice] datetime2 NULL,
        [DueOriginalBl] datetime2 NULL,
        [DueInsuranceCert] datetime2 NULL,
        [DueLcCopy] datetime2 NULL,
        [MarksSerial] nvarchar(500) NULL,
        [ContainerNo] nvarchar(200) NULL,
        [ContainerStatus] int NOT NULL,
        [ContainerSize] nvarchar(10) NOT NULL,
        [NoPackages] int NOT NULL,
        [CustomsCode] nvarchar(60) NULL,
        [MiscDescription] nvarchar(300) NULL,
        [Unit] nvarchar(200) NULL,
        [CargoDescription] nvarchar(1000) NULL,
        [Currency] nvarchar(5) NOT NULL,
        [ExchangeRate] decimal(18,4) NULL,
        [FobValue] decimal(18,2) NULL,
        [Freight] decimal(18,2) NULL,
        [CifFc] decimal(18,2) NULL,
        [CifFcReference] decimal(18,2) NULL,
        [InsuranceFc] decimal(18,2) NULL,
        [InsuranceValue] decimal(18,2) NULL,
        [InsuranceExRate] decimal(18,4) NULL,
        [InsuranceRate] decimal(18,3) NULL,
        [InsuranceValueInr] decimal(18,2) NULL,
        [CifInr] decimal(18,2) NULL,
        [MarketRate] decimal(18,2) NULL,
        [MarketValueInr] decimal(18,2) NULL,
        [GrossWeight] decimal(18,3) NULL,
        [NetWeight] decimal(18,3) NULL,
        [LcAmount] decimal(18,2) NULL,
        [ShipmentExpiry] datetime2 NULL,
        [PartialShipment] nvarchar(20) NOT NULL,
        [DutyAmount] decimal(18,2) NULL,
        [CreatedBy] nvarchar(150) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_JobIsnes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
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
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [Id] int NOT NULL IDENTITY,
        [Role] nvarchar(50) NOT NULL,
        [ModuleKey] nvarchar(60) NOT NULL,
        [Allowed] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [TransitRoutes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(250) NOT NULL,
        [Distance] nvarchar(50) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_TransitRoutes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [Transporters] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Fleet] nvarchar(100) NULL,
        [City] nvarchar(100) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [Email] nvarchar(150) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Transporters] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [AlertLogs] (
        [Id] int NOT NULL IDENTITY,
        [AlertRuleId] int NULL,
        [Channel] int NOT NULL,
        [To] nvarchar(200) NOT NULL,
        [Trigger] nvarchar(200) NOT NULL,
        [JobNo] nvarchar(30) NULL,
        [SentAt] datetime2 NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AlertLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AlertLogs_AlertRules_AlertRuleId] FOREIGN KEY ([AlertRuleId]) REFERENCES [AlertRules] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] int NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [CtdJobs] (
        [Id] int NOT NULL IDENTITY,
        [JobNo] nvarchar(30) NOT NULL,
        [JobDate] datetime2 NOT NULL,
        [ImporterId] int NULL,
        [AgentId] int NULL,
        [TransporterId] int NULL,
        [OriginCountry] nvarchar(80) NOT NULL,
        [PortArrival] nvarchar(80) NULL,
        [BorderPointId] int NULL,
        [ShipmentType] int NOT NULL,
        [CreatedBy] nvarchar(150) NULL,
        [Remarks] nvarchar(500) NULL,
        [InvoiceNo] nvarchar(40) NULL,
        [InvoiceDate] datetime2 NULL,
        [Currency] nvarchar(10) NOT NULL,
        [InvoiceValue] decimal(18,2) NOT NULL,
        [CommodityId] int NULL,
        [HsCode] nvarchar(20) NULL,
        [GrossWt] decimal(18,3) NOT NULL,
        [NetWt] decimal(18,3) NOT NULL,
        [Packages] int NOT NULL,
        [CtdType] int NOT NULL,
        [CtdNumber] nvarchar(40) NULL,
        [CtdDate] datetime2 NULL,
        [CustomsHouseId] int NULL,
        [TransitRouteId] int NULL,
        [ExpDeliveryDate] datetime2 NULL,
        [CtdDocGenerated] bit NOT NULL,
        [ChecklistDocGenerated] bit NOT NULL,
        [ForwardingDocGenerated] bit NOT NULL,
        [Status] int NOT NULL,
        [ArrivalDate] datetime2 NULL,
        [DeliveryDate] datetime2 NULL,
        [DeliveryStatus] int NOT NULL,
        [ServiceCharge] decimal(18,2) NOT NULL,
        [TransportCharge] decimal(18,2) NOT NULL,
        [OtherCharge] decimal(18,2) NOT NULL,
        [TaxPercent] decimal(5,2) NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [Tax] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [BillingStatus] int NOT NULL,
        [InvoiceGenerated] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CtdJobs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CtdJobs_Agents_AgentId] FOREIGN KEY ([AgentId]) REFERENCES [Agents] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CtdJobs_BorderPoints_BorderPointId] FOREIGN KEY ([BorderPointId]) REFERENCES [BorderPoints] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CtdJobs_Commodities_CommodityId] FOREIGN KEY ([CommodityId]) REFERENCES [Commodities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CtdJobs_CustomsHouses_CustomsHouseId] FOREIGN KEY ([CustomsHouseId]) REFERENCES [CustomsHouses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CtdJobs_Importers_ImporterId] FOREIGN KEY ([ImporterId]) REFERENCES [Importers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CtdJobs_TransitRoutes_TransitRouteId] FOREIGN KEY ([TransitRouteId]) REFERENCES [TransitRoutes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CtdJobs_Transporters_TransporterId] FOREIGN KEY ([TransporterId]) REFERENCES [Transporters] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [GeneratedDocuments] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(250) NOT NULL,
        [Type] nvarchar(60) NOT NULL,
        [CtdJobId] int NULL,
        [JobNo] nvarchar(30) NULL,
        [UploadedBy] nvarchar(150) NULL,
        [DocumentDate] datetime2 NOT NULL,
        [Size] nvarchar(20) NULL,
        [SystemGenerated] bit NOT NULL,
        [StoragePath] nvarchar(400) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_GeneratedDocuments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GeneratedDocuments_CtdJobs_CtdJobId] FOREIGN KEY ([CtdJobId]) REFERENCES [CtdJobs] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [JobChecklistItems] (
        [Id] int NOT NULL IDENTITY,
        [CtdJobId] int NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Done] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_JobChecklistItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_JobChecklistItems_CtdJobs_CtdJobId] FOREIGN KEY ([CtdJobId]) REFERENCES [CtdJobs] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE TABLE [JobContainers] (
        [Id] int NOT NULL IDENTITY,
        [CtdJobId] int NOT NULL,
        [ContainerNo] nvarchar(30) NOT NULL,
        [Size] nvarchar(40) NULL,
        [Seal] nvarchar(30) NULL,
        [Weight] decimal(18,3) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_JobContainers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_JobContainers_CtdJobs_CtdJobId] FOREIGN KEY ([CtdJobId]) REFERENCES [CtdJobs] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Agents_License] ON [Agents] ([License]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Agents_Name] ON [Agents] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AlertLogs_AlertRuleId] ON [AlertLogs] ([AlertRuleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AlertLogs_SentAt] ON [AlertLogs] ([SentAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_JobNo] ON [AuditLogs] ([JobNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_Timestamp] ON [AuditLogs] ([Timestamp]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_User] ON [AuditLogs] ([User]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BorderPoints_Name] ON [BorderPoints] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Commodities_HsCode] ON [Commodities] ([HsCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Commodities_Name] ON [Commodities] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CtdJobs_AgentId] ON [CtdJobs] ([AgentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CtdJobs_BorderPointId] ON [CtdJobs] ([BorderPointId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CtdJobs_CommodityId] ON [CtdJobs] ([CommodityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CtdJobs_CtdNumber] ON [CtdJobs] ([CtdNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CtdJobs_CustomsHouseId] ON [CtdJobs] ([CustomsHouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CtdJobs_ImporterId] ON [CtdJobs] ([ImporterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CtdJobs_JobDate] ON [CtdJobs] ([JobDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CtdJobs_JobNo] ON [CtdJobs] ([JobNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CtdJobs_Status] ON [CtdJobs] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CtdJobs_TransitRouteId] ON [CtdJobs] ([TransitRouteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CtdJobs_TransporterId] ON [CtdJobs] ([TransporterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CustomsHouses_Code] ON [CustomsHouses] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomsHouses_Name] ON [CustomsHouses] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GeneratedDocuments_CtdJobId] ON [GeneratedDocuments] ([CtdJobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GeneratedDocuments_JobNo] ON [GeneratedDocuments] ([JobNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GeneratedDocuments_Type] ON [GeneratedDocuments] ([Type]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Importers_Gstin] ON [Importers] ([Gstin]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Importers_Name] ON [Importers] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_JobChecklistItems_CtdJobId] ON [JobChecklistItems] ([CtdJobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_JobContainers_ContainerNo] ON [JobContainers] ([ContainerNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_JobContainers_CtdJobId] ON [JobContainers] ([CtdJobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_JobIsnes_JobNumber] ON [JobIsnes] ([JobNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NumberSequences_Key] ON [NumberSequences] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RolePermissions_Role_ModuleKey] ON [RolePermissions] ([Role], [ModuleKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TransitRoutes_Name] ON [TransitRoutes] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Transporters_Name] ON [Transporters] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710123442_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260710123442_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    CREATE TABLE [Parties] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [City] nvarchar(100) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [Email] nvarchar(150) NULL,
        [IsImporter] bit NOT NULL,
        [IsTransporter] bit NOT NULL,
        [IsAgent] bit NOT NULL,
        [Gstin] nvarchar(20) NULL,
        [License] nvarchar(50) NULL,
        [Fleet] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Parties] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    ALTER TABLE [CtdJobs] DROP CONSTRAINT [FK_CtdJobs_Agents_AgentId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    ALTER TABLE [CtdJobs] DROP CONSTRAINT [FK_CtdJobs_Importers_ImporterId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    ALTER TABLE [CtdJobs] DROP CONSTRAINT [FK_CtdJobs_Transporters_TransporterId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    ALTER TABLE [Parties] ADD [LegacyImporterId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    ALTER TABLE [Parties] ADD [LegacyAgentId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    ALTER TABLE [Parties] ADD [LegacyTransporterId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN

    INSERT INTO [Parties] ([Name],[City],[Phone],[Email],[IsImporter],[IsTransporter],[IsAgent],[Gstin],[License],[Fleet],[CreatedAt],[UpdatedAt],[LegacyImporterId])
    SELECT [Name],[City],[Phone],[Email],1,0,0,[Gstin],NULL,NULL,[CreatedAt],[UpdatedAt],[Id]
    FROM [Importers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN

    INSERT INTO [Parties] ([Name],[City],[Phone],[Email],[IsImporter],[IsTransporter],[IsAgent],[Gstin],[License],[Fleet],[CreatedAt],[UpdatedAt],[LegacyAgentId])
    SELECT [Name],[City],[Phone],[Email],0,0,1,NULL,[License],NULL,[CreatedAt],[UpdatedAt],[Id]
    FROM [Agents];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN

    INSERT INTO [Parties] ([Name],[City],[Phone],[Email],[IsImporter],[IsTransporter],[IsAgent],[Gstin],[License],[Fleet],[CreatedAt],[UpdatedAt],[LegacyTransporterId])
    SELECT [Name],[City],[Phone],[Email],0,1,0,NULL,NULL,[Fleet],[CreatedAt],[UpdatedAt],[Id]
    FROM [Transporters];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN

    UPDATE j SET j.[ImporterId] = p.[Id]
    FROM [CtdJobs] j
    JOIN [Parties] p ON p.[LegacyImporterId] = j.[ImporterId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN

    UPDATE j SET j.[AgentId] = p.[Id]
    FROM [CtdJobs] j
    JOIN [Parties] p ON p.[LegacyAgentId] = j.[AgentId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN

    UPDATE j SET j.[TransporterId] = p.[Id]
    FROM [CtdJobs] j
    JOIN [Parties] p ON p.[LegacyTransporterId] = j.[TransporterId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Parties]') AND [c].[name] = N'LegacyImporterId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Parties] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Parties] DROP COLUMN [LegacyImporterId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Parties]') AND [c].[name] = N'LegacyAgentId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Parties] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Parties] DROP COLUMN [LegacyAgentId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Parties]') AND [c].[name] = N'LegacyTransporterId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Parties] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Parties] DROP COLUMN [LegacyTransporterId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    DROP TABLE [Agents];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    DROP TABLE [Importers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    DROP TABLE [Transporters];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Parties_Gstin] ON [Parties] ([Gstin]) WHERE [Gstin] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Parties_License] ON [Parties] ([License]) WHERE [License] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    CREATE INDEX [IX_Parties_Name] ON [Parties] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    ALTER TABLE [CtdJobs] ADD CONSTRAINT [FK_CtdJobs_Parties_AgentId] FOREIGN KEY ([AgentId]) REFERENCES [Parties] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    ALTER TABLE [CtdJobs] ADD CONSTRAINT [FK_CtdJobs_Parties_ImporterId] FOREIGN KEY ([ImporterId]) REFERENCES [Parties] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    ALTER TABLE [CtdJobs] ADD CONSTRAINT [FK_CtdJobs_Parties_TransporterId] FOREIGN KEY ([TransporterId]) REFERENCES [Parties] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711105120_MergePartyMaster'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260711105120_MergePartyMaster', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    DROP INDEX [IX_Parties_Gstin] ON [Parties];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [TradeName] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [Constitution] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [Pan] nvarchar(10) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [IecCode] nvarchar(15) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [CinNumber] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [LicenseValidUpto] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [AeoStatus] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [AeoCertificateNo] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [BankName] nvarchar(150) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [BankAccountNo] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [BankIfsc] nvarchar(15) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [AdCode] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [Website] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [ContactPersonName] nvarchar(150) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [ContactPersonDesignation] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [ContactPersonPhone] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [ContactPersonEmail] nvarchar(150) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    ALTER TABLE [Parties] ADD [Remarks] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    CREATE TABLE [PartyBranches] (
        [Id] int NOT NULL IDENTITY,
        [PartyId] int NOT NULL,
        [BranchName] nvarchar(150) NOT NULL,
        [IsPrimary] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [AddressLine1] nvarchar(300) NOT NULL,
        [AddressLine2] nvarchar(300) NULL,
        [City] nvarchar(100) NOT NULL,
        [State] nvarchar(100) NULL,
        [PinCode] nvarchar(15) NULL,
        [Country] nvarchar(80) NOT NULL,
        [Gstin] nvarchar(20) NULL,
        [Phone] nvarchar(30) NULL,
        [Email] nvarchar(150) NULL,
        [ContactPersonName] nvarchar(150) NULL,
        [CustomsRegistrationNo] nvarchar(50) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PartyBranches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PartyBranches_Parties_PartyId] FOREIGN KEY ([PartyId]) REFERENCES [Parties] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN

    INSERT INTO [PartyBranches] ([PartyId],[BranchName],[IsPrimary],[IsActive],[AddressLine1],[City],[Country],[Gstin],[Phone],[Email],[CreatedAt],[UpdatedAt])
    SELECT [Id], 'Head Office', 1, 1, [City], [City], 'India', [Gstin], [Phone], [Email], [CreatedAt], [UpdatedAt]
    FROM [Parties];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Parties]') AND [c].[name] = N'City');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Parties] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Parties] DROP COLUMN [City];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Parties]') AND [c].[name] = N'Phone');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Parties] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Parties] DROP COLUMN [Phone];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Parties]') AND [c].[name] = N'Email');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Parties] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Parties] DROP COLUMN [Email];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Parties]') AND [c].[name] = N'Gstin');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Parties] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Parties] DROP COLUMN [Gstin];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Parties_CinNumber] ON [Parties] ([CinNumber]) WHERE [CinNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Parties_IecCode] ON [Parties] ([IecCode]) WHERE [IecCode] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Parties_Pan] ON [Parties] ([Pan]) WHERE [Pan] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PartyBranches_Gstin] ON [PartyBranches] ([Gstin]) WHERE [Gstin] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    CREATE INDEX [IX_PartyBranches_PartyId] ON [PartyBranches] ([PartyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260711112611_AddPartyBranchesAndDetails'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260711112611_AddPartyBranchesAndDetails', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713095009_AddSubAgentMaster'
)
BEGIN
    CREATE TABLE [SubAgents] (
        [Id] int NOT NULL IDENTITY,
        [SubAgentCode] nvarchar(20) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [AddressLine1] nvarchar(250) NOT NULL,
        [AddressLine2] nvarchar(250) NULL,
        [City] nvarchar(100) NULL,
        [State] nvarchar(100) NULL,
        [PinCode] nvarchar(12) NULL,
        [LicenseNo] nvarchar(50) NULL,
        [PanNo] nvarchar(10) NULL,
        [GstinNo] nvarchar(15) NULL,
        [ContactPersonName] nvarchar(150) NULL,
        [Phone] nvarchar(20) NULL,
        [Email] nvarchar(150) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SubAgents] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713095009_AddSubAgentMaster'
)
BEGIN
    CREATE INDEX [IX_SubAgents_Name] ON [SubAgents] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713095009_AddSubAgentMaster'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SubAgents_SubAgentCode] ON [SubAgents] ([SubAgentCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713095009_AddSubAgentMaster'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713095009_AddSubAgentMaster', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713113924_AddSubAgentCodeToParty'
)
BEGIN
    ALTER TABLE [Parties] ADD [SubAgentCode] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713113924_AddSubAgentCodeToParty'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713113924_AddSubAgentCodeToParty', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713122631_AddPartyCodeAndJobIsneFieldChanges'
)
BEGIN
    ALTER TABLE [Parties] ADD [PartyCode] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713122631_AddPartyCodeAndJobIsneFieldChanges'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'VesselName');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [JobIsnes] ALTER COLUMN [VesselName] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713122631_AddPartyCodeAndJobIsneFieldChanges'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'TsVessel');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [JobIsnes] ALTER COLUMN [TsVessel] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713122631_AddPartyCodeAndJobIsneFieldChanges'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'CtdNumber');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [JobIsnes] ALTER COLUMN [CtdNumber] nvarchar(25) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713122631_AddPartyCodeAndJobIsneFieldChanges'
)
BEGIN
    ALTER TABLE [JobIsnes] ADD [InwardDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713122631_AddPartyCodeAndJobIsneFieldChanges'
)
BEGIN
    ALTER TABLE [JobIsnes] ADD [RotDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713122631_AddPartyCodeAndJobIsneFieldChanges'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Parties_PartyCode] ON [Parties] ([PartyCode]) WHERE [PartyCode] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713122631_AddPartyCodeAndJobIsneFieldChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713122631_AddPartyCodeAndJobIsneFieldChanges', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713123009_WidenJobIsneCountryFields'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'CountryOrigin');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [JobIsnes] ALTER COLUMN [CountryOrigin] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713123009_WidenJobIsneCountryFields'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'CountryCgn');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [JobIsnes] ALTER COLUMN [CountryCgn] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713123009_WidenJobIsneCountryFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713123009_WidenJobIsneCountryFields', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713154342_AddDueDatesAndContainerNoLength'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'ContainerNo');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [JobIsnes] ALTER COLUMN [ContainerNo] nvarchar(15) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713154342_AddDueDatesAndContainerNoLength'
)
BEGIN
    ALTER TABLE [JobIsnes] ADD [DueLoa] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713154342_AddDueDatesAndContainerNoLength'
)
BEGIN
    ALTER TABLE [JobIsnes] ADD [DueOrigin] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713154342_AddDueDatesAndContainerNoLength'
)
BEGIN
    ALTER TABLE [JobIsnes] ADD [DueProformaInvoice] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713154342_AddDueDatesAndContainerNoLength'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713154342_AddDueDatesAndContainerNoLength', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145356_CombineJobIsneContainerGrid'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'ContainerNo');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [JobIsnes] DROP COLUMN [ContainerNo];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145356_CombineJobIsneContainerGrid'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'ContainerSize');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [JobIsnes] DROP COLUMN [ContainerSize];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145356_CombineJobIsneContainerGrid'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'CustomsCode');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [JobIsnes] DROP COLUMN [CustomsCode];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145356_CombineJobIsneContainerGrid'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'MarksSerial');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [JobIsnes] DROP COLUMN [MarksSerial];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145356_CombineJobIsneContainerGrid'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'Unit');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [JobIsnes] DROP COLUMN [Unit];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145356_CombineJobIsneContainerGrid'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'NoPackages');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [JobIsnes] DROP COLUMN [NoPackages];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145356_CombineJobIsneContainerGrid'
)
BEGIN
    EXEC sp_rename N'[JobIsnes].[ContainerStatus]', N'ShipmentType', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145356_CombineJobIsneContainerGrid'
)
BEGIN
    CREATE TABLE [JobIsneContainers] (
        [Id] int NOT NULL IDENTITY,
        [JobIsneId] int NOT NULL,
        [SortOrder] int NOT NULL,
        [ContainerNo] nvarchar(15) NULL,
        [ContainerSize] nvarchar(10) NOT NULL,
        [ShipmentType] int NOT NULL,
        [NoPackages] int NOT NULL,
        [PackageType] nvarchar(40) NULL,
        [GrossWeight] decimal(18,3) NULL,
        [GrossWeightUnit] nvarchar(10) NOT NULL,
        [NetWeight] decimal(18,3) NULL,
        [NetWeightUnit] nvarchar(10) NOT NULL,
        [MarksSerial] nvarchar(200) NULL,
        [CustomsCode] nvarchar(60) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_JobIsneContainers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_JobIsneContainers_JobIsnes_JobIsneId] FOREIGN KEY ([JobIsneId]) REFERENCES [JobIsnes] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145356_CombineJobIsneContainerGrid'
)
BEGIN
    CREATE INDEX [IX_JobIsneContainers_ContainerNo] ON [JobIsneContainers] ([ContainerNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145356_CombineJobIsneContainerGrid'
)
BEGIN
    CREATE INDEX [IX_JobIsneContainers_JobIsneId] ON [JobIsneContainers] ([JobIsneId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717145356_CombineJobIsneContainerGrid'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717145356_CombineJobIsneContainerGrid', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717152314_AddEntryForDataSheetFields'
)
BEGIN
    ALTER TABLE [JobIsnes] ADD [CertificateOfOrigin] nvarchar(30) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717152314_AddEntryForDataSheetFields'
)
BEGIN
    ALTER TABLE [JobIsnes] ADD [CertificateOfOriginDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717152314_AddEntryForDataSheetFields'
)
BEGIN
    ALTER TABLE [JobIsnes] ADD [ImporterCode] nvarchar(10) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717152314_AddEntryForDataSheetFields'
)
BEGIN
    ALTER TABLE [JobIsnes] ADD [InsuranceCompanyNameAddress] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717152314_AddEntryForDataSheetFields'
)
BEGIN
    ALTER TABLE [JobIsnes] ADD [InvoiceDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717152314_AddEntryForDataSheetFields'
)
BEGIN
    ALTER TABLE [JobIsnes] ADD [InvoiceNumber] nvarchar(20) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717152314_AddEntryForDataSheetFields'
)
BEGIN
    ALTER TABLE [JobIsnes] ADD [SensitiveCargo] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717152314_AddEntryForDataSheetFields'
)
BEGIN
    ALTER TABLE [JobIsnes] ADD [SensitiveCifValue] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717152314_AddEntryForDataSheetFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717152314_AddEntryForDataSheetFields', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717160937_MakeDataSheetFieldsOptional'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'InvoiceNumber');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [JobIsnes] ALTER COLUMN [InvoiceNumber] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717160937_MakeDataSheetFieldsOptional'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'InvoiceDate');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [JobIsnes] ALTER COLUMN [InvoiceDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717160937_MakeDataSheetFieldsOptional'
)
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'ImporterCode');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [JobIsnes] ALTER COLUMN [ImporterCode] nvarchar(10) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717160937_MakeDataSheetFieldsOptional'
)
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'CertificateOfOriginDate');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var22 + '];');
    ALTER TABLE [JobIsnes] ALTER COLUMN [CertificateOfOriginDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717160937_MakeDataSheetFieldsOptional'
)
BEGIN
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'CertificateOfOrigin');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var23 + '];');
    ALTER TABLE [JobIsnes] ALTER COLUMN [CertificateOfOrigin] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717160937_MakeDataSheetFieldsOptional'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717160937_MakeDataSheetFieldsOptional', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718054304_ShrinkImporterCodeToSixChars'
)
BEGIN
    UPDATE [JobIsnes] SET [ImporterCode] = NULL WHERE [ImporterCode] IS NOT NULL AND LEN([ImporterCode]) > 6;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718054304_ShrinkImporterCodeToSixChars'
)
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[JobIsnes]') AND [c].[name] = N'ImporterCode');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [JobIsnes] DROP CONSTRAINT [' + @var24 + '];');
    ALTER TABLE [JobIsnes] ALTER COLUMN [ImporterCode] nvarchar(6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718054304_ShrinkImporterCodeToSixChars'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260718054304_ShrinkImporterCodeToSixChars', N'8.0.11');
END;
GO

COMMIT;
GO

