using CloudStoragePlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Infrastructure.DbContext
{
    public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Core.Domain.Entities.File> Files { get; set; }
        public DbSet<Sharing> Shares { get; set; }
        public DbSet<Metadata> MetaDatasets { get; set; }
        public DbSet<Recents> Recents { get; set; }
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
            modelBuilder.Entity<Recents>().ToTable("Recents");

            modelBuilder.Entity<Folder>()
                .HasOne(f => f.ParentFolder)
                .WithMany(f => f.SubFolders)
                .HasForeignKey(f => f.FolderParentId);

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




            modelBuilder.Entity<Folder>().HasData(new Folder()
            {
                FolderId = new Guid("9E2ABD0A-94AC-43E2-A212-9DC9F7590447"),
                FolderName = "home",
                FolderPath = @"C:\CloudStoragePlatform\home",
                CreationDate = DateTime.Now,
                IsFavorite = false,
                IsTrash = false
            });
        }
    }
}
