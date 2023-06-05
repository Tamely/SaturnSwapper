using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EpicManifestParser.Objects;
using Newtonsoft.Json;
using RestSharp;
using Saturn.Backend.Data.Fortnite;
using Saturn.Backend.Data.Manifest.Objects;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.Manifest;

public class Fortnite
{
    private readonly EpicManifestParser.Objects.Manifest _manifest;
    public Fortnite()
    {
        RestResponse? manifestResponse;
        EpicManifestEndpoint endpoint = new(Constants.MANIFEST_URL);

        Logger.Log("Getting manifest response");
        manifestResponse = endpoint.GetResponse();
        Logger.Log($"Got response with status code: {manifestResponse.StatusCode}");
        ManifestInfo info = new ManifestInfo(manifestResponse.Content);
        Logger.Log("Downloading manifest");

        byte[] data = Array.Empty<byte>();
        Task.Run(async () =>
        {
            data = await info.DownloadManifestDataAsync();
        }).Wait();

        _manifest = new EpicManifestParser.Objects.Manifest(data, new ManifestOptions()
        {
            ChunkBaseUri = new Uri("http://epicgames-download1.akamaized.net/Builds/Fortnite/CloudDir/ChunksV4/", UriKind.Absolute),
            ChunkCacheDirectory = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FortniteChunks"))
        });
        Logger.Log("Finished downloading manifest!");
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
            if (_manifest.FileManifests.Any(x => x.Name.Contains(file)) || file.Contains(".o.") || file.Contains("UEFN")) continue;
            
            Logger.Log($"Deleting non-stock file {file} in paks folder!", LogLevel.Warning);
            File.Delete(Path.Join(DataCollection.GetGamePath(), file));
        }
    }

    public void CheckForModifiedFiles()
    {
        foreach (var curManifest in _manifest.FileManifests)
        {
            if (curManifest.Name != Path.GetFileName(curManifest.Name)) continue;
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