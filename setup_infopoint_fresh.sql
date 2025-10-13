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
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    IF SCHEMA_ID(N'InfoPoint') IS NULL EXEC(N'CREATE SCHEMA [InfoPoint];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [InfoPoint].[AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [InfoPoint].[AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(max) NULL,
        [GoogleId] nvarchar(max) NULL,
        [DateCreated] datetime2 NOT NULL,
        [LastLoginDate] datetime2 NOT NULL,
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
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [HRAuthorizedUsers] (
        [Id] int NOT NULL IDENTITY,
        [Email] nvarchar(100) NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_HRAuthorizedUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [PDRQuestions] (
        [Id] int NOT NULL IDENTITY,
        [StaffQuestionText] nvarchar(500) NOT NULL,
        [ManagerQuestionText] nvarchar(500) NULL,
        [Category] nvarchar(100) NOT NULL,
        [Order] int NOT NULL,
        [IsActive] bit NOT NULL,
        [HasManagerQuestion] bit NOT NULL,
        [Description] nvarchar(1000) NULL,
        CONSTRAINT [PK_PDRQuestions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [PDRs] (
        [Id] int NOT NULL IDENTITY,
        [StaffReference] nvarchar(8) NOT NULL,
        [Year] int NOT NULL,
        [Month] int NOT NULL,
        [Period] nvarchar(50) NULL,
        [Status] int NOT NULL,
        [AssignedDate] datetime2 NOT NULL,
        [StaffCompletedDate] datetime2 NULL,
        [ManagerCompletedDate] datetime2 NULL,
        [CollaborativeCompletedDate] datetime2 NULL,
        [DueDate] datetime2 NULL,
        [CreatedDate] datetime2 NOT NULL,
        [LastUpdated] datetime2 NULL,
        CONSTRAINT [PK_PDRs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [StaffHR] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeNumber] int NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [HierarchyLevel1] nvarchar(200) NULL,
        [HierarchyLevel2] nvarchar(200) NULL,
        [HierarchyLevel3] nvarchar(200) NULL,
        [HierarchyLevel4] nvarchar(200) NULL,
        [HierarchyLevel5] nvarchar(200) NULL,
        [JobTitle] nvarchar(200) NOT NULL,
        [ContractType] nvarchar(100) NOT NULL,
        [ManagerEmployeeNumber] int NULL,
        [ManagerName] nvarchar(200) NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [LastUpdated] datetime2 NULL,
        CONSTRAINT [PK_StaffHR] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [InfoPoint].[AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [InfoPoint].[AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [InfoPoint].[AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [InfoPoint].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [InfoPoint].[AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [InfoPoint].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [InfoPoint].[AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [InfoPoint].[AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [InfoPoint].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [InfoPoint].[AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [InfoPoint].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [PDRComparisons] (
        [Id] int NOT NULL IDENTITY,
        [PDRId] int NOT NULL,
        [QuestionId] int NOT NULL,
        [StaffResponse] TEXT NULL,
        [ManagerResponse] TEXT NULL,
        [StaffRating] int NULL,
        [ManagerRating] int NULL,
        [CollaborativeResponse] TEXT NULL,
        [CollaborativeRating] int NULL,
        [ComparisonNotes] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_PDRComparisons] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PDRComparisons_PDRQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [PDRQuestions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PDRComparisons_PDRs_PDRId] FOREIGN KEY ([PDRId]) REFERENCES [PDRs] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [PDRResponses] (
        [Id] int NOT NULL IDENTITY,
        [PDRId] int NOT NULL,
        [QuestionId] int NOT NULL,
        [ResponseType] int NOT NULL,
        [Response] TEXT NOT NULL,
        [Rating] int NULL,
        [ResponseDate] datetime2 NOT NULL,
        [Notes] nvarchar(1000) NULL,
        CONSTRAINT [PK_PDRResponses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PDRResponses_PDRQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [PDRQuestions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PDRResponses_PDRs_PDRId] FOREIGN KEY ([PDRId]) REFERENCES [PDRs] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE TABLE [SmartTargets] (
        [Id] int NOT NULL IDENTITY,
        [PDRId] int NOT NULL,
        [Title] nvarchar(500) NOT NULL,
        [Description] TEXT NOT NULL,
        [Specific] bit NOT NULL,
        [Measurable] bit NOT NULL,
        [Achievable] bit NOT NULL,
        [Relevant] bit NOT NULL,
        [TimeBound] bit NOT NULL,
        [TargetDate] datetime2 NOT NULL,
        [Priority] nvarchar(50) NULL,
        [Category] nvarchar(100) NULL,
        [Status] nvarchar(50) NULL,
        [SuccessCriteria] TEXT NULL,
        [ActionPlan] TEXT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [LastUpdated] datetime2 NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_SmartTargets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SmartTargets_PDRs_PDRId] FOREIGN KEY ([PDRId]) REFERENCES [PDRs] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [InfoPoint].[AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [InfoPoint].[AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [InfoPoint].[AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [InfoPoint].[AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [InfoPoint].[AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [InfoPoint].[AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [InfoPoint].[AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HRAuthorizedUsers_Email] ON [HRAuthorizedUsers] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PDRComparisons_PDRId_QuestionId] ON [PDRComparisons] ([PDRId], [QuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE INDEX [IX_PDRComparisons_QuestionId] ON [PDRComparisons] ([QuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE INDEX [IX_PDRQuestions_Category_Order] ON [PDRQuestions] ([Category], [Order]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PDRResponses_PDRId_QuestionId_ResponseType] ON [PDRResponses] ([PDRId], [QuestionId], [ResponseType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE INDEX [IX_PDRResponses_QuestionId] ON [PDRResponses] ([QuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PDRs_StaffReference_Year] ON [PDRs] ([StaffReference], [Year]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE INDEX [IX_SmartTargets_PDRId_DisplayOrder] ON [SmartTargets] ([PDRId], [DisplayOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StaffHR_EmployeeNumber] ON [StaffHR] ([EmployeeNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251009102501_InitialWithInfoPointSchema', N'8.0.11');
END;
GO

COMMIT;
GO

