using System.Threading.Tasks;
using RestSharp;

namespace Saturn.Backend.Data.Manifest.Objects;

public abstract class EndpointBase : IEndpoint
{
    protected RestClient Client { get; set; }
    protected RestRequest Request { get; set; }

    public void Accept(params string[] acceptedTypes)
    {
        Client.AcceptedContentTypes = acceptedTypes;
    }

    public void WithHeaders(params (string, string)[] headers)
    {
        foreach (var header in headers)
            Request.AddOrUpdateHeader(header.Item1, header.Item2);
    }

    public void WithFormBody(params (string, string)[] bodyParams)
    {
        Request.AddOrUpdateHeader("Content-Type", "application/x-www-form-urlencoded");

        foreach (var param in bodyParams)
            Request.AddOrUpdateParameter(
                Parameter.CreateParameter(param.Item1, param.Item2, ParameterType.GetOrPost));
    }

    public void WithFormBody(params Parameter[] bodyParams)
    {
        Request.AddOrUpdateHeader("Content-Type", "application/x-www-form-urlencoded");

        foreach (var param in bodyParams)
            Request.AddOrUpdateParameter(param);
    }

    public RestResponse GetResponse()
    {
        return Client.Execute(Request);
    }

    public RestResponse<T> GetResponse<T>()
    {
        return Client.Execute<T>(Request);
    }

    public async Task<RestResponse> GetResponseAsync()
    {
        return await Client.ExecuteAsync(Request);
    }
    public async Task<RestResponse<T>> GetResponseAsync<T>()
    {
        return await Client.ExecuteAsync<T>(Request);
    }
}