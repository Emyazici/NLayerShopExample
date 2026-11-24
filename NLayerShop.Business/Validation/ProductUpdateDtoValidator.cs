using FluentValidation;
using NLayerShop.Contracts.DTOs.Products;

namespace NLayerShop.Business.Validation
{
    // PUT = full replace: tum alanlar zorunlu, CategoryId null = kategorisiz.
    public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
    {
        public ProductUpdateDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotNull().MaximumLength(1000);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.StockCount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CategoryId).GreaterThan(0).When(x => x.CategoryId.HasValue);
            RuleFor(x => x.RowVersion)
                .NotEmpty()
                .Must(RowVersionValidation.BeValidRowVersion)
                    // CurrentValidator: without it the condition would also disable NotEmpty above.
                    .When(x => !string.IsNullOrEmpty(x.RowVersion), ApplyConditionTo.CurrentValidator)
                    .WithMessage("'Row Version' must be a valid Base64-encoded rowversion value.");
        }
    }
}
