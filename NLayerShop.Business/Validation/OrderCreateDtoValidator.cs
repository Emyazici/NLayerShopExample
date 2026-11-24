using FluentValidation;
using NLayerShop.Contracts.DTOs.Orders;

namespace NLayerShop.Business.Validation
{
    public class OrderCreateDtoValidator : AbstractValidator<OrderCreateDto>
    {
        public OrderCreateDtoValidator()
        {
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("An order must contain at least one item.")
                .Must(HaveDistinctProducts).WithMessage("Each product may appear only once in an order; combine quantities instead.");

            RuleForEach(x => x.Items)
                .NotNull()
                .ChildRules(item =>
                {
                    item.RuleFor(i => i.ProductId).GreaterThan(0);
                    item.RuleFor(i => i.Quantity).GreaterThan(0);
                });
        }

        private static bool HaveDistinctProducts(List<OrderItemCreateDto>? items)
        {
            if (items is null)
                return true; // NotEmpty kurali zaten yakalar

            var ids = items.Where(i => i is not null).Select(i => i.ProductId).ToList();
            return ids.Distinct().Count() == ids.Count;
        }
    }
}
