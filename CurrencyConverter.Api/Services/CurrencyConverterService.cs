using CurrencyConverter.Api.Models;
using CurrencyConverter.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace CurrencyConverter.Api.Services
{
    public class CurrencyConverterService(IHttpClientFactory httpClientFactory,
        ILogger<CurrencyConverterService> logger) : ICurrencyConverterService
    {
        public async Task<ExchangeResponse> ConvertAsync(ExchangeRequest exchangeRequest)
        {
            logger.LogInformation("***  ConvertAsync: ***");

            try
            {
                if (string.IsNullOrWhiteSpace(exchangeRequest?.InputCurrency) || string.IsNullOrWhiteSpace(exchangeRequest?.OutputCurrency) || exchangeRequest?.Amount <= 0)
                    //TODO throw domain specific exceptions
                    throw new ValidationException("Invalid request!");
                //TODO baseUrl and api key should be settings 
                string baseUrl = "https://v6.exchangerate-api.com";
                string apiKey = "fb26a56ef07a26d08e3863d2";
                using (var httpClient = httpClientFactory.CreateClient())
                {
                    httpClient.BaseAddress = new Uri(baseUrl);
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await httpClient.GetFromJsonAsync<ExchangeRateResponse>($"/v6/{apiKey}/pair/{exchangeRequest.InputCurrency}/{exchangeRequest.OutputCurrency}");
                    logger.LogInformation($"*** end ConvertAsync ***");
                    //TODO may use AutoMapper to map response to ExchangeResponse if needed
                    return new ExchangeResponse
                    {
                        InputCurrency = exchangeRequest.InputCurrency,
                        OutputCurrency = exchangeRequest.OutputCurrency,
                        Amount = exchangeRequest.Amount,
                        Value = response?.conversion_rate * exchangeRequest.Amount ?? 0
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "*** ConvertAsync: Http error api ***");
                //TODO if add global exception handling then no need to catch and re-throw here
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "*** ConvertAsync error  ***");
                //TODO if add global exception handling then no need to catch and re-throw here
                throw;
            }
        }
    }
}
