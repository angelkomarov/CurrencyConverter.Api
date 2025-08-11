using CurrencyConverter.Api.Models;

namespace CurrencyConverter.Api.Services.Interfaces
{
    public interface ICurrencyConverterService

    {
        Task<ExchangeResponse> ConvertAsync(ExchangeRequest exchangeRequest);
    }
}
