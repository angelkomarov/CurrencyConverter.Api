namespace CurrencyConverter.Api.Models
{
    public class ProgramSettings
    {
        public string ExchangeRateUrl { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;

    }
}
