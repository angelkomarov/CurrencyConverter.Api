using CurrencyConverter.Api.Helpers;
using CurrencyConverter.Api.Models.ExchangeRate;
using CurrencyConverter.Api.Models.OpenWeather;
using CurrencyConverter.Api.Services;
using CurrencyConverter.Api.Services.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using System.Net.Http.Headers;

// -------------------- POLICIES  --------------------
//using helpers: to create separate policies for both http clients: ExchangeRateApi and OpenWeatherApi

//Retry happens if the API fails with: Network errors(connection reset, DNS failure, etc.)
//5xx errors
//408 Request Timeout

//using jitter - 3 attempts on any for the error codes set below, exponential backoff (2s, 4s, 8s)
static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
{
    var jitterer = new Random();
    return HttpPolicyExtensions
        .HandleTransientHttpError() //HTTP 5xx responses (500, 502, 503, 504, etc. + 408 Request Timeout)
        //.OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound) //HTTP 404 Not Found
        .WaitAndRetryAsync(
            3, // number of retries
            retryAttempt => {
                // Exponential backoff (2, 4, 8) with jitter up to 1s
                var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                var jitter = TimeSpan.FromMilliseconds(jitterer.Next(0, 1000));
                return baseDelay + jitter;
            });
}

static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicyNoJitter()
{
    var jitterer = new Random();
    return HttpPolicyExtensions
        .HandleTransientHttpError() //HTTP 5xx responses (500, 502, 503, 504, etc. + 408 Request Timeout)
        //.OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound) //HTTP 404 Not Found
        .WaitAndRetryAsync(
            3,
            retryAttempt =>
                // Exponential backoff (2, 4, 8) with jitter up to 1s
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

// Circuit breaker: break for 30s after 5 consecutive errors (any of 5xx, 408 Request Timeout).
//Any subsequent requests within the next 30 seconds will fail immediately with BrokenCircuitException.
//After 30 seconds, it goes into half-open state:
//Allows 1 trial request through, then circuit closes (normal again)
//If that fails - circuit re-opens for another 30 sec.
static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() //HTTP 5xx responses (500, 502, 503, 504, etc. + 408 Request Timeout)
        //.OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound) //HTTP 404 Not Found
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}

var builder = WebApplication.CreateBuilder(args);

// -------------------- SERVICES --------------------
builder.Services.AddControllers();
builder.Services.Configure<ExchangeRateApiSettings>(builder.Configuration.GetSection("ExchangeRateApiSettings"));
builder.Services.Configure<OpenWeatherApiSettings>(builder.Configuration.GetSection("OpenWeatherApiSettings"));

//!!AK register HttpClients with Polly policies
builder.Services.AddConfiguredHttpClient("ExchangeRateApi", builder.Configuration["ExchangeRateApiSettings:Url"]!, CreateRetryPolicy(), CreateCircuitBreakerPolicy());
builder.Services.AddConfiguredHttpClient("OpenWeatherApi", builder.Configuration["OpenWeatherApiSettings:Url"]!, CreateRetryPolicy(), CreateCircuitBreakerPolicy());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICurrencyConverterService, CurrencyConverterService>();
builder.Services.AddScoped<IOpenWeatherService, OpenWeatherService>();

var app = builder.Build();

// -------------------- Configure the HTTP request pipeline --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }