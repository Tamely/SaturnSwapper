namespace Saturn.Backend.Data.SaturnAPI.Models;

public class ItemCountModel
{
    public string _id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ID { get; set; }
    public string ItemType { get; set; }
    public ulong SwapCount { get; set; }
}