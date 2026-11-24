using NLayerShop.Domain.Common;

namespace NLayerShop.Data.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<T>> GetAllAsync(CancellationToken ct = default);
        IQueryable<T> Query();
        Task AddAsync(T entity, CancellationToken ct = default);
        void Update(T entity);         // sync kalması yeterli
        void Remove(T entity);         // sync kalması yeterli
    }

}
