using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using Saturn.Backend.Data.Manifest.Managers;

namespace Saturn.Backend.Data.Manifest.Auth;

public class EpicLauncherAuthenticator : IAuthenticator
{
    private string _token;

    public EpicLauncherAuthenticator()
    {
        if (!AuthManager.TryCreateToken(out var token))
        {
            throw new System.ArgumentNullException("Couldn't get token for launcher authenticator");
        }

        _token = token;
    }

    public ValueTask Authenticate(IRestClient client, RestRequest request)
    {
        request.AddOrUpdateHeader("Authorization", $"Bearer {_token}");

        return new();
    }
}