using AutoMapper;
using NLayerShop.Contracts.DTOs.Categories;
using NLayerShop.Contracts.DTOs.Orders;
using NLayerShop.Contracts.DTOs.Products;
using NLayerShop.Domain.Entities;

namespace NLayerShop.Business.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // RowVersion: byte[] <-> Base64 string (ProjectTo uyumlu expression converter)
            CreateMap<byte[], string>().ConvertUsing(src => Convert.ToBase64String(src));
            CreateMap<string, byte[]>().ConvertUsing(src => Convert.FromBase64String(src));

            CreateMap<ProductCreateDto, Product>()
                .ForMember(d => d.Category, opt => opt.Ignore());

            // PUT = full replace. RowVersion servis icinde orijinal deger olarak set edilir.
            CreateMap<ProductUpdateDto, Product>()
                .ForMember(d => d.Category, opt => opt.Ignore())
                .ForMember(d => d.RowVersion, opt => opt.Ignore());

            CreateMap<Product, ProductDetailDto>()
                .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null));

            CreateMap<CategoryCreateDto, Category>();
            CreateMap<CategoryUpdateDto, Category>();
            CreateMap<Category, CategoryDetailDto>();

            // Orders: sadece okuma yonu (olusturma kurallari OrderService icinde).
            // Status enum -> string; ProjectTo ile SQL'e cevrilir (kolon zaten string saklaniyor).
            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<Order, OrderDetailDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items.OrderBy(i => i.Id)));
            CreateMap<Order, OrderListItemDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
        }
    }
}
