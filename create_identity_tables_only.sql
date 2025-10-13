-- Create InfoPoint schema if it doesn't exist
IF SCHEMA_ID(N'InfoPoint') IS NULL
BEGIN
    EXEC(N'CREATE SCHEMA [InfoPoint];');
END
GO

-- Create AspNetRoles table
IF OBJECT_ID(N'[InfoPoint].[AspNetRoles]', 'U') IS NULL
BEGIN
    CREATE TABLE [InfoPoint].[AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_AspNetRoles_NormalizedName] ON [InfoPoint].[AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
END
GO

-- Create AspNetUsers table
IF OBJECT_ID(N'[InfoPoint].[AspNetUsers]', 'U') IS NULL
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

    CREATE INDEX [IX_AspNetUsers_NormalizedEmail] ON [InfoPoint].[AspNetUsers] ([NormalizedEmail]);
    CREATE UNIQUE INDEX [IX_AspNetUsers_NormalizedUserName] ON [InfoPoint].[AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
END
GO

-- Create AspNetRoleClaims table
IF OBJECT_ID(N'[InfoPoint].[AspNetRoleClaims]', 'U') IS NULL
BEGIN
    CREATE TABLE [InfoPoint].[AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [InfoPoint].[AspNetRoles] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [InfoPoint].[AspNetRoleClaims] ([RoleId]);
END
GO

-- Create AspNetUserClaims table
IF OBJECT_ID(N'[InfoPoint].[AspNetUserClaims]', 'U') IS NULL
BEGIN
    CREATE TABLE [InfoPoint].[AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [InfoPoint].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [InfoPoint].[AspNetUserClaims] ([UserId]);
END
GO

-- Create AspNetUserLogins table
IF OBJECT_ID(N'[InfoPoint].[AspNetUserLogins]', 'U') IS NULL
BEGIN
    CREATE TABLE [InfoPoint].[AspNetUserLogins] (
        [LoginProvider] nvarchar(128) NOT NULL,
        [ProviderKey] nvarchar(128) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [InfoPoint].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [InfoPoint].[AspNetUserLogins] ([UserId]);
END
GO

-- Create AspNetUserRoles table
IF OBJECT_ID(N'[InfoPoint].[AspNetUserRoles]', 'U') IS NULL
BEGIN
    CREATE TABLE [InfoPoint].[AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [InfoPoint].[AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [InfoPoint].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [InfoPoint].[AspNetUserRoles] ([RoleId]);
END
GO

-- Create AspNetUserTokens table
IF OBJECT_ID(N'[InfoPoint].[AspNetUserTokens]', 'U') IS NULL
BEGIN
    CREATE TABLE [InfoPoint].[AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(128) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [InfoPoint].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END
GO

-- Mark migration as applied
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20251009102501_InitialWithInfoPointSchema')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251009102501_InitialWithInfoPointSchema', N'9.0.0');
END
GO

PRINT 'InfoPoint Identity tables created successfully in [InfoPoint] schema';
