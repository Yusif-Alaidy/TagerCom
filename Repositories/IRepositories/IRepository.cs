using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace TagerCom.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {

        void Update(T entity);

        void Delete(T entity);
        Task CommitAsync();

        Task<List<T>> GetAsync(Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null, bool tracked = true);

        Task<T> GetOneAsync(Expression<Func<T, bool>>? filter = null, Expression<Func<T, object>>[]? include = null, bool tracked = true);


        Task DeleteRangeAsync(List<T> entity);

        #region Create
        Task AddAsync(T entity);
        #endregion
        public IQueryable<T> Query();
    }
}
