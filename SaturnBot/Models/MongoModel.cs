using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SaturnBot.Models;

public class MongoModel
{
    public string dataSource { get; set; }
    public string database { get; set; }
    public string collection { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<User> documents = new();

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public FilterDefinition<BsonDocument> filter { get; set; }
}

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string _id { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string id { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string generic { get; set; }
}