using FluentValidation;
using NLayerShop.Business.Querying;
using NLayerShop.Contracts.Common;

namespace NLayerShop.Business.Validation
{
    public class ProductQueryRequestValidator : AbstractValidator<ProductQueryRequest>
    {
        public const int MaxSearchLength = 100;

        public ProductQueryRequestValidator()
        {
            // Page / Size rules are shared with every paged endpoint.
            Include(new PagedRequestValidator());

            RuleFor(x => x.Search).MaximumLength(MaxSearchLength);

            RuleFor(x => x.CategoryId).GreaterThan(0).When(x => x.CategoryId.HasValue);

            RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue);
            RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue);

            RuleFor(x => x.MaxPrice)
                .GreaterThanOrEqualTo(x => x.MinPrice!.Value)
                .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
                .WithMessage("'Max Price' must be greater than or equal to 'Min Price'.");

            RuleFor(x => x.SortBy)
                .Must(ProductSortOptions.IsValidSortField)
                .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
                .WithMessage($"'Sort By' must be one of: {string.Join(", ", ProductSortOptions.SortFields)}.");

            RuleFor(x => x.SortDir)
                .Must(ProductSortOptions.IsValidSortDirection)
                .When(x => !string.IsNullOrWhiteSpace(x.SortDir))
                .WithMessage($"'Sort Dir' must be one of: {string.Join(", ", ProductSortOptions.SortDirections)}.");
        }
    }
}
