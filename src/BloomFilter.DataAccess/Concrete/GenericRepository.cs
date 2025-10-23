using BloomFilter.DataAccess.Abstract;
using BloomFilter.DataAccess.Concrete.Context;
using BloomFilter.Entity.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BloomFilter.DataAccess.Concrete;

public class GenericRepository<T> : IGenericRepositoryDal<T> where T : BaseEntity
{
    protected readonly BloomFilterDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(BloomFilterDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }

    public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.Where(x => x.IsActive).ToListAsync();
    }

    public virtual async Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var query = _dbSet.Where(x => x.IsActive);

        if (predicate != null)
            query = query.Where(predicate);

        return await query.ToListAsync();
    }

    public virtual async Task<List<T>> GetPagedListAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool orderByDescending = false)
    {
        var query = _dbSet.Where(x => x.IsActive);

        if (predicate != null)
            query = query.Where(predicate);

        if (orderBy != null)
        {
            query = orderByDescending
                ? query.OrderByDescending(orderBy)
                : query.OrderBy(orderBy);
        }
        else
        {
            query = query.OrderByDescending(x => x.CreatedDate);
        }

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public virtual async Task<int> GetCountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var query = _dbSet.Where(x => x.IsActive);

        if (predicate != null)
            query = query.Where(predicate);

        return await query.CountAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.CreatedDate = DateTime.UtcNow;
        entity.IsActive = true;

        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual async Task<List<T>> AddRangeAsync(List<T> entities)
    {
        foreach (var entity in entities)
        {
            entity.CreatedDate = DateTime.UtcNow;
            entity.IsActive = true;
        }

        await _dbSet.AddRangeAsync(entities);
        return entities;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedDate = DateTime.UtcNow;
        _dbSet.Update(entity);
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;

        return await DeleteAsync(entity);
    }

    public virtual async Task<bool> DeleteAsync(T entity)
    {
        entity.IsActive = false;
        entity.UpdatedDate = DateTime.UtcNow;
        await UpdateAsync(entity);
        return true;
    }

    public virtual async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}