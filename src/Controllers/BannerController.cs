using System;
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
    /// Represents a RESTful service for banner management.
    /// </summary>
    /// <remarks>
    /// *Role: Admin*
    /// </remarks>
    [Authorize(Roles = "Psef.Admin")]
    [ApiVersion(V1_0)]
    [Route("Banner")]
    public class BannerController : Controller
    {
        /// <summary>
        /// Banner management REST service.
        /// </summary>
        /// <param name="operation">File operation.</param>
        public BannerController(FileOperation operation)
        {
            _operation = operation;
        }

        /// <summary>
        /// Uploads a new banner.
        /// </summary>
        /// <param name="file">The banner to uploads.</param>
        /// <returns>The banner file relative path.</returns>
        /// <response code="201">The banner was successfully uploaded.</response>
        /// <response code="204">The banner was successfully uploaded.</response>
        /// <response code="400">The banner is invalid.</response>
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(string), Status201Created)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> Post(IFormFile file)
        {
            string[] pathSegment = { "upload", "banner" };
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
                !url.Contains("upload/banner/");
        }

        private readonly FileOperation _operation;
        private readonly string[] _permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
    }
}
