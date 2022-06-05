using RestSharp;
using Saturn.Backend.Core.Utils.BenBot.Models;

namespace Saturn.Backend.Core.Utils.BenBot;

public class RestApiHelper
{
    private readonly IRestClient _client = new RestClient
    {
        UserAgent = $"Saturn Swapper v{Constants.UserVersion}",
        Timeout = 3 * 1000
    }.UseSerializer<JsonNetSerializer>();
    
    public BenbotApiEndpoint BenbotApi { get; }
    public ThreadWorker ThreadWorker { get; }
    
    public RestApiHelper()
    {
        BenbotApi = new BenbotApiEndpoint(_client);
        ThreadWorker = new ThreadWorker();
    }
}