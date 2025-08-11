using System.Linq.Expressions;

namespace ProjectControlsReportingTool.API.Repositories.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        // Basic CRUD operations
        Task<T?> GetByIdAsync(Guid id);
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entities);
        Task DeleteAsync(T entity);
        Task DeleteAsync(Guid id);
        Task DeleteAsync(int id);
        Task DeleteRangeAsync(IEnumerable<T> entities);
        
        // Advanced operations
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? predicate = null);
        
        // Stored procedure support
        Task<IEnumerable<T>> ExecuteStoredProcedureAsync(string procedureName, params object[] parameters);
        Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, params object[] parameters);
        Task<TResult> ExecuteStoredProcedureScalarAsync<TResult>(string procedureName, params object[] parameters);
    }
}
