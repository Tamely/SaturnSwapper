using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Saturn.Backend.Data.Manifest.Objects;

namespace Saturn.Backend.Data.Manifest.Managers;

public static class AuthManager
{
    private static DefaultEndpoint Endpoint { get; set; }

    static AuthManager()
    {
        Endpoint = new("https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token", RestSharp.Method.Post);

        Endpoint.WithHeaders(("Authorization", "basic MzQ0NmNkNzI2OTRjNGE0NDg1ZDgxYjc3YWRiYjIxNDE6OTIwOWQ0YTVlMjVhNDU3ZmI5YjA3NDg5ZDMxM2I0MWE="));
        Endpoint.WithFormBody(("grant_type", "client_credentials"));
    }

    public static bool TryCreateToken([NotNullWhen(true)] out string? token)
    {
        token = string.Empty;
        
        var response = Endpoint.GetResponse();

        if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
        {
            Logger.Log($"Couldn't get token response. Status code {response.StatusCode}", LogLevel.Error);
            return false;
        }

        using var doc = JsonDocument.Parse(response.Content);

        if (!doc.RootElement.TryGetProperty("access_token", out var tokenProp))
            return false;

        token = tokenProp.GetString();

        return !string.IsNullOrEmpty(token);
    }
}