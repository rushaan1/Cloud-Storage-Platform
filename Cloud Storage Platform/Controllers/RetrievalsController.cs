using Cloud_Storage_Platform.CustomModelBinders;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using CloudStoragePlatform.Core.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Cloud_Storage_Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RetrievalsController : ControllerBase
    {
        private readonly IBulkRetrievalService _retrievalService;
        private readonly IFoldersRetrievalService _foldersRetrievalService;
        private readonly IFilesRetrievalService _filesRetrievalService;
        private readonly IConfiguration _configuration;

        public RetrievalsController(IBulkRetrievalService retrievalService, IFoldersRetrievalService foldersRetrievalService, IConfiguration configuration, IFilesRetrievalService filesRetrievalService)
        {
            _foldersRetrievalService = foldersRetrievalService;
            _configuration = configuration;
            _retrievalService = retrievalService;
            _filesRetrievalService = filesRetrievalService;
        }

        [HttpGet("filePreview")]
        public async Task<IActionResult> GetFileForPreview(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found");
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var mimeType = extension switch
            {
                ".txt" => "text/plain",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                _ => null
            };

            if (mimeType == null)
            {
                return BadRequest("Unsupported file type");
            }

            //var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 64 * 1024, useAsync: true);
            Response.Headers.Add("X-Accel-Buffering", "no");
            return new RentedStreamResult(filePath, mimeType);
        }

        [HttpGet]
        [Route("download")]
        public async Task Download([FromQuery] List<Guid>? folderIds, [FromQuery] List<Guid>? fileIds, string name)
        {
            folderIds ??= new List<Guid>();
            fileIds ??= new List<Guid>();

            if (folderIds.Count == 0 && fileIds.Count == 0)
            {
                return;
            }

            if (folderIds.Count > 1)
            {
                name = folderIds.Count + " folders ";
                if (fileIds.Count > 0) { name += "and " + fileIds.Count + " file(s) "; }
                name += "download from cloud";
            }
            else if (folderIds.Count == 1 && fileIds.Count > 0)
            {
                name += " folder and " + fileIds.Count + " file(s) download from cloud";
            }
            else if (fileIds.Count > 1)
            {
                name = fileIds.Count + " files download from cloud";
            }
            string finalFileName = Uri.UnescapeDataString(name);
            if (folderIds.Count == 0 && fileIds.Count == 1)
            {
                Response.ContentType = "application/octet-stream";
            }
            else
            {
                Response.ContentType = "application/zip";
                finalFileName += ".zip";
            }

            Response.Headers["Content-Disposition"] =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = finalFileName
                }.ToString();
            await _retrievalService.DownloadFolder(folderIds, fileIds, new AsyncOnlyStreamWrapper(Response.Body));
        }


        [HttpGet]
        [Route("getFolderById")]
        public async Task<ActionResult<FolderResponse>> GetFolderById(Guid id)
        {
            FolderResponse? folderResponse = await _foldersRetrievalService.GetFolderByFolderId(id);
            return (folderResponse != null) ? folderResponse : NotFound();
        }

        [HttpGet]
        [Route("getFolderByPath")]
        public async Task<ActionResult<FolderResponse>> GetFolderByPath([ModelBinder(typeof(AppendToPath))] string path)
        {
            FolderResponse? folderResponse = await _foldersRetrievalService.GetFolderByFolderPath(path);
            return (folderResponse != null) ? folderResponse : NotFound();
        }

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
        public async Task<ActionResult<FileOrFolderMetadataResponse>> GetMetadata(Guid id, bool isFolder)
        {
            FileOrFolderMetadataResponse? folderDetailsResponse;
            if (isFolder)
            {
                folderDetailsResponse = await _foldersRetrievalService.GetMetadata(id);
            }
            else
            {
                folderDetailsResponse = await _filesRetrievalService.GetMetadata(id);
            }
            return (folderDetailsResponse != null) ? folderDetailsResponse : NotFound();
        }



        [HttpGet]
        [Route("getAllInHome")]
        public async Task<ActionResult<BulkResponse>> GetAllFoldersInHomeFolder(SortOrderOptions sortOrder = SortOrderOptions.DATEADDED_ASCENDING)
        {
            (List<FolderResponse> folders, List<FileResponse> files) res = await _retrievalService.GetAllInHome(sortOrder);
            if (res.folders.Count == 0 && res.files.Count == 0)
            {
                return NotFound();
            }
            return new BulkResponse { folders = res.folders, files = res.files };
        }

        [HttpGet]
        [Route("getAllChildrenById")]
        public async Task<ActionResult<BulkResponse>> GetAllSubFolders(Guid parentFolderId, SortOrderOptions sortOrder = SortOrderOptions.DATEADDED_ASCENDING)
        {
            (List<FolderResponse> folders, List<FileResponse> files) res = await _retrievalService.GetAllChildren(parentFolderId, sortOrder);
            return new BulkResponse { folders = res.folders, files = res.files };
        }

        [HttpGet]
        [Route("getAllChildrenByPath")]
        public async Task<ActionResult<BulkResponse>> GetAllSubFolders([ModelBinder(typeof(AppendToPath))] string path, SortOrderOptions sortOrder = SortOrderOptions.DATEADDED_ASCENDING)
        {
            FolderResponse? parent = await _foldersRetrievalService.GetFolderByFolderPath(path);
            if (parent == null)
            {
                return NotFound();
            }
            (List<FolderResponse> folders, List<FileResponse> files) res = await _retrievalService.GetAllChildren(parent.FolderId, sortOrder);
            return new BulkResponse { folders = res.folders, files = res.files };
        }

        [HttpGet]
        [Route("getAllFiltered")]
        public async Task<ActionResult<BulkResponse>> GetFilteredFolders([ModelBinder(BinderType = typeof(RemoveInvalidFileFolderNameCharactersBinder))] string searchString, SortOrderOptions sortOrder = SortOrderOptions.DATEADDED_ASCENDING)
        {
            string searchStringTrimmed = searchString.Trim();
            (List<FolderResponse> folders, List<FileResponse> files) res = await _retrievalService.GetAllFilteredChildren(searchStringTrimmed, sortOrder);
            return new BulkResponse { folders = res.folders, files = res.files };
        }

        [HttpGet]
        [Route("getAllFavorites")]
        public async Task<ActionResult<BulkResponse>> GetAllFavoriteFolders(SortOrderOptions sortOrder = SortOrderOptions.DATEADDED_ASCENDING)
        {
            (List<FolderResponse> folders, List<FileResponse> files) res = await _retrievalService.GetAllFavorites(sortOrder);
            return new BulkResponse { folders = res.folders, files = res.files };
        }

        [HttpGet]
        [Route("getAllTrashes")]
        public async Task<ActionResult<BulkResponse>> GetAllTrashFolders(SortOrderOptions sortOrder = SortOrderOptions.DATEADDED_ASCENDING)
        {
            (List<FolderResponse> folders, List<FileResponse> files) res = await _retrievalService.GetAllTrashes(sortOrder);
            return new BulkResponse { folders = res.folders, files = res.files };
        }
    }
}
