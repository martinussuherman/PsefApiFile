using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace PsefApiFile
{
    /// <summary>
    /// Api helpers.
    /// </summary>
    internal class ApiHelper
    {
        internal static string Authority { get; set; }

        internal static string Audience { get; set; }

        /// <summary>
        /// Retrieve id of the user executing the request.
        /// </summary>
        /// <param name="user">User info.</param>
        /// <returns>User Id.</returns>
        internal static string GetUserId(ClaimsPrincipal user)
        {
            return user.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                .Value;
        }

        /// <summary>
        /// Retrieve role of the user executing the request.
        /// </summary>
        /// <param name="user">User info.</param>
        /// <returns>User role.</returns>
        internal static string GetUserRole(ClaimsPrincipal user)
        {
            return user.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?
                .Value;
        }

        /// <summary>
        /// Read configuration data.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        internal static void ReadConfiguration(IConfiguration configuration)
        {
            IConfigurationSection bearerConfiguration = configuration.GetSection("Bearer");
            Authority = bearerConfiguration.GetValue<string>("Authority");
            Audience = bearerConfiguration.GetValue<string>("Audience");
        }

        internal static IActionResult DeleteFile(
            IWebHostEnvironment environment,
            HttpRequest request,
            string relativeUrl)
        {
            StringBuilder builder = new StringBuilder(relativeUrl);

            builder
                .Replace("../", string.Empty)
                .Replace("./", string.Empty)
                .Replace(request.PathBase.Value, string.Empty);

            string cleanedUrl = builder.ToString();
            string filePath = Path.Combine(
                environment.WebRootPath,
                Path.Combine(cleanedUrl.Split('/')));

            if (!File.Exists(filePath))
            {
                return new BadRequestResult();
            }

            File.Delete(filePath);
            return new OkObjectResult(cleanedUrl);
        }
    }
}