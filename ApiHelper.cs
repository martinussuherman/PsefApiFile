using System.Linq;
using System.Security.Claims;
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
        /// Read configuration data.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        internal static void ReadConfiguration(IConfiguration configuration)
        {
            IConfigurationSection bearerConfiguration = configuration.GetSection("Bearer");
            Authority = bearerConfiguration.GetValue<string>("Authority");
            Audience = bearerConfiguration.GetValue<string>("Audience");
        }
    }
}