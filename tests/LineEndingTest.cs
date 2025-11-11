using System;
using AwesomeAssertions;

namespace Serilog.Formatting.Log4Net.Tests;

public class LineEndingTest
{
    [Test]
    public void InvalidLineEnding()
    {
        Action action = () => _ = new Log4NetTextFormatter(c => c.UseLineEnding((LineEnding)4));

        action.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithMessage("The value of argument 'lineEnding' (4) is invalid for enum type 'LineEnding'.*");
    }
}