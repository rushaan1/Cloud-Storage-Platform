﻿using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.IdentityEntites;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Infrastructure.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Core.Domain.Entities.File> Files { get; set; }
        public DbSet<Sharing> Shares { get; set; }
        public DbSet<Metadata> MetaDatasets { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public ApplicationDbContext(DbContextOptions options) : base(options) {}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Folder>().ToTable("Folders");
            modelBuilder.Entity<Core.Domain.Entities.File>().ToTable("Files");
            modelBuilder.Entity<Sharing>().ToTable("Shares");
            modelBuilder.Entity<Metadata>().ToTable("Metadatasets");
            modelBuilder.Entity<UserSession>().ToTable("UserSessions");

            modelBuilder.Entity<Folder>()
                .HasOne(f => f.ParentFolder)
                .WithMany(f => f.SubFolders)
                .HasForeignKey(f => f.ParentFolderId);

            modelBuilder.Entity<Core.Domain.Entities.File>()
                .HasOne(f => f.ParentFolder)
                .WithMany(p => p.Files)
                .HasForeignKey(f => f.ParentFolderId);

            modelBuilder.Entity<Folder>()
                .HasOne(f => f.Metadata)
                .WithOne(m => m.Folder)
                .HasForeignKey<Folder>(f => f.MetadataId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Core.Domain.Entities.File>()
                .HasOne(f => f.Metadata)
                .WithOne(m => m.File)
                .HasForeignKey<Core.Domain.Entities.File>(f => f.MetadataId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<Folder>()
                .HasOne(f => f.Sharing)
                .WithOne(s => s.Folder)
                .HasForeignKey<Folder>(f => f.SharingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Core.Domain.Entities.File>()
                .HasOne(f => f.Sharing)
                .WithOne(s => s.File)
                .HasForeignKey<Core.Domain.Entities.File>(f => f.SharingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u=>u.Sessions)
                .WithOne(s=>s.User)
                .HasForeignKey(s=>s.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Folders)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Files)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.MetaDatasets)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Shares)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
