namespace Saturn.Backend.Data.Swapper.Swapping.Models;

public class Swap
{
    public string File { get; set; }
    public long Offset { get; set; }
    public byte[] Data { get; set; }
}

public class ItemModel
{
    public string Name { get; set; }
    public Swap[] Swaps { get; set; }
}