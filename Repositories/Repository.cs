using Microsoft.EntityFrameworkCore;
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
        //Old Code
        /*
        public async Task<List<T>> GetAsync(Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null, bool tracked = true)
        {
            var entities = _db.AsQueryable();

            if (expression is not null)
            {
                entities = entities.Where(expression);
            }

            if (includes is not null)
            {
                foreach (var item in includes)
                {
                    entities = entities.Include(item);
                }
            }

            if (!tracked)
            {
                entities = entities.AsNoTracking();
            }

            return await entities.ToListAsync();
        }
        */
        public async Task<List<T>> GetAsync(Expression<Func<T, bool>>? filter = null, Expression<Func<T, object>>[]? include = null, bool tracked = true)
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

        public async Task<T?> GetOneAsync(Expression<Func<T, bool>> expression, Expression<Func<T, object>>[]? includes = null, bool tracked = true)
        {
            return (await GetAsync(expression, includes, tracked)).FirstOrDefault();
        }

        public async Task DeleteRangeAsync(List<T> entity)
        {
            _db.RemoveRange(entity);
        }
    }
}
