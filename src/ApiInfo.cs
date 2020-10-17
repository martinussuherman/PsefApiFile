using Microsoft.AspNetCore.Mvc;

namespace PsefApiFile
{
    internal class ApiInfo
    {
        internal const string JsonOutput = "application/json";
        internal const string SchemeOauth2 = "oauth2";

        internal const string V1_0 = "1.0";

        internal static readonly ApiVersion Ver1_0 = ApiVersion.Parse(V1_0);
    }
}