using CloudStoragePlatform.Core.ServiceContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Text;
using System.Threading.Tasks;
using CloudStoragePlatform.Core;

namespace Cloud_Storage_Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharesController : ControllerBase
    {
        private readonly ISharingService _sharingService;
        private readonly IFilesRetrievalService _filesRetrievalService;
        private readonly IBulkRetrievalService _bulkRetrievalService;

        public SharesController(ISharingService sharingService, IFilesRetrievalService filesRetrievalService, IBulkRetrievalService bulkRetrievalService)
        {
            _sharingService = sharingService;
            _filesRetrievalService = filesRetrievalService;
            _bulkRetrievalService = bulkRetrievalService;
        }

        [HttpPost("CreateShare")]
        public async Task<IActionResult> CreateShare([FromBody] CreateShareRequest request)
        {
            try
            {
                Guid? sharingId = null;
                
                if (request.isFile)
                {
                    sharingId = await _sharingService.CreateShareForFile(request.FileOrFolderId, request.ExpiryDate);
                }
                else if (!request.isFile)
                {
                    sharingId = await _sharingService.CreateShareForFolder(request.FileOrFolderId, request.ExpiryDate);
                }
                else
                {
                    return BadRequest("Either FileId or FolderId must be provided");
                }

                if (sharingId.HasValue)
                {
                    return Ok(new { SharingId = sharingId.Value, Message = "Share created successfully" });
                }
                else
                {
                    return NotFound("File or folder not found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the share", Error = ex.Message });
            }
        }



        [HttpDelete("RemoveShare")]
        public async Task<IActionResult> RemoveShare([FromQuery] Guid fileOrFolderId, [FromQuery] bool isFile)
        {
            try
            {
                bool success = false;
                
                if (isFile)
                {
                    success = await _sharingService.RemoveShareForFile(fileOrFolderId);
                }
                else if (!isFile)
                {
                    success = await _sharingService.RemoveShareForFolder(fileOrFolderId);
                }
                else
                {
                    return BadRequest("Either FileId or FolderId must be provided");
                }

                if (success)
                {
                    return Ok(new { Message = "Share removed successfully" });
                }
                else
                {
                    return NotFound("File/folder not found or no share exists");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while removing the share", Error = ex.Message });
            }
        }





        [HttpGet("FetchSharedContent")]
        public async Task<IActionResult> FetchSharedContent([FromQuery] Guid sharingId, [FromQuery] Guid fileFolderSubjectId)
        {
            try
            {
                var result = await _sharingService.ValidateShareFetchSubject(sharingId, fileFolderSubjectId);
                
                if (result == null)
                {
                    return NotFound("Share not found, expired, or subject not accessible");
                }

                var (file, folder, childFile, relativeSubjectPath) = result.Value;
                
                // Add RelativePath header with base64 encoded relativeSubjectPath
                var encodedPath = Convert.ToBase64String(Encoding.UTF8.GetBytes(relativeSubjectPath));
                Response.Headers.Add("RelativePath", encodedPath);

                if (file != null)
                {
                    // Get file stream for preview
                    var stream = await _filesRetrievalService.GetFilePreview(file.FilePath);
                    var mimeType = Utilities.GetMimeType(file.FilePath);
                    
                    if (mimeType == null)
                    {
                        return BadRequest("Unsupported file type");
                    }

                    Response.Headers["X-Accel-Buffering"] = "no";
                    return File(stream, mimeType, true);
                }
                else if (folder != null)
                {
                    // Handle folder children
                    var children = await _bulkRetrievalService.GetAllChildren(folder.FolderId, SortOrderOptions.DATEADDED_ASCENDING);
                    return Ok(new BulkResponse { folders = children.Folders, files = children.Files });
                }

                return BadRequest("Invalid share content");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching shared content", Error = ex.Message });
            }
        }





        [HttpGet("DownloadSharedContent")]
        public async Task DownloadSharedContent([FromQuery] Guid sharingId, [FromQuery] Guid fileFolderSubjectId)
        {
            try
            {
                var result = await _sharingService.ValidateShareFetchSubject(sharingId, fileFolderSubjectId);
                
                if (result == null)
                {
                    Response.StatusCode = 404;
                    return;
                }

                var (file, folder, childFile, relativeSubjectPath) = result.Value;
                
                List<Guid> folderIds = new List<Guid>();
                List<Guid> fileIds = new List<Guid>();
                string fileName = "";

                if (file != null)
                {
                    // Handle file download
                    fileIds.Add(file.FileId);
                    fileName = file.FileName;
                }
                else if (folder != null)
                {
                    // Handle folder download
                    folderIds.Add(folder.FolderId);
                    fileName = folder.FolderName;
                }
                else
                {
                    Response.StatusCode = 400;
                    return;
                }

                // Set content type based on whether it's a single file or folder
                if (file != null)
                {
                    Response.ContentType = "application/octet-stream";
                }
                else
                {
                    Response.ContentType = "application/zip";
                    fileName += ".zip";
                }

                // Set Content-Disposition header
                Response.Headers["Content-Disposition"] =
                    new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = fileName
                    }.ToString();

                // Call DownloadFolder with AsyncOnlyStreamWrapper
                await _bulkRetrievalService.DownloadFolder(folderIds, fileIds, new AsyncOnlyStreamWrapper(Response.Body));
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
            }
        }
    }

    public class CreateShareRequest
    {
        public Guid FileOrFolderId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool isFile { get; set; }
    }
}
