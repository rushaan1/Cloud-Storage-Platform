using Cloud_Storage_Platform.CustomModelBinders;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.ServiceContracts;
using CloudStoragePlatform.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cloud_Storage_Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFilesModificationService _filesModificationService;
        private readonly IRetrievalService _retrievalService;
        private readonly IFilesRetrievalService _filesRetrievalService;
        private readonly IConfiguration _configuration;

        public FileController(IFilesModificationService filesModificationService, IRetrievalService retrievalService, IFilesRetrievalService filesRetrievalService, IConfiguration configuration)
        {
            _filesModificationService = filesModificationService;
            _filesRetrievalService = filesRetrievalService;
            _configuration = configuration;
            _retrievalService = retrievalService;
        }

        #region Retrievals
        [HttpGet]
        [Route("getFileById")]
        public async Task<ActionResult<FileResponse>> GetFileById(Guid fileId)
        {
            FileResponse? fileResponse = await _filesRetrievalService.GetFileByFileId(fileId);
            return (fileResponse != null) ? fileResponse : NotFound();
        }

        [HttpGet]
        [Route("getFileByPath")]
        public async Task<ActionResult<FileResponse>> GetFileByPath([ModelBinder(typeof(AppendToPath))] string path)
        {
            FileResponse? fileResponse = await _filesRetrievalService.GetFileByFilePath(path);
            return (fileResponse != null) ? fileResponse : NotFound();
        }

        [HttpGet]
        [Route("getMetadata")]
        public async Task<ActionResult<FileOrFolderMetadataResponse>> GetMetadata(Guid fileId)
        {
            FileOrFolderMetadataResponse? fileDetailsResponse = await _filesRetrievalService.GetMetadata(fileId);
            return (fileDetailsResponse != null) ? fileDetailsResponse : NotFound();
        }
        #endregion


        #region Modifications
        [HttpPost]
        [Route("upload")]
        public async Task<ActionResult<FileResponse>> UploadFile([FromQuery] FileAddRequest fileAddRequest)
        {
            fileAddRequest.FilePath = _configuration["InitialPathForStorage"] + Uri.UnescapeDataString(fileAddRequest.FilePath);
            FileResponse fileResponse = await _filesModificationService.UploadFile(fileAddRequest, Request.Body);
            return fileResponse;
        }

        [HttpPatch]
        [Route("rename")]
        public async Task<ActionResult<FileResponse>> RenameFile(FileRenameRequest fileRenameRequest)
        {
            FileResponse fileResponse = await _filesModificationService.RenameFile(fileRenameRequest);
            return fileResponse;
        }

        [HttpPatch]
        [Route("move")]
        public async Task<ActionResult<FileResponse>> MoveFile(Guid fileId, [ModelBinder(typeof(AppendToPath))] string newFilePath)
        {
            FileResponse fileResponse = await _filesModificationService.MoveFile(fileId, newFilePath);
            return fileResponse;
        }

        [HttpPatch]
        [Route("addOrRemoveFromFavorite")]
        public async Task<ActionResult<FileResponse>> AddOrRemoveFromFavorite(Guid fileId)
        {
            FileResponse fileResponse = await _filesModificationService.AddOrRemoveFavorite(fileId);
            return fileResponse;
        }

        [HttpPatch]
        [Route("addOrRemoveFromTrash")]
        public async Task<ActionResult<FileResponse>> AddOrRemoveFromTrash(Guid fileId)
        {
            FileResponse fileResponse = await _filesModificationService.AddOrRemoveTrash(fileId);
            return fileResponse;
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<ActionResult<bool>> DeleteFile(Guid fileId)
        {
            bool isDeleted = await _filesModificationService.DeleteFile(fileId);
            return isDeleted;
        }

        [HttpPatch]
        [Route("batchAddOrRemoveFromTrash")]
        public async Task<ActionResult> BatchAddOrRemoveFromTrash(List<Guid> fileIds)
        {
            foreach (Guid id in fileIds)
            {
                await _filesModificationService.AddOrRemoveTrash(id);
            }
            return NoContent();
        }

        [HttpDelete]
        [Route("batchDelete")]
        public async Task<ActionResult> BatchDelete([FromQuery] List<Guid> fileIds)
        {
            int deleted = 0;
            foreach (Guid id in fileIds)
            {
                if (await _filesModificationService.DeleteFile(id))
                {
                    deleted++;
                }
            }
            return (deleted == fileIds.Count) ? NoContent() : StatusCode(500);
        }

        [HttpPatch]
        [Route("batchMove")]
        public async Task<ActionResult> BatchMove(List<Guid> fileIds, [ModelBinder(typeof(AppendToPath))] string newFilePath)
        {
            foreach (Guid id in fileIds)
            {
                await _filesModificationService.MoveFile(id, newFilePath);
            }
            return NoContent();
        }
        #endregion
    }
}
