
# CurrencyConverter.Api

This project is a test API service exploring HttpClient custom configuration. It connects to two external APIs (exchange rate and open weather):
https://v6.exchangerate-api.com <br>
http://api.openweathermap.org

The program is using IHttpClientFactory to get new HttpClient, where the factory reuses an existing HttpMessageHandler for multiple HttpClients until the handler’s lifetime expires. Once the handler lifetime expires, a new one is created and old connections are gracefully closed.
- IHttpClientFactory keeps the HttpMessageHandler alive behind the scenes for a configured lifetime (default: 2 minutes).
- Multiple HttpClient instances can reuse the same underlying connections.
- This avoids extra handshakes and reduces latency

Other functionality includes:
- Retry logic of Http calls: 3 attempts, exponential backoff (2s, 4s, 8s)
- Circuit breaker: break after 5 consecutive failures, for 30 seconds

## Prerequisites

Before running the solution, ensure you have the following installed:

- .NET 8.0 or higher
- Visual Studio or Visual Studio Code (with C# extension)
- NuGet packages (e.g. Swashbuckle, Polly, Moq, MSTest) installed via NuGet Package Manager.

## How to Run the Solution

1. Clone the repository to your local machine:

   ```bash
   git clone https://github.com/angelkomarov/CurrencyConverter.Api.git


2. Open the solution in Visual Studio or your preferred IDE.

3. **Restore dependencies**:

   If you're using Visual Studio, the dependencies should restore automatically. Otherwise, you can manually restore the NuGet packages from the terminal:

   ```bash
   dotnet restore
   ```

4. **Run the application**:

   To run the API, use the following command from the project root:

   ```bash
   dotnet run --project CurrencyConverter.Api
   ```

5. **Run Tests**:

   The solution includes unit tests using MSTest and Moq. To run the tests, use the following command:

## 🔧 Configuration

   This will run all the tests defined in the `CurrencyConverter.Api.Tests` project.

## How the Currency Conversion Works

1. The main functionality of the service is in the `CurrencyConverterService` class, specifically the `ConvertAsync` method. The method:

1.1 Takes an `ExchangeRequest` object containing the input and output currencies and the amount to convert. <br>
1.2 Sends a request to the external exchange rate API. <br>
1.3 Returns an `ExchangeResponse` with the converted value if successful.

Example request:

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
2. To simulate calls to mutiple APIS an additional service has been created for getting weather temperature for particular city: `OpenWeatherService`

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

In the case of an error, an appropriate error message is logged, and the exception is propagated upwards.

However, a few improvements could be made to handle errors in a more robust way:

### Potential Improvements:

1. **Global Exception Handling**:

   * It might be beneficial to implement global exception handling using middleware or a centralized error handler, which would help avoid repetitive try-catch blocks in individual services.

2. **API Key Management**:

   * The API key is hardcoded in the code (`fb26a56ef07a26d08e3863d2`). This is not secure for production use. The API key better be stored in a secure configuration file (e.g., `appsettings.json`) or environment variables.

   Example (`appsettings.json`):

   ```json
   {
     "ApiSettings": {
       "BaseUrl": "https://v6.exchangerate-api.com",
       "ApiKey": "your-api-key-here"
     }
   }
   ```

   These settings then can be injected into the `CurrencyConverterService` through dependency injection.

3. **API Response Validation**:

   * Currently, the code assumes the API will always return a valid response in the expected format. It would be safer to add validation logic to check if the response is not `null` and has a valid `conversion_rate`.
   * Example validation:

   ```csharp
   if (response == null || response.conversion_rate == null)
   {
       throw new Exception("Invalid response from the exchange rate API.");
   }
   ```

4. **Caching Exchange Rates**:

   * Consider caching the exchange rates for a period of time (e.g., 1 hour if possible) to avoid unnecessary API calls and reduce external dependencies.

5. **Rate Limiting**:

## 📦 Dependencies
- `Polly` – resiliency policies
- `Microsoft.Extensions.Http` – HttpClient factory
- `Swashbuckle` – Swagger/OpenAPI
- `MSTest, Moq` – testing

6. **Logging Improvements**:

## 📄 License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).


