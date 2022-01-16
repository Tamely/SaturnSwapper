using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Saturn.Backend.Data.Models.SaturnAPI;
using Saturn.Backend.Data.Utils;

namespace Saturn.Backend.Data.Services
{
    public interface IBenBotAPIService
    {
        public Task<string> ReturnEndpointAsync(string url);
        public Task<byte[]> ReturnBytesAsync(string? url);
    }

    public class BenBotAPIService : IBenBotAPIService
    {
        public BenBotAPIService()
        {
            Base = new Uri("https://benbot.app/api/v1/");
            Logger.Log("Started BenBot API service!");
        }

        private Uri Base { get; }

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
