using DockerHubBackend.Data;
using DockerHubBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DockerHubBackend.Repository.Utils
{
    public abstract class CrudRepository<T> : ICrudRepository<T> where T : BaseEntity
    {
        protected DataContext _context;
        protected DbSet<T> _entities;

        protected CrudRepository(DataContext context)
        {
            _context = context;
            _entities = context.Set<T>();
        }
        public async virtual Task<T> Create(T entity)
        {
            await _entities.AddAsync(entity);         
            await _context.SaveChangesAsync();            
            return entity;
        }

        public async virtual Task<T?> Delete(Guid id)
        {            
            var entity = await Get(id);

            if (entity == null)
            {
                return null;
            }

            _entities.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async virtual Task<T?> Get(Guid id)
        {            
            return await _entities.FindAsync(id);
        }

        public async virtual Task<IEnumerable<T>> GetAll()
        {
            return await _entities.ToListAsync();
        }

        public async virtual Task<T?> Update(T entity)
        {
            if (_entities.Any(e => e.Equals(entity)))
            {
                _entities.Update(entity);
                await _context.SaveChangesAsync();
                return entity;
            }            
            return null;
        }

    }
}
