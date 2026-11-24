using FluentValidation;
using NLayerShop.Contracts.DTOs.Products;

namespace NLayerShop.Business.Validation
{
    public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
    {
        public ProductUpdateDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        }

    }
}
