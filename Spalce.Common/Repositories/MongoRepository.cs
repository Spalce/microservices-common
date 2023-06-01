using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Spalce.Common.Classes;

namespace Spalce.Common.Repositories;

public class MongoRepository<T> : IRepository<T> where T : IEntity
{
    private readonly IMongoCollection<T> _dbCollection;
    private readonly FilterDefinitionBuilder<T> _filterBuilder = Builders<T>.Filter;

    public MongoRepository(IMongoDatabase db, string collection)
    {
        _dbCollection = db.GetCollection<T>(collection);
    }

    public async Task<Response<IReadOnlyCollection<T>>> GetAllAsync()
    {
        var items = await _dbCollection.Find(_ => true).ToListAsync();
        if (items != null)
        {
            return new Response<IReadOnlyCollection<T>>
            {
                Message = "Records found successfully",
                Data = items,
                Errors = null,
                IsSuccess = true
            };
        }

        return new Response<IReadOnlyCollection<T>>
        {
            Message = "Records not found",
            Data = null,
            Errors = new List<string> { "Records not found" },
            IsSuccess = false
        };
    }

    public async Task<Response<IReadOnlyCollection<T>>> GetAllAsync(Expression<Func<T, bool>> filter)
    {
        var items = await _dbCollection.Find(filter).ToListAsync();
        if (items != null)
        {
            return new Response<IReadOnlyCollection<T>>
            {
                Message = "Records found successfully",
                Data = items,
                Errors = null,
                IsSuccess = true
            };
        }

        return new Response<IReadOnlyCollection<T>>
        {
            Message = "Records not found",
            Data = null,
            Errors = new List<string> { "Records not found" },
            IsSuccess = false
        };
    }

    public async Task<Response<T>> GetByIdAsync(Guid id)
    {
        var filter = _filterBuilder.Eq(item => item.Id, id);
        var item =  await _dbCollection.Find(filter).SingleOrDefaultAsync();
        if (item != null)
        {
            return new Response<T>
            {
                Message = "Record found successfully",
                Data = item,
                Errors = null,
                IsSuccess = true
            };
        }

        return new Response<T>
        {
            Message = "Record not found",
            Data = default,
            Errors = new List<string> { "Record not found" },
            IsSuccess = false
        };
    }

    public async Task<Response<T>> GetByIdAsync(Expression<Func<T, bool>> filter)
    {
        var item =  await _dbCollection.Find(filter).SingleOrDefaultAsync();
        if (item != null)
        {
            return new Response<T>
            {
                Message = "Record found successfully",
                Data = item,
                Errors = null,
                IsSuccess = true
            };
        }

        return new Response<T>
        {
            Message = "Record not found",
            Data = default,
            Errors = new List<string> { "Record not found" },
            IsSuccess = false
        };
    }

    public async Task<Response<T>> CreateAsync(T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        await _dbCollection.InsertOneAsync(item);

        return new Response<T>
        {
            Message = "Record created successfully",
            Data = item,
            Errors = null,
            IsSuccess = true
        };
    }

    public async Task<Response<T>> UpdateAsync(T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        var filter = _filterBuilder.Eq(existingItem => existingItem.Id, item.Id);
        var result = await _dbCollection.ReplaceOneAsync(filter, item);
        if (result.IsAcknowledged && result.ModifiedCount == 0)
        {
            return new Response<T>
            {
                Message = "Record not found",
                Data = default,
                Errors = new List<string> { "Record not found" },
                IsSuccess = false
            };
        }

        return new Response<T>
        {
            Message = "Record updated successfully",
            Data = item,
            Errors = null,
            IsSuccess = true
        };
    }

    public async Task<Response<T>> DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(id));
        }

        var filter = _filterBuilder.Eq(item => item.Id, id);
        var result = await _dbCollection.DeleteOneAsync(filter);
        if (result.IsAcknowledged && result.DeletedCount == 0)
        {
            return new Response<T>
            {
                Message = "Record not found",
                Data = default,
                Errors = new List<string> { "Record not found" },
                IsSuccess = false
            };
        }

        return new Response<T>
        {
            Message = "Record deleted successfully",
            Data = default,
            Errors = null,
            IsSuccess = true
        };
    }

    public async Task<Response<T>> CreateManyAsync(IReadOnlyCollection<T> items)
    {
        try
        {
            await _dbCollection.InsertManyAsync(items);
            return new Response<T> { IsSuccess = true, Message = "Records created successfully" };
        }
        catch (Exception ex)
        {
            return new Response<T> { IsSuccess = false, Errors = new List<string> { ex.Message } };
        }
    }

    public async Task<Response<T>> UpdateManyAsync(IReadOnlyCollection<T> items)
    {
        try
        {
            var updateTasks = new List<Task<ReplaceOneResult>>();
            var successCount = 0;
            var failureCount = 0;
            foreach (var item in items)
            {
                var filter = Builders<T>.Filter.Eq("_id", item.Id);
                var options = new ReplaceOptions { IsUpsert = false };
                var updateTask = _dbCollection.ReplaceOneAsync(filter, item, options);
                if (updateTask.Result.IsAcknowledged && updateTask.Result.ModifiedCount == 0)
                {
                    failureCount++;
                }
                updateTasks.Add(updateTask);
                successCount++;
            }

            await Task.WhenAll(updateTasks);

            return new Response<T>
            {
                IsSuccess = true,
                Message = $"{successCount} records updated successfully and {failureCount} records failed to update"
            };
        }
        catch (Exception ex)
        {
            return new Response<T> { IsSuccess = false, Errors = new List<string> { ex.Message } };
        }
    }

    public async Task<Response<T>> DeleteManyAsync(IReadOnlyCollection<Guid> ids)
    {
        try
        {
            var filter = Builders<T>.Filter.In("_id", ids);
            var result = await _dbCollection.DeleteManyAsync(filter);

            return new Response<T>
            {
                IsSuccess = true,
                Message = $"{result.DeletedCount} records deleted successfully and {ids.Count - result.DeletedCount} records failed to delete",
            };
        }
        catch (Exception ex)
        {
            return new Response<T> { IsSuccess = false, Errors = new List<string> { ex.Message } };
        }
    }
}
