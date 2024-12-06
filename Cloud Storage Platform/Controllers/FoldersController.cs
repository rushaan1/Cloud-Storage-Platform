using Cloud_Storage_Platform.CustomModelBinders;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using CloudStoragePlatform.Core.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Cloud_Storage_Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoldersController : ControllerBase
    {
        private readonly IFoldersModificationService _foldersModificationService;
        private readonly IFoldersRetrievalService _foldersRetrievalService;
        private readonly IConfiguration _configuration;
        
        public FoldersController(IFoldersModificationService foldersModificationService, IFoldersRetrievalService foldersRetrievalService, IConfiguration configuration) 
        {
            _foldersModificationService = foldersModificationService;
            _foldersRetrievalService = foldersRetrievalService;
            _configuration = configuration;
        }

        #region Retrievals
        [HttpGet]
        [Route("getAllFoldersInHome")]
        public async Task<ActionResult<List<FolderResponse>>> GetAllFoldersInHomeFolder(SortOrderOptions sortOrder = SortOrderOptions.DATEADDED) 
        {
            List<FolderResponse> folders = await _foldersRetrievalService.GetAllFoldersInHomeFolder(sortOrder);
            if (folders.Count == 0) 
            {
                return NotFound();
            }
            return folders;
        }

        [HttpGet]
        [Route("getAllSubFolders")]
        public async Task<ActionResult<List<FolderResponse>>> GetAllSubFolders(Guid parentFolderId, SortOrderOptions sortOrder = SortOrderOptions.DATEADDED) 
        {
            List<FolderResponse> folders = await _foldersRetrievalService.GetAllSubFolders(parentFolderId, sortOrder);
            return folders;
        }

        [HttpGet]
        [Route("getFilteredFolders")]
        public async Task<ActionResult<List<FolderResponse>>> GetFilteredFolders([ModelBinder(BinderType = typeof(RemoveInvalidFileFolderNameCharactersBinder))] string searchString, SortOrderOptions sortOrder)
        {
            string searchStringTrimmed = searchString.Trim();
            List<FolderResponse> folders = await _foldersRetrievalService.GetFilteredFolders(searchStringTrimmed, sortOrder);
            return folders;
        }

        [HttpGet]
        [Route("getFolderById")]
        public async Task<ActionResult<FolderResponse>> GetFolderById(Guid id) 
        {
            FolderResponse? folderResponse = await _foldersRetrievalService.GetFolderByFolderId(id);
            return (folderResponse!=null) ? folderResponse : NotFound();
        }

        [HttpGet]
        [Route("getFolderByPath")]
        public async Task<ActionResult<FolderResponse>> GetFolderByPath([ModelBinder(typeof(AppendToPath))] string path)
        {
            FolderResponse? folderResponse = await _foldersRetrievalService.GetFolderByFolderPath(path);
            return (folderResponse!=null) ? folderResponse : NotFound();
        }
        #endregion


        #region Modifications
        [HttpPost]
        [Route("/add")]
        public async Task<ActionResult<FolderResponse>> AddFolder(FolderAddRequest folderAddRequest)
        {
            FolderAddRequest updatedAddRequest = folderAddRequest;
            updatedAddRequest.FolderPath = _configuration["InitialPathForStorage"] + folderAddRequest.FolderPath;

            FolderResponse folderResponse = await _foldersModificationService.AddFolder(folderAddRequest);
            return folderResponse;
        }

        [HttpPatch]
        [Route("/rename")]
        public async Task<ActionResult<FolderResponse>> RenameFolder(FolderRenameRequest folderRenameRequest) 
        {
            FolderResponse folderResponse = await _foldersModificationService.RenameFolder(folderRenameRequest);
            return folderResponse;
        }

        [HttpPatch]
        [Route("/move")]
        public async Task<ActionResult<FolderResponse>> MoveFolder(Guid folderId, [ModelBinder(typeof(AppendToPath))] string newFolderPath) 
        {
            FolderResponse folderResponse = await _foldersModificationService.MoveFolder(folderId, newFolderPath);
            return folderResponse;
        }

        [HttpPatch]
        [Route("/addOrRemoveFromFavorite")]
        public async Task<ActionResult<FolderResponse>> AddOrRemoveFromFavorite(Guid folderId)
        {
            FolderResponse folderResponse = await _foldersModificationService.AddOrRemoveFavorite(folderId);
            return folderResponse;
        }

        [HttpPatch]
        [Route("/addOrRemoveFromTrash")]
        public async Task<ActionResult<FolderResponse>> AddOrRemoveFromTrash(Guid folderId)
        {
            FolderResponse folderResponse = await _foldersModificationService.AddOrRemoveFavorite(folderId);
            return folderResponse;
        }

        [HttpDelete]
        [Route("/delete")]
        public async Task<ActionResult<bool>> DeleteFolder(Guid folderId)
        {
            bool isDeleted = await _foldersModificationService.DeleteFolder(folderId);
            return isDeleted;
        }
        #endregion

        // TODO: Handling folder uploads with files and sub-folders and folder replacement with files and sub-folders.
    }
}
