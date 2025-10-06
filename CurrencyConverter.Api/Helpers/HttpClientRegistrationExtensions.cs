using Polly;
using System.Net.Http.Headers;

namespace CurrencyConverter.Api.Helpers
{
    public static class HttpClientRegistrationExtensions
    {
        public static IHttpClientBuilder AddConfiguredHttpClient(this IServiceCollection services, string name, string baseUrl, 
            IAsyncPolicy<HttpResponseMessage> retryPolicy,
            IAsyncPolicy<HttpResponseMessage> circuitBreakerPolicy,
            IAsyncPolicy<HttpResponseMessage> timeoutPolicy)
        {
            //!!AK1 Registers IHttpClientFactory (as Singleton!) in DI.Adds connection pooling and handler lifecycle management automatically
            //!!AK1.1 PooledConnectionLifetime: The maximum lifetime a connection can stay in the pool before it is forcibly closed and recreated.
            //This means even if the TCP connection is still healthy, after 15 minutes it will be dropped and a new one will be created.
            //!!AK1.2 PooledConnectionIdleTimeout: The maximum time an idle (unused) connection can sit in the pool before being closed.
            //This means if a connection hasn’t been used for 2 minutes, it will be closed.
            return services.AddHttpClient(name, client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = Timeout.InfiniteTimeSpan; //!!AK1.4 Let Polly handle timeouts instead
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                // TCP connection settings
                PooledConnectionLifetime = TimeSpan.FromMinutes(15), // dictates the absolute maximum age of a connection in the pool - recycle to respect DNS changes
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2), // dictates the maximum idle time for a connection in the pool.
                MaxConnectionsPerServer = 20,                          // avoid simutaneous connections flooding on a single server
                ConnectTimeout = TimeSpan.FromSeconds(5)               // fail fast on bad hosts
            })
            // Apply resilience policies
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(circuitBreakerPolicy)
            .AddPolicyHandler(timeoutPolicy); //!!AK1.4 add policy timeout
        }
    }
}
