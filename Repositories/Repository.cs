using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using TagerCom.Repositories.IRepositories;

namespace TagerCom.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private ApplicationDbContext _context;// = new();
        private DbSet<T> _db;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _db = _context.Set<T>();
        }

        // CRUD
        public async Task<T> CreateAsync(T entity)
        {
            await _db.AddAsync(entity);
            return entity;
        }

        public void Update(T entity)
        {
            _db.Update(entity);
        }

        public void Delete(T entity)
        {
            _db.Remove(entity);
        }

        public async Task DeleteRangeAsync(List<T> entity)
        {
            _db.RemoveRange(entity);
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        

       

        public async Task<T?> GetOneAsync(Expression<Func<T, bool>>? filter = null,
      Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
      bool tracked = true)
        {
            IQueryable<T> query = _db;

            // لو مش عايز التتبع
            if (!tracked)
                query = query.AsNoTracking();

            // لو فيه علاقات عايز تضمها
            if (include != null)
                query = include(query);

            // لو فيه شرط فلترة
            if (filter != null)
                query = query.Where(filter);

            // رجع أول عنصر أو null
            return await query.FirstOrDefaultAsync();

        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null,
     Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
     bool tracked = true)
        {
            IQueryable<T> query = _db;

            // لو مش عايز التتبع
            if (!tracked)
                query = query.AsNoTracking();

            // ضم العلاقات (Includes)
            if (include != null)
                query = include(query);

            // لو فيه شرط فلترة
            if (filter != null)
                query = query.Where(filter);

            // نفذ الاستعلام ورجع النتايج كلها كـ List
            return await query.ToListAsync();

        }


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



       
    }
}
