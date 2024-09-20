using MetarTaf.Components.Services;
using Moq;
using Moq.Protected;
using System.Net;

namespace MetarTaf.Tests
{
    public class MetarServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _mockHttpClient;
        private readonly MetarService _metarService;
        private const string ApiKey = "test-api-key";

        public MetarServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://avwx.rest/")
            };
            _metarService = new MetarService(_mockHttpClient, ApiKey);
        }

        [Fact]
        public async Task GetMetarAsync_ReturnsValidMetar_OnSuccessfulResponse()
        {
            // Arrange
            string icao = "EKEB";
            string jsonFilePath = Path.Combine("TestCases", "Metar", "EKEB_metars.json");
            string jsonResponse = await File.ReadAllTextAsync(jsonFilePath);
            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            var result = await _metarService.GetMetarAsync(icao);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EKEB", result.Station);
            Assert.Equal(12, result.Temperature.Value);
            Assert.Equal(30, result.WindDirection.Value);
            Assert.Equal(5, result.WindSpeed.Value);
            Assert.Equal(1028, result.Altimeter.Value);
            Assert.Equal("202120Z", result.Time.Repr);

        }

        [Fact]
        public async Task GetMetarAsync_ReturnsNull_OnInvalidJsonResponse()
        {
            // Arrange
            string icao = "INVALID";
            var invalidJsonResponse = "{invalid-json}";
            SetupHttpResponse(HttpStatusCode.OK, invalidJsonResponse);

            // Act
            var result = await _metarService.GetMetarAsync(icao);

            // Assert
            Assert.Null(result); // Expect null due to JSON deserialization failure
        }

        [Fact]
        public async Task GetMetarAsync_ThrowsHttpRequestException_OnErrorResponse()
        {
            // Arrange
            string icao = "KJFK";
            SetupHttpResponse(HttpStatusCode.InternalServerError, string.Empty);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _metarService.GetMetarAsync(icao));
        }

        [Fact]
        public async Task GetMetarAsync_ReturnsNull_OnEmptyResponse()
        {
            // Arrange
            string icao = "KJFK";
            SetupHttpResponse(HttpStatusCode.OK, string.Empty);

            // Act
            var result = await _metarService.GetMetarAsync(icao);

            // Assert
            Assert.Null(result); // Expect null due to empty response body
        }

        private void SetupHttpResponse(HttpStatusCode statusCode, string content)
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.AbsolutePath.Contains("/metar/")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
        }
    }
}
