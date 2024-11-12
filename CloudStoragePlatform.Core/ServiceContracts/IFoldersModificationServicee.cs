using CloudStoragePlatform.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.ServiceContracts
{
    public interface IFoldersModificationServicee
    {
        Task<FolderResponse> AddFolder(FolderAddRequest folderAddRequest);
        Task<FolderResponse> RenameFolder(FolderRenameRequest folderRenameRequest);
        Task<FolderResponse> ReplaceFolder(FolderReplaceRequest folderReplaceRequest);
        Task<FolderResponse> MoveFolder(Guid folderId, string newFolderPath);
        Task<bool> DeleteFolder(Guid id);
    }
}
