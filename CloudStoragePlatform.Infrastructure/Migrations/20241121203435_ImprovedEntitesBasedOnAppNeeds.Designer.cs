﻿// <auto-generated />
using System;
using CloudStoragePlatform.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CloudStoragePlatform.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241121203435_ImprovedEntitesBasedOnAppNeeds")]
    partial class ImprovedEntitesBasedOnAppNeeds
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.35")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("CloudStoragePlatform.Core.Domain.Entities.File", b =>
                {
                    b.Property<Guid>("FileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("FileType")
                        .HasColumnType("int");

                    b.Property<bool>("IsFavorite")
                        .HasColumnType("bit");

                    b.Property<bool>("IsTrash")
                        .HasColumnType("bit");

                    b.Property<Guid>("MetadataId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ParentFolderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("RecentsRecentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("SharingId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("FileId");

                    b.HasIndex("MetadataId")
                        .IsUnique();

                    b.HasIndex("ParentFolderId");

                    b.HasIndex("RecentsRecentId");

                    b.HasIndex("SharingId")
                        .IsUnique();

                    b.ToTable("Files", (string)null);
                });

            modelBuilder.Entity("CloudStoragePlatform.Core.Domain.Entities.Folder", b =>
                {
                    b.Property<Guid>("FolderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("FolderName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("FolderParentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("FolderPath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsFavorite")
                        .HasColumnType("bit");

                    b.Property<bool>("IsTrash")
                        .HasColumnType("bit");

                    b.Property<Guid>("MetadataId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("RecentsRecentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("SharingId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("FolderId");

                    b.HasIndex("FolderParentId");

                    b.HasIndex("MetadataId")
                        .IsUnique();

                    b.HasIndex("RecentsRecentId");

                    b.HasIndex("SharingId")
                        .IsUnique();

                    b.ToTable("Folders", (string)null);
                });

            modelBuilder.Entity("CloudStoragePlatform.Core.Domain.Entities.Metadata", b =>
                {
                    b.Property<Guid>("MetadataId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("LastOpened")
                        .HasColumnType("datetime2");

                    b.Property<int?>("MoveCount")
                        .HasColumnType("int");

                    b.Property<int?>("OpenCount")
                        .HasColumnType("int");

                    b.Property<DateTime?>("PreviousMoveDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("PreviousPath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("PreviousRenameDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("PreviousReplacementDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("RenameCount")
                        .HasColumnType("int");

                    b.Property<int?>("ReplaceCount")
                        .HasColumnType("int");

                    b.Property<int?>("ShareCount")
                        .HasColumnType("int");

                    b.Property<long?>("Size")
                        .HasColumnType("bigint");

                    b.HasKey("MetadataId");

                    b.ToTable("Metadatasets", (string)null);
                });

            modelBuilder.Entity("CloudStoragePlatform.Core.Domain.Entities.Recents", b =>
                {
                    b.Property<Guid>("RecentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("RecentId");

                    b.ToTable("Recents", (string)null);
                });

            modelBuilder.Entity("CloudStoragePlatform.Core.Domain.Entities.Sharing", b =>
                {
                    b.Property<Guid>("SharingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("CurrentShareLinkTimesSeen")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ShareLinkCreateDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ShareLinkExpiry")
                        .HasColumnType("datetime2");

                    b.Property<string>("ShareLinkUrl")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SharingId");

                    b.ToTable("Shares", (string)null);
                });

            modelBuilder.Entity("CloudStoragePlatform.Core.Domain.Entities.File", b =>
                {
                    b.HasOne("CloudStoragePlatform.Core.Domain.Entities.Metadata", "Metadata")
                        .WithOne("File")
                        .HasForeignKey("CloudStoragePlatform.Core.Domain.Entities.File", "MetadataId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CloudStoragePlatform.Core.Domain.Entities.Folder", "ParentFolder")
                        .WithMany("Files")
                        .HasForeignKey("ParentFolderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CloudStoragePlatform.Core.Domain.Entities.Recents", null)
                        .WithMany("RecentFiles")
                        .HasForeignKey("RecentsRecentId");

                    b.HasOne("CloudStoragePlatform.Core.Domain.Entities.Sharing", "Sharing")
                        .WithOne("File")
                        .HasForeignKey("CloudStoragePlatform.Core.Domain.Entities.File", "SharingId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Metadata");

                    b.Navigation("ParentFolder");

                    b.Navigation("Sharing");
                });

            modelBuilder.Entity("CloudStoragePlatform.Core.Domain.Entities.Folder", b =>
                {
                    b.HasOne("CloudStoragePlatform.Core.Domain.Entities.Folder", "ParentFolder")
                        .WithMany("SubFolders")
                        .HasForeignKey("FolderParentId");

                    b.HasOne("CloudStoragePlatform.Core.Domain.Entities.Metadata", "Metadata")
                        .WithOne("Folder")
                        .HasForeignKey("CloudStoragePlatform.Core.Domain.Entities.Folder", "MetadataId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CloudStoragePlatform.Core.Domain.Entities.Recents", null)
                        .WithMany("RecentFolders")
                        .HasForeignKey("RecentsRecentId");

                    b.HasOne("CloudStoragePlatform.Core.Domain.Entities.Sharing", "Sharing")
                        .WithOne("Folder")
                        .HasForeignKey("CloudStoragePlatform.Core.Domain.Entities.Folder", "SharingId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Metadata");

                    b.Navigation("ParentFolder");

                    b.Navigation("Sharing");
                });

            modelBuilder.Entity("CloudStoragePlatform.Core.Domain.Entities.Folder", b =>
                {
                    b.Navigation("Files");

                    b.Navigation("SubFolders");
                });

            modelBuilder.Entity("CloudStoragePlatform.Core.Domain.Entities.Metadata", b =>
                {
                    b.Navigation("File");

                    b.Navigation("Folder");
                });

            modelBuilder.Entity("CloudStoragePlatform.Core.Domain.Entities.Recents", b =>
                {
                    b.Navigation("RecentFiles");

                    b.Navigation("RecentFolders");
                });

            modelBuilder.Entity("CloudStoragePlatform.Core.Domain.Entities.Sharing", b =>
                {
                    b.Navigation("File");

                    b.Navigation("Folder");
                });
#pragma warning restore 612, 618
        }
    }
}