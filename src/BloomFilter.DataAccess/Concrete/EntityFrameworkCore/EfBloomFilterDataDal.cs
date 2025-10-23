using BloomFilter.DataAccess.Abstract;
using BloomFilter.DataAccess.Concrete.Context;
using BloomFilter.Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace BloomFilter.DataAccess.Concrete.EntityFrameworkCore;

public class EfBloomFilterDataDal : GenericRepository<BloomFilterData>, IBloomFilterDataDal
{
    public EfBloomFilterDataDal(BloomFilterDbContext context) : base(context)
    {
    }

    public async Task<BloomFilterData?> GetByFilterNameAsync(string filterName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.FilterName.ToLower() == filterName.ToLower() && x.IsActive);
    }

    public async Task<bool> UpdateBitArrayAsync(string filterName, byte[] bitArray, int elementCount)
    {
        var filterData = await GetByFilterNameAsync(filterName);
        if (filterData == null) return false;

        filterData.BitArray = bitArray;
        filterData.ElementCount = elementCount;
        filterData.LastUpdatedDate = DateTime.UtcNow;
        filterData.UpdatedDate = DateTime.UtcNow;

        await SaveChangesAsync();
        return true;
    }

    public async Task<List<BloomFilterData>> GetAllActiveFiltersAsync()
    {
        return await _dbSet
            .Where(x => x.IsActive)
            .OrderBy(x => x.FilterName)
            .ToListAsync();
    }
}