using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace Serilog.Formatting.Log4Net.Tests;

public class LineEndingTest
{
    [Fact]
    public Task InvalidLineEnding()
    {
        return Verifier.Throws(() => _ = new Log4NetTextFormatter(c => c.UseLineEnding((LineEnding)4)));
    }
}