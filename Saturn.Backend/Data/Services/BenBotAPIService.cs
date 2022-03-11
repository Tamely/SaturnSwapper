#pragma warning disable CA1416, SYSLIB0014 // Disable the warning that says something is deprecated and obsolete
using Saturn.Backend.Data.Utils;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Saturn.Backend.Data.Services
{
    public interface IBenBotAPIService
    {
        public Task<string> ReturnEndpointAsync(string url);
        public Task<byte[]> ReturnBytesAsync(string? url);
        public Task<bool> IsBenAlive();
    }

    public class BenBotAPIService : IBenBotAPIService
    {
        public BenBotAPIService()
        {
            Base = new Uri("https://benbot.app/api/v1/");
            Logger.Log("Started BenBot API service!");
        }

        private Uri Base { get; }

        /// <summary>
        /// Checks if benbots mappings are up
        /// </summary>
        /// <returns>Bool: true if they are up, false if they are down</returns>
        public async Task<bool> IsBenAlive()
        {
            var request = WebRequest.Create("https://benbot.app/api/v1/mappings"); // Create a response to benbots mappings page
            request.Timeout = 5000; // Set the timeout to 5 seconds
            using var response = await request.GetResponseAsync(); // Get the response
            return response.ContentLength > 5; // If the content length is less than 5, return false, otherwise return true
        }

        public async Task<string> ReturnEndpointAsync(string url)
        {
            using var wc = new WebClient();
            return await wc.DownloadStringTaskAsync(new Uri(Base, url));
        }

        public async Task<byte[]> ReturnBytesAsync(string? url)
        {
            using var wc = new WebClient();
            return await wc.DownloadDataTaskAsync(new Uri(Base, url));
        }
    }
}
