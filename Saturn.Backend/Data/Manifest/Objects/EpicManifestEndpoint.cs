using Saturn.Backend.Data.Manifest.Auth;

namespace Saturn.Backend.Data.Manifest.Objects;

public class EpicManifestEndpoint : DefaultEndpoint
{
    public EpicManifestEndpoint(string manifestPath)
        : base($"https://launcher-public-service-prod06.ol.epicgames.com/{manifestPath}")
    {
        Request.Authenticator = new EpicLauncherAuthenticator();
    }
}