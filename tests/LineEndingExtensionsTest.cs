using System;
using FluentAssertions;
using Xunit;

namespace Serilog.Formatting.Log4Net.Tests
{
    public class LineEndingExtensionsTest
    {
        [Fact]
        public void None()
        {
            LineEnding.None.ToCharacters().Should().Be("");
        }

        [Fact]
        public void LineFeed()
        {
            LineEnding.LineFeed.ToCharacters().Should().Be("\n");
        }

        [Fact]
        public void CarriageReturn()
        {
            LineEnding.CarriageReturn.ToCharacters().Should().Be("\r");
        }

        [Fact]
        public void CarriageReturn_LineFeed()
        {
            (LineEnding.CarriageReturn | LineEnding.LineFeed).ToCharacters().Should().Be("\r\n");
        }

        [Fact]
        public void InvalidLineEnding()
        {
            FluentActions.Invoking(() => ((LineEnding)4).ToCharacters())
                .Should().ThrowExactly<ArgumentOutOfRangeException>()
                .And.Message.Should().StartWith("The value of argument 'lineEnding' (4) is invalid for enum type 'LineEnding'.");

        }
    }
}