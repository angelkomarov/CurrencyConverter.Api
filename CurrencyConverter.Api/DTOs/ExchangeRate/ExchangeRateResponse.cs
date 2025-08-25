namespace CurrencyConverter.Api.DTOs.ExchangeRate
{
    public class ExchangeRateResponse
    {
        public string result { get; set; } = null!;
        public string documentation { get; set; } = null!;
        public string terms_of_use { get; set; } = null!;
        public int time_last_update_unix { get; set; } 
        public string time_last_update_utc { get; set; } = null!;
        public int time_next_update_unix { get; set; }
        public string time_next_update_utc { get; set; } = null!;
        public string base_code { get; set; } = null!; 
        public string target_code { get; set; } = null!;
        public float conversion_rate { get; set; }
    }

}
