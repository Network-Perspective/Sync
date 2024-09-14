using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Pagination;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Tests.Pagination
{
    public class CursorPaginationHandlerTests
    {
        [Fact]
        public async Task ShouldKeepCallingUntilEndOfCollection()
        {
            // Arrange
            var apiCallsCount = 0;
            var handler = new CursorPaginationHandler(NullLogger<CursorPaginationHandler>.Instance);
            var token = CancellationToken.None;

            var responseMock = new Mock<ICursorPagination>();
            responseMock
                .SetupSequence(x => x.Metadata)
                .Returns(new MetadataResponse { NextCursor = Guid.NewGuid().ToString() })
                .Returns(new MetadataResponse { NextCursor = Guid.NewGuid().ToString() })
                .Returns(new MetadataResponse { NextCursor = Guid.NewGuid().ToString() })
                .Returns(new MetadataResponse { NextCursor = string.Empty });

            var callApiMock = new Func<string, CancellationToken, Task<ICursorPagination>>((nextCursor, token) =>
           {
               apiCallsCount++;
               return Task.FromResult(responseMock.Object);
           });

            var getEntitiesMock = new Func<ICursorPagination, IEnumerable<Entity>>(x =>
            {
                return new List<Entity>
               {
                   new Entity { Counter = apiCallsCount }
               };
            });

            // Act
            var result = await handler.GetAllAsync(callApiMock, getEntitiesMock, token);

            // Assert
            result.Should().HaveCount(4);
        }

        [Fact]
        public async Task ShouldReturnEmptyOnNullResponse()
        {
            // Arrange
            var handler = new CursorPaginationHandler(NullLogger<CursorPaginationHandler>.Instance);

            var responseMock = new Mock<ICursorPagination>();
            responseMock
                .SetupSequence(x => x.Metadata)
                .Returns(new MetadataResponse { NextCursor = Guid.NewGuid().ToString() })
                .Returns(new MetadataResponse { NextCursor = string.Empty });

            var callApiMock = new Func<string, CancellationToken, Task<ICursorPagination>>((nextCursor, token) =>
            {
                return Task.FromResult(responseMock.Object);
            });

            var getEntitiesMock = new Func<ICursorPagination, IEnumerable<Entity>>(x =>
            {
                return null;
            });

            // Act
            var result = await handler.GetAllAsync(callApiMock, getEntitiesMock, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        class Entity
        {
            public int Counter = 0;
        }
    }
}