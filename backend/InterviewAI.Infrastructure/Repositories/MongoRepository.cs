using MongoDB.Driver;
using InterviewAI.Application.Interfaces;
using System.Linq.Expressions;

namespace InterviewAI.Infrastructure.Repositories;

/// <summary>
/// Generic MongoDB repository implementation.
/// Handles all basic CRUD operations against a typed collection.
/// </summary>
public class MongoRepository<T> : IRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;

    public MongoRepository(IMongoCollection<T> collection)
    {
        _collection = collection;
    }

    public async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _collection.Find(_ => true).ToListAsync(ct);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _collection.Find(predicate).ToListAsync(ct);
    }

    public async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _collection.Find(predicate).FirstOrDefaultAsync(ct);
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _collection.CountDocumentsAsync(predicate, cancellationToken: ct);
    }

    public async Task CreateAsync(T entity, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: ct);
    }

    public async Task<bool> UpdateAsync(string id, T entity, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
        var result = await _collection.ReplaceOneAsync(filter, entity, cancellationToken: ct);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
        var result = await _collection.DeleteOneAsync(filter, ct);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _collection.Find(predicate).AnyAsync(ct);
    }
}
