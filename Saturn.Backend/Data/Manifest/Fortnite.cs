using System;
using System.IO;
using System.Linq;
using EpicManifestParser.Objects;
using Newtonsoft.Json;
using RestSharp;
using Saturn.Backend.Data.Fortnite;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.Manifest;

public class Fortnite
{
    private readonly EpicManifestParser.Objects.Manifest _manifest;
    private readonly RestClient _restClient = new();
    public Fortnite()
    {
        var request = new RestRequest("https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token", Method.Post);
        request.AddHeader("Authorization", "basic MzQ0NmNkNzI2OTRjNGE0NDg1ZDgxYjc3YWRiYjIxNDE6OTIwOWQ0YTVlMjVhNDU3ZmI5YjA3NDg5ZDMxM2I0MWE=");
        request.AddParameter("grant_type", "client_credentials");
        string data = _restClient.ExecuteAsync(request).GetAwaiter().GetResult().Content;
        AuthModel auth = JsonConvert.DeserializeObject<AuthModel>(data);

        request = new RestRequest(Constants.MANIFEST_URL);
        request.AddHeader("Authorization", $"bearer {auth.AccessToken}");
        
        var response = _restClient.ExecuteAsync(request).GetAwaiter().GetResult();
        var manifestInfo = new ManifestInfo(response.Content);
        byte[] manifestData = manifestInfo.DownloadManifestData();
        
        _manifest = new EpicManifestParser.Objects.Manifest(manifestData, new ManifestOptions()
        {
            ChunkBaseUri = new Uri("http://epicgames-download1.akamaized.net/Builds/Fortnite/CloudDir/ChunksV4/", UriKind.Absolute),
            ChunkCacheDirectory = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FortniteChunks"))
        });
    }

    public void CleanFiles()
    {
        string pakFolder = DataCollection.GetGamePath() + "\\";

        foreach (var directory in Directory.EnumerateDirectories(pakFolder))
        {
            Logger.Log($"Deleting non-stock directory {directory} in paks folder!", LogLevel.Warning);
            Directory.Delete(directory, true);
        }
        
        string[] paks = Directory.GetFiles(pakFolder, "*", SearchOption.AllDirectories).Select(Path.GetFileName).ToArray()!;
        foreach (var file in paks)
        {
            if (_manifest.FileManifests.Any(x => x.Name.Contains(file))) continue;
            
            Logger.Log($"Deleting non-stock file {file} in paks folder!", LogLevel.Warning);
            File.Delete(file);
        }
    }

    public void CheckForModifiedFiles()
    {
        foreach (var curManifest in _manifest.FileManifests)
        {
            string localFilePath = Path.Combine(DataCollection.GetGamePath(), curManifest.Name);

            if (!File.Exists(localFilePath))
            {
                throw new Exception($"Could not find file path {localFilePath}!\nTo fix this, please verify your Fortnite installation in Epic Games!");
            }

            using (var localFileStream = File.OpenRead(localFilePath))
            {
                string hash = SHA1Hash.HashFileStream(localFileStream);
                
                if (curManifest.Hash != hash)
                {
                    throw new Exception($"Detected modified file {localFilePath}!\nTo fix this, please verify your Fortnite installation in Epic Games!");
                }
            }
        }
    }
}