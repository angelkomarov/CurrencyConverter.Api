
# CurrencyConverter.Api

A test API project demonstrating **HttpClient resiliency**, **Polly policies**, and **centralized exception handling** using **Problem Details (RFC 7807)**.  
It acts as a currency converter (via an external exchange-rate API) and also fetches weather data to showcase multi-API integration.
## 🚀 Features

- Typed `HttpClient` via `IHttpClientFactory`
- **Polly-based resiliency**:
  - Timeout policy
  - Retry with exponential backoff
  - Circuit breaker
- Centralized exception handling with structured RFC 7807 error responses
- Clean, extensible architecture (services, DI, logging)
- Unit test coverage for core logic

## Prerequisites

Before running the solution, ensure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- API keys for:
  - [ExchangeRate API](https://www.exchangerate-api.com/)
  - [OpenWeatherMap](https://openweathermap.org/api)

   ```bash
   dotnet test
   ```

Use `appsettings.json` (or override with environment variables). Example:

```json
{
  "InputCurrency": "AUD",
  "OutputCurrency": "USD",
  "Amount": 100
}
```

## Caveats

### Handling `HttpRequestException`

The project currently handles `HttpRequestException` when making requests to the external exchange rate API. The exception handling is implemented in the following way:

* If the API fails (e.g., network issues, invalid response, etc.), an `HttpRequestException` is thrown.
* The `ConvertAsync` method catches this exception, logs the error, and rethrows the exception to allow the calling function to handle it appropriately.

Example:

```csharp
catch (HttpRequestException ex)
{
    logger.LogError(ex, "*** ConvertAsync: Http error api ***");
    throw; // Re-throwing the exception for further handling
}
```

## 🛡️ Error Handling & Problem Details
This API uses standardized error responses based on RFC 7807 - Problem Details for HTTP APIs.

# 📦 Error Format
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.X.X",
  "title": "Short description",
  "status": 4xx/5xx,
  "detail": "More details about the error"
}
```
## 🔁 Exception-to-Response Mapping

| Exception Type              | Scenario                                | HTTP Status | `type` URI                                            | Title                      |
|----------------------------|------------------------------------------|-------------|--------------------------------------------------------|----------------------------|
| `ValidationException`      | Invalid input                            | 400         | `https://tools.ietf.org/html/rfc7231#section-6.5.1`   | Bad Request                |
| `HttpRequestException`     | External API failure                     | 502         | `https://tools.ietf.org/html/rfc7231#section-6.6.3`   | External API error         |
| `TimeoutRejectedException`| Polly timeout triggered                  | 503         | `https://tools.ietf.org/html/rfc7231#section-6.6.4`   | External API error            |
| `BrokenCircuitException`   | Circuit breaker is open                  | 503         | `https://tools.ietf.org/html/rfc7231#section-6.6.4`   | External API error |
| *(Unhandled exceptions)*   | Internal server error                    | 500         | `https://tools.ietf.org/html/rfc7231#section-6.6.1`   | Internal Server Error      |

## ✅ Testing
- Unit tests included
- Uses Moq and testable HttpMessageHandler
- Covers:
  - Service logic
  - Timeout/retry behavior
  - Exception handling
  - Input validation

Run tests with:
```bash
dotnet test
```

   * The free version of external exchange rate API may have rate limits or restrictions. 

## 🧭 Future Enhancements
- Add caching for currency rates
- Support fallback APIs for redundancy
- Rate limiting / API throttling
- Use secrets manager (e.g., Azure Key Vault)
- Integration tests with wiremock or similar

   * Logging could be enhanced with additional context information (e.g., request IDs, transaction IDs) to make it easier to trace issues, especially in production environments.

## Additional Notes

* The external API might not be available all the time, so it's a good idea to implement retries for transient failures and notify the user when the service is temporarily unavailable.

