using MongoDB.Driver;
using Newtonsoft.Json;
using RestSharp;
using SaturnBot.Models;

namespace SaturnBot.Utils;

public class UpdateDB
{
    public static async Task UpdateMongoDB()
    {
        Uri BaseURL = new Uri("https://data.mongodb-api.com/app/data-fnkiu/endpoint/data/beta/");

        var settings = MongoClientSettings.FromConnectionString("mongodb+srv://Tamely:Tamely@cluster0.irotl.mongodb.net/PlatoDB?retryWrites=true&w=majority");
        var client = new MongoClient(settings);
        var database = client.GetDatabase("SaturnData");
        await database.DropCollectionAsync("BetaUsers");

        var mongo = new MongoModel()
        {
            collection = "BetaUsers",
            database = "SaturnData",
            dataSource = "Cluster0"
        };
                
        foreach (var userId in Program.BetaUserIds)
            mongo.documents.Add(new User()
            {
                id = userId,
                generic = "Saturn"
            });
                
        var restClient = new RestClient(new Uri(BaseURL, "action/insertMany"))
        {
            Timeout = -1
        };
        var request = new RestRequest(Method.POST);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Access-Control-Request-Headers", "*");
        request.AddHeader("api-key", "cuLZNmR6rCbaD2GP0yeBv6x4EglxzVLqkWuti40RHMFjzz7jzlNCoCqpB2j3jk7W");
        var body = JsonConvert.SerializeObject(mongo);
        request.AddParameter("application/json", body,  ParameterType.RequestBody);
        await restClient.ExecuteAsync(request);
    }
}