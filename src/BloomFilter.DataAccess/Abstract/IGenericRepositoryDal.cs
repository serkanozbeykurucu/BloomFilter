using BloomFilter.Entity.Concrete;
using System.Linq.Expressions;

namespace BloomFilter.DataAccess.Abstract;

public interface IGenericRepositoryDal<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null);
    Task<List<T>> GetPagedListAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool orderByDescending = false);
    Task<int> GetCountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<T> AddAsync(T entity);
    Task<List<T>> AddRangeAsync(List<T> entities);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAsync(T entity);
    Task<int> SaveChangesAsync();
}