using System;

namespace Saturn.Backend.Data.SaturnAPI.Models;

public class CacheModel<T>
{
    public T Data { get; set; }
    public DateTime Expiration { get; set; }
}