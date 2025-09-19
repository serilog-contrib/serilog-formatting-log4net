using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
            FormatProvider: null,
            CDataMode: CDataMode.Always,
            NullText: "(null)",
            XmlNamespace: new XmlQualifiedName("log4net", "http://logging.apache.org/log4net/schemas/log4net-events-1.2/"),
            XmlWriterSettings: new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\n", ConformanceLevel = ConformanceLevel.Fragment },
            FilterProperty: DefaultPropertyFilter,
            FormatMessage: DefaultMessageFormatter,
            FormatException: DefaultExceptionFormatter
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
            FormatProvider: null,
            CDataMode: cDataMode,
            NullText: "(null)",
            XmlNamespace: new XmlQualifiedName("log4net", "http://logging.apache.org/log4net/schemas/log4net-events-1.2/"),
            XmlWriterSettings: new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\n", ConformanceLevel = ConformanceLevel.Fragment },
            FilterProperty: DefaultPropertyFilter,
            FormatMessage: DefaultMessageFormatter,
            FormatException: DefaultExceptionFormatter
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
    [InlineData("(null)")]
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
    [InlineData(null, 0)]
    [InlineData(Indentation.Space, 1)]
    [InlineData(Indentation.Tab, 1)]
    [InlineData(Indentation.Space, 4)]
    [InlineData(Indentation.Tab, 4)]
    [InlineData(Indentation.Space, 255)]
    [InlineData(Indentation.Tab, 255)]
    public void ConfigureIndentation(Indentation? indentation, byte size)
    {
        var indentChars = indentation == null ? "" : new IndentationSettings(indentation.Value, size).ToString();
        // lang=toml
        var config =
            $"""
             [[Serilog.WriteTo]]
             Name = 'File'
             Args.path = 'logs.xml'
             Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
             Args.formatter.indentation = "{indentChars.Replace("\t", "\\t")}"
             """;

        var options = GetLog4NetTextFormatterOptions(config);

        options.XmlWriterSettings.Indent.Should().Be(indentChars.Length > 0);
        if (indentChars.Length > 0)
        {
            options.XmlWriterSettings.IndentChars.Should().Be(indentChars);
        }
    }

    [Theory]
    [InlineData(@"  \t\t")] // Mixed spaces and tabs
    [InlineData("<")] // Unsupported indentation
    [InlineData(" ")] // NO-BREAK SPACE (U+00A0)
    public void InvalidIndentation(string indentation)
    {
        // lang=toml
        var config =
            $"""
             [[Serilog.WriteTo]]
             Name = 'File'
             Args.path = 'logs.xml'
             Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
             Args.formatter.indentation = "{indentation}"
             """;

        var action = () => GetLog4NetTextFormatterOptions(config);

        action.Should().ThrowExactly<ArgumentException>().WithParameterName(nameof(indentation)).WithMessage("The indentation must contains only space or tab characters.*");
    }

    [Theory]
    [InlineData(Indentation.Space)]
    [InlineData(Indentation.Tab)]
    public void InvalidIndentation_TooLarge(Indentation indentation)
    {
        // lang=toml
        var config =
            $"""
             [[Serilog.WriteTo]]
             Name = 'File'
             Args.path = 'logs.xml'
             Args.formatter.type = 'Serilog.Formatting.Log4Net.Log4NetTextFormatter, Serilog.Formatting.Log4Net'
             Args.formatter.indentation = "{Enumerable.Repeat(indentation == Indentation.Space ? " " : @"\t", 256).Aggregate("", (current, s) => current + s)}"
             """;

        var action = () => GetLog4NetTextFormatterOptions(config);

        var ident = indentation == Indentation.Space ? "spaces" : "tabs";
        action.Should().ThrowExactly<ArgumentException>().WithParameterName(nameof(indentation)).WithMessage($"The indentation exceeds the maximum number of allowed {ident}. (256 > 255)*");
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
        try
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
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            throw exception.InnerException;
        }
    }
}