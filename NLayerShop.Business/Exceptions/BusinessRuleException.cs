namespace NLayerShop.Business.Exceptions;

/// <summary>
/// Thrown when a request is well-formed but violates a business rule. Mapped to HTTP 422.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
