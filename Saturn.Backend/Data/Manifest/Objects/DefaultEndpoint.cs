using RestSharp;
using Saturn.Backend.Data.Manifest.Auth;

namespace Saturn.Backend.Data.Manifest.Objects;

public class DefaultEndpoint : EndpointBase
{

    public DefaultEndpoint(string url, Method requestMethod = Method.Get, Parameter? body = null)
    {
        Client = new();
        
        Request = new(url, requestMethod);

        if (body is not null)
        {
            Request.AddParameter(body);
        }
    }
}