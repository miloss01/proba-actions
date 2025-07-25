using Microsoft.EntityFrameworkCore.Storage;

namespace DockerHubBackend.Repository.Utils
{
    public interface IUnitOfWork
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task SaveChangesAsync();
    }
}
