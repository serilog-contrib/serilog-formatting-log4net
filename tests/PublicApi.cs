using System.IO;
using System.Reflection;
using ApprovalTests;
using ApprovalTests.Core;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using FluentAssertions;
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
            Approvals.Verify(writer, new AssemblyNamer(assembly), new MultiReporter(DiffReporter.INSTANCE, new FluentAssertionReporter()));
        }

        private class AssemblyNamer : UnitTestFrameworkNamer
        {
            private readonly Assembly _assembly;

            public AssemblyNamer(Assembly assembly) => _assembly = assembly;

            public override string Name => nameof(PublicApi) + "." + Path.GetFileNameWithoutExtension(_assembly.Location);
        }

        // So that we have the full approved + received text written on the console in case it fails on continuous integration server
        // where the DiffEngine used by `DiffReporter` is [automatically disabled](https://github.com/VerifyTests/DiffEngine/blob/6.4.5/src/DiffEngine/DisabledChecker.cs#L10).
        private class FluentAssertionReporter : IApprovalFailureReporter
        {
            public void Report(string approved, string received) => File.ReadAllText(received).Should().Be(File.ReadAllText(approved));
        }
    }
}