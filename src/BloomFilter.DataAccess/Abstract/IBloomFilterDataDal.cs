using BloomFilter.Entity.Concrete;

namespace BloomFilter.DataAccess.Abstract;

public interface IBloomFilterDataDal : IGenericRepositoryDal<BloomFilterData>
{
    Task<BloomFilterData?> GetByFilterNameAsync(string filterName);
    Task<bool> UpdateBitArrayAsync(string filterName, byte[] bitArray, int elementCount);
    Task<List<BloomFilterData>> GetAllActiveFiltersAsync();
}
