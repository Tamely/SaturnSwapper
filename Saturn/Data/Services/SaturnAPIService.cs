using Newtonsoft.Json;
using Saturn.Data.Models.SaturnAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Data.Services
{

    public interface ISaturnAPIService
    {
        public Task<Offsets> GetOffsets(string parentAsset);
        public Task<string> GetDownloadUrl(string assetName);
        public Task<string> ReturnEndpoint(string url);
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

        public async Task<string> GetDownloadUrl(string assetName)
        {
            return JsonConvert.DeserializeObject<CustomAsset>(
                    await ReturnEndpoint($"/api/v1/ProjectPlatoV2/Lengths/{Path.GetFileNameWithoutExtension(assetName)}"))
                .DownloadUrl;
        }

        public async Task<Offsets> GetOffsets(string parentAsset)
        {
            var json = JsonConvert.DeserializeObject<List<Offsets>>(await ReturnEndpoint("/api/v1/ProjectPlatoV2/Offsets"));
            return json.Find(x => x.ParentAsset.Contains(parentAsset));
        }

        public async Task<string> ReturnEndpoint(string url)
        {
            using var wc = new WebClient();
            wc.Headers.Add("ApiKey", ApiKey);
            return await wc.DownloadStringTaskAsync(new Uri(Base, url));
        }
    }
}
