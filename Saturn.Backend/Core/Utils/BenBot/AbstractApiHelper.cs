using RestSharp;

namespace Saturn.Backend.Core.Utils.BenBot;

public abstract class AbstractApiHelper
{
    protected readonly IRestClient _client;

    public AbstractApiHelper(IRestClient client)
    {
        _client = client;
    }
}