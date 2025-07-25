using DockerHubBackend.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace DockerHubBackend.Repository.Utils
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;

        public EfUnitOfWork(DataContext context)
        {
            _context = context;
        }

        public Task<IDbContextTransaction> BeginTransactionAsync()
            => _context.Database.BeginTransactionAsync();

        public Task SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}
