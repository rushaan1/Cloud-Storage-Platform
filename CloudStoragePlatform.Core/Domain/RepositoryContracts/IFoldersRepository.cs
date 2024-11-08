using CloudStoragePlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.RepositoryContracts
{
    public interface IFoldersRepository
    {
        /// <summary>
        /// Adds a folder to the repository.
        /// </summary>
        /// <param name="folder">The folder to add.</param>
        /// <returns>The added folder.</returns>
        Task<Folder> AddFolder(Folder folder);

        /// <summary>
        /// Retrieves all folders from the repository.
        /// </summary>
        /// <returns>A list of folders.</returns>
        Task<List<Folder>> GetAllFolders();

        /// <summary>
        /// Retrieves folders based on a specified filter predicate.
        /// </summary>
        /// <param name="predicate">The filter condition.</param>
        /// <returns>A list of folders that match the filter.</returns>
        Task<List<Folder>> GetFilteredFolders(Expression<Func<Folder, bool>> predicate);

        /// <summary>
        /// Retrieves subfolders of a specified parent folder based on a filter predicate.
        /// </summary>
        /// <param name="parent">The parent folder.</param>
        /// <param name="predicate">The filter condition.</param>
        /// <returns>A list of subfolders that match the filter.</returns>
        Task<List<Folder>> GetFilteredSubFolders(Folder parent, Func<Folder, bool> predicate);

        /// <summary>
        /// Retrieves files within a specified parent folder based on a filter predicate.
        /// </summary>
        /// <param name="parent">The parent folder.</param>
        /// <param name="predicate">The filter condition.</param>
        /// <returns>A list of files that match the filter.</returns>
        Task<List<Entities.File>> GetFilteredSubFiles(Folder parent, Func<Entities.File, bool> predicate);

        /// <summary>
        /// Retrieves a folder by its Guid.
        /// </summary>
        /// <param name="id">The folder ID.</param>
        /// <returns>The folder with the specified ID, or null if not found.</returns>
        Task<Folder?> GetFolderByFolderId(Guid id);

        /// <summary>
        /// Retrieves a folder by its folder path.
        /// </summary>
        /// <param name="path">The folder path.</param>
        /// <returns>The folder with the specified path, or null if not found.</returns>
        Task<Folder?> GetFolderByFolderPath(string path);

        /// <summary>
        /// Updates an existing folder with new properties, parent folder, metadata, sharing options, subfolders, and files.
        /// </summary>
        /// <param name="folder">The folder to update.</param>
        /// <param name="updateProperties">If true, updates the folder's properties.</param>
        /// <param name="updateParentFolder">If true, updates the folder's parent folder.</param>
        /// <param name="updateMetadata">If true, updates the folder's metadata.</param>
        /// <param name="updateSharing">If true, updates the folder's sharing options.</param>
        /// <param name="updateSubFolders">If true, updates the folder's subfolders.</param>
        /// <param name="updateSubFiles">If true, updates the folder's files.</param>
        /// <returns>The updated folder, or null if not found.</returns>
        Task<Folder?> UpdateFolder(Folder folder, bool updateProperties, bool updateParentFolder, bool updateMetadata, bool updateSharing, bool updateSubFolders, bool updateSubFiles);

        /// <summary>
        /// Deletes a folder and its associated metadata, sharing options, subfolders, and files from the repository.
        /// </summary>
        /// <param name="folder">The folder to delete.</param>
        /// <returns>True if the folder was successfully deleted; otherwise, false.</returns>
        Task<bool> DeleteFolder(Folder folder);
    }
}
