using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Infrastructure.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Infrastructure.Repositories
{
    internal static class LoadingHelperMethods
    {
        internal static async Task LoadFolderNavigationProperties(ApplicationDbContext _db, Folder folder, bool shouldBeLoaded)
        {
            if (shouldBeLoaded)
            {
                await _db.Entry(folder).Reference(f => f.ParentFolder).LoadAsync();
                await _db.Entry(folder).Collection(f => f.SubFolders).LoadAsync();
                await _db.Entry(folder).Collection(f => f.Files).LoadAsync();
                await _db.Entry(folder).Reference(f => f.Metadata).LoadAsync();
                await _db.Entry(folder).Reference(f => f.Sharing).LoadAsync();
            }
        }

        internal static async Task LoadFolderNavigationPropertiesForMultipleEntities(ApplicationDbContext _db, IEnumerable<Folder> folders, bool shouldBeLoaded)
        {
            foreach (var folder in folders)
            {
                await LoadFolderNavigationProperties(_db, folder, shouldBeLoaded);
            }
        }

        internal static async Task LoadFileNavigationProperties(ApplicationDbContext _db, Core.Domain.Entities.File file, bool shouldBeLoaded)
        {
            if (shouldBeLoaded)
            {
                await _db.Entry(file).Reference(f => f.ParentFolder).LoadAsync();
                await _db.Entry(file).Reference(f => f.Metadata).LoadAsync();
                await _db.Entry(file).Reference(f => f.Sharing).LoadAsync();
            }
        }

        internal static async Task LoadFileNavigationPropertiesForMultipleEntities(ApplicationDbContext _db, IEnumerable<Core.Domain.Entities.File> files, bool shouldBeLoaded)
        {
            foreach (var file in files)
            {
                await LoadFileNavigationProperties(_db, file, shouldBeLoaded);
            }
        }
    }
}
