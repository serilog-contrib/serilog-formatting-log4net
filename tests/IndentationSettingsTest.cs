using System;
using FluentAssertions;
using Xunit;

namespace Serilog.Formatting.Log4Net.Tests;

public class IndentationSettingsTest
{
    [Theory]
    [InlineData(Indentation.Space, 2, "  ")]
    [InlineData(Indentation.Tab, 2, "\t\t")]
    [InlineData(Indentation.Space, 4, "    ")]
    [InlineData(Indentation.Tab, 4, "\t\t\t\t")]
    public void IndentationSettingsToString(Indentation indentation, byte size, string expectedString)
    {
        // Arrange
        var indentationSettings = new IndentationSettings(indentation, size);

        // Act
        var indentationString = indentationSettings.ToString();

        // Assert
        indentationString.Should().Be(expectedString);
    }

    [Fact]
    public void InvalidIndentation()
    {
        // Act
        var action = () => new IndentationSettings((Indentation)(-1), size: 1);

        // Assert
        action.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .Which.Message.Should().StartWith("The value of argument 'indentation' (-1) is invalid for enum type 'Indentation'.");
    }

    [Fact]
    public void InvalidSize()
    {
        // Act
        var action = () => new IndentationSettings(indentation: default, size: 0);

        // Assert
        action.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .Which.Message.Should().StartWith("The value of argument 'size' must be greater than 0.");
    }
}