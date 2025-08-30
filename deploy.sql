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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    CREATE TABLE [Metadatasets] (
        [MetadataId] uniqueidentifier NOT NULL,
        [PreviousReplacementDate] datetime2 NULL,
        [ReplaceCount] int NULL,
        [PreviousRenameDate] datetime2 NULL,
        [RenameCount] int NULL,
        [PreviousPath] nvarchar(max) NULL,
        [PreviousMoveDate] datetime2 NULL,
        [MoveCount] int NULL,
        [LastOpened] datetime2 NULL,
        [OpenCount] int NULL,
        [ShareCount] int NULL,
        CONSTRAINT [PK_Metadatasets] PRIMARY KEY ([MetadataId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    CREATE TABLE [Recents] (
        [RecentId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Recents] PRIMARY KEY ([RecentId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    CREATE TABLE [Shares] (
        [SharingId] uniqueidentifier NOT NULL,
        [ShareLinkUrl] nvarchar(max) NULL,
        [ShareLinkExpiry] datetime2 NULL,
        [CurrentShareLinkTimesSeen] int NULL,
        [ShareLinkCreateDate] datetime2 NULL,
        CONSTRAINT [PK_Shares] PRIMARY KEY ([SharingId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    CREATE TABLE [Folders] (
        [FolderId] uniqueidentifier NOT NULL,
        [FolderName] nvarchar(max) NOT NULL,
        [FolderPath] nvarchar(max) NOT NULL,
        [FolderParentId] uniqueidentifier NULL,
        [FolderSize] bigint NULL,
        [RecentsRecentId] uniqueidentifier NULL,
        [SharingId] uniqueidentifier NOT NULL,
        [MetadataId] uniqueidentifier NOT NULL,
        [CreationDate] datetime2 NOT NULL,
        [IsFavorite] bit NOT NULL,
        [IsTrash] bit NOT NULL,
        CONSTRAINT [PK_Folders] PRIMARY KEY ([FolderId]),
        CONSTRAINT [FK_Folders_Folders_FolderParentId] FOREIGN KEY ([FolderParentId]) REFERENCES [Folders] ([FolderId]),
        CONSTRAINT [FK_Folders_Metadatasets_MetadataId] FOREIGN KEY ([MetadataId]) REFERENCES [Metadatasets] ([MetadataId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Folders_Recents_RecentsRecentId] FOREIGN KEY ([RecentsRecentId]) REFERENCES [Recents] ([RecentId]),
        CONSTRAINT [FK_Folders_Shares_SharingId] FOREIGN KEY ([SharingId]) REFERENCES [Shares] ([SharingId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    CREATE TABLE [Files] (
        [FileId] uniqueidentifier NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [FileSize] bigint NOT NULL,
        [FileType] int NOT NULL,
        [ParentFolderId] uniqueidentifier NOT NULL,
        [RecentsRecentId] uniqueidentifier NULL,
        [SharingId] uniqueidentifier NOT NULL,
        [MetadataId] uniqueidentifier NOT NULL,
        [CreationDate] datetime2 NOT NULL,
        [IsFavorite] bit NOT NULL,
        [IsTrash] bit NOT NULL,
        CONSTRAINT [PK_Files] PRIMARY KEY ([FileId]),
        CONSTRAINT [FK_Files_Folders_ParentFolderId] FOREIGN KEY ([ParentFolderId]) REFERENCES [Folders] ([FolderId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Files_Metadatasets_MetadataId] FOREIGN KEY ([MetadataId]) REFERENCES [Metadatasets] ([MetadataId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Files_Recents_RecentsRecentId] FOREIGN KEY ([RecentsRecentId]) REFERENCES [Recents] ([RecentId]),
        CONSTRAINT [FK_Files_Shares_SharingId] FOREIGN KEY ([SharingId]) REFERENCES [Shares] ([SharingId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    CREATE UNIQUE INDEX [IX_Files_MetadataId] ON [Files] ([MetadataId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    CREATE INDEX [IX_Files_ParentFolderId] ON [Files] ([ParentFolderId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    CREATE INDEX [IX_Files_RecentsRecentId] ON [Files] ([RecentsRecentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    CREATE UNIQUE INDEX [IX_Files_SharingId] ON [Files] ([SharingId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    CREATE INDEX [IX_Folders_FolderParentId] ON [Folders] ([FolderParentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    CREATE UNIQUE INDEX [IX_Folders_MetadataId] ON [Folders] ([MetadataId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    CREATE INDEX [IX_Folders_RecentsRecentId] ON [Folders] ([RecentsRecentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    CREATE UNIQUE INDEX [IX_Folders_SharingId] ON [Folders] ([SharingId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241102165813_Initial')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241102165813_Initial', N'6.0.35');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241121203435_ImprovedEntitesBasedOnAppNeeds')
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Folders]') AND [c].[name] = N'FolderSize');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Folders] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Folders] DROP COLUMN [FolderSize];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241121203435_ImprovedEntitesBasedOnAppNeeds')
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Files]') AND [c].[name] = N'FileSize');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Files] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Files] DROP COLUMN [FileSize];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241121203435_ImprovedEntitesBasedOnAppNeeds')
BEGIN
    ALTER TABLE [Metadatasets] ADD [Size] bigint NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241121203435_ImprovedEntitesBasedOnAppNeeds')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241121203435_ImprovedEntitesBasedOnAppNeeds', N'6.0.35');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241207203529_SeedingFolder')
BEGIN
    DROP INDEX [IX_Folders_MetadataId] ON [Folders];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241207203529_SeedingFolder')
BEGIN
    DROP INDEX [IX_Folders_SharingId] ON [Folders];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241207203529_SeedingFolder')
BEGIN
    DROP INDEX [IX_Files_MetadataId] ON [Files];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241207203529_SeedingFolder')
BEGIN
    DROP INDEX [IX_Files_SharingId] ON [Files];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241207203529_SeedingFolder')
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Folders]') AND [c].[name] = N'SharingId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Folders] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Folders] ALTER COLUMN [SharingId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241207203529_SeedingFolder')
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Folders]') AND [c].[name] = N'MetadataId');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Folders] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Folders] ALTER COLUMN [MetadataId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241207203529_SeedingFolder')
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Files]') AND [c].[name] = N'SharingId');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Files] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Files] ALTER COLUMN [SharingId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241207203529_SeedingFolder')
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Files]') AND [c].[name] = N'MetadataId');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Files] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Files] ALTER COLUMN [MetadataId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241207203529_SeedingFolder')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Folders_MetadataId] ON [Folders] ([MetadataId]) WHERE [MetadataId] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241207203529_SeedingFolder')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Folders_SharingId] ON [Folders] ([SharingId]) WHERE [SharingId] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241207203529_SeedingFolder')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Files_MetadataId] ON [Files] ([MetadataId]) WHERE [MetadataId] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241207203529_SeedingFolder')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Files_SharingId] ON [Files] ([SharingId]) WHERE [SharingId] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241207203529_SeedingFolder')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241207203529_SeedingFolder', N'6.0.35');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241208034952_RenamedParentId')
BEGIN
    ALTER TABLE [Folders] DROP CONSTRAINT [FK_Folders_Folders_FolderParentId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241208034952_RenamedParentId')
BEGIN
    EXEC sp_rename N'[Folders].[FolderParentId]', N'ParentFolderId', N'COLUMN';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241208034952_RenamedParentId')
BEGIN
    EXEC sp_rename N'[Folders].[IX_Folders_FolderParentId]', N'IX_Folders_ParentFolderId', N'INDEX';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241208034952_RenamedParentId')
BEGIN
    EXEC(N'UPDATE [Folders] SET [CreationDate] = ''2024-12-08T09:19:52.2629408+05:30''
    WHERE [FolderId] = ''9e2abd0a-94ac-43e2-a212-9dc9f7590447'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241208034952_RenamedParentId')
BEGIN
    ALTER TABLE [Folders] ADD CONSTRAINT [FK_Folders_Folders_ParentFolderId] FOREIGN KEY ([ParentFolderId]) REFERENCES [Folders] ([FolderId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20241208034952_RenamedParentId')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20241208034952_RenamedParentId', N'6.0.35');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250320165005_AddedDefaultCountVals')
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Metadatasets]') AND [c].[name] = N'Size');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Metadatasets] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Metadatasets] ALTER COLUMN [Size] bigint NOT NULL;
    ALTER TABLE [Metadatasets] ADD DEFAULT CAST(0 AS bigint) FOR [Size];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250320165005_AddedDefaultCountVals')
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Metadatasets]') AND [c].[name] = N'ShareCount');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Metadatasets] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Metadatasets] ALTER COLUMN [ShareCount] int NOT NULL;
    ALTER TABLE [Metadatasets] ADD DEFAULT 0 FOR [ShareCount];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250320165005_AddedDefaultCountVals')
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Metadatasets]') AND [c].[name] = N'ReplaceCount');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Metadatasets] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Metadatasets] ALTER COLUMN [ReplaceCount] int NOT NULL;
    ALTER TABLE [Metadatasets] ADD DEFAULT 0 FOR [ReplaceCount];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250320165005_AddedDefaultCountVals')
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Metadatasets]') AND [c].[name] = N'RenameCount');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Metadatasets] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [Metadatasets] ALTER COLUMN [RenameCount] int NOT NULL;
    ALTER TABLE [Metadatasets] ADD DEFAULT 0 FOR [RenameCount];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250320165005_AddedDefaultCountVals')
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Metadatasets]') AND [c].[name] = N'OpenCount');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Metadatasets] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [Metadatasets] ALTER COLUMN [OpenCount] int NOT NULL;
    ALTER TABLE [Metadatasets] ADD DEFAULT 0 FOR [OpenCount];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250320165005_AddedDefaultCountVals')
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Metadatasets]') AND [c].[name] = N'MoveCount');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Metadatasets] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [Metadatasets] ALTER COLUMN [MoveCount] int NOT NULL;
    ALTER TABLE [Metadatasets] ADD DEFAULT 0 FOR [MoveCount];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250320165005_AddedDefaultCountVals')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250320165005_AddedDefaultCountVals', N'6.0.35');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250407082345_RemovedReplaceRelatedPropertiesInMetadata')
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Metadatasets]') AND [c].[name] = N'PreviousReplacementDate');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Metadatasets] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [Metadatasets] DROP COLUMN [PreviousReplacementDate];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250407082345_RemovedReplaceRelatedPropertiesInMetadata')
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Metadatasets]') AND [c].[name] = N'ReplaceCount');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Metadatasets] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [Metadatasets] DROP COLUMN [ReplaceCount];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250407082345_RemovedReplaceRelatedPropertiesInMetadata')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250407082345_RemovedReplaceRelatedPropertiesInMetadata', N'6.0.35');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250429193619_SizePropertyMoved')
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Metadatasets]') AND [c].[name] = N'Size');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Metadatasets] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [Metadatasets] DROP COLUMN [Size];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250429193619_SizePropertyMoved')
BEGIN
    ALTER TABLE [Folders] ADD [Size] real NOT NULL DEFAULT CAST(0 AS real);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250429193619_SizePropertyMoved')
BEGIN
    ALTER TABLE [Files] ADD [Size] real NOT NULL DEFAULT CAST(0 AS real);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250429193619_SizePropertyMoved')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250429193619_SizePropertyMoved', N'6.0.35');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250501144157_RemovedRecentsTable')
BEGIN
    ALTER TABLE [Files] DROP CONSTRAINT [FK_Files_Recents_RecentsRecentId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250501144157_RemovedRecentsTable')
BEGIN
    ALTER TABLE [Folders] DROP CONSTRAINT [FK_Folders_Recents_RecentsRecentId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250501144157_RemovedRecentsTable')
BEGIN
    DROP TABLE [Recents];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250501144157_RemovedRecentsTable')
BEGIN
    DROP INDEX [IX_Folders_RecentsRecentId] ON [Folders];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250501144157_RemovedRecentsTable')
BEGIN
    DROP INDEX [IX_Files_RecentsRecentId] ON [Files];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250501144157_RemovedRecentsTable')
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Folders]') AND [c].[name] = N'RecentsRecentId');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Folders] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [Folders] DROP COLUMN [RecentsRecentId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250501144157_RemovedRecentsTable')
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Files]') AND [c].[name] = N'RecentsRecentId');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Files] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [Files] DROP COLUMN [RecentsRecentId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250501144157_RemovedRecentsTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250501144157_RemovedRecentsTable', N'6.0.35');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [PersonName] nvarchar(max) NULL,
        [RefreshToken] nvarchar(max) NULL,
        [RefreshTokenExpirationDateTime] datetime2 NOT NULL,
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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706112553_IdentityUpdate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250706112553_IdentityUpdate', N'6.0.35');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706183600_UserPropertyUpdated')
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Country] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250706183600_UserPropertyUpdated')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250706183600_UserPropertyUpdated', N'6.0.35');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250709115109_AddedUserSessions')
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'RefreshToken');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [AspNetUsers] DROP COLUMN [RefreshToken];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250709115109_AddedUserSessions')
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'RefreshTokenExpirationDateTime');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [AspNetUsers] DROP COLUMN [RefreshTokenExpirationDateTime];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250709115109_AddedUserSessions')
BEGIN
    CREATE TABLE [UserSessions] (
        [UserSessionId] uniqueidentifier NOT NULL,
        [RefreshToken] nvarchar(max) NOT NULL,
        [RefreshTokenExpirationDateTime] datetime2 NOT NULL,
        [ApplicationUserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserSessions] PRIMARY KEY ([UserSessionId]),
        CONSTRAINT [FK_UserSessions_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250709115109_AddedUserSessions')
BEGIN
    CREATE INDEX [IX_UserSessions_ApplicationUserId] ON [UserSessions] ([ApplicationUserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250709115109_AddedUserSessions')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250709115109_AddedUserSessions', N'6.0.35');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250715094534_CreatedAtPropertyInUser')
BEGIN
    ALTER TABLE [AspNetUsers] ADD [CreatedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250715094534_CreatedAtPropertyInUser')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250715094534_CreatedAtPropertyInUser', N'6.0.35');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250721133126_UserSpecificDbStorage')
BEGIN
    ALTER TABLE [Shares] ADD [UserId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250721133126_UserSpecificDbStorage')
BEGIN
    ALTER TABLE [Metadatasets] ADD [UserId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250721133126_UserSpecificDbStorage')
BEGIN
    ALTER TABLE [Folders] ADD [UserId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250721133126_UserSpecificDbStorage')
BEGIN
    ALTER TABLE [Files] ADD [UserId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250721133126_UserSpecificDbStorage')
BEGIN
    CREATE INDEX [IX_Shares_UserId] ON [Shares] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250721133126_UserSpecificDbStorage')
BEGIN
    CREATE INDEX [IX_Metadatasets_UserId] ON [Metadatasets] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250721133126_UserSpecificDbStorage')
BEGIN
    CREATE INDEX [IX_Folders_UserId] ON [Folders] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250721133126_UserSpecificDbStorage')
BEGIN
    CREATE INDEX [IX_Files_UserId] ON [Files] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250721133126_UserSpecificDbStorage')
BEGIN
    ALTER TABLE [Files] ADD CONSTRAINT [FK_Files_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250721133126_UserSpecificDbStorage')
BEGIN
    ALTER TABLE [Folders] ADD CONSTRAINT [FK_Folders_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250721133126_UserSpecificDbStorage')
BEGIN
    ALTER TABLE [Metadatasets] ADD CONSTRAINT [FK_Metadatasets_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250721133126_UserSpecificDbStorage')
BEGIN
    ALTER TABLE [Shares] ADD CONSTRAINT [FK_Shares_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250721133126_UserSpecificDbStorage')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250721133126_UserSpecificDbStorage', N'6.0.35');
END;
GO

COMMIT;
GO

