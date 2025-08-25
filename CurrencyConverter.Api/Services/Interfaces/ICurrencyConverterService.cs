using CurrencyConverter.Api.DTOs.ExchangeRate;

namespace CurrencyConverter.Api.Services.Interfaces
{
    public interface ICurrencyConverterService

    {
        Task<ExchangeResponse> ConvertAsync(ExchangeRequest exchangeRequest);
    }
}
