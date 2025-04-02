using CloudStoragePlatform.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.ServiceContracts
{
    public interface IFoldersModificationService
    {
        Task<FolderResponse> AddFolder(FolderAddRequest folderAddRequest, bool skipSSE=false);
        Task<FolderResponse> RenameFolder(FolderRenameRequest folderRenameRequest);



        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="newFolderPath">MUST ONLY INCLUDE THE DESTINATION FOLDER PATH WITHOUT THE FOLDER TO BE MOVED'S PATH</param>
        /// <returns></returns>
        Task<FolderResponse> MoveFolder(Guid folderId, string newFolderPath, bool skipSSE = false);
        Task<FolderResponse> AddOrRemoveFavorite(Guid folderId);
        Task<FolderResponse> AddOrRemoveTrash(Guid folderId, bool skipSSE = false);
        Task<bool> DeleteFolder(Guid folderId, bool skipSSE = false);
    }
}
