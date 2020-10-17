using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PsefApiFile.Helpers
{
    public class FileOperation
    {
        public FileOperation(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public bool ValidateUploadFile()
        {

            return false;
        }

        public async Task<IActionResult> UploadFile(
            IUrlHelper urlHelper,
            IFormFile file,
            string[] pathSegment,
            string[] permittedExtensions)
        {
            string uploaded = await CopyUploadedFile(
                file,
                pathSegment,
                permittedExtensions);

            if (string.IsNullOrEmpty(uploaded))
            {
                return new BadRequestResult();
            }

            return new CreatedResult(
                string.Empty,
                urlHelper.Content($"~/{string.Join('/', pathSegment)}/{uploaded}"));
        }

        public IActionResult DeleteFile(
            Func<string, bool> validateUrl,
            HttpRequest request,
            string relativeUrl)
        {
            if (!validateUrl(relativeUrl))
            {
                return new BadRequestResult();
            }

            StringBuilder builder = new StringBuilder(relativeUrl);

            builder
                .Replace("../", string.Empty)
                .Replace("./", string.Empty)
                .Replace(request.PathBase.Value, string.Empty);

            string cleanedUrl = builder.ToString();
            string filePath = Path.Combine(
                _environment.WebRootPath,
                Path.Combine(cleanedUrl.Split('/')));

            if (!File.Exists(filePath))
            {
                return new BadRequestResult();
            }

            File.Delete(filePath);
            return new OkObjectResult(cleanedUrl);
        }

        private async Task<string> CopyUploadedFile(
            IFormFile file,
            string[] pathSegment,
            string[] permittedExtensions)
        {
            if (file == null ||
                string.IsNullOrEmpty(file.FileName) ||
                file.Length == 0)
            {
                return string.Empty;
            }

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                return string.Empty;
            }

            pathSegment.Prepend(_environment.WebRootPath);
            string folderPath = Path.Combine(pathSegment);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, file.FileName);

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return WebUtility.HtmlEncode(file.FileName);
        }

        private readonly IWebHostEnvironment _environment;
    }
}
