
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

## 🔧 Configuration

Use `appsettings.json` (or override with environment variables). Example:

```json
{
  "ApiSettings": {
    "ExchangeRate": {
      "BaseUrl": "https://v6.exchangerate-api.com",
      "ApiKey": "YOUR_EXCHANGE_KEY"
    },
    "OpenWeather": {
      "BaseUrl": "https://api.openweathermap.org",
      "ApiKey": "YOUR_WEATHER_KEY"
    }
  }
}
```

## 🚦 Running the API
```bash
git clone https://github.com/angelkomarov/CurrencyConverter.Api.git
cd CurrencyConverter.Api
dotnet restore
dotnet run --project CurrencyConverter.Api
API will be available at https://localhost:5001 (or http://localhost:5000)
```
### 🔄 Currency Conversion Flow
Endpoint
```bash
POST /ExchangeService
```
Sample Request
```bash json
{
  "inputCurrency": "AUD",
  "outputCurrency": "USD",
  "amount": 100
}
```
Sample Response
```json
{
  "inputCurrency": "AUD",
  "outputCurrency": "USD",
  "amount": 100,
  "value": 64.32
}
```
### 🔄 Get Temperature Flow
Endpoint
```bash
GET /TemperatureService?city=Rome
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

## 📦 Dependencies
- `Polly` – resiliency policies
- `Microsoft.Extensions.Http` – HttpClient factory
- `Swashbuckle` – Swagger/OpenAPI
- `MSTest, Moq` – testing

## 🧭 Future Enhancements
- Add caching for currency rates
- Support fallback APIs for redundancy
- Rate limiting / API throttling
- Use secrets manager (e.g., Azure Key Vault)
- Integration tests with wiremock or similar

## 📄 License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).


