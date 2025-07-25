using DockerHubBackend.Models;

namespace DockerHubBackend.Repository.Utils
{
    public interface ICrudRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAll();
        Task<T?> Get(Guid id);
        Task<T> Create(T entity);
        Task<T?> Update(T entity);
        Task<T?> Delete(Guid id);
    }
}
