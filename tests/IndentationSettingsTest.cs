using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace Serilog.Formatting.Log4Net.Tests;

public class IndentationSettingsTest
{
    [Theory]
    [InlineData(Indentation.Space, 2)]
    [InlineData(Indentation.Tab, 2)]
    [InlineData(Indentation.Space, 4)]
    [InlineData(Indentation.Tab, 4)]
    public Task IndentationSettingsToString(Indentation indentation, byte size)
    {
        // Arrange
        var indentationSettings = new IndentationSettings(indentation, size);

        // Act
        var indentationString = indentationSettings.ToString();

        // Assert
        return Verifier.Verify(indentationString).UseParameters(indentation, size);
    }

    [Fact]
    public Task InvalidIndentation()
    {
        return Verifier.Throws(() => new IndentationSettings((Indentation)(-1), size: 1));
    }

    [Fact]
    public Task InvalidSize()
    {
        return Verifier.Throws(() => new IndentationSettings(indentation: default, size: 0));
    }
}