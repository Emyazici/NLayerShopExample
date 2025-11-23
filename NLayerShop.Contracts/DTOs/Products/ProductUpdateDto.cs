using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLayerShop.Contracts.DTOs.Products
{
    public record ProductUpdateDto(string? Name, decimal? Price, string? Description,int? StockCount,bool? RemoveCategory,int? CategoryId);
}
