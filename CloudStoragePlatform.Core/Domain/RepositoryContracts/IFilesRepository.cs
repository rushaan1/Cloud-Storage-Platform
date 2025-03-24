using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.RepositoryContracts
{
    public interface IFilesRepository
    {
        /// <summary>
        /// Adds a file to the repository.
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <returns>The added file.</returns>
        Task<Core.Domain.Entities.File> AddFile(Core.Domain.Entities.File file);

        /// <summary>
        /// Retrieves all files from the repository.
        /// </summary>
        /// <returns>A list of files.</returns>
        Task<List<Core.Domain.Entities.File>> GetAllFiles();

        /// <summary>
        /// Retrieves a file by its Guid.
        /// </summary>
        /// <param name="id">The file ID.</param>
        /// <returns>The file with the specified ID, or null if not found.</returns>
        Task<Core.Domain.Entities.File?> GetFileByFileId(Guid id);

        /// <summary>
        /// Retrieves a file by its file path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The file with the specified path, or null if not found.</returns>
        Task<Core.Domain.Entities.File?> GetFileByFilePath(string path);

        /// <summary>
        /// Updates an existing file with new properties, parent folder, metadata, and sharing options.
        /// </summary>
        /// <param name="file">The file to update.</param>
        /// <param name="updateProperties">If true, updates the file's properties.</param>
        /// <param name="updateParentFolder">If true, updates the file's parent folder.</param>
        /// <param name="updateMetadata">If true, updates the file's metadata.</param>
        /// <param name="updateSharing">If true, updates the file's sharing options.</param>
        /// <returns>The updated file, or null if not found.</returns>
        Task<Core.Domain.Entities.File?> UpdateFile(Core.Domain.Entities.File file, bool updateProperties, bool updateParentFolder, bool updateMetadata, bool updateSharing);

        /// <summary>
        /// Deletes a file and associated metadata and sharing options from the repository.
        /// </summary>
        /// <param name="file">The file to delete.</param>
        /// <returns>True if the file was successfully deleted; otherwise, false.</returns>
        Task<bool> DeleteFile(Core.Domain.Entities.File file);
        /// <summary>
        /// Filters all the files to return files matching the predicate
        /// </summary>
        /// <param name="predicate">The predicate to run against each file.</param>
        /// <returns>Filtered files.</returns>
        Task<List<Core.Domain.Entities.File>> GetFilteredFiles(Func<Core.Domain.Entities.File, bool> predicate);
    }
}
