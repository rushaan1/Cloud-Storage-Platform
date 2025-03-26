using Cloud_Storage_Platform.CustomModelBinders;
using Cloud_Storage_Platform.Filters;
using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using CloudStoragePlatform.Core.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Cloud_Storage_Platform.Controllers
{
    [TypeFilter(typeof(EnsureGuidIsNotEmptyFilter), Arguments = new object[] { new string[] { "parentFolderId", "folderId", "id" } } )]
    [Route("api/[controller]")]
    [ApiController]
    public class FoldersController : ControllerBase
    {
        private readonly IFoldersModificationService _foldersModificationService;
        private readonly IRetrievalService _retrievalService;
        private readonly IFoldersRetrievalService _foldersRetrievalService;
        private readonly IConfiguration _configuration;
        
        public FoldersController(IFoldersModificationService foldersModificationService, IRetrievalService retrievalService, IFoldersRetrievalService foldersRetrievalService, IConfiguration configuration) 
        {
            _foldersModificationService = foldersModificationService;
            _foldersRetrievalService = foldersRetrievalService;
            _configuration = configuration;
            _retrievalService = retrievalService;
        }

        #region Retrievals
        [HttpGet]
        [Route("getById")]
        public async Task<ActionResult<FolderResponse>> GetFolderById(Guid id)
        {
            FolderResponse? folderResponse = await _foldersRetrievalService.GetFolderByFolderId(id);
            return (folderResponse != null) ? folderResponse : NotFound();
        }

        [HttpGet]
        [Route("getByPath")]
        public async Task<ActionResult<FolderResponse>> GetFolderByPath([ModelBinder(typeof(AppendToPath))] string path)
        {
            FolderResponse? folderResponse = await _foldersRetrievalService.GetFolderByFolderPath(path);
            return (folderResponse != null) ? folderResponse : NotFound();
        }

        [HttpGet]
        [Route("getMetadata")]
        public async Task<ActionResult<FileOrFolderMetadataResponse>> GetMetadata(Guid id)
        {
            FileOrFolderMetadataResponse? folderDetailsResponse = await _foldersRetrievalService.GetMetadata(id);
            return (folderDetailsResponse != null) ? folderDetailsResponse : NotFound();
        }



        [HttpGet]
        [Route("getAllInHome")]
        public async Task<ActionResult<BulkResponse>> GetAllFoldersInHomeFolder(bool fetchFiles, SortOrderOptions sortOrder = SortOrderOptions.DATEADDED_ASCENDING) 
        {
            (List<FolderResponse> folders, List<FileResponse> files) res = await _retrievalService.GetAllInHome(sortOrder, fetchFiles);
            if (res.folders.Count == 0 && res.files.Count == 0) 
            {
                return NotFound();
            }
            return new BulkResponse { folders = res.folders, files = res.files};
        }

        [HttpGet]
        [Route("getAllChildrenById")]
        public async Task<ActionResult<BulkResponse>> GetAllSubFolders(bool fetchFiles, Guid parentFolderId, SortOrderOptions sortOrder = SortOrderOptions.DATEADDED_ASCENDING) 
        {
            (List<FolderResponse> folders, List<FileResponse> files) res = await _retrievalService.GetAllChildren(parentFolderId, sortOrder, fetchFiles);
            return new BulkResponse { folders = res.folders, files = res.files};
        }

        [HttpGet]
        [Route("getAllChildrenByPath")]
        public async Task<ActionResult<BulkResponse>> GetAllSubFolders(bool fetchFiles, [ModelBinder(typeof(AppendToPath))] string path, SortOrderOptions sortOrder = SortOrderOptions.DATEADDED_ASCENDING)
        {
            FolderResponse? parent = await _foldersRetrievalService.GetFolderByFolderPath(path);
            if (parent == null) 
            {
                return NotFound();
            }
            (List<FolderResponse> folders, List<FileResponse> files) res = await _retrievalService.GetAllChildren(parent.FolderId, sortOrder, fetchFiles);
            return new BulkResponse { folders = res.folders, files = res.files};
        }

        [HttpGet]
        [Route("getAllFiltered")]
        public async Task<ActionResult<BulkResponse>> GetFilteredFolders([ModelBinder(BinderType = typeof(RemoveInvalidFileFolderNameCharactersBinder))] string searchString, bool fetchFiles, SortOrderOptions sortOrder = SortOrderOptions.DATEADDED_ASCENDING)
        {
            string searchStringTrimmed = searchString.Trim();
            (List<FolderResponse> folders, List<FileResponse> files) res = await _retrievalService.GetAllFilteredChildren(searchStringTrimmed, sortOrder, fetchFiles);
            return new BulkResponse { folders = res.folders, files = res.files};
        }

        [HttpGet]
        [Route("getAllFavorites")]
        public async Task<ActionResult<BulkResponse>> GetAllFavoriteFolders(bool fetchFiles, SortOrderOptions sortOrder = SortOrderOptions.DATEADDED_ASCENDING)
        {
            (List<FolderResponse> folders, List<FileResponse> files) res = await _retrievalService.GetAllFavorites(sortOrder, fetchFiles);
            return new BulkResponse { folders = res.folders, files = res.files};
        }

        [HttpGet]
        [Route("getAllTrashes")]
        public async Task<ActionResult<BulkResponse>> GetAllTrashFolders(bool fetchFiles, SortOrderOptions sortOrder = SortOrderOptions.DATEADDED_ASCENDING)
        {
            (List<FolderResponse> folders, List<FileResponse> files) res = await _retrievalService.GetAllTrashes(sortOrder, fetchFiles);
            return new BulkResponse { folders = res.folders, files = res.files};
        }
        #endregion


        #region Modifications
        [HttpPost]
        [Route("add")]
        public async Task<ActionResult<FolderResponse>> AddFolder(FolderAddRequest folderAddRequest)
        {
            folderAddRequest.FolderPath = _configuration["InitialPathForStorage"] + Uri.UnescapeDataString(folderAddRequest.FolderPath);
            FolderResponse folderResponse = await _foldersModificationService.AddFolder(folderAddRequest);
            return folderResponse;
        }

        [HttpPatch]
        [Route("rename")]
        public async Task<ActionResult<FolderResponse>> RenameFolder(FolderRenameRequest folderRenameRequest) 
        {
            FolderResponse folderResponse = await _foldersModificationService.RenameFolder(folderRenameRequest);
            return folderResponse;
        }

        [HttpPatch]
        [Route("move")]
        public async Task<ActionResult<FolderResponse>> MoveFolder(Guid id, [ModelBinder(typeof(AppendToPath))] string newPath) 
        {
            FolderResponse folderResponse = await _foldersModificationService.MoveFolder(id, newPath);
            return folderResponse;
        }

        [HttpPatch]
        [Route("addOrRemoveFromFavorite")]
        public async Task<ActionResult<FolderResponse>> AddOrRemoveFromFavorite(Guid id)
        {
            FolderResponse folderResponse = await _foldersModificationService.AddOrRemoveFavorite(id);
            return folderResponse;
        }

        [HttpPatch]
        [Route("addOrRemoveFromTrash")]
        public async Task<ActionResult<FolderResponse>> AddOrRemoveFromTrash(Guid id)
        {
            FolderResponse folderResponse = await _foldersModificationService.AddOrRemoveTrash(id);
            return folderResponse;
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<ActionResult<bool>> DeleteFolder(Guid id)
        {
            bool isDeleted = await _foldersModificationService.DeleteFolder(id);
            return isDeleted;
        }

        [HttpPatch]
        [Route("batchAddOrRemoveFromTrash")]
        public async Task<ActionResult> BatchAddOrRemoveFromTrash(List<Guid> ids) 
        {
            foreach (Guid id in ids) 
            {
                await _foldersModificationService.AddOrRemoveTrash(id);
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
                if (await _foldersModificationService.DeleteFolder(id))
                {
                    deleted++;
                }
            }
            return (deleted == ids.Count) ? NoContent() : StatusCode(500);
        }

        [HttpPatch]
        [Route("batchMove")]
        public async Task<ActionResult> BatchMove(List<Guid> ids, [ModelBinder(typeof(AppendToPath))] string newFolderPath) 
        {
            foreach (Guid id in ids) 
            {
                await _foldersModificationService.MoveFolder(id, newFolderPath);
            }
            return NoContent();
        }
        #endregion

        // TODO: Handling folder uploads with files and sub-folders.
    }
}
