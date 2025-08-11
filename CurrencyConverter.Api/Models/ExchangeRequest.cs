namespace CurrencyConverter.Api.Models
{
    public class ExchangeRequest
    {
        public float Amount { get; set; }
        public string InputCurrency { get; set; } = null!;
        public string OutputCurrency { get; set; } = null!;
    }

}
