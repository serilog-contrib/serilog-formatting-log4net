using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using FluentAssertions;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using VerifyTests;
using VerifyXunit;
using Xunit;

namespace Serilog.Formatting.Log4Net.Tests;

public sealed class Log4NetTextFormatterTest : IDisposable
{
    private readonly TextWriter _selfLogWriter;
    private string? SelfLogValue => _selfLogWriter.ToString();

    /// <summary>
    /// Create a <see cref="DictionaryValue"/> containing two entries, mapping scalar values 1 to "one" and "two" to 2.
    /// </summary>
    private static DictionaryValue CreateDictionary()
    {
        return new DictionaryValue([
            new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue(1), new ScalarValue("one")),
            new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("two"), new ScalarValue(2)),
        ]);
    }

    private static LogEvent CreateLogEvent(LogEventLevel level = Events.LogEventLevel.Information, string messageTemplate = "Hello from Serilog", Exception? exception = null, params LogEventProperty[] properties)
    {
        return new LogEvent(
            new DateTimeOffset(2003, 1, 4, 15, 9, 26, 535, TimeSpan.FromHours(1)),
            level,
            exception,
            new MessageTemplateParser().Parse(messageTemplate),
            properties
        );
    }

    public Log4NetTextFormatterTest()
    {
        _selfLogWriter = new StringWriter();
        Debugging.SelfLog.Enable(_selfLogWriter);
    }

    public void Dispose()
    {
        Debugging.SelfLog.Disable();
        _selfLogWriter.Dispose();
    }

    [Fact]
    public void NullLogEventThrowsArgumentNullException()
    {
        // Arrange
        var formatter = new Log4NetTextFormatter();

        // Act
        var action = () => formatter.Format(null!, TextWriter.Null);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("logEvent")
            .Which.StackTrace!.TrimStart().Should().StartWith("at Serilog.Formatting.Log4Net.Log4NetTextFormatter.Format");
    }

    [Fact]
    public void NullOutputThrowsArgumentNullException()
    {
        // Arrange
        var logEvent = CreateLogEvent();
        var formatter = new Log4NetTextFormatter();

        // Act
        var action = () => formatter.Format(logEvent, null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("output")
            .Which.StackTrace!.TrimStart().Should().StartWith("at Serilog.Formatting.Log4Net.Log4NetTextFormatter.Format");
    }

    [Fact]
    public void SettingPropertyFilterToNullThrowsArgumentNullException()
    {
        // Act
        Action action = () => _ = new Log4NetTextFormatter(c => c.UsePropertyFilter(null!));

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>()
            .WithMessage("The property filter can not be null.*")
            .And.ParamName.Should().Be("filterProperty");
    }

    [Fact]
    public void SettingExceptionFormatterToNullThrowsArgumentNullException()
    {
        // Act
        Action action = () => _ = new Log4NetTextFormatter(c => c.UseExceptionFormatter(null!));

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>()
            .WithMessage("The exception formatter can not be null.*")
            .And.ParamName.Should().Be("formatException");
    }

    [Theory]
    [InlineData(Events.LogEventLevel.Verbose)]
    [InlineData(Events.LogEventLevel.Debug)]
    [InlineData(Events.LogEventLevel.Information)]
    [InlineData(Events.LogEventLevel.Warning)]
    [InlineData(Events.LogEventLevel.Error)]
    [InlineData(Events.LogEventLevel.Fatal)]
    public Task LogEventLevel(LogEventLevel level)
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(level);
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output).UseParameters(level);
    }

    [Fact]
    public void InvalidLogEventLevelThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var logEvent = CreateLogEvent((LogEventLevel)(-1));
        var formatter = new Log4NetTextFormatter();

        // Act
        var action = () => formatter.Format(logEvent, TextWriter.Null);

        // Assert
        action.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .And.Message.Should().StartWith("The value of argument 'level' (-1) is invalid for enum type 'LogEventLevel'.");
    }

    [Theory]
    [InlineData(CDataMode.Always, true)]
    [InlineData(CDataMode.Always, false)]
    [InlineData(CDataMode.Never, true)]
    [InlineData(CDataMode.Never, false)]
    [InlineData(CDataMode.IfNeeded, true)]
    [InlineData(CDataMode.IfNeeded, false)]
    public Task MessageCDataMode(CDataMode mode, bool needsEscaping)
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(messageTemplate: needsEscaping ? ">> Hello from Serilog <<" : "Hello from Serilog");
        var formatter = new Log4NetTextFormatter(options => options.UseCDataMode(mode));

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output).UseParameters(mode, needsEscaping);
    }

    [Theory]
    [InlineData(LineEnding.None)]
    [InlineData(LineEnding.LineFeed)]
    [InlineData(LineEnding.CarriageReturn)]
    [InlineData(LineEnding.CarriageReturn | LineEnding.LineFeed)]
    public Task XmlElementsLineEnding(LineEnding lineEnding)
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent();
        var formatter = new Log4NetTextFormatter(options => options.UseLineEnding(lineEnding));

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output).UseParameters(lineEnding);
    }

    [Theory]
    [InlineData(Indentation.Space, 2)]
    [InlineData(Indentation.Space, 4)]
    [InlineData(Indentation.Tab, 2)]
    [InlineData(Indentation.Tab, 4)]
    public Task IndentationSettings(Indentation indentation, byte size)
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent();
        var formatter = new Log4NetTextFormatter(options => options.UseIndentationSettings(new IndentationSettings(indentation, size)));

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output).UseParameters(indentation, size);
    }

    [Fact]
    public Task NoIndentation()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent();
        var formatter = new Log4NetTextFormatter(options => options.UseNoIndentation());

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task NoNamespace()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent();
        var formatter = new Log4NetTextFormatter(options => options.UseNoXmlNamespace());

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task NullProperty()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: new LogEventProperty("n/a", new ScalarValue(null)));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task LoggerName()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: new LogEventProperty(Constants.SourceContextPropertyName, new ScalarValue("Namespace.Component.Class")));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task LoggerNameStructureValue()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: new LogEventProperty(Constants.SourceContextPropertyName, new StructureValue(new List<LogEventProperty>())));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task DefaultFormatProvider()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(messageTemplate: "π = {π}", properties: new LogEventProperty("π", new ScalarValue(3.14m)));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Log4JCompatibility(bool useStaticInstance)
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(
            exception: ExceptionDispatchInfo.SetRemoteStackTrace(new Exception("An error occurred"), "  at Serilog.Formatting.Log4Net.Tests.Log4NetTextFormatterTest.BasicMessage_WithException() in Log4NetTextFormatterTest.cs:123"),
            properties: new LogEventProperty("π", new ScalarValue(3.14m))
        );
        var formatter = useStaticInstance ? Log4NetTextFormatter.Log4JFormatter : new Log4NetTextFormatter(c => c.UseLog4JCompatibility());

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output).IgnoreParameters().DisableRequireUniquePrefix();
    }

    [Fact]
    public Task ExplicitFormatProvider()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(messageTemplate: "π = {π}", properties: new LogEventProperty("π", new ScalarValue(3.14m)));
        var formatProvider = new NumberFormatInfo { NumberDecimalSeparator = "," };
        var formatter = new Log4NetTextFormatter(options => options.UseFormatProvider(formatProvider));

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task TwoProperties()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: [
            new LogEventProperty("one", new ScalarValue(1)),
            new LogEventProperty("two", new ScalarValue(2)),
        ]);
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task TwoPropertiesOneNull()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: [
            new LogEventProperty("n/a", new ScalarValue(null)),
            new LogEventProperty("one", new ScalarValue(1)),
        ]);
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task FilterProperty()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: [
            new LogEventProperty("one", new ScalarValue(1)),
            new LogEventProperty("two", new ScalarValue(2)),
        ]);
        var formatter = new Log4NetTextFormatter(options => options.UsePropertyFilter((_, propertyName) => propertyName != "one"));

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task FilterPropertyThrowing()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: [
            new LogEventProperty("one", new ScalarValue(1)),
            new LogEventProperty("two", new ScalarValue(2)),
        ]);
        var formatter = new Log4NetTextFormatter(options => options.UsePropertyFilter((_, propertyName) =>
        {
            if (propertyName == "one")
            {
                return true;
            }
            throw new InvalidOperationException($"Can't handle property '{propertyName}'.");
        }));

        // Act
        formatter.Format(logEvent, output);

        // Assert
        SelfLogValue.Should().Contain("[Serilog.Formatting.Log4Net.Log4NetTextFormatter] An exception was thrown while filtering property 'two'.");
        return Verify(output);
    }

    [Fact]
    public Task TwoEvents()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent();
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task Exception()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(exception: ExceptionDispatchInfo.SetRemoteStackTrace(new Exception("An error occurred"), "  at Serilog.Formatting.Log4Net.Tests.Log4NetTextFormatterTest.BasicMessage_WithException() in Log4NetTextFormatterTest.cs:123"));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task ExceptionFormatter()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(exception: new Exception("An error occurred"));
        var formatter = new Log4NetTextFormatter(options => options.UseExceptionFormatter(e => $"Type = {e.GetType().FullName}\nMessage = {e.Message}"));

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task ExceptionFormatterReturningNull()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(exception: new Exception("An error occurred"));
        var formatter = new Log4NetTextFormatter(options => options.UseExceptionFormatter(_ => null!));

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task ExceptionFormatterThrowing()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(exception: new Exception("An error occurred"));
        var formatter = new Log4NetTextFormatter(options => options.UseExceptionFormatter(_ => throw new InvalidOperationException("💥 Boom 💥")));

        // Act
        formatter.Format(logEvent, output);

        // Assert
        SelfLogValue.Should().Contain("[Serilog.Formatting.Log4Net.Log4NetTextFormatter] An exception was thrown while formatting an exception.");
        return Verify(output);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1)]
    public Task ThreadIdProperty(int? threadId)
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: new LogEventProperty("ThreadId", new ScalarValue(threadId)));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output).UseParameters(threadId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("TheUser")]
    [InlineData(@"TheDomain\TheUser")]
    [InlineData(@"TheDomain\TheUser\Name")]
    public Task DomainAndUserNameProperty(string? environmentUserName)
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: new LogEventProperty("EnvironmentUserName", new ScalarValue(environmentUserName)));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output).UseParameters(environmentUserName);
    }

    [Fact]
    public Task DomainAndUserNamePropertyStructureValue()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: new LogEventProperty("EnvironmentUserName", new StructureValue(new List<LogEventProperty>())));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("TheMachineName")]
    public Task MachineNameProperty(string? machineName)
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: new LogEventProperty("MachineName", new ScalarValue(machineName)));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output).UseParameters(machineName);
    }

    [Fact]
    public Task MachineNamePropertyStructureValue()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: new LogEventProperty("MachineName", new StructureValue(new List<LogEventProperty>())));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task Caller()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: new LogEventProperty("Caller", new ScalarValue("Fully.Qualified.ClassName.MethodName(System.String)")));

        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task CallerNonScalar()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: new LogEventProperty("Caller", new CustomLogEventPropertyValue("")));

        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task CallerWithFile()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: new LogEventProperty("Caller", new ScalarValue("Fully.Qualified.ClassName.MethodName(System.String) /Absolute/Path/To/FileName.cs:123")));

        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task CallerLog4J()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: new LogEventProperty("Caller", new ScalarValue("Fully.Qualified.ClassName.MethodName(System.String)")));

        var formatter = new Log4NetTextFormatter(c => c.UseLog4JCompatibility());

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task SequenceProperty()
    {
        // Arrange
        using var output = new StringWriter();
        var values = new LogEventPropertyValue[] { new ScalarValue(1), new ScalarValue("two"), CreateDictionary() };
        var logEvent = CreateLogEvent(properties: new LogEventProperty("Sequence", new SequenceValue(values)));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task DictionaryProperty()
    {
        // Arrange
        using var output = new StringWriter();
        var values = new[]
        {
            new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue(1), new ScalarValue("one")),
            new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("two"), new ScalarValue(2)),
            new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("dictionary"), CreateDictionary()),
        };
        var logEvent = CreateLogEvent(properties: new LogEventProperty("Dictionary", new DictionaryValue(values)));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task StructureProperty()
    {
        // Arrange
        using var output = new StringWriter();
        var values = new[]
        {
            new LogEventProperty("1", new ScalarValue("one")),
            new LogEventProperty("two", new ScalarValue(2)),
            new LogEventProperty("dictionary", CreateDictionary()),
        };
        var logEvent = CreateLogEvent(properties: new LogEventProperty("Structure", new StructureValue(values)));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    [Fact]
    public Task CustomLogEventPropertyValue()
    {
        // Arrange
        using var output = new StringWriter();
        var logEvent = CreateLogEvent(properties: new LogEventProperty("Custom", new CustomLogEventPropertyValue("CustomValue")));
        var formatter = new Log4NetTextFormatter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        return Verify(output);
    }

    private static SettingsTask Verify(StringWriter output)
    {
        var xml = output.ToString();
        return Verifier.Verify(xml, "xml");
    }
}