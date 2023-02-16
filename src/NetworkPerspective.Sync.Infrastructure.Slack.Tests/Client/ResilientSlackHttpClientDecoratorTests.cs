using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Slack.Configs;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Client
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
                    .SetupSequence(x => x.Get<SampleResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.FatalError))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.InternalError))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.ServiceUnavailable))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.RequestTimeout))
                    .ReturnsAsync(new SampleResponse { IsOk = true, Error = null });

                var resilientClient = new ResilientSlackHttpClientDecorator(_internalHttpClient.Object, _resiliency, _logger);

                // Act
                var response = await resilientClient.Get<SampleResponse>("foo");

                // Assert
                response.IsOk.Should().BeTrue();
                _internalHttpClient.Verify(x => x.Get<SampleResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(5));
            }
        }

        public class Post : ResilientSlackHttpClientDecoratorTests
        {
            [Fact]
            public async Task ShouldRetryOnServerSideProblem()
            {
                // Arrange
                _internalHttpClient
                    .SetupSequence(x => x.Post<SampleResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.FatalError))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.InternalError))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.ServiceUnavailable))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.RequestTimeout))
                    .ReturnsAsync(new SampleResponse { IsOk = true, Error = null });

                var resilientClient = new ResilientSlackHttpClientDecorator(_internalHttpClient.Object, _resiliency, _logger);

                // Act
                var response = await resilientClient.Post<SampleResponse>("foo");

                // Assert
                response.IsOk.Should().BeTrue();
                _internalHttpClient.Verify(x => x.Post<SampleResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(5));
            }
        }

        public class PostWithContent : ResilientSlackHttpClientDecoratorTests
        {
            [Fact]
            public async Task ShouldRetryOnServerSideProblem()
            {
                // Arrange
                _internalHttpClient
                    .SetupSequence(x => x.Post<SampleResponse>(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.FatalError))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.InternalError))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.ServiceUnavailable))
                    .ThrowsAsync(new ApiException(400, SlackApiErrorCodes.RequestTimeout))
                    .ReturnsAsync(new SampleResponse { IsOk = true, Error = null });

                var resilientClient = new ResilientSlackHttpClientDecorator(_internalHttpClient.Object, _resiliency, _logger);

                // Act
                var response = await resilientClient.Post<SampleResponse>("foo", new StringContent("bar"));

                // Assert
                response.IsOk.Should().BeTrue();
                _internalHttpClient.Verify(x => x.Post<SampleResponse>(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()), Times.Exactly(5));
            }
        }

        public class SampleResponse : IResponseWithError
        {
            public bool IsOk { get; set; }
            public string Error { get; set; }
        }
    }
}