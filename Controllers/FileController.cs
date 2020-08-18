using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static PsefApiFile.ApiInfo;

namespace PsefApiFile.Controllers
{
    /// <summary>
    /// Represents a RESTful service for file management.
    /// </summary>
    [Authorize]
    [ApiVersion(V1_0)]
    [Route("File")]
    public class FileController : Controller
    {
        /// <summary>
        /// File management REST service.
        /// </summary>
        /// <param name="environment">Web host environment.</param>
        public FileController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// Uploads a new file.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <param name="file">The file to uploads.</param>
        /// <returns>The uploaded file relative path.</returns>
        /// <response code="201">The file was successfully uploaded.</response>
        /// <response code="204">The file was successfully uploaded.</response>
        /// <response code="400">The file is invalid.</response>
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(string), Status201Created)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> Post(IFormFile file)
        {
            string uploaded = await CopyUploadedFile(file);

            if (string.IsNullOrEmpty(uploaded))
            {
                return BadRequest();
            }

            return Created(string.Empty, uploaded);
        }

        private async Task<string> CopyUploadedFile(IFormFile file)
        {
            if (file == null ||
                string.IsNullOrEmpty(file.FileName) ||
                file.Length == 0)
            {
                return string.Empty;
            }

            string userId = ApiHelper.GetUserId(HttpContext.User);
            string currentDate = DateTime.Today.ToString(
                "yyyy-MM-dd",
                DateTimeFormatInfo.InvariantInfo);
            string folderPath = Path.Combine(
                _environment.WebRootPath,
                "upload",
                userId,
                currentDate);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(
                folderPath,
                file.FileName);

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Url.Content($"~/upload/{userId}/{currentDate}/{file.FileName}");
        }

        private readonly IWebHostEnvironment _environment;
    }
}