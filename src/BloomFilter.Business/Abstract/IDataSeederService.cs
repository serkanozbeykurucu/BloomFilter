using BloomFilter.Shared.Responses.Concrete;

namespace BloomFilter.Business.Abstract;

public interface IDataSeederService
{
    Task<Response> SeedDataAsync();
}
