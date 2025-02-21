using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Utils.Tests.Extensions;

public class StringExtensionsTests
{
    public class SanitizeMethod : StringExtensionsTests
    {
        [Theory]
        [InlineData("test\n", "test")]
        [InlineData("test\r", "test")]
        [InlineData("test\t", "test")]
        [InlineData("test<", "test&lt;")]
        [InlineData("test>", "test&gt;")]
        [InlineData("test<script>", "test&lt;script&gt;")]
        public void ShouldRemoveCharacters(string input, string expectedresult)
        {
            // Act
            var actualResult = input.Sanitize();

            // Assert
            Assert.Equal(expectedresult, actualResult);

        }
    }
}