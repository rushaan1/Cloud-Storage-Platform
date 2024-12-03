using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using CloudStoragePlatform.Core.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cloud_Storage_Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoldersController : ControllerBase
    {
        private readonly IFoldersModificationService _foldersModificationService;
        private readonly IFoldersRetrievalService _foldersRetrievalService;
        
        public FoldersController(IFoldersModificationService foldersModificationService, IFoldersRetrievalService foldersRetrievalService) 
        {
            _foldersModificationService = foldersModificationService;
            _foldersRetrievalService = foldersRetrievalService;
        }


        [HttpGet]
        [Route("getAllFoldersInHome")]
        public async Task<ActionResult<List<FolderResponse>>> GetAllFoldersInHomeFolder(SortOrderOptions sortOrder) 
        {
            List<FolderResponse> folders = await _foldersRetrievalService.GetAllFoldersInHomeFolder(sortOrder);
            if (folders.Count == 0) 
            {
                return NotFound();
            }
            return folders;
        }
        
        [HttpPost]
        [Route("/add")]
        public async Task<ActionResult<FolderResponse>> AddFolder(FolderAddRequest folderAddRequest)
        {
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
        public async Task<ActionResult<FolderResponse>> MoveFolder(Guid folderId, string newFolderPath) 
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
    }
}
