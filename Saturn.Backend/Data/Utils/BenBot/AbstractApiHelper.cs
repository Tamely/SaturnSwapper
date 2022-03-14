using RestSharp;

namespace Saturn.Backend.Data.Utils.BenBot;

public abstract class AbstractApiHelper
{
    protected readonly IRestClient _client;

    public AbstractApiHelper(IRestClient client)
    {
        _client = client;
    }
}