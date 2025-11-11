using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using PublicApiGenerator;
using VerifyTUnit;

namespace Serilog.Formatting.Log4Net.Tests;

public class PublicApi
{
    [Test]
    [MethodDataSource(nameof(GetTargetFrameworks))]
    public Task ApprovePublicApi(string targetFramework)
    {
        var testAssembly = typeof(PublicApi).Assembly;
        var configuration = testAssembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration
                            ?? throw new InvalidDataException($"{nameof(AssemblyConfigurationAttribute)} not found in {testAssembly.Location}");
        var assemblyPath = Path.Combine(GetSrcDirectoryPath(), "bin", configuration, targetFramework, "Serilog.Formatting.Log4Net.dll");
        var assembly = System.Reflection.Assembly.LoadFile(assemblyPath);
        var publicApi = assembly.GeneratePublicApi();
        return Verifier.Verify(publicApi, "cs").UseFileName($"PublicApi.{targetFramework}");
    }

    private static string GetSrcDirectoryPath([CallerFilePath] string path = "") => Path.Combine(Path.GetDirectoryName(path)!, "..", "src");

    public static IEnumerable<string> GetTargetFrameworks()
    {
        var csprojPath = Path.Combine(GetSrcDirectoryPath(), "Serilog.Formatting.Log4Net.csproj");
        var project = XDocument.Load(csprojPath);
        var targetFrameworks = project.XPathSelectElement("/Project/PropertyGroup/TargetFrameworks")
                               ?? throw new InvalidDataException($"TargetFrameworks element not found in {csprojPath}");
        return targetFrameworks.Value.Split(';');
    }
}