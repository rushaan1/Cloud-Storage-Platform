using CloudStoragePlatform.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.ServiceContracts
{
    public interface IFilesModificationService
    {
        Task<FileResponse> UploadFile(FileAddRequest fileAddRequest, Stream file);
        Task<FileResponse> RenameFile(FileRenameRequest fileRenameRequest);



        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="newFilePath">MUST ONLY INCLUDE THE DESTINATION File PATH WITHOUT THE FILE TO BE MOVED'S PATH</param>
        /// <returns></returns>
        Task<FileResponse> MoveFile(Guid fileId, string newFilePath);
        Task<FileResponse> AddOrRemoveFavorite(Guid fileId);
        Task<FileResponse> AddOrRemoveTrash(Guid fileId);
        Task<bool> DeleteFile(Guid fileId);
    }
}
