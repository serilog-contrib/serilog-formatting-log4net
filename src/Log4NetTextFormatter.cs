using System;
using System.IO;
using System.Linq;
using System.Xml;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Serilog.Formatting.Log4Net
{
    /// <summary>
    /// A text formatter that serialize log events into [log4net](https://logging.apache.org/log4net/) compatible XML format.
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
            ThreadIdPropertyName, UserNamePropertyName, MachineNamePropertyName, ProcessNamePropertyName
        };

        /// <summary>
        /// The name of the thread id property, set by [Serilog.Enrichers.Thread](https://www.nuget.org/packages/Serilog.Enrichers.Thread/)
        /// </summary>
        /// <remarks>https://github.com/serilog/serilog-enrichers-thread/blob/v3.1.0/src/Serilog.Enrichers.Thread/Enrichers/ThreadIdEnricher.cs#L30</remarks>
        private const string ThreadIdPropertyName = "ThreadId";

        /// <summary>
        /// The name of the user name property, set by [Serilog.Enrichers.Environment](https://www.nuget.org/packages/Serilog.Enrichers.Environment/)
        /// </summary>
        /// <remarks>https://github.com/serilog/serilog-enrichers-environment/blob/v2.1.3/src/Serilog.Enrichers.Environment/Enrichers/EnvironmentUserNameEnricher.cs#L31</remarks>
        private const string UserNamePropertyName = "EnvironmentUserName";

        /// <summary>
        /// The name of the machine name property, set by [Serilog.Enrichers.Environment](https://www.nuget.org/packages/Serilog.Enrichers.Environment/)
        /// </summary>
        /// <remarks>https://github.com/serilog/serilog-enrichers-environment/blob/v2.1.3/src/Serilog.Enrichers.Environment/Enrichers/MachineNameEnricher.cs#L36</remarks>
        private const string MachineNamePropertyName = "MachineName";

        /// <summary>
        /// The name of the process name property, set by [Serilog.Enrichers.Process](https://www.nuget.org/packages/Serilog.Enrichers.Process/)
        /// </summary>
        /// <remarks>https://github.com/serilog/serilog-enrichers-process/blob/v2.0.1/src/Serilog.Enrichers.Process/Enrichers/ProcessNameEnricher.cs#L30</remarks>
        private const string ProcessNamePropertyName = "ProcessName";

        private readonly Log4NetTextFormatterOptions _options;

        /// <summary>
        /// Initialize a new instance of the <see cref="Log4NetTextFormatter"/> class.
        /// </summary>
        public Log4NetTextFormatter() : this(configureOptions: null)
        {
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="Log4NetTextFormatter"/> class.
        /// </summary>
        /// <param name="configureOptions">An optional callback to configure the <see cref="Log4NetTextFormatterOptions"/> used to write the XML fragments.</param>
        public Log4NetTextFormatter(Action<Log4NetTextFormatterOptions>? configureOptions)
        {
            _options = new Log4NetTextFormatterOptions();
            configureOptions?.Invoke(_options);
            _options.XmlWriterSettings.ConformanceLevel = ConformanceLevel.Fragment;
            _options.FilterProperty ??= new Log4NetTextFormatterOptions().FilterProperty;
        }

        /// <summary>
        /// Format the log event as log4net compatible XML format into the output.
        /// </summary>
        /// <param name="logEvent">The event to format.</param>
        /// <param name="output">The output.</param>
        public void Format(LogEvent logEvent, TextWriter output)
        {
            using var writer = XmlWriter.Create(output, _options.XmlWriterSettings);
            WriteEvent(logEvent, writer);
            writer.Flush();
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
            writer.WriteAttributeString("timestamp", XmlConvert.ToString(logEvent.Timestamp));
            writer.WriteAttributeString("level", LogLevel(logEvent.Level));
            WriteEventAttribute(logEvent, writer, "thread", ThreadIdPropertyName);
            WriteEventAttribute(logEvent, writer, "domain", ProcessNamePropertyName);
            WriteEventAttribute(logEvent, writer, "username", UserNamePropertyName);
            WriteProperties(logEvent, writer);
            WriteMessage(logEvent, writer);
            WriteException(logEvent, writer);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Write an attribute in the current XML element, started with <see cref="WriteStartElement"/>.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="writer">The XML writer.</param>
        /// <param name="attributeName">The name of the XML attribute.</param>
        /// <param name="propertyName">The name of the Serilog property.</param>
        /// <remarks>Only properties with a <see cref="ScalarValue"/> are supported, other types are ignored.</remarks>
        private static void WriteEventAttribute(LogEvent logEvent, XmlWriter writer, string attributeName, string propertyName)
        {
            if (logEvent.Properties.TryGetValue(propertyName, out var propertyValue) && propertyValue is ScalarValue scalarValue)
            {
                writer.WriteAttributeString(attributeName, scalarValue.Value.ToString());
            }
        }

        /// <summary>
        /// Convert Serilog <see cref="LogEventLevel"/> into log4net equivalent level.
        /// </summary>
        /// <param name="level">The serilog level.</param>
        /// <returns>The equivalent log4net level.</returns>
        /// <remarks>https://github.com/apache/logging-log4net/blob/rel/2.0.8/src/Core/Level.cs#L509-L603</remarks>
        private static string LogLevel(LogEventLevel level) =>
            level switch
            {
                LogEventLevel.Verbose => "VERBOSE",
                LogEventLevel.Debug => "DEBUG",
                LogEventLevel.Information => "INFO",
                LogEventLevel.Warning => "WARN",
                LogEventLevel.Error => "ERROR",
                LogEventLevel.Fatal => "FATAL",
                _ => "OFF"
            };

        /// <summary>
        /// Write the Serilog <see cref="LogEvent.Properties"/> into the <c>properties</c> XML element.
        /// Only properties without a special log4net equivalent are written.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="writer">The XML writer.</param>
        /// <remarks>https://github.com/apache/logging-log4net/blob/rel/2.0.8/src/Layout/XmlLayout.cs#L262-L286</remarks>
        private void WriteProperties(LogEvent logEvent, XmlWriter writer)
        {
            var properties = logEvent.Properties.Where(e => !SpecialProperties.Contains(e.Key)).ToList();
            if (properties.Count == 0 && !logEvent.Properties.ContainsKey(MachineNamePropertyName))
            {
                return;
            }
            WriteStartElement(writer, "properties");
            {
                // https://github.com/apache/logging-log4net/blob/rel/2.0.8/src/Core/LoggingEvent.cs#L1609
                WriteProperty(logEvent, writer, "log4net:HostName", MachineNamePropertyName);
                foreach (var propertyName in properties.Select(e => e.Key).Where(e => _options.FilterProperty(logEvent, e)))
                {
                    WriteProperty(logEvent, writer, propertyName, propertyName);
                }
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Write a single Serilog property into log4net property format, handling all <see cref="LogEventPropertyValue"/> types, i.e.
        /// <see cref="ScalarValue"/>, <see cref="DictionaryValue"/>, <see cref="SequenceValue"/> and <see cref="StructureValue"/>.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="writer">The XML writer.</param>
        /// <param name="log4NetPropertyName">The log4net property name.</param>
        /// <param name="serilogPropertyName">The Serilog property name.</param>
        private void WriteProperty(LogEvent logEvent, XmlWriter writer, string log4NetPropertyName, string serilogPropertyName)
        {
            void WriteProperty(string name, LogEventPropertyValue value)
            {
                var valueWriter = new StringWriter();
                // The "l" format specifier switches off quoting of strings, see https://github.com/serilog/serilog/wiki/Formatting-Output#formatting-plain-text
                value.Render(valueWriter, value is ScalarValue scalarValue && scalarValue.Value is string ? "l" : null);
                WriteStartElement(writer, "data");
                writer.WriteAttributeString("name", name);
                writer.WriteAttributeString("value", valueWriter.ToString());
                writer.WriteEndElement();
            }

            if (logEvent.Properties.TryGetValue(serilogPropertyName, out var propertyValue))
            {
                if (propertyValue is ScalarValue scalarValue)
                {
                    WriteProperty(log4NetPropertyName, scalarValue);
                }
                else if (propertyValue is DictionaryValue dictionaryValue)
                {
                    foreach (var element in dictionaryValue.Elements)
                    {
                        WriteProperty($"{log4NetPropertyName}.{element.Key.Value}", element.Value);
                    }
                }
                else if (propertyValue is SequenceValue sequenceValue)
                {
                    foreach (var value in sequenceValue.Elements)
                    {
                        WriteProperty(log4NetPropertyName, value);
                    }
                }
                else if (propertyValue is StructureValue structureValue)
                {
                    foreach (var property in structureValue.Properties)
                    {
                        WriteProperty($"{log4NetPropertyName}.{property.Name}", property.Value);
                    }
                }
            }
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
            var message = logEvent.RenderMessage();
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
            if (exception != null)
            {
                WriteContent(writer, "exception", exception.ToString());
            }
        }

        /// <summary>
        /// Start writing an XML element, taking into account the configured <see cref="Log4NetTextFormatterOptions.Log4NetXmlNamespace"/>.
        /// </summary>
        /// <param name="writer">The XML writer.</param>
        /// <param name="elementName">The name of the XML element to write.</param>
        private void WriteStartElement(XmlWriter writer, string elementName)
        {
            var qualifiedName = _options.Log4NetXmlNamespace;
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
}
