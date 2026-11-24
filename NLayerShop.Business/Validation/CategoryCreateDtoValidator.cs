using FluentValidation;
using NLayerShop.Contracts.DTOs.Categories;

namespace NLayerShop.Business.Validation
{
    public class CategoryCreateDtoValidator : AbstractValidator<CategoryCreateDto>
    {
        public CategoryCreateDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        }
    }
}
