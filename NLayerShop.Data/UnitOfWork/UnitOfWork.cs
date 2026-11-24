using NLayerShop.Data.Context;
using NLayerShop.Data.Repositories.Implementations;
using NLayerShop.Data.Repositories.Interfaces;
using NLayerShop.Domain.Entities;

namespace NLayerShop.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IGenericRepository<Product> Products { get; }

        public IGenericRepository<Category> Categories { get; }

        public IGenericRepository<Order> Orders { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Products = new GenericRepository<Product>(context);
            Categories = new GenericRepository<Category>(context);
            Orders = new GenericRepository<Order>(context);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
           return _context.SaveChangesAsync(ct);
        }
    }
}
