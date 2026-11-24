namespace NLayerShop.Business.Validation
{
    internal static class RowVersionValidation
    {
        // SQL Server rowversion = 8 byte.
        private const int RowVersionLength = 8;

        public static bool BeValidRowVersion(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            Span<byte> buffer = stackalloc byte[RowVersionLength + 4];
            return Convert.TryFromBase64String(value, buffer, out var written) && written == RowVersionLength;
        }
    }
}
