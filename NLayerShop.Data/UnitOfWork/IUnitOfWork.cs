using NLayerShop.Data.Repositories.Interfaces;
using NLayerShop.Domain.Entities;

namespace NLayerShop.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IGenericRepository<Product> Products { get; }
        IGenericRepository<Category> Categories { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
