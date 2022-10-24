using System.Threading.Tasks;
using PublicApiGenerator;
using VerifyXunit;
using Xunit;

namespace Serilog.Formatting.Log4Net.Tests;

[UsesVerify]
public class PublicApi
{
    [Fact]
    public Task ApprovePublicApi()
    {
        var publicApi = typeof(Log4NetTextFormatter).Assembly.GeneratePublicApi();
        return Verifier.Verify(publicApi, "cs").UseFileName("PublicApi");
    }
}