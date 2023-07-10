using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using Saturn.Backend.Data.SaturnAPI.Models;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.SaturnAPI
{
    public interface ISaturnAPIService
    {
        public Task<T> ReturnEndpointAsync<T>(string url);
        public string ReturnEndpoint(string url);
    }
    public class SaturnAPIService : ISaturnAPIService
    {
        private readonly RestClient _client;
        private readonly string _APIKey;
        private readonly Uri _endpoint;
        public SaturnAPIService() 
        {
            _APIKey = "\u0701\u06A1\u08C1\u06A1\u0601\u0841\u0621\u0881\u0661\u0621";
            for (int queVe = 0, jCarg = 0; queVe < 10; queVe++)
            {
                jCarg = _APIKey[queVe];
                jCarg--;
                jCarg = (((jCarg & 0xFFFF) >> 5) | (jCarg << 11)) & 0xFFFF;
                _APIKey = _APIKey[..queVe] + (char)(jCarg & 0xFFFF) + _APIKey[(queVe + 1)..];
            }

            _endpoint = new Uri("https://tamelyapi.azurewebsites.net");

            _client = new RestClient(new RestClientOptions()
            {
                UserAgent = $"Saturn/{Constants.USER_VERSION}",
                MaxTimeout = 5 * 1000
            }, configureSerialization: s => s.UseSerializer<JsonNetSerializer>());
        }

        public async Task<T> ReturnEndpointAsync<T>(string url)
        {
            if (!Directory.Exists(Constants.APICachePath))
                Directory.CreateDirectory(Constants.APICachePath);
            
            CacheModel<T> data;
            if (File.Exists(Constants.APICachePath + "index" + url.Replace("/", "").Replace("?", "").Replace("&", "")))
            {
                data = JsonConvert.DeserializeObject<CacheModel<T>>(await File.ReadAllTextAsync(Constants.APICachePath + "index" + url.Replace("/", "").Replace("?", "").Replace("&", "")));
                if (data?.Expiration > DateTime.UtcNow && url != "/" && url != "/api/v1/Saturn/Dependencies" && url != "/api/v1/Saturn/PluginMarketplace")
                    return data.Data;

                File.Delete(Constants.APICachePath + "index" + url.Replace("/", "").Replace("?", "").Replace("&", ""));
            }
            
            var request = new RestRequest(new Uri(_endpoint, url));
            request.AddHeader("ApiKey", _APIKey);
            var response = await _client.ExecuteAsync(request);
            Logger.Log($"[{request.Method}] [{response.StatusDescription}({(int)response.StatusCode})] '{response.ResponseUri?.OriginalString}'");
            
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return default;

            data = new CacheModel<T>
            {
                Data = JsonConvert.DeserializeObject<T>(response.Content ?? "{}"),
                Expiration = DateTime.UtcNow.AddHours(3)
            };

            await File.WriteAllTextAsync(Constants.APICachePath + "index" + url.Replace("/", "").Replace("?", "").Replace("&", ""), JsonConvert.SerializeObject(data));
            
            return JsonConvert.DeserializeObject<T>(response.Content ?? "{}");
        }

        public string ReturnEndpoint(string url)
        {
            var request = new RestRequest(new Uri(_endpoint, url));
            request.AddHeader("ApiKey", _APIKey);
            var response = _client.Execute(request);
            Logger.Log(
                $"[{request.Method}] [{response.StatusDescription}({(int)response.StatusCode})] '{response.ResponseUri?.OriginalString}'");
            return response.Content ?? "{}";
        }
    }
}
