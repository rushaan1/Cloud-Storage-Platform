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
        /// <summary>
        /// Retrieves sharing information by its Guid.
        /// </summary>
        /// <param name="id">The sharing Guid.</param>
        /// <returns>The sharing entity with the specified ID, or null if not found.</returns>
        Task<Sharing?> GetSharingById(Guid id);

        /// <summary>
        /// Creates a new sharing entry.
        /// </summary>
        Task<Sharing> CreateSharing(Sharing sharing);

        /// <summary>
        /// Deletes an existing sharing entry.
        /// </summary>
        Task<bool> DeleteSharing(Sharing sharing);

        /// <summary>
        /// Increments the visit count for a sharing entry by 1 and saves.
        /// </summary>
        /// <param name="sharingId">The sharing Guid.</param>
        /// <returns>The updated visits count.</returns>
        Task<int> IncrementVisits(Guid sharingId);

        /// <summary>
        /// Sets the visit count for a sharing entry and saves.
        /// </summary>
        /// <param name="sharingId">The sharing Guid.</param>
        /// <param name="visits">The new visits count.</param>
        /// <returns>A task.</returns>
        Task SetVisits(Guid sharingId, int visits);

        /// <summary>
        /// Updates an existing sharing entity.
        /// </summary>
        /// <param name="sharing">The sharing entity with updated values.</param>
        /// <returns>The updated sharing entity, or null if not found.</returns>
        Task<Sharing?> UpdateSharing(Sharing sharing);
    }
}
