using System;
using AwesomeAssertions;

namespace Serilog.Formatting.Log4Net.Tests;

public class IndentationSettingsTest
{
    [Test]
    [Arguments(Indentation.Space, (byte)2, "  ")]
    [Arguments(Indentation.Tab, (byte)2, "\t\t")]
    [Arguments(Indentation.Space, (byte)4, "    ")]
    [Arguments(Indentation.Tab, (byte)4, "\t\t\t\t")]
    public void IndentationSettingsToString(Indentation indentation, byte size, string expectedString)
    {
        // Arrange
        var indentationSettings = new IndentationSettings(indentation, size);

        // Act
        var indentationString = indentationSettings.ToString();

        // Assert
        indentationString.Should().Be(expectedString);
    }

    [Test]
    public void InvalidIndentation()
    {
        // Act
        var action = () => new IndentationSettings((Indentation)(-1), size: 1);

        // Assert
        action.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .Which.Message.Should().StartWith("The value of argument 'indentation' (-1) is invalid for enum type 'Indentation'.");
    }

    [Test]
    public void InvalidSize()
    {
        // Act
        var action = () => new IndentationSettings(indentation: default, size: 0);

        // Assert
        action.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .Which.Message.Should().StartWith("The value of argument 'size' must be greater than 0.");
    }
}