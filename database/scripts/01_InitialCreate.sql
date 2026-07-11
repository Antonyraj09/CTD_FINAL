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

