using CloudStoragePlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.RepositoryContracts
{
    public interface IRecentsRepository
    {
        /// <summary>
        /// Retrieve recents entity by Guid.
        /// </summary>
        /// <param name="id">The Guid of the recents entity.</param>
        /// <returns>The recents entity.</returns>
        Task<Recents?> GetRecentsById(Guid id);
        /// <summary>
        /// Update recents entity.
        /// </summary>
        /// <param name="recents">Recents entity to update.</param>
        /// <param name="updateFolders">If true recent entity's folders will be updated.</param>
        /// <param name="updateFiles">If true recent entity's files will be updated.</param>
        /// <returns>The updated recents entity.</returns>
        Task<Recents?> UpdateRecents(Recents recents, bool updateFolders, bool updateFiles);
    }
}
