namespace CurrencyConverter.Api.Exceptions
{
    public class ExternalApiUnavailableException : ExternalApiException
    {
        public ExternalApiUnavailableException(string message, Exception? inner = null)
                : base(message)
        { }
    }
}
