using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using Saturn.Backend.Data.SaturnAPI;
using Saturn.Backend.Data.SaturnAPI.Models;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.FortniteCentral;

public interface IFortniteCentralService
{
    public Task<T> ReturnEndpointAsync<T>(string url);
    public T ReturnEndpoint<T>(string url);
    public string ReturnEndpoint(string url);
}
    
public class FortniteCentralService : IFortniteCentralService
{
    public readonly Uri _endpoint;
    private readonly RestClient _client;

    public FortniteCentralService()
    {
        _endpoint = new Uri("https://fortnitecentral.genxgames.gg/");

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
        if (url != "/api/v1/aes" && File.Exists(Constants.APICachePath + "index" + url.Replace("/", "").Replace(" ", "").Replace("?", "")))
        {
            data = JsonConvert.DeserializeObject<CacheModel<T>>(await File.ReadAllTextAsync(Constants.APICachePath + "index" + url.Replace("/", "").Replace(" ", "").Replace("?", "")));
            if (data?.Expiration > DateTime.UtcNow)
                return data.Data;

            File.Delete(Constants.APICachePath + "index" + url.Replace("/", "").Replace(" ", "").Replace("?", ""));
        }
            
        var request = new RestRequest(new Uri(_endpoint, url));
        var response = await _client.ExecuteAsync(request);

        if (response.StatusCode != HttpStatusCode.OK)
            return default;

        data = new CacheModel<T>
        {
            Data = JsonConvert.DeserializeObject<T>(response.Content ?? "{}"),
            Expiration = DateTime.UtcNow.AddHours(3)
        };

        await File.WriteAllTextAsync(Constants.APICachePath + "index" + url.Replace("/", "").Replace(" ", "").Replace("?", ""), JsonConvert.SerializeObject(data));
            
        return JsonConvert.DeserializeObject<T>(response.Content ?? "{}");
    }

    public T ReturnEndpoint<T>(string url)
    {
        if (!Directory.Exists(Constants.APICachePath))
            Directory.CreateDirectory(Constants.APICachePath);
            
        CacheModel<T> data;
        if (File.Exists(Constants.APICachePath + "index" + url.Replace("/", "").Replace(" ", "").Replace("?", "")))
        {
            data = JsonConvert.DeserializeObject<CacheModel<T>>(File.ReadAllText(Constants.APICachePath + "index" + url.Replace("/", "").Replace(" ", "").Replace("?", "")));
            if (data?.Expiration > DateTime.UtcNow)
                return data.Data;

            File.Delete(Constants.APICachePath + "index" + url.Replace("/", "").Replace(" ", "").Replace("?", ""));
        }
            
        var request = new RestRequest(new Uri(_endpoint, url));
        var response = _client.Execute(request);
            
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
            return default;

        data = new CacheModel<T>
        {
            Data = JsonConvert.DeserializeObject<T>(response.Content ?? "{}"),
            Expiration = DateTime.UtcNow.AddDays(7)
        };

        File.WriteAllText(Constants.APICachePath + "index" + url.Replace("/", "").Replace(" ", "").Replace("?", ""), JsonConvert.SerializeObject(data));
            
        return JsonConvert.DeserializeObject<T>(response.Content ?? "{}");
    }
        
    public string ReturnEndpoint(string url)
    {
        using var wc = new WebClient();
        return wc.DownloadString(new Uri(_endpoint, url));
    }
}