using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Linq.Expressions;
using TagerCom.Repositories.IRepositories;

namespace TagerCom.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private DataAccess.ApplicationDbContext _context;// = new();
        private DbSet<T> _db;

        public Repository(DataAccess.ApplicationDbContext context)
        {
            _context = context;
            _db = _context.Set<T>();
        }

        // CRUD
        public async Task AddAsync(T entity)
        {
            await _db.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _db.Update(entity);
        }

        public void Delete(T entity)
        {
            _db.Remove(entity);
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

       

        public async Task<List<T>> GetAsync(Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>[]? include = null, bool tracked = true)
        {
            var data = _db.AsQueryable();

            if (filter is not null)
            {
                data = data.Where(filter);
            }
            if (include is not null)
            {
                foreach (var item in include)
                {
                    data = data.Include(item);
                }
            }
            if (!tracked)
            {
                data = data.AsNoTracking();
            }

            return await data.ToListAsync();
        }

        public async Task<T> GetOneAsync(Expression<Func<T, bool>>? filter = null, Expression<Func<T,
            object>>[]? include = null, bool tracked = true)
        {

            return (await GetAsync(filter, include, tracked)).FirstOrDefault()!;
        }

        //public async Task<T?> GetOneAsync(Expression<Func<T, bool>> expression, bool tracked = true,
        //      params Expression<Func<T, object>>[] includes)
        //{
        //    return (await GetAsync(expression, includes, tracked)).FirstOrDefault();
        //}

        public async Task DeleteRangeAsync(List<T> entity)
        {
            _db.RemoveRange(entity);
        }

        public IQueryable<T> Query()
        {
            return _db.AsQueryable();
        }

       

    }
}
