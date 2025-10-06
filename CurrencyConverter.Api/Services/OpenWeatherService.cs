using CurrencyConverter.Api.DTOs.ExchangeRate;
using CurrencyConverter.Api.DTOs.OpenWeather;
using CurrencyConverter.Api.Exceptions;
using CurrencyConverter.Api.Models.OpenWeather;
using CurrencyConverter.Api.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace CurrencyConverter.Api.Services
{
    public class OpenWeatherService : IOpenWeatherService
    {
        private readonly ILogger<CurrencyConverterService> _logger;
        private readonly OpenWeatherApiSettings _openWeatherApiSettings;
        private readonly HttpClient _httpClient;

        public OpenWeatherService(IHttpClientFactory httpClientFactory,
            ILogger<CurrencyConverterService> logger,
            IOptions<OpenWeatherApiSettings> openWeatherApiSettings)
        {
            _logger = logger;
            _openWeatherApiSettings = openWeatherApiSettings.Value;
            _httpClient = httpClientFactory.CreateClient("OpenWeatherApi");           
        }

        public async Task<float?> GetCityTemperatureAsync(string city)
        {
            _logger.LogInformation("GetCityTemperatureAsync started with {@Request}", city);
            if (string.IsNullOrEmpty(city)) 
            {
                throw new ValidationException("Invalid request!");
            }

            var url = $"/data/{_openWeatherApiSettings.Version}/find?q={city}&appid={_openWeatherApiSettings.AppId}";
            try
            {
                CityWeatherResponse? weatherInfo = await _httpClient.GetFromJsonAsync<CityWeatherResponse>(url);

                float? temp = null;
                if (weatherInfo?.list?.Length > 0 && weatherInfo?.list[0]?.main?.temp != null)
                    temp = weatherInfo?.list[0]?.main?.temp - 273; //kelvin
                _logger.LogInformation($"*** end GetCityTemperatureAsync ***");
                return temp;
            }
            catch (BrokenCircuitException ex)
            {
                // Circuit breaker is OPEN, no request was even attempted
                _logger.LogError(ex, "+++ Circuit is open - Weather API unavailable.");
                throw new ExternalApiUnavailableException("External service temporarily unavailable.", ex);
            }
            catch (TimeoutRejectedException ex)
            {
                _logger.LogError(ex, "+++ Weather API timed out.");
                throw new ExternalApiUnavailableException("External service timeout.", ex);
            }
            catch (HttpRequestException ex)
            {
                // This means all retries were exhausted (transient errors each time)
                _logger.LogError(ex, "+++ Weather API HTTP error");
                throw new ExternalApiException("Can't obtain city temperature!");
            }
            catch (Exception ex)
            {
                // Unexpected error (serialization, mapping, etc.)
                _logger.LogError(ex, "+++ Weather API: unexpected error");
                throw;
            }
        }
    }
}
