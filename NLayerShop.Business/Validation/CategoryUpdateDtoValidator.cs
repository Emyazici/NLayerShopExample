using FluentValidation;
using NLayerShop.Contracts.DTOs.Categories;

namespace NLayerShop.Business.Validation
{
    public class CategoryUpdateDtoValidator : AbstractValidator<CategoryUpdateDto>
    {
        public CategoryUpdateDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        }
    }
}
