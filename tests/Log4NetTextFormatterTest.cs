using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using FluentAssertions;
using Serilog.Core;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Parsing;
using Xunit;

namespace Serilog.Formatting.Log4Net.Tests
{
    public static class ExceptionExtensions
    {
        // See https://stackoverflow.com/questions/37093261/attach-stacktrace-to-exception-without-throwing-in-c-sharp-net/37605142#37605142
        private static readonly FieldInfo StackTraceStringField = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new MissingFieldException(nameof(Exception), "_stackTraceString");

        public static Exception SetStackTrace(this Exception exception, string stackTrace)
        {
            StackTraceStringField.SetValue(exception, stackTrace) ;
            return exception;
        }
    }

    [UseReporter(typeof(DiffReporter))]
    public class Log4NetTextFormatterTest : IDisposable
    {
        private readonly TextWriter _selfLogWriter;
        private string? SelfLogValue => _selfLogWriter.ToString();

        /// <summary>
        /// Create a <see cref="DictionaryValue"/> containing two entries, mapping scalar values 1 to "one" and "two" to 2.
        /// </summary>
        private static DictionaryValue CreateDictionary()
        {
            return new DictionaryValue(new[]
            {
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue(1), new ScalarValue("one")),
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("two"), new ScalarValue(2)),
            });
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
            Action action = () => formatter.Format(null!, TextWriter.Null);

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
            Action action = () => formatter.Format(logEvent, null!);

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
        public void LogEventLevel(LogEventLevel level)
        {
            NamerFactory.AdditionalInformation = level.ToString();

            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(level);
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void InvalidLogEventLevelThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var logEvent = CreateLogEvent((LogEventLevel)(-1));
            var formatter = new Log4NetTextFormatter();

            // Act
            Action action = () => formatter.Format(logEvent, TextWriter.Null);

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
        public void MessageCDataMode(CDataMode mode, bool needsEscaping)
        {
            NamerFactory.AdditionalInformation = mode + "." + (needsEscaping ? "NeedsEscaping" : "DoesntNeedEscaping");

            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(messageTemplate: needsEscaping ? ">> Hello from Serilog <<" : "Hello from Serilog");
            var formatter = new Log4NetTextFormatter(options => options.UseCDataMode(mode));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Theory]
        [InlineData(LineEnding.None)]
        [InlineData(LineEnding.LineFeed)]
        [InlineData(LineEnding.CarriageReturn)]
        [InlineData(LineEnding.CarriageReturn | LineEnding.LineFeed)]
        public void XmlElementsLineEnding(LineEnding lineEnding)
        {
            NamerFactory.AdditionalInformation = lineEnding.ToString();

            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent();
            var formatter = new Log4NetTextFormatter(options => options.UseLineEnding(lineEnding));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Theory]
        [InlineData(Indentation.Space, 2)]
        [InlineData(Indentation.Space, 4)]
        [InlineData(Indentation.Tab, 2)]
        [InlineData(Indentation.Tab, 4)]
        public void IndentationSettings(Indentation indentation, byte size)
        {
            NamerFactory.AdditionalInformation = indentation + "." + size;

            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent();
            var formatter = new Log4NetTextFormatter(options => options.UseIndentationSettings(new IndentationSettings(indentation, size)));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void NoIndentation()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent();
            var formatter = new Log4NetTextFormatter(options => options.UseNoIndentation());

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void NoNamespace()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent();
            var formatter = new Log4NetTextFormatter(options => options.UseNoXmlNamespace());

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void NullProperty()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(properties: new LogEventProperty("n/a", new ScalarValue(null)));
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void LoggerName()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(properties: new LogEventProperty(Constants.SourceContextPropertyName, new ScalarValue("Namespace.Component.Class")));
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void LoggerNameStructureValue()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(properties: new LogEventProperty(Constants.SourceContextPropertyName, new StructureValue(new List<LogEventProperty>())));
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void DefaultFormatProvider()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(messageTemplate: "π = {π}", properties: new LogEventProperty("π", new ScalarValue(3.14m)));
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void Log4JCompatibility()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(
                exception: new Exception("An error occurred").SetStackTrace(@"  at Serilog.Formatting.Log4Net.Tests.Log4NetTextFormatterTest.BasicMessage_WithException() in Log4NetTextFormatterTest.cs:123"),
                properties: new LogEventProperty("π", new ScalarValue(3.14m))
            );
            var formatter = new Log4NetTextFormatter(c => c.UseLog4JCompatibility());

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void ExplicitFormatProvider()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(messageTemplate: "π = {π}", properties: new LogEventProperty("π", new ScalarValue(3.14m)));
            var formatProvider = new NumberFormatInfo { NumberDecimalSeparator = "," };
            var formatter = new Log4NetTextFormatter(options => options.UseFormatProvider(formatProvider));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void TwoProperties()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(properties: new[]{
                new LogEventProperty("one", new ScalarValue(1)),
                new LogEventProperty("two", new ScalarValue(2)),
            });
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void TwoPropertiesOneNull()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(properties: new[]{
                new LogEventProperty("n/a", new ScalarValue(null)),
                new LogEventProperty("one", new ScalarValue(1)),
            });
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void FilterProperty()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(properties: new[]{
                new LogEventProperty("one", new ScalarValue(1)),
                new LogEventProperty("two", new ScalarValue(2)),
            });
            var formatter = new Log4NetTextFormatter(options => options.UsePropertyFilter((_, propertyName) => propertyName != "one"));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void FilterPropertyThrowing()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(properties: new[]{
                new LogEventProperty("one", new ScalarValue(1)),
                new LogEventProperty("two", new ScalarValue(2)),
            });
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
            Approvals.VerifyWithExtension(output.ToString(), "xml");
            SelfLogValue.Should().Contain("[Serilog.Formatting.Log4Net.Log4NetTextFormatter] An exception was thrown while filtering property 'two'.");
        }

        [Fact]
        public void TwoEvents()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent();
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void Exception()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(exception: new Exception("An error occurred").SetStackTrace(@"  at Serilog.Formatting.Log4Net.Tests.Log4NetTextFormatterTest.BasicMessage_WithException() in Log4NetTextFormatterTest.cs:123"));
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void ExceptionFormatter()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(exception: new Exception("An error occurred"));
            var formatter = new Log4NetTextFormatter(options => options.UseExceptionFormatter(e => $"Type = {e.GetType().FullName}\nMessage = {e.Message}"));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void ExceptionFormatterReturningNull()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(exception: new Exception("An error occurred"));
            var formatter = new Log4NetTextFormatter(options => options.UseExceptionFormatter(e => null!));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void ExceptionFormatterThrowing()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(exception: new Exception("An error occurred"));
            var formatter = new Log4NetTextFormatter(options => options.UseExceptionFormatter(e => throw new InvalidOperationException("💥 Boom 💥")));

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
            SelfLogValue.Should().Contain("[Serilog.Formatting.Log4Net.Log4NetTextFormatter] An exception was thrown while formatting an exception.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData(1)]
        public void ThreadIdProperty(int? threadId)
        {
            NamerFactory.AdditionalInformation = threadId?.ToString() ?? "_null";

            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(properties: new LogEventProperty(ThreadIdEnricher.ThreadIdPropertyName, new ScalarValue(threadId)));
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Theory]
        [InlineData(null)]
        [InlineData(@"")]
        [InlineData(@"TheUser")]
        [InlineData(@"TheDomain\TheUser")]
        [InlineData(@"TheDomain\TheUser\Name")]
        public void DomainAndUserNameProperty(string? environmentUserName)
        {
            NamerFactory.AdditionalInformation = environmentUserName == null ? "_null" : environmentUserName.Length == 0 ? "_empty" : environmentUserName.Replace(@"\", "_");

            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(properties: new LogEventProperty(EnvironmentUserNameEnricher.EnvironmentUserNamePropertyName, new ScalarValue(environmentUserName)));
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void DomainAndUserNamePropertyStructureValue()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(properties: new LogEventProperty(EnvironmentUserNameEnricher.EnvironmentUserNamePropertyName, new StructureValue(new List<LogEventProperty>())));
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("TheMachineName")]
        public void MachineNameProperty(string? machineName)
        {
            NamerFactory.AdditionalInformation = machineName ?? "_null";

            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(properties: new LogEventProperty(MachineNameEnricher.MachineNamePropertyName, new ScalarValue(machineName)));
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void MachineNamePropertyStructureValue()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(properties: new LogEventProperty(MachineNameEnricher.MachineNamePropertyName, new StructureValue(new List<LogEventProperty>())));
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void SequenceProperty()
        {
            // Arrange
            using var output = new StringWriter();
            var values = new LogEventPropertyValue[] { new ScalarValue(1), new ScalarValue("two"), CreateDictionary() };
            var logEvent = CreateLogEvent(properties: new LogEventProperty("Sequence", new SequenceValue(values)));
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void DictionaryProperty()
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
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void StructureProperty()
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
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }

        [Fact]
        public void CustomLogEventPropertyValue()
        {
            // Arrange
            using var output = new StringWriter();
            var logEvent = CreateLogEvent(properties: new LogEventProperty("Custom", new CustomLogEventPropertyValue("CustomValue")));
            var formatter = new Log4NetTextFormatter();

            // Act
            formatter.Format(logEvent, output);

            // Assert
            Approvals.VerifyWithExtension(output.ToString(), "xml");
        }
    }
}
