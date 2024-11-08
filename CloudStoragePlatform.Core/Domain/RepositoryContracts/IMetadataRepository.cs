using CloudStoragePlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.RepositoryContracts
{
    public interface IMetadataRepository
    {
        /// <summary>
        /// Retrieves Metadata using its Guid.
        /// </summary>
        /// <param name="id">The Guid of the metadata.</param>
        /// <returns>The Metadata.</returns>
        Task<Metadata?> GetMetadataByMetadatId(Guid id);
        /// <summary>
        /// Update an existing metadata.
        /// </summary>
        /// <param name="metadata">The existing metadata entity.</param>
        /// <returns>The updated metadata.</returns>
        Task<Metadata?> UpdateMetadata(Metadata metadata);
    }
}
