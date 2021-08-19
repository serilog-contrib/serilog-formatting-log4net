using System.IO;
using System.Reflection;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using PublicApiGenerator;
using Xunit;

namespace Serilog.Formatting.Log4Net.Tests
{
    public class PublicApi
    {
        [Fact]
        public void ApprovePublicApi()
        {
            var assembly = typeof(Log4NetTextFormatter).Assembly;
            var publicApi = assembly.GeneratePublicApi();
            var writer = new ApprovalTextWriter(publicApi);
            Approvals.Verify(writer, new AssemblyNamer(assembly), DiffReporter.INSTANCE);
        }

        private class AssemblyNamer : UnitTestFrameworkNamer
        {
            private readonly Assembly _assembly;

            public AssemblyNamer(Assembly assembly) => _assembly = assembly;

            public override string Name => nameof(PublicApi) + "." + Path.GetFileNameWithoutExtension(_assembly.Location);
        }
    }
}