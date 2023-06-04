using System;
using Newtonsoft.Json;

namespace Saturn.Backend.Data.Manifest;

public class AuthModel
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("client_id")]
    public string ClientId { get; set; }

    [JsonProperty("internal_client")]
    public bool InternalClient { get; set; }

    [JsonProperty("client_service")]
    public string ClientService { get; set; }

    [JsonProperty("product_id")]
    public string ProductId { get; set; }

    [JsonProperty("application_id")]
    public string ApplicationId { get; set; }
}