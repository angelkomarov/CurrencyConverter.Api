namespace CurrencyConverter.Api.Services.Interfaces
{
    public interface IOpenWeatherService
    {
        Task<float?> GetCityTemperatureAsync(string city);
    }
}
