using FluentValidation;
using NLayerShop.Contracts.Common;

namespace NLayerShop.Business.Validation
{
    public class PagedRequestValidator : AbstractValidator<PagedRequest>
    {
        public const int MaxPageSize = 100;

        public PagedRequestValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.Size).InclusiveBetween(1, MaxPageSize);
        }
    }
}
