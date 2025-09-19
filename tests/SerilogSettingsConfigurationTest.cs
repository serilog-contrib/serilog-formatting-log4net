using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using NuGet.Versioning;
using ReflectionMagic;
using Serilog.Core;
using Serilog.Settings.Configuration;
using Serilog.Sinks.File;
using Tomlyn.Extensions.Configuration;
using Xunit;

namespace Serilog.Formatting.Log4Net.Tests;

public class SerilogSettingsConfigurationTest
{
    private static readonly PropertyFilter DefaultPropertyFilter;
    private static readonly MessageFormatter DefaultMessageFormatter;
    private static readonly ExceptionFormatter DefaultExceptionFormatter;
    private static readonly SemanticVersion MicrosoftExtensionsConfigurationVersion;

    static SerilogSettingsConfigurationTest()
    {
        var defaultBuilder = new Log4NetTextFormatterOptionsBuilder().AsDynamic();

        DefaultPropertyFilter = defaultBuilder._filterProperty;
        DefaultMessageFormatter = defaultBuilder._formatMessage;
        DefaultExceptionFormatter = defaultBuilder._formatException;

        var assembly = typeof(JsonStreamConfigurationSource).Assembly;
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        MicrosoftExtensionsConfigurationVersion = SemanticVersion.Parse(informationalVersion ?? throw new InvalidDataException($"The [AssemblyInformationalVersion] attribute is missing from {assembly}"));
    }

    [Fact]
    public void ConfigureDefault()
    {
        // lang=toml
        const string config =
            """
            [[Serilog.WriteTo]]
            Name = 'File'
            Args.path = 'logs.xml'
            Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
            """;

        var options = GetLog4NetTextFormatterOptions(config);

        options.Should().BeEquivalentTo(new Log4NetTextFormatterOptions(
            formatProvider: null,
            cDataMode: CDataMode.Always,
            nullText: "(null)",
            xmlNamespace: new XmlQualifiedName("log4net", "http://logging.apache.org/log4net/schemas/log4net-events-1.2/"),
            xmlWriterSettings: new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\n", ConformanceLevel = ConformanceLevel.Fragment },
            filterProperty: DefaultPropertyFilter,
            formatMessage: DefaultMessageFormatter,
            formatException: DefaultExceptionFormatter
        ));
    }

    [Theory]
    [InlineData(CDataMode.Always)]
    [InlineData(CDataMode.Never)]
    [InlineData(CDataMode.IfNeeded)]
    public void ConfigureCDataMode(CDataMode cDataMode)
    {
        // lang=toml
        var config =
            $"""
             [[Serilog.WriteTo]]
             Name = 'File'
             Args.path = 'logs.xml'
             Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
             Args.formatter.cDataMode = '{cDataMode}'
             """;

        var options = GetLog4NetTextFormatterOptions(config);

        options.Should().BeEquivalentTo(new Log4NetTextFormatterOptions(
            formatProvider: null,
            cDataMode: cDataMode,
            nullText: "(null)",
            xmlNamespace: new XmlQualifiedName("log4net", "http://logging.apache.org/log4net/schemas/log4net-events-1.2/"),
            xmlWriterSettings: new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\n", ConformanceLevel = ConformanceLevel.Fragment },
            filterProperty: DefaultPropertyFilter,
            formatMessage: DefaultMessageFormatter,
            formatException: DefaultExceptionFormatter
        ));
    }

    [Theory]
    [InlineData("iv")]
    [InlineData("fr-CH")]
    public void ConfigureFormatProvider(string formatProvider)
    {
        // lang=toml
        var config =
            $"""
             [[Serilog.WriteTo]]
             Name = 'File'
             Args.path = 'logs.xml'
             Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
             Args.formatter.formatProvider = '{formatProvider}'
             """;

        var options = GetLog4NetTextFormatterOptions(config);

        options.FormatProvider.Should().BeSameAs(CultureInfo.GetCultureInfo(formatProvider));
    }

    [SkippableTheory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("<null>")]
    [InlineData("🌀")]
    public void ConfigureNullText(string? nullText)
    {
        // Null values preserved in configuration was introduced in .NET 10 Preview 7
        // See https://learn.microsoft.com/en-us/dotnet/core/compatibility/extensions/10.0/configuration-null-values-preserved
        Skip.If(nullText == null && MicrosoftExtensionsConfigurationVersion.Major < 10, "null values are preserved in configuration since .NET 10 Preview 7");

        // lang=json
        var config =
            $$"""
              {
                "Serilog": {
                  "WriteTo:File": {
                    "Name": "File",
                    "Args": {
                      "path": "logs.xml",
                      "formatter": {
                        "type": "Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net",
                        "nullText": {{(nullText == null ? "null" : $"\"{nullText}\"")}}
                      }
                    }
                  }
                }
              }
              """;
        var options = GetLog4NetTextFormatterOptions(config);

        options.NullText.Should().Be(nullText);
    }

    [Fact]
    public void ConfigureNoNullText()
    {
        // lang=toml
        const string config =
            """
            [[Serilog.WriteTo]]
            Name = 'File'
            Args.path = 'logs.xml'
            Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
            Args.formatter.noNullText = true
            """;

        var options = GetLog4NetTextFormatterOptions(config);

        options.NullText.Should().BeNull();
    }

