using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Infrastructure.InteractionsCache;

using Newtonsoft.Json;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Infrastructure.InteractionsCache
{
    public class InteractionsFileCacheTests : IDisposable
    {
        private readonly string _tempDirPath;
        private readonly ILogger<InteractionsFileCache> _logger = NullLogger<InteractionsFileCache>.Instance;
        private readonly IDataProtector _dataProtector = new EphemeralDataProtectionProvider().CreateProtector("foo");

        public InteractionsFileCacheTests()
        {
            _tempDirPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirPath);
        }

        public void Dispose()
        {
            Directory.Delete(_tempDirPath, true);
        }

        [Fact]
        public async Task ShouldStoreAndAllowToFetch()
        {
            // Arrange
            var employee1 = Employee.CreateBot("bot1");
            var employee2 = Employee.CreateBot("bot2");

            var timestamp1 = new DateTime(2022, 01, 01, 12, 00, 00);
            var timestamp2 = new DateTime(2022, 01, 01, 13, 00, 00);
            var timestamp3 = new DateTime(2022, 01, 02, 12, 00, 00);
            var timestamp4 = new DateTime(2022, 01, 02, 13, 00, 00);

            var interaction1_1 = Interaction.CreateEmail(timestamp1, employee1, employee2, Guid.NewGuid().ToString());
            var interaction1_2 = Interaction.CreateEmail(timestamp2, employee1, employee2, Guid.NewGuid().ToString());
            var interaction2_1 = Interaction.CreateEmail(timestamp3, employee1, employee2, Guid.NewGuid().ToString());
            var interaction2_2 = Interaction.CreateEmail(timestamp4, employee1, employee2, Guid.NewGuid().ToString());

            var interactions1 = new HashSet<Interaction> { interaction1_1, interaction1_2, interaction2_1 };
            var interactions2 = new HashSet<Interaction> { interaction2_2 };

            var storage = new InteractionsFileCache(_tempDirPath, _dataProtector, _logger);

            // Act
            await storage.PushInteractionsAsync(interactions1);
            await storage.PushInteractionsAsync(interactions2);

            // Assert
            var storedInteractions1 = await storage.PullInteractionsAsync(timestamp1.Date);
            var storedInteractions2 = await storage.PullInteractionsAsync(timestamp3.Date);

            storedInteractions1.Should().HaveCount(2);
            storedInteractions2.Should().HaveCount(2);
            Directory.GetFiles(_tempDirPath).Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnEmptySetOnNoInteractions()
        {
            // Arrange
            var storage = new InteractionsFileCache(_tempDirPath, _dataProtector, _logger);

            // Act
            var result = await storage.PullInteractionsAsync(DateTime.UtcNow.Date);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldEncryptData()
        {
            // Arrange
            var employee1 = Employee.CreateBot("bot1");
            var employee2 = Employee.CreateBot("bot2");

            var timestamp1 = new DateTime(2022, 01, 01, 12, 00, 00);

            var interaction = Interaction.CreateEmail(timestamp1, employee1, employee2, Guid.NewGuid().ToString());

            var interactions = new HashSet<Interaction> { interaction };

            var storage = new InteractionsFileCache(_tempDirPath, _dataProtector, _logger);

            // Act
            await storage.PushInteractionsAsync(interactions);

            // Assert
            var storedBytes = await File.ReadAllBytesAsync(Path.Combine(_tempDirPath, "2022-01-01"));
            var storedContent = Encoding.Unicode.GetString(storedBytes);
            Func<IEnumerable<Interaction>> func = () => JsonConvert.DeserializeObject<IEnumerable<Interaction>>(storedContent);
            func.Should().Throw<JsonReaderException>();
        }
    }
}