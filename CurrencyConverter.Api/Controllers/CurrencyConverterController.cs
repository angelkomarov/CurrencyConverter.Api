using CurrencyConverter.Api.DTOs.ExchangeRate;
using CurrencyConverter.Api.Services;
using CurrencyConverter.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

//!!AK1.4 Microsoft.Extensions.Http.Polly is a NuGet package that integrates the Polly library with IHttpClientFactory in .NET applications.
//Its primary purpose is to provide robust resilience and transient fault-handling capabilities for HTTP client operations.
//dotnet add package Microsoft.Extensions.Http.Polly
//NuGet\Install-Package Microsoft.Extensions.Http.Polly -Version 9.0.8

namespace CurrencyConverter.Api.Controllers
{
    //Extensibility - add more methods for different operations in the future
    [Route("")]
    [ApiController]
    public class CurrencyConverterController(ICurrencyConverterService currencyConverterService) : ControllerBase
    {
        [HttpPost("ExchangeService")]
        public async Task<IActionResult> ExchangeServiceAsync(ExchangeRequest exchangeRequest)
        {
            //TODO add global exception handling - return uniform error response
            try
            {
                //TODO may use Clean Architecture and add a mediator to handle requests
                var result = await currencyConverterService.ConvertAsync(exchangeRequest);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest("Invalid exchange request.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unable to process exchange request." });
            }
        }
    }
}
