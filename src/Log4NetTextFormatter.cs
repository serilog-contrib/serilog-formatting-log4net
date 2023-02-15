using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Serilog.Formatting.Log4Net;

/// <summary>
/// A text formatter that serialize log events into <a href="https://logging.apache.org/log4net/">log4net</a> or <a href="https://logging.apache.org/log4j/">log4j</a> compatible XML format.
/// </summary>
public class Log4NetTextFormatter : ITextFormatter
{
    /// <summary>
    /// The characters that must be escaped inside an XML element. Used when <see cref="CDataMode"/> is <see cref="CDataMode.IfNeeded"/>.
    /// </summary>
    private static readonly char[] XmlElementCharactersToEscape = { '&', '<', '>' };

    /// <summary>
    /// The Serilog properties which have a special mapping in a log4net event and must not be part of the log4net:properties element.
    /// </summary>
    private static readonly string[] SpecialProperties = {
        Constants.SourceContextPropertyName, OutputProperties.MessagePropertyName,
        ThreadIdPropertyName, UserNamePropertyName, MachineNamePropertyName
    };

    /// <summary>
    /// The name of the thread id property, set by <a href="https://www.nuget.org/packages/Serilog.Enrichers.Thread/">Serilog.Enrichers.Thread</a>
    /// </summary>
    /// <remarks>https://github.com/serilog/serilog-enrichers-thread/blob/v3.1.0/src/Serilog.Enrichers.Thread/Enrichers/ThreadIdEnricher.cs#L30</remarks>
    private const string ThreadIdPropertyName = "ThreadId";

    /// <summary>
    /// The name of the user name property, set by <a href="https://www.nuget.org/packages/Serilog.Enrichers.Environment/">Serilog.Enrichers.Environment</a>
    /// </summary>
    /// <remarks>https://github.com/serilog/serilog-enrichers-environment/blob/v2.1.3/src/Serilog.Enrichers.Environment/Enrichers/EnvironmentUserNameEnricher.cs#L31</remarks>
    private const string UserNamePropertyName = "EnvironmentUserName";

    /// <summary>
    /// The name of the machine name property, set by <a href="https://www.nuget.org/packages/Serilog.Enrichers.Environment/">Serilog.Enrichers.Environment</a>
    /// </summary>
    /// <remarks>https://github.com/serilog/serilog-enrichers-environment/blob/v2.1.3/src/Serilog.Enrichers.Environment/Enrichers/MachineNameEnricher.cs#L36</remarks>
    private const string MachineNamePropertyName = "MachineName";

    private readonly Log4NetTextFormatterOptions _options;
    private readonly bool _usesLog4JCompatibility;

    /// <summary>
    /// A <see cref="Log4NetTextFormatter"/> instance configured for the log4j XML layout.
    /// Accessible with <c>Serilog.Formatting.Log4Net.Log4NetTextFormatter::Log4JFormatter, Serilog.Formatting.Log4Net</c> when using the
    /// <a href="https://github.com/serilog/serilog-settings-configuration/">Serilog.Settings.Configuration</a> package.
    /// </summary>
    public static Log4NetTextFormatter Log4JFormatter { get; } = new(c => c.UseLog4JCompatibility());

