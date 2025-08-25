namespace CurrencyConverter.Api.DTOs.ExchangeRate
{
    public class ExchangeResponse
    {
        public float Amount { get; set; }
        public string InputCurrency { get; set; } = null!;
        public string OutputCurrency { get; set; } = null!;
        public float Value { get; set; }
    }

}
