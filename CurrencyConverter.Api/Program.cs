using CurrencyConverter.Api.Models.ExchangeRate;
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

//!!AK1 Registers IHttpClientFactory in DI.Adds connection pooling and handler lifecycle management automatically
//!!AK1.1 PooledConnectionLifetime: The maximum lifetime a connection can stay in the pool before it is forcibly closed and recreated.
//This means even if the TCP connection is still healthy, after 15 minutes it will be dropped and a new one will be created.
//!!AK1.2 PooledConnectionIdleTimeout: The maximum time an idle (unused) connection can sit in the pool before being closed.
//This means: if a connection hasn’t been used for 2 minutes, it will be closed.
builder.Services.AddHttpClient("ExchangeRateApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExchangeRateApiSettings:Url"] ?? "");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    // Request timeout (safe guard, in addition to handler timeouts)
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    // TCP connection settings
    PooledConnectionLifetime = TimeSpan.FromMinutes(15),   // recycle to respect DNS changes
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2), // drop idle pool connections
    MaxConnectionsPerServer = 20,                          // avoid simutaneous connections flooding on a single server
    ConnectTimeout = TimeSpan.FromSeconds(5)               // fail fast on bad hosts
})
// Apply resilience policies
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICurrencyConverterService, CurrencyConverterService>();

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