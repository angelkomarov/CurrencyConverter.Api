using CurrencyConverter.Api.DTOs.ExchangeRate;
using CurrencyConverter.Api.Services;
using CurrencyConverter.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Api.Controllers
{
    [Route("")]
    [ApiController]
    public class OpenWeatherController(IOpenWeatherService weatherService) : ControllerBase
    {
        [HttpGet("TemperatureService")]
        public async Task<IActionResult> GetCityTemperatureAsync(string city)
        {
            //TODO add global exception handling - return uniform error response
            try
            {
                //TODO may use Clean Architecture (add a mediator to handle requests)
                var result = await weatherService.GetCityTemperatureAsync(city);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest("Invalid Weather Temperature request.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unable to process Weather Temperature request." });
            }
        }

    }
}
