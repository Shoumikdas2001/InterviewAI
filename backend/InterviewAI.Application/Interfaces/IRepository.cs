using System.Linq.Expressions;

namespace InterviewAI.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<long> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task CreateAsync(T entity, CancellationToken ct = default);
    Task<bool> UpdateAsync(string id, T entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
}
