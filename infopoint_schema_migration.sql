BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102338_UseInfoPointSchema'
)
BEGIN
    ALTER TABLE [PDRs] DROP CONSTRAINT [FK_PDRs_Staff_StaffReference];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102338_UseInfoPointSchema'
)
BEGIN
    IF SCHEMA_ID(N'InfoPoint') IS NULL EXEC(N'CREATE SCHEMA [InfoPoint];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102338_UseInfoPointSchema'
)
BEGIN
    ALTER SCHEMA [InfoPoint] TRANSFER [AspNetUserTokens];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102338_UseInfoPointSchema'
)
BEGIN
    ALTER SCHEMA [InfoPoint] TRANSFER [AspNetUsers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102338_UseInfoPointSchema'
)
BEGIN
    ALTER SCHEMA [InfoPoint] TRANSFER [AspNetUserRoles];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102338_UseInfoPointSchema'
)
BEGIN
    ALTER SCHEMA [InfoPoint] TRANSFER [AspNetUserLogins];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102338_UseInfoPointSchema'
)
BEGIN
    ALTER SCHEMA [InfoPoint] TRANSFER [AspNetUserClaims];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102338_UseInfoPointSchema'
)
BEGIN
    ALTER SCHEMA [InfoPoint] TRANSFER [AspNetRoles];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102338_UseInfoPointSchema'
)
BEGIN
    ALTER SCHEMA [InfoPoint] TRANSFER [AspNetRoleClaims];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009102338_UseInfoPointSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251009102338_UseInfoPointSchema', N'8.0.11');
END;
GO

COMMIT;
GO

