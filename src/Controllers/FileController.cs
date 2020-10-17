using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PsefApiFile.Helpers;
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
        /// <param name="operation">File operation.</param>
        public FileController(FileOperation operation)
        {
            _operation = operation;
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
            string userId = ApiHelper.GetUserId(HttpContext.User);
            string currentDate = DateTime.Today.ToString(
                "yyyy-MM-dd",
                DateTimeFormatInfo.InvariantInfo);
            string[] pathSegment = { userId, currentDate };

            return await _operation.UploadFile(Url, file, pathSegment, _permittedExtensions);
        }

        [HttpDelete]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Uri), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public IActionResult Delete(string relativeUrl)
        {
            return _operation.DeleteFile(ValidateFileUrl, Request, relativeUrl);
        }

        private bool ValidateFileUrl(string url)
        {
            return string.IsNullOrEmpty(url) ||
                !url.Contains("upload/") ||
                (string.IsNullOrEmpty(ApiHelper.GetUserRole(HttpContext.User)) &&
                !url.Contains(ApiHelper.GetUserId(HttpContext.User)));
        }

        private readonly FileOperation _operation;
        private readonly string[] _permittedExtensions = { ".pdf" };
    }
}