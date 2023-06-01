using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Spalce.Common.Classes;

namespace Spalce.Common.Repositories;

public interface IRepository<T> where T : IEntity
{
    Task<Response<IReadOnlyCollection<T>>> GetAllAsync();
    Task<Response<IReadOnlyCollection<T>>> GetAllAsync(Expression<Func<T, bool>> filter);
    Task<Response<T>> GetByIdAsync(Guid id);
    Task<Response<T>> GetByIdAsync(Expression<Func<T, bool>> filter);
    Task<Response<T>> CreateAsync(T item);
    Task<Response<T>> UpdateAsync(T item);
    Task<Response<T>> DeleteAsync(Guid id);
    Task<Response<T>> CreateManyAsync(IReadOnlyCollection<T> items);
    Task<Response<T>> UpdateManyAsync(IReadOnlyCollection<T> items);
    Task<Response<T>> DeleteManyAsync(IReadOnlyCollection<Guid> ids);
}
