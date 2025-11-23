using AutoMapper;
using NLayerShop.Contracts.DTOs.Categories;
using NLayerShop.Contracts.DTOs.Products;
using NLayerShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLayerShop.Business.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() {
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>()
                .ForMember(d => d.CategoryId, opt => opt.Ignore())
                .ForMember(d => d.Category, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Product,ProductDetailDto>()
                .ForMember(d=>d.CategoryName, opt=>opt.MapFrom(s => s.Category!=null ? s.Category.Name : null));
             
            CreateMap<CategoryCreateDto, Category>();
            CreateMap<CategoryUpdateDto, Category>();
            CreateMap<Category, CategoryDetailDto>();
        }
    }
}
