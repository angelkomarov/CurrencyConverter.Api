using CurrencyConverter.Api.Models;
using CurrencyConverter.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Moq.Protected;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CurrencyConverter.Test
{
    [TestClass]
    public class CurrencyConverterApiIntegrationTests
    {
        private static WebApplicationFactory<Program> factory;
        private static HttpClient client;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            factory = new WebApplicationFactory<Program>();
            client = factory.CreateClient();
        }

        [TestMethod]
        public async Task PostJsonRequest_ReturnsJsonResponse()
        {
            // Arrange
            string json = """
            {
                "amount": 10,
                "inputCurrency": "AUD",
                "outputCurrency": "USD"
            }
            """;

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/ExchangeService", content);
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
            StringAssert.Contains(responseString, "6.52");
            StringAssert.Contains(response.Content.Headers.ContentType.ToString(), "application/json");
        }



    }
}