using FileService.Api.Models;
using FileService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Api.Controllers
{
    [ApiController]
    // [Authorize]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly FileService.Api.Services.FileService _fileService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(FileService.Api.Services.FileService fileService, ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<FileResponse>>> UploadFile(
            IFormFile file, 
            [FromForm] string serviceName)
        {
            try
            {
                if (string.IsNullOrEmpty(serviceName))
                {
                    return BadRequest(new ApiResponse<FileResponse>
                    {
                        Success = false,
                        Message = "Service name is required"
                    });
                }

                var fileDocument = await _fileService.UploadFileAsync(file, serviceName);
                
                var response = new ApiResponse<FileResponse>
                {
                    Success = true,
                    Message = "Upload success",
                    Data = new FileResponse
                    {
                        Id = fileDocument.Id,
                        FileName = fileDocument.FileName,
                        OriginalFileName = fileDocument.OriginalFileName,
                        Path = fileDocument.Path,
                        Url = $"{Request.Scheme}://{Request.Host}/api/files/{fileDocument.Id}",
                        Size = fileDocument.Size,
                        ContentType = fileDocument.ContentType,
                        CreatedAt = fileDocument.CreatedAt
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return BadRequest(new ApiResponse<FileResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadFile(string id)
        {
            try
            {
                var (stream, contentType, fileSize) = await _fileService.GetFileStreamAsync(id);

                Response.Headers.ContentLength = fileSize;
                return File(stream, contentType);
            }
            catch (FileNotFoundException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "File not found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FileId}", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteFile(string id)
        {
            try
            {
                var result = await _fileService.DeleteFileAsync(id);

                if (!result)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "File not found"
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "File deleted successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", id);
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("meta/{id}")]
        public async Task<ActionResult<ApiResponse<FileResponse>>> GetFileMetadata(string id)
        {
            try
            {
                var fileDocument = await _fileService.GetFileByIdAsync(id);

                if (fileDocument == null)
                {
                    return NotFound(new ApiResponse<FileResponse>
                    {
                        Success = false,
                        Message = "File not found"
                    });
                }

                var response = new ApiResponse<FileResponse>
                {
                    Success = true,
                    Message = "Metadata retrieved successfully",
                    Data = new FileResponse
                    {
                        Id = fileDocument.Id,
                        FileName = fileDocument.FileName,
                        OriginalFileName = fileDocument.OriginalFileName,
                        Path = fileDocument.Path,
                        Url = $"{Request.Scheme}://{Request.Host}/api/files/{fileDocument.Id}",
                        Size = fileDocument.Size,
                        ContentType = fileDocument.ContentType,
                        CreatedAt = fileDocument.CreatedAt
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file metadata {FileId}", id);
                return BadRequest(new ApiResponse<FileResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}