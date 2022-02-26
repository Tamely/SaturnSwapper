#pragma warning disable CA1416, SYSLIB0014 // Disable the warning that says something is deprecated and obsolete

using System;
using System.Net;
using System.Threading.Tasks;

namespace Saturn.Backend.Data.Services
{
    public interface ISaturnAPIService
    {
        public Task<string> ReturnEndpointAsync(string url);
        public string ReturnEndpoint(string url);
    }

    public class SaturnAPIService : ISaturnAPIService
    {
        public SaturnAPIService()
        {
            ApiKey = "\u0701\u06A1\u08C1\u06A1\u0601\u0841\u0621\u0881\u0661\u0621";
            for (int queVe = 0, jCarg; queVe < 10; queVe++)
            {
                jCarg = ApiKey[queVe];
                jCarg--;
                jCarg = (((jCarg & 0xFFFF) >> 5) | (jCarg << 11)) & 0xFFFF;
                ApiKey = ApiKey[..queVe] + (char)(jCarg & 0xFFFF) + ApiKey[(queVe + 1)..];
            }

            Base = new Uri("https://tamelyapi.azurewebsites.net");
        }

        private string ApiKey { get; }

        private Uri Base { get; }

        public async Task<string> ReturnEndpointAsync(string url)
        {
            using var wc = new WebClient();
            wc.Headers.Add("ApiKey", ApiKey);
            return await wc.DownloadStringTaskAsync(new Uri(Base, url));
        }

        public string ReturnEndpoint(string url)
        {
            using var wc = new WebClient();
            wc.Headers.Add("ApiKey", ApiKey);
            return wc.DownloadString(new Uri(Base, url));
        }
    }
}
