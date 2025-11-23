using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLayerShop.Contracts.DTOs.Products
{
    public record ProductCreateDto(string Name, decimal Price, string Description,int StockCount, int? CategoryId);

}