    /// <summary>
    /// Initialize a new instance of the <see cref="Log4NetTextFormatter"/> class.
    /// </summary>
    public Log4NetTextFormatter() : this(configureOptions: null)
    {
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="Log4NetTextFormatter"/> class.
    /// </summary>
    /// <param name="configureOptions">An optional callback to configure the options used to write the XML fragments.</param>
    public Log4NetTextFormatter(Action<Log4NetTextFormatterOptionsBuilder>? configureOptions)
    {
        var optionsBuilder = new Log4NetTextFormatterOptionsBuilder();
        configureOptions?.Invoke(optionsBuilder);
        _options = optionsBuilder.Build();
        _usesLog4JCompatibility = ReferenceEquals(Log4NetTextFormatterOptionsBuilder.Log4JXmlNamespace, _options.XmlNamespace);
    }

    /// <summary>
    /// Format the log event as log4net or log4j compatible XML format into the output.
    /// </summary>
    /// <param name="logEvent">The event to format.</param>
    /// <param name="output">The output.</param>
    /// <exception cref="ArgumentNullException">If either <paramref name="logEvent"/> or <paramref name="output"/> is <c>null</c>.</exception>
    public void Format(LogEvent logEvent, TextWriter output)
    {
        if (logEvent == null)
        {
            throw new ArgumentNullException(nameof(logEvent));
        }
        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }
        var xmlWriterOutput = _usesLog4JCompatibility ? new StringWriter() : output;
        using var writer = XmlWriter.Create(xmlWriterOutput, _options.XmlWriterSettings);
        WriteEvent(logEvent, writer);
        writer.Flush();
        if (_usesLog4JCompatibility)
        {
            // log4j writes the XML "manually", see https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L137-L145
            // The resulting XML is impossible to write with a standard compliant XML writer such as XmlWriter.
            // That's why we write the event in a StringWriter then massage the output to remove the xmlns:log4j attribute to match log4j output.
            // The XML fragment becomes valid when surrounded by an external entity, see https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L31-L49
            const string log4JNamespaceAttribute = @" xmlns:log4j=""http://jakarta.apache.org/log4j/""";
            var xmlString = ((StringWriter)xmlWriterOutput).ToString();
            var i = xmlString.IndexOf(log4JNamespaceAttribute, StringComparison.Ordinal);
#if NETSTANDARD2_0
            output.Write(xmlString.Substring(0, i));
            output.Write(xmlString.Substring(i + log4JNamespaceAttribute.Length));
#else
            output.Write(xmlString.AsSpan(0, i));
            output.Write(xmlString.AsSpan(i + log4JNamespaceAttribute.Length));
#endif
        }
        output.Write(_options.XmlWriterSettings.NewLineChars);
    }

    /// <summary>
    /// Write the log event into the XML writer.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="writer">The XML writer.</param>
    /// <remarks>https://github.com/apache/logging-log4net/blob/rel/2.0.8/src/Layout/XmlLayout.cs#L218-L310</remarks>
    private void WriteEvent(LogEvent logEvent, XmlWriter writer)
    {
        WriteStartElement(writer, "event");
        WriteEventAttribute(logEvent, writer, "logger", Constants.SourceContextPropertyName);
        var timestamp = _usesLog4JCompatibility ? XmlConvert.ToString(logEvent.Timestamp.ToUnixTimeMilliseconds()) : XmlConvert.ToString(logEvent.Timestamp);
        writer.WriteAttributeString("timestamp", timestamp);
        writer.WriteAttributeString("level", LogLevel(logEvent.Level));
        WriteEventAttribute(logEvent, writer, "thread", ThreadIdPropertyName);
        WriteDomainAndUserName(logEvent, writer);
        var properties = logEvent.Properties.Where(e => !SpecialProperties.Contains(e.Key, StringComparer.Ordinal)).ToList();
        var hasMachineNameProperty = logEvent.Properties.TryGetValue(MachineNamePropertyName, out var machineNameProperty) && machineNameProperty is ScalarValue { Value: not null };
        if (properties.Any() || hasMachineNameProperty)
        {
            WriteProperties(logEvent, writer, properties, machineNameProperty);
        }
        WriteMessage(logEvent, writer);
        WriteException(logEvent, writer);
        writer.WriteEndElement();
    }

    /// <summary>
    /// Write the <c>domain</c> and <c>username</c> XML attributes if found in the <c>EnvironmentUserName</c> property.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="writer">The XML writer.</param>
    private static void WriteDomainAndUserName(LogEvent logEvent, XmlWriter writer)
    {
        if (logEvent.Properties.TryGetValue(UserNamePropertyName, out var propertyValue) && propertyValue is ScalarValue { Value: string userNameProperty })
        {
            // See https://github.com/serilog/serilog-enrichers-environment/blob/3fc7cf78c5f34816633000ae74d846033498e44b/src/Serilog.Enrichers.Environment/Enrichers/EnvironmentUserNameEnricher.cs#L53
            var separatorIndex = userNameProperty.IndexOf(@"\", StringComparison.OrdinalIgnoreCase);
            if (separatorIndex != -1)
            {
                writer.WriteAttributeString("domain", userNameProperty.Substring(0, separatorIndex));
                writer.WriteAttributeString("username", userNameProperty.Substring(separatorIndex + 1));
            }
            else
            {
                writer.WriteAttributeString("username", userNameProperty);
            }
        }
    }

    /// <summary>
    /// Write an attribute in the current XML element, started with <see cref="WriteStartElement"/>.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="writer">The XML writer.</param>
    /// <param name="attributeName">The name of the XML attribute.</param>
    /// <param name="propertyName">The name of the Serilog property.</param>
    /// <remarks>Only properties with a non null <see cref="ScalarValue"/> are supported, other types are ignored.</remarks>
    private void WriteEventAttribute(LogEvent logEvent, XmlWriter writer, string attributeName, string propertyName)
    {
        if (logEvent.Properties.TryGetValue(propertyName, out var propertyValue) && propertyValue is ScalarValue { Value: not null })
        {
            writer.WriteAttributeString(attributeName, RenderValue(propertyValue));
        }
    }

    /// <summary>
    /// Convert Serilog <see cref="LogEventLevel"/> into log4net/log4j equivalent level.
    /// </summary>
    /// <param name="level">The serilog level.</param>
    /// <returns>The equivalent log4net/log4j level.</returns>
    /// <remarks>https://github.com/apache/logging-log4net/blob/rel/2.0.8/src/Core/Level.cs#L509-L603</remarks>
    /// <remarks>https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/Level.java#L48-L92</remarks>
    private static string LogLevel(LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Verbose => "TRACE",
            LogEventLevel.Debug => "DEBUG",
            LogEventLevel.Information => "INFO",
            LogEventLevel.Warning => "WARN",
            LogEventLevel.Error => "ERROR",
            LogEventLevel.Fatal => "FATAL",
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, $"The value of argument '{nameof(level)}' ({level}) is invalid for enum type '{nameof(LogEventLevel)}'.")
        };
    }

    /// <summary>
    /// Write the Serilog <paramref name="properties"/> into the <c>properties</c> XML element.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="writer">The XML writer.</param>
    /// <param name="properties">The collection of properties to write.</param>
    /// <param name="machineNameProperty">The machine name property to write or <see langword="null"/> if doesn't exist.</param>
    /// <remarks>https://github.com/apache/logging-log4net/blob/rel/2.0.8/src/Layout/XmlLayout.cs#L262-L286</remarks>
    private void WriteProperties(LogEvent logEvent, XmlWriter writer, IEnumerable<KeyValuePair<string, LogEventPropertyValue>> properties, LogEventPropertyValue? machineNameProperty)
    {
        WriteStartElement(writer, "properties");
        {
            if (machineNameProperty != null)
            {
                // https://github.com/apache/logging-log4net/blob/rel/2.0.8/src/Core/LoggingEvent.cs#L1609
                WriteProperty(writer, "log4net:HostName", machineNameProperty);
            }
            foreach (var (propertyName, propertyValue) in properties.Select(e => (e.Key, e.Value)))
            {
                if (IncludeProperty(logEvent, propertyName))
                {
                    WriteProperty(writer, propertyName, propertyValue);
                }
            }
        }
        writer.WriteEndElement();
    }

    /// <summary>
    /// Determine whether the property should be included with the user-provided <see cref="PropertyFilter"/>.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns><see langword="true"/> if the property must be included in the log4net properties, <see langword="false"/> otherwise.</returns>
    /// <remarks>If the property filter throws an exception, a self-log is emitted and the property is included.</remarks>
    [SuppressMessage("Microsoft.Design", "CA1031", Justification = "Protecting from user-provided code which might throw anything")]
    private bool IncludeProperty(LogEvent logEvent, string propertyName)
    {
        try
        {
            return _options.FilterProperty(logEvent, propertyName);
        }
        catch (Exception filterException)
        {
            Debugging.SelfLog.WriteLine($"[{GetType().FullName}] An exception was thrown while filtering property '{propertyName}'. Including the property in the log4net event.\n{filterException}");
            return true;
        }
    }

    /// <summary>
    /// Render a <see cref="LogEventPropertyValue"/> as <see cref="string"/>, without quotes if the value is a <see cref="ScalarValue"/> containing a <see cref="string"/>.
    /// This is appropriate to use for an XML attribute.
    /// </summary>
    /// <param name="value">The value to render.</param>
    /// <returns>A string representation of the <paramref name="value"/>.</returns>
    private string RenderValue(LogEventPropertyValue value)
    {
        using var valueWriter = new StringWriter();
        // The "l" format specifier switches off quoting of strings, see https://github.com/serilog/serilog/wiki/Formatting-Output#formatting-plain-text
        value.Render(valueWriter, value is ScalarValue { Value: string } ? "l" : null, _options.FormatProvider);
        return valueWriter.ToString();
    }

    /// <summary>
    /// Write a single Serilog property into log4net property format, handling all <see cref="LogEventPropertyValue"/> types, i.e.
    /// <see cref="ScalarValue"/>, <see cref="DictionaryValue"/>, <see cref="SequenceValue"/> and <see cref="StructureValue"/>.
    /// </summary>
    /// <param name="writer">The XML writer.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="propertyValue">The property value.</param>
    private void WriteProperty(XmlWriter writer, string propertyName, LogEventPropertyValue propertyValue)
    {
        switch (propertyValue)
        {
            case ScalarValue scalarValue:
                WriteScalarProperty(writer, propertyName, scalarValue);
                break;
            case DictionaryValue dictionaryValue:
                WriteDictionaryProperty(writer, propertyName, dictionaryValue);
                break;
            case SequenceValue sequenceValue:
                WriteSequenceProperty(writer, propertyName, sequenceValue);
                break;
            case StructureValue structureValue:
                WriteStructureProperty(writer, propertyName, structureValue);
                break;
            default:
                WritePropertyElement(writer, propertyName, propertyValue);
                break;
        }
    }

    /// <summary>
    /// Write a <see cref="ScalarValue"/> property.
    /// </summary>
    /// <param name="writer">The XML writer.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="scalarValue">The <see cref="ScalarValue"/> to write.</param>
    private void WriteScalarProperty(XmlWriter writer, string propertyName, ScalarValue scalarValue)
    {
        WritePropertyElement(writer, propertyName, scalarValue);
    }

    /// <summary>
    /// Write a <see cref="DictionaryValue"/> property.
    /// </summary>
    /// <param name="writer">The XML writer.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="dictionaryValue">The <see cref="DictionaryValue"/> to write.</param>
    private void WriteDictionaryProperty(XmlWriter writer, string propertyName, DictionaryValue dictionaryValue)
    {
        foreach (var element in dictionaryValue.Elements)
        {
            WritePropertyElement(writer, $"{propertyName}.{RenderValue(element.Key)}", element.Value);
        }
    }

    /// <summary>
    /// Write a <see cref="SequenceValue"/> property.
    /// </summary>
    /// <param name="writer">The XML writer.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="sequenceValue">The <see cref="SequenceValue"/> to write.</param>
    private void WriteSequenceProperty(XmlWriter writer, string propertyName, SequenceValue sequenceValue)
    {
        foreach (var (value, i) in sequenceValue.Elements.Select((e, i) => (e, i)))
        {
            WritePropertyElement(writer, $"{propertyName}[{i}]", value);
        }
    }

    /// <summary>
    /// Write a <see cref="StructureValue"/> property.
    /// </summary>
    /// <param name="writer">The XML writer.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="structureValue">The <see cref="StructureValue"/> to write.</param>
    private void WriteStructureProperty(XmlWriter writer, string propertyName, StructureValue structureValue)
    {
        foreach (var property in structureValue.Properties)
        {
            WritePropertyElement(writer, $"{propertyName}.{property.Name}", property.Value);
        }
    }

    /// <summary>
    /// Write the log4net <c>data</c> element with <c>name</c> and <c>value</c> attributes.
    /// </summary>
    /// <param name="writer">The XML writer.</param>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    private void WritePropertyElement(XmlWriter writer, string name, LogEventPropertyValue value)
    {
        WriteStartElement(writer, "data");
        writer.WriteAttributeString("name", name);
        var isNullValue = value is ScalarValue { Value: null };
        if (!isNullValue)
        {
            writer.WriteAttributeString("value", RenderValue(value));
        }
        writer.WriteEndElement();
    }

    /// <summary>
    /// Write the message associated to the log event.
    /// Uses the <see cref="LogEvent.RenderMessage(System.IFormatProvider)"/> method to render the message.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="writer">The XML writer.</param>
    /// <remarks>https://github.com/apache/logging-log4net/blob/rel/2.0.8/src/Layout/XmlLayout.cs#L245-L257</remarks>
    private void WriteMessage(LogEvent logEvent, XmlWriter writer)
    {
        var message = logEvent.RenderMessage(_options.FormatProvider);
        WriteContent(writer, "message", message);
    }

    /// <summary>
    /// Write the exception associated to the log event.
    /// If no exception is associated with the log event, then no XML element is written at all.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="writer">The XML writer.</param>
    /// <remarks>https://github.com/apache/logging-log4net/blob/rel/2.0.8/src/Layout/XmlLayout.cs#L288-L295</remarks>
    private void WriteException(LogEvent logEvent, XmlWriter writer)
    {
        var exception = logEvent.Exception;
        if (exception == null)
        {
            return;
        }
        var formattedException = FormatException(exception);
        if (formattedException != null)
        {
            var elementName = _usesLog4JCompatibility ? "throwable" : "exception";
            WriteContent(writer, elementName, formattedException);
        }
    }

    /// <summary>
    /// Formats the given <paramref name="exception"/> with the user-provided <see cref="ExceptionFormatter"/>.
    /// </summary>
    /// <param name="exception">The exception to format.</param>
    /// <returns>The formatted exception.</returns>
    /// <remarks>
    /// If the exception formatter throws an exception, a self-log is emitted and
    /// the default formatting is applied, i.e. <c>exception.ToString()</c>.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1031", Justification = "Protecting from user-provided code which might throw anything")]
    private string? FormatException(Exception exception)
    {
        try
        {
            return (string?)_options.FormatException(exception);
        }
        catch (Exception formattingException)
        {
            Debugging.SelfLog.WriteLine($"[{GetType().FullName}] An exception was thrown while formatting an exception. Using the default exception formatter.\n{formattingException}");
            return exception.ToString();
        }
    }

    /// <summary>
    /// Start writing an XML element, taking into account the configured <see cref="Log4NetTextFormatterOptions.XmlNamespace"/>.
    /// </summary>
    /// <param name="writer">The XML writer.</param>
    /// <param name="elementName">The name of the XML element to write.</param>
    private void WriteStartElement(XmlWriter writer, string elementName)
    {
        var qualifiedName = _options.XmlNamespace;
        if (qualifiedName != null)
        {
            writer.WriteStartElement(qualifiedName.Name, elementName, qualifiedName.Namespace);
        }
        else
        {
            writer.WriteStartElement(elementName);
        }
    }

    /// <summary>
    /// Write an XML element, taking into account the <see cref="Log4NetTextFormatterOptions.CDataMode"/>.
    /// </summary>
    /// <param name="writer">The XML writer.</param>
    /// <param name="elementName">The name of the XML element to write.</param>
    /// <param name="content">The content of the XML element to write.</param>
    private void WriteContent(XmlWriter writer, string elementName, string content)
    {
        WriteStartElement(writer, elementName);
        var writeCDataSection = _options.CDataMode == CDataMode.Always || (_options.CDataMode == CDataMode.IfNeeded && content.IndexOfAny(XmlElementCharactersToEscape) != -1);
        if (writeCDataSection)
        {
            writer.WriteCData(content);
        }
        else
        {
            writer.WriteString(content);
        }
        writer.WriteEndElement();
    }
}