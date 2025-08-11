using Microsoft.EntityFrameworkCore;
using ProjectControlsReportingTool.API.Data;
using ProjectControlsReportingTool.API.Repositories.Interfaces;
using System.Linq.Expressions;

namespace ProjectControlsReportingTool.API.Repositories.Base
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return entities;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();
            
            return await _dbSet.CountAsync(predicate);
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? predicate = null)
        {
            var query = predicate == null ? _dbSet : _dbSet.Where(predicate);
            
            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> ExecuteStoredProcedureAsync(string procedureName, params object[] parameters)
        {
            var parameterString = string.Join(", ", parameters.Select((_, i) => $"{{{i}}}"));
            var sql = $"EXEC {procedureName} {parameterString}";
            return await _dbSet.FromSqlRaw(sql, parameters).ToListAsync();
        }

        public virtual async Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, params object[] parameters)
        {
            var parameterString = string.Join(", ", parameters.Select((_, i) => $"{{{i}}}"));
            var sql = $"EXEC {procedureName} {parameterString}";
            return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public virtual async Task<TResult> ExecuteStoredProcedureScalarAsync<TResult>(string procedureName, params object[] parameters)
        {
            var parameterString = string.Join(", ", parameters.Select((_, i) => $"{{{i}}}"));
            var sql = $"SELECT dbo.{procedureName}({parameterString})";
            
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@p{i}";
                parameter.Value = parameters[i] ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            await _context.Database.OpenConnectionAsync();
            var result = await command.ExecuteScalarAsync();
            return (TResult)result!;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
