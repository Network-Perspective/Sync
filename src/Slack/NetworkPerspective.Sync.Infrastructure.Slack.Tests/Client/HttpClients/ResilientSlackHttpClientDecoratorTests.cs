using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Slack.Configs;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Client.HttpClients
{
    public class ResilientSlackHttpClientDecoratorTests
    {
        private readonly ILogger<ResilientSlackHttpClientDecorator> _logger = NullLogger<ResilientSlackHttpClientDecorator>.Instance;
        private readonly Mock<ISlackHttpClient> _internalHttpClient = new Mock<ISlackHttpClient>();
        private readonly Resiliency _resiliency = new Resiliency
        {
            Retries = Enumerable
                .Range(0, 10)
                .Select(x => TimeSpan.FromMilliseconds(1))
                .ToArray()
        };


        public class Get : ResilientSlackHttpClientDecoratorTests
        {
            [Fact]
            public async Task ShouldRetryOnServerSideProblem()
            {
                // Arrange
                _internalHttpClient
                    .SetupSequence(x => x.GetAsync<SampleResponseWithError>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.FatalError))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.InternalError))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.ServiceUnavailable))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.RequestTimeout))
                    .ReturnsAsync(new SampleResponseWithError { IsOk = true, Error = null });

                var resilientClient = new ResilientSlackHttpClientDecorator(_internalHttpClient.Object, _resiliency, _logger);

                // Act
                var response = await resilientClient.GetAsync<SampleResponseWithError>("foo");

                // Assert
                response.IsOk.Should().BeTrue();
                _internalHttpClient.Verify(x => x.GetAsync<SampleResponseWithError>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(5));
            }
        }

        public class Post : ResilientSlackHttpClientDecoratorTests
        {
            [Fact]
            public async Task ShouldRetryOnServerSideProblem()
            {
                // Arrange
                _internalHttpClient
                    .SetupSequence(x => x.PostAsync<SampleResponseWithError>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.FatalError))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.InternalError))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.ServiceUnavailable))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.RequestTimeout))
                    .ReturnsAsync(new SampleResponseWithError { IsOk = true, Error = null });

                var resilientClient = new ResilientSlackHttpClientDecorator(_internalHttpClient.Object, _resiliency, _logger);

                // Act
                var response = await resilientClient.PostAsync<SampleResponseWithError>("foo");

                // Assert
                response.IsOk.Should().BeTrue();
                _internalHttpClient.Verify(x => x.PostAsync<SampleResponseWithError>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(5));
            }
        }

        public class PostWithContent : ResilientSlackHttpClientDecoratorTests
        {
            [Fact]
            public async Task ShouldRetryOnServerSideProblem()
            {
                // Arrange
                _internalHttpClient
                    .SetupSequence(x => x.PostAsync<SampleResponseWithError>(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.FatalError))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.InternalError))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.ServiceUnavailable))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.RequestTimeout))
                    .ReturnsAsync(new SampleResponseWithError { IsOk = true, Error = null });

                var resilientClient = new ResilientSlackHttpClientDecorator(_internalHttpClient.Object, _resiliency, _logger);

                // Act
                var response = await resilientClient.PostAsync<SampleResponseWithError>("foo", new StringContent("bar"));

                // Assert
                response.IsOk.Should().BeTrue();
                _internalHttpClient.Verify(x => x.PostAsync<SampleResponseWithError>(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Exactly(5));
            }
        }
    }
}