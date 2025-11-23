using NLayerShop.Data.Repositories.Interfaces;
using NLayerShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLayerShop.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IGenericRepository<Product> Products { get; }
        IGenericRepository<Category> Categories { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
