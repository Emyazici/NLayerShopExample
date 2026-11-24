namespace NLayerShop.Business.Exceptions;

/// <summary>
/// Thrown when a requested resource does not exist (or is soft-deleted). Mapped to HTTP 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with id '{key}' was not found.") { }
}