    [Fact]
    public void ConfigureNoXmlNamespace()
    {
        // lang=toml
        const string config =
            """
            [[Serilog.WriteTo]]
            Name = 'File'
            Args.path = 'logs.xml'
            Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
            Args.formatter.noXmlNamespace = true
            """;

        var options = GetLog4NetTextFormatterOptions(config);

        options.XmlNamespace.Should().BeNull();
    }

    [Theory]
    [InlineData(LineEnding.None, "")]
    [InlineData(LineEnding.LineFeed, "\n")]
    [InlineData(LineEnding.CarriageReturn, "\r")]
    [InlineData(LineEnding.CarriageReturn | LineEnding.LineFeed, "\r\n")]
    public void ConfigureLineEnding(LineEnding lineEnding, string expected)
    {
        // lang=toml
        var config =
            $"""
             [[Serilog.WriteTo]]
             Name = 'File'
             Args.path = 'logs.xml'
             Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
             Args.formatter.lineEnding = '{lineEnding}'
             """;

        var options = GetLog4NetTextFormatterOptions(config);

        options.XmlWriterSettings.NewLineChars.Should().Be(expected);
    }

    [Theory]
    [InlineData(Indentation.Space, "  ")]
    [InlineData(Indentation.Tab, "\t\t")]
    public void ConfigureIndentationStyle(Indentation indentation, string expected)
    {
        // lang=toml
        var config =
            $"""
             [[Serilog.WriteTo]]
             Name = 'File'
             Args.path = 'logs.xml'
             Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
             Args.formatter.indentation = '{indentation}'
             """;

        var options = GetLog4NetTextFormatterOptions(config);

        options.XmlWriterSettings.Indent.Should().BeTrue();
        options.XmlWriterSettings.IndentChars.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, " ")]
    [InlineData(4, "    ")]
    public void ConfigureIndentationSize(int size, string expected)
    {
        // lang=toml
        var config =
            $"""
             [[Serilog.WriteTo]]
             Name = 'File'
             Args.path = 'logs.xml'
             Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
             Args.formatter.indentationSize = '{size}'
             """;

        var options = GetLog4NetTextFormatterOptions(config);

        options.XmlWriterSettings.Indent.Should().BeTrue();
        options.XmlWriterSettings.IndentChars.Should().Be(expected);
    }

    [Theory]
    [InlineData(Indentation.Space, 1, " ")]
    [InlineData(Indentation.Space, 4, "    ")]
    [InlineData(Indentation.Tab, 1, "\t")]
    [InlineData(Indentation.Tab, 4, "\t\t\t\t")]
    public void ConfigureIndentation(Indentation indentation, int size, string expected)
    {
        // lang=toml
        var config =
            $"""
             [[Serilog.WriteTo]]
             Name = 'File'
             Args.path = 'logs.xml'
             Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
             Args.formatter.indentation = '{indentation}'
             Args.formatter.indentationSize = '{size}'
             """;

        var options = GetLog4NetTextFormatterOptions(config);

        options.XmlWriterSettings.Indent.Should().BeTrue();
        options.XmlWriterSettings.IndentChars.Should().Be(expected);
    }

    [Fact]
    public void ConfigureNoIndentation()
    {
        // lang=toml
        const string config =
            """
            [[Serilog.WriteTo]]
            Name = 'File'
            Args.path = 'logs.xml'
            Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
            Args.formatter.noIndentation = true
            """;

        var options = GetLog4NetTextFormatterOptions(config);

        options.XmlWriterSettings.Indent.Should().BeFalse();
    }

    [Fact]
    public void ConfigureLog4JCompatibility()
    {
        // lang=toml
        const string config =
            """
            [[Serilog.WriteTo]]
            Name = 'File'
            Args.path = 'logs.xml'
            Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
            Args.formatter.log4JCompatibility = true
            """;

        var options = GetLog4NetTextFormatterOptions(config);

        options.XmlWriterSettings.NewLineChars.Should().Be("\r\n");
        options.XmlNamespace.Should().Be(new XmlQualifiedName("log4j", "http://jakarta.apache.org/log4j/"));
        options.CDataMode.Should().Be(CDataMode.Always);
    }

    private static Log4NetTextFormatterOptions GetLog4NetTextFormatterOptions(string config)
    {
        using var configStream = new MemoryStream(Encoding.UTF8.GetBytes(config));
        var configBuilder = new ConfigurationBuilder();
        var configuration = (config.StartsWith('{') ? configBuilder.AddJsonStream(configStream) : configBuilder.AddTomlStream(configStream)).Build();
        var options = new ConfigurationReaderOptions(typeof(ILogger).Assembly, typeof(FileSink).Assembly, typeof(Log4NetTextFormatter).Assembly);
        using var logger = new LoggerConfiguration().ReadFrom.Configuration(configuration, options).CreateLogger();
        IEnumerable<ILogEventSink> sinks = logger.AsDynamic()._sink._sinks;
        var fileSink = sinks.Should().ContainSingle().Which.Should().BeOfType<FileSink>().Subject;
        return fileSink.AsDynamic()._textFormatter._options;
    }
}