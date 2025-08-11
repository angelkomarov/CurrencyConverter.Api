using CurrencyConverter.Api.Models;
using CurrencyConverter.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.ComponentModel.DataAnnotations;

//TODO: controller only tests - to mock the service layer and test the controller logic
namespace CurrencyConverter.Test
{
    [TestClass]
    public class CurrencyConverterServiceTests
    {
        private Mock<IHttpClientFactory> httpClientFactoryMock;
        private Mock<ILogger<CurrencyConverterService>> loggerMock;
        private CurrencyConverterService converterService;

        [TestInitialize]
        public void Setup()
        {
            httpClientFactoryMock = new Mock<IHttpClientFactory>();
            loggerMock = new Mock<ILogger<CurrencyConverterService>>();

            converterService = new CurrencyConverterService(httpClientFactoryMock.Object, loggerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public async Task ConvertAsync_InvalidRequest_ThrowsValidationException()
        {
            var invalidRequest = new ExchangeRequest
            {
                InputCurrency = "USD",
                OutputCurrency = "",
                Amount = 100
            };

            // Act
            await converterService.ConvertAsync(invalidRequest);
        }

        [TestMethod]
        public async Task ConvertAsync_ValidRequest_ReturnsExchangeResponse()
        {
            var exchangeRequest = new ExchangeRequest
            {
                InputCurrency = "AUD",
                OutputCurrency = "USD",
                Amount = 100
            };

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Protected() // Required for mocking protected methods like SendAsync
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("{\"conversion_rate\": 0.65}") });

            // Create an HttpClient instance using the mocked HttpMessageHandler
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            // Set HttpClientFactory to return the mocked HttpClient
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await converterService.ConvertAsync(exchangeRequest);

            // Assert
            Assert.AreEqual(65, result.Value); 
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task ConvertAsync_ApiThrowsHttpRequestException()
        {
            var exchangeRequest = new ExchangeRequest
            {
                InputCurrency = "AUD",
                OutputCurrency = "USD",
                Amount = 100
            };

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Protected() // Required for mocking protected methods like SendAsync
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Throws(new HttpRequestException("API error"));

            // Create an HttpClient instance using the mocked HttpMessageHandler
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            // Set HttpClientFactory to return the mocked HttpClient
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await converterService.ConvertAsync(exchangeRequest);
        }

    }
}