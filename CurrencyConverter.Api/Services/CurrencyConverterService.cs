using CurrencyConverter.Api.DTOs.ExchangeRate;
using CurrencyConverter.Api.Models.ExchangeRate;
using CurrencyConverter.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Runtime;

namespace CurrencyConverter.Api.Services
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        private readonly ILogger<CurrencyConverterService> _logger;
        private readonly ExchangeRateApiSettings _exchangeRateApiSettings;
        private readonly HttpClient _httpClient;

        public CurrencyConverterService(IHttpClientFactory httpClientFactory,
        ILogger<CurrencyConverterService> logger, 
        IOptions<ExchangeRateApiSettings> exchangeRateApiSettings)
        {
            _logger = logger;
            _exchangeRateApiSettings = exchangeRateApiSettings.Value;
            //!!AK1.3 You get a new HttpClient instance each time.
            //BUT the factory reuses an existing HttpMessageHandler (the heavy part that manages TCP/SSL connections)
            //for multiple HttpClients until the handler’s lifetime expires (default: 2 minutes).
            //Once the handler lifetime expires, a new one is created and old connections are gracefully closed.
            _httpClient = httpClientFactory.CreateClient("ExchangeRateApi");
        }

        public async Task<ExchangeResponse> ConvertAsync(ExchangeRequest exchangeRequest)
        {
            _logger.LogInformation("ConvertAsync started with {@Request}", exchangeRequest);

            if (string.IsNullOrWhiteSpace(exchangeRequest?.InputCurrency) || 
                string.IsNullOrWhiteSpace(exchangeRequest?.OutputCurrency) || 
                exchangeRequest?.Amount <= 0)
            {
                throw new ValidationException("Invalid request!");
            }

            var url = $"/{_exchangeRateApiSettings.Version}/{_exchangeRateApiSettings.ApiKey}/{_exchangeRateApiSettings.Operation}/" +
                      $"{exchangeRequest.InputCurrency}/{exchangeRequest.OutputCurrency}";
            try 
            { 
                var response = await _httpClient.GetFromJsonAsync<ExchangeRateResponse>(url);
                _logger.LogInformation($"*** end ConvertAsync ***");
                //TODO may use AutoMapper to map response to ExchangeResponse if needed
                return new ExchangeResponse
                {
                    InputCurrency = exchangeRequest.InputCurrency,
                    OutputCurrency = exchangeRequest.OutputCurrency,
                    Amount = exchangeRequest.Amount,
                    Value = response?.conversion_rate * exchangeRequest.Amount ?? 0
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "ConvertAsync: HTTP error calling ExchangeRate API");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConvertAsync: Unexpected error");
                throw;
            }
        }
    }
}
