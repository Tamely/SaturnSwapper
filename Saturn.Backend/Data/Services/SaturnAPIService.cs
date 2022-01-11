using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Saturn.Backend.Data.Models.SaturnAPI;

namespace Saturn.Backend.Data.Services
{
    public interface ISaturnAPIService
    {
        public Task<Offsets> GetOffsets(string parentAsset);
        public Task<string> ReturnEndpointAsync(string url);
        public string ReturnEndpoint(string url);
    }

    public class SaturnAPIService : ISaturnAPIService
    {
        public SaturnAPIService()
        {
            ApiKey = "\u0701\u06A1\u08C1\u06A1\u0601\u0841\u0621\u0881\u0661\u0621";
            for (int queVe = 0, jCarg = 0; queVe < 10; queVe++)
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

        public async Task<Offsets> GetOffsets(string parentAsset)
        {
            var json = JsonConvert.DeserializeObject<List<Offsets>>(
                await ReturnEndpointAsync("/api/v1/ProjectPlatoV2/Offsets"));
            return json.Find(x => x.ParentAsset.Contains(parentAsset));
        }

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
