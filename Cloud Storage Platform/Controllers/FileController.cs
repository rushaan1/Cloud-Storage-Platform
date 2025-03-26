using Cloud_Storage_Platform.CustomModelBinders;
using Cloud_Storage_Platform.Filters;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.ServiceContracts;
using CloudStoragePlatform.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

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

        private string GetBoundary(MediaTypeHeaderValue mediaTypeHeaderValue) 
        {
            var boundary = HeaderUtilities.RemoveQuotes(mediaTypeHeaderValue.Boundary).Value;
            return boundary;
        }

        #region Retrievals
        [HttpGet]
        [Route("getFileById")]
        public async Task<ActionResult<FileResponse>> GetFileById(Guid id)
        {
            FileResponse? fileResponse = await _filesRetrievalService.GetFileByFileId(id);
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
        public async Task<ActionResult<FileOrFolderMetadataResponse>> GetMetadata(Guid id)
        {
            FileOrFolderMetadataResponse? fileDetailsResponse = await _filesRetrievalService.GetMetadata(id);
            return (fileDetailsResponse != null) ? fileDetailsResponse : NotFound();
        }
        #endregion


        #region Modifications
        [HttpPost]
        [Route("upload")]
        [DisableFormValueModelBinding]
        public async Task<ActionResult<FileResponse>> UploadFile([FromQuery] FileAddRequest fileAddRequest)
        {
            fileAddRequest.FilePath = _configuration["InitialPathForStorage"] + Uri.UnescapeDataString(fileAddRequest.FilePath);

            var boundary = GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType));
            var multipartReader = new MultipartReader(boundary, Request.Body);
            var section = await multipartReader.ReadNextSectionAsync();
            while (section is not null)
            {
                var fileSection = section.AsFileSection();
                if (fileSection is not null && fileSection.FileStream is not null)
                {
                    await _filesModificationService.UploadFile(fileAddRequest, fileSection.FileStream);
                }
                section = await multipartReader.ReadNextSectionAsync();
            }
            return new FileResponse();
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
        public async Task<ActionResult<FileResponse>> MoveFile(Guid id, [ModelBinder(typeof(AppendToPath))] string newFilePath)
        {
            FileResponse fileResponse = await _filesModificationService.MoveFile(id, newFilePath);
            return fileResponse;
        }

        [HttpPatch]
        [Route("addOrRemoveFromFavorite")]
        public async Task<ActionResult<FileResponse>> AddOrRemoveFromFavorite(Guid id)
        {
            FileResponse fileResponse = await _filesModificationService.AddOrRemoveFavorite(id);
            return fileResponse;
        }

        [HttpPatch]
        [Route("addOrRemoveFromTrash")]
        public async Task<ActionResult<FileResponse>> AddOrRemoveFromTrash(Guid id)
        {
            FileResponse fileResponse = await _filesModificationService.AddOrRemoveTrash(id);
            return fileResponse;
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<ActionResult<bool>> DeleteFile(Guid id)
        {
            bool isDeleted = await _filesModificationService.DeleteFile(id);
            return isDeleted;
        }

        [HttpPatch]
        [Route("batchAddOrRemoveFromTrash")]
        public async Task<ActionResult> BatchAddOrRemoveFromTrash(List<Guid> ids)
        {
            foreach (Guid id in ids)
            {
                await _filesModificationService.AddOrRemoveTrash(id);
            }
            return NoContent();
        }

        [HttpDelete]
        [Route("batchDelete")]
        public async Task<ActionResult> BatchDelete([FromQuery] List<Guid> ids)
        {
            int deleted = 0;
            foreach (Guid id in ids)
            {
                if (await _filesModificationService.DeleteFile(id))
                {
                    deleted++;
                }
            }
            return (deleted == ids.Count) ? NoContent() : StatusCode(500);
        }

        [HttpPatch]
        [Route("batchMove")]
        public async Task<ActionResult> BatchMove(List<Guid> ids, [ModelBinder(typeof(AppendToPath))] string newFilePath)
        {
            foreach (Guid id in ids)
            {
                await _filesModificationService.MoveFile(id, newFilePath);
            }
            return NoContent();
        }
        #endregion
    }
}
