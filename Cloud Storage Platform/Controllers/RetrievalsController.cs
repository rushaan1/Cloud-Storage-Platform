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
        private readonly IRetrievalService _retrievalService;
        private readonly IFoldersRetrievalService _foldersRetrievalService;
        private readonly IFilesRetrievalService _filesRetrievalService;
        private readonly IConfiguration _configuration;

        public RetrievalsController(IRetrievalService retrievalService, IFoldersRetrievalService foldersRetrievalService, IConfiguration configuration, IFilesRetrievalService filesRetrievalService)
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

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, mimeType);
        }

        [HttpGet]
        [Route("downloadFolder")]
        public async Task DownloadFolder([FromQuery] List<Guid> ids, string name) 
        {
            if (ids.Count > 1) 
            {
                name = ids.Count + " folders download";
            }
            Response.ContentType = "application/zip";
            Response.Headers["Content-Disposition"] =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Uri.UnescapeDataString(name)+".zip"
                }.ToString(); 
            await _foldersRetrievalService.DownloadFolder(ids, new AsyncOnlyStreamWrapper(Response.Body));
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
