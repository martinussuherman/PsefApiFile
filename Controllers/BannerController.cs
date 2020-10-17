using System;
using System.IO;
using System.Linq;
using System.Net;
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
        /// <param name="environment">Web host environment.</param>
        public BannerController(IWebHostEnvironment environment)
        {
            _environment = environment;
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
            string uploaded = await CopyUploadedFile(file);

            if (string.IsNullOrEmpty(uploaded))
            {
                return BadRequest();
            }

            return Created(string.Empty, uploaded);
        }

        [HttpDelete]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Uri), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public IActionResult Delete(string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl) ||
                !relativeUrl.Contains("upload/banner/"))
            {
                return BadRequest();
            }

            return ApiHelper.DeleteFile(_environment, Request, relativeUrl);
        }

        private async Task<string> CopyUploadedFile(IFormFile file)
        {
            if (file == null ||
                string.IsNullOrEmpty(file.FileName) ||
                file.Length == 0)
            {
                return string.Empty;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
            {
                return string.Empty;
            }

            string folderPath = Path.Combine(
                _environment.WebRootPath,
                "upload",
                "banner");

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

            return Url.Content($"~/upload/banner/{WebUtility.HtmlEncode(file.FileName)}");
        }

        private readonly IWebHostEnvironment _environment;
        private readonly string[] _permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
    }
}