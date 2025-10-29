using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace TagerCom.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task<T> CreateAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        Task DeleteRangeAsync(List<T> entity);

        Task CommitAsync();

        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null,
      Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
      bool tracked = true);

        Task<T?> GetOneAsync(Expression<Func<T, bool>>? filter = null,
      Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
      bool tracked = true);

        Task<List<T>> GetAsync(Expression<Func<T, bool>>? expression = null,
             Expression<Func<T, object>>[]? includes = null, bool tracked = true);


        Task DeleteRangeAsync(List<T> entity);
    }
}
