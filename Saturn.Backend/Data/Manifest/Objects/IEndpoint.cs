using System.Threading.Tasks;
using RestSharp;

namespace Saturn.Backend.Data.Manifest.Objects;

public interface IEndpoint
{
    RestResponse GetResponse();
    RestResponse<T> GetResponse<T>();
    Task<RestResponse> GetResponseAsync();
    Task<RestResponse<T>> GetResponseAsync<T>();
}