using EatIT.Core.Entities;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Infrastructure.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BasicEntity<int>
    {
        private readonly ApplicationDBContext _context;

        public GenericRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync() => await _context.Set<T>().CountAsync();

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null) return; 
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<T> GetAll() => _context.Set<T>().AsNoTracking().ToList();

        public IEnumerable<T> GetAll(params Expression<Func<T, bool>>[] includes) => _context.Set<T>().AsNoTracking().ToList();

        public async Task<IReadOnlyList<T>> GetAllAsync() => await _context.Set<T>().AsNoTracking().ToListAsync();

        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().AsQueryable();
            foreach (var item in includes)
            {
                query = query.Include(item);
            }
            return await query.ToListAsync();
        }

        public async Task<T> GetAsync(T id) => await _context.Set<T>().FindAsync(id);

        public async Task<T> GetAsync(int id) => await _context.Set<T>().FindAsync(id);

        public async Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            //IQueryable<T> query = _context.Set<T>();
            IQueryable<T> query = _context.Set<T>().Where(x => x.Id == id);
            foreach (var item in includes)
            {
                query = query.Include(item);
            }
            return await query.FirstOrDefaultAsync();
            //return await((DbSet<T>)query).FindAsync(id);
        }

        public async Task UpdateAsync(int id, T entity)
        {
            var entity_value = await _context.Set<T>().FindAsync(id);
            if(entity_value != null)
            {
                _context.Update(entity_value);
                await _context.SaveChangesAsync();
            }
        }
    }
}
