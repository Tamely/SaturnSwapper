using CUE4Parse;

namespace Saturn.Backend.Data.Swapper.Core.Models;

public class SwapData
{
    public NonStaticSaturnData SaturnData { get; set; }
    public byte[] Data { get; set; }
}