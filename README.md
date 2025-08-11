````markdown
# CurrencyConverter.Api

This project is an API service designed to convert currencies using an external exchange rate API. The main functionality is provided by the `CurrencyConverterService` class, which takes in an `ExchangeRequest` and returns an `ExchangeResponse`.

The project leverages the `HttpClient` class for communication with an external currency conversion API. 

## Prerequisites

Before running the solution, ensure you have the following installed:

- .NET 8.0 or higher
- Visual Studio or Visual Studio Code (with C# extension)
- NuGet packages (e.g. Moq, MSTest) installed via NuGet Package Manager.

## How to Run the Solution

1. Clone the repository to your local machine:

   ```bash
   git clone https://github.com/angelkomarov/CurrencyConverter.Api.git
````

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

   ```bash
   dotnet test
   ```

   This will run all the tests defined in the `CurrencyConverter.Api.Tests` project.

## How the Currency Conversion Works

The main functionality of the service is in the `CurrencyConverterService` class, specifically the `ConvertAsync` method. The method:

1. Takes an `ExchangeRequest` object containing the input and output currencies and the amount to convert.
2. Sends a request to the external exchange rate API.
3. Returns an `ExchangeResponse` with the converted value if successful.

Example request:

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

In the case of an error, an appropriate error message is logged, and the exception is propagated upwards.

However, a few improvements could be made to handle errors in a more robust way:

### Potential Improvements:

1. **Global Exception Handling**:

   * It might be beneficial to implement global exception handling using middleware or a centralized error handler, which would help avoid repetitive try-catch blocks in individual services.
   * You could use a custom exception filter to handle `HttpRequestException` and return a structured error response (e.g., HTTP 500 with a meaningful error message).

2. **API Key Management**:

   * The API key is hardcoded in the code (`fb26a56ef07a26d08e3863d2`). This is not secure for production use. Consider storing the API key in a secure configuration file (e.g., `appsettings.json`) or environment variables.

   Example (`appsettings.json`):

   ```json
   {
     "ApiSettings": {
       "BaseUrl": "https://v6.exchangerate-api.com",
       "ApiKey": "your-api-key-here"
     }
   }
   ```

   You could then inject these settings into the `CurrencyConverterService` through dependency injection.

3. **API Response Validation**:

   * Currently, the code assumes the API will always return a valid response in the expected format. It would be safer to add validation logic to check if the response is not `null` and has a valid `conversion_rate`.
   * Example validation:

   ```csharp
   if (response == null || response.conversion_rate == null)
   {
       throw new Exception("Invalid response from the exchange rate API.");
   }
   ```

4. **Timeout Handling**:

   * Add proper handling for HTTP request timeouts. The `HttpClient` should have a timeout setting, and we should ensure we catch and handle timeouts explicitly, potentially offering a retry mechanism or graceful error handling.

   Example:

   ```csharp
   httpClient.Timeout = TimeSpan.FromSeconds(30); // Set a reasonable timeout for the request
   ```

5. **Caching Exchange Rates**:

   * Consider caching the exchange rates for a period of time (e.g., 1 hour if possible) to avoid unnecessary API calls and reduce external dependencies.

6. **Unit Tests**:

   * Extend unit tests mock API calls correctly. The tests use Moq to mock the `IHttpClientFactory` and `HttpClient`, simulating responses from the API. This prevents the tests from making actual API calls and ensures faster, more reliable tests.

7. **Rate Limiting**:

   * The external exchange rate API may have rate limits or restrictions. Implement a strategy to handle this, such as retries with exponential backoff or fallback mechanisms.

8. **Logging Improvements**:

   * Logging could be enhanced with additional context information (e.g., request IDs, transaction IDs) to make it easier to trace issues, especially in production environments.

## Additional Notes

* The solution currently uses `HttpClient` in a `using` block, which is generally discouraged. The recommended approach is to use `IHttpClientFactory` (which is already implemented here) to prevent socket exhaustion issues that can occur if `HttpClient` is not reused.

* The external API might not be available all the time, so it's a good idea to implement retries for transient failures and notify the user when the service is temporarily unavailable.

