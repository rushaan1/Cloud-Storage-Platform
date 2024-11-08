using CloudStoragePlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.RepositoryContracts
{
    public interface ISharingRepository
    {
        public interface ISharingRepository
        {
            /// <summary>
            /// Retrieves sharing information by its Guid.
            /// </summary>
            /// <param name="id">The sharing Guid.</param>
            /// <returns>The sharing entity with the specified ID, or null if not found.</returns>
            Task<Sharing?> GetSharingById(Guid id);

            /// <summary>
            /// Retrieves sharing information by the share link URL.
            /// </summary>
            /// <param name="url">The share link URL.</param>
            /// <returns>The sharing entity with the specified URL, or null if not found.</returns>
            Task<Sharing?> GetSharingByUrl(string url);

            /// <summary>
            /// Updates an existing sharing entity.
            /// </summary>
            /// <param name="sharing">The sharing entity with updated values.</param>
            /// <returns>The updated sharing entity, or null if not found.</returns>
            Task<Sharing?> UpdateSharing(Sharing sharing);
        }
    }
}
