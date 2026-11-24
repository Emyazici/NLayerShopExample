using FluentValidation;
using NLayerShop.Contracts.DTOs.Orders;
using NLayerShop.Domain.Enums;

namespace NLayerShop.Business.Validation
{
    public class OrderQueryRequestValidator : AbstractValidator<OrderQueryRequest>
    {
        public OrderQueryRequestValidator()
        {
            // Page/Size kurallari tek yerde: PagedRequestValidator.
            Include(new PagedRequestValidator());

            RuleFor(x => x.Status)
                .Must(s => TryParseStatus(s, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.Status))
                .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<OrderStatus>())}.");
        }

        // "5" gibi sayisal degerleri de reddetmek icin IsDefined kontrolu yapilir.
        public static bool TryParseStatus(string? value, out OrderStatus status)
        {
            return Enum.TryParse(value, ignoreCase: true, out status)
                   && Enum.IsDefined(status)
                   && !int.TryParse(value, out _);
        }
    }
}
