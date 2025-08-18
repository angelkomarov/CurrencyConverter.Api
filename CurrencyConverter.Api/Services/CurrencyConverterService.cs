using CurrencyConverter.Api.Models;
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
        private readonly ProgramSettings _settings;
        private readonly HttpClient _httpClient;

        public CurrencyConverterService(IHttpClientFactory httpClientFactory,
        ILogger<CurrencyConverterService> logger, 
        IOptions<ProgramSettings> programSettings)
        {
            _logger = logger;
            _settings = programSettings.Value;
            _httpClient = httpClientFactory.CreateClient("ExchangeRateApi");
        }

        public async Task<ExchangeResponse> ConvertAsync(ExchangeRequest exchangeRequest)
        {
            _logger.LogInformation("***  ConvertAsync: ***");

            try
            {
                if (string.IsNullOrWhiteSpace(exchangeRequest?.InputCurrency) || string.IsNullOrWhiteSpace(exchangeRequest?.OutputCurrency) || exchangeRequest?.Amount <= 0)
                    //TODO throw domain specific exceptions
                    throw new ValidationException("Invalid request!");

                var url = $"/{_settings.Version}/{_settings.ApiKey}/{_settings.Operation}/" +
                  $"{exchangeRequest.InputCurrency}/{exchangeRequest.OutputCurrency}";

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
                _logger.LogError(ex, "*** ConvertAsync: Http error api ***");
                //TODO if add global exception handling then no need to catch and re-throw here
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "*** ConvertAsync error  ***");
                //TODO if add global exception handling then no need to catch and re-throw here
                throw;
            }
        }
    }
}
