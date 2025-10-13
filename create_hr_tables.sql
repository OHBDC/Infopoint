-- HR Management Tables for InfoPoint
-- Run this script on your SQL Server database (DigitalLearning)

-- Create HRAuthorizedUsers table
CREATE TABLE [HRAuthorizedUsers] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(100) NOT NULL,
    [FullName] nvarchar(200) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_HRAuthorizedUsers] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_HRAuthorizedUsers_Email] ON [HRAuthorizedUsers] ([Email]);
GO

-- Create StaffHR table
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
GO

CREATE UNIQUE INDEX [IX_StaffHR_EmployeeNumber] ON [StaffHR] ([EmployeeNumber]);
GO

-- Insert initial HR authorized user (oliver.hill@g.bdc.ac.uk)
INSERT INTO [HRAuthorizedUsers] ([Email], [FullName], [IsActive], [CreatedDate], [CreatedBy])
VALUES ('oliver.hill@g.bdc.ac.uk', 'Oliver Hill', 1, GETDATE(), 'System Setup');
GO

-- Create migrations history table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

-- Mark migration as applied
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251009095650_InitialSQLServerMigration', N'8.0.11');
GO

PRINT 'HR Management tables created successfully!';
