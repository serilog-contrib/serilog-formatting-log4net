using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using PublicApiGenerator;
using VerifyXunit;
using Xunit;

namespace Serilog.Formatting.Log4Net.Tests;

public class PublicApi
{
    [Theory]
    [ClassData(typeof(TargetFrameworksTheoryData))]
    public Task ApprovePublicApi(string targetFramework)
    {
        var testAssembly = typeof(PublicApi).Assembly;
        var configuration = testAssembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration.ToLowerInvariant()
                            ?? throw new InvalidDataException($"{nameof(AssemblyConfigurationAttribute)} not found in {testAssembly.Location}");
        var assemblyPath = Path.Combine(GetRootDirectoryPath(), "artifacts", "bin", "Serilog.Formatting.Log4Net", $"{configuration}_{targetFramework}", "Serilog.Formatting.Log4Net.dll");
        var assembly = Assembly.LoadFile(assemblyPath);
        var publicApi = assembly.GeneratePublicApi();
        return Verifier.Verify(publicApi, "cs").UseFileName($"PublicApi.{targetFramework}");
    }

    private static string GetRootDirectoryPath([CallerFilePath] string path = "") => Path.Combine(Path.GetDirectoryName(path)!, "..");

    private class TargetFrameworksTheoryData : TheoryData<string>
    {
        public TargetFrameworksTheoryData()
        {
            var csprojPath = Path.Combine(GetRootDirectoryPath(), "src", "Serilog.Formatting.Log4Net.csproj");
            var project = XDocument.Load(csprojPath);
            var targetFrameworks = project.XPathSelectElement("/Project/PropertyGroup/TargetFrameworks")
                                   ?? throw new InvalidDataException($"TargetFrameworks element not found in {csprojPath}");
            AddRange(targetFrameworks.Value.Split(';'));
        }
    }
}