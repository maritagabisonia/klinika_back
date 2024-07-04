using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using hospital.service;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using hospital.models;

namespace hospital.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileUploadController(IFileService fileService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] RegisterUser user)
        {
            if (user.Photo == null || user.Photo.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            try
            {
                var fileName = await _fileService.SaveFileAsync(user.Photo, allowedExtensions);
                return Ok(new { FileName = fileName });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("delete/{fileName}")]
        public IActionResult DeleteFile(string fileName)
        {
            try
            {
                _fileService.DeleteFile(fileName);
                return Ok(new { Message = "File deleted successfully." });
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
