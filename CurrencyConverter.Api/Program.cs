using CurrencyConverter.Api.Helpers;
using CurrencyConverter.Api.Models.ExchangeRate;
using CurrencyConverter.Api.Models.OpenWeather;
using CurrencyConverter.Api.Services;
using CurrencyConverter.Api.Services.Interfaces;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using System.Net.Http.Headers;

// -------------------- POLICIES --------------------

// Retry: 3 attempts, exponential backoff (2s, 4s, 8s)
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError() //HTTP 5xx responses (500, 502, 503, 504, etc.)
    .OrResult(msg => msg.StatusCode == HttpStatusCode.RequestTimeout) //HTTP 408 Request Timeout
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // using Polly - 2s, 4s, 8s

// Circuit breaker: break for 30s after 5 consecutive errors - this protects your app if the API keeps failing.
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

var builder = WebApplication.CreateBuilder(args);

// -------------------- SERVICES --------------------

builder.Services.AddControllers();
builder.Services.Configure<ExchangeRateApiSettings>(builder.Configuration.GetSection("ExchangeRateApiSettings"));
builder.Services.Configure<OpenWeatherApiSettings>(builder.Configuration.GetSection("OpenWeatherApiSettings"));

builder.Services.AddConfiguredHttpClient("ExchangeRateApi", builder.Configuration["ExchangeRateApiSettings:Url"]!, retryPolicy, circuitBreakerPolicy);
builder.Services.AddConfiguredHttpClient("OpenWeatherApi", builder.Configuration["OpenWeatherApiSettings:Url"]!, retryPolicy, circuitBreakerPolicy);

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