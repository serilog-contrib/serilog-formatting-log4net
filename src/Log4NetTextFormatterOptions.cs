using System;
using System.Xml;
using Serilog.Events;

namespace Serilog.Formatting.Log4Net
{
    /// <summary>
    /// Options for configuring the XML format produced by <see cref="Log4NetTextFormatter"/>.
    /// </summary>
    public class Log4NetTextFormatterOptions
    {
        internal Log4NetTextFormatterOptions()
        {
        }

        /// <summary>
        /// The <see cref="CDataMode"/> controlling how <c>message</c> and <c>exception</c> XML elements are written.
        /// <para>The default value is <see cref="Log4Net.CDataMode.Always"/>.</para>
        /// </summary>
        public CDataMode CDataMode { get; set; } = CDataMode.Always;

        /// <summary>
        /// The XML namespace used on log events. Set to <c>null</c> to remove namespaces.
        /// <para>The default value has prefix <c>log4net</c> and namespace <c>http://logging.apache.org/log4net/schemas/log4net-events-1.2/</c>.</para>
        /// </summary>
        /// <remarks>https://github.com/apache/logging-log4net/blob/rel/2.0.8/src/Layout/XmlLayout.cs#L49</remarks>
        public XmlQualifiedName? Log4NetXmlNamespace { get; set; } = new XmlQualifiedName("log4net", "http://logging.apache.org/log4net/schemas/log4net-events-1.2/");

        /// <summary>
        /// The <see cref="XmlWriterSettings"/> controlling the formatting of the XML fragments.
        /// <para>The default value sets <see cref="System.Xml.XmlWriterSettings.ConformanceLevel"/> to <see cref="ConformanceLevel.Fragment"/> and <see cref="System.Xml.XmlWriterSettings.Indent"/> to <c>true</c>.</para>
        /// </summary>
        /// <remarks>If the <see cref="System.Xml.XmlWriterSettings.ConformanceLevel"/> is changed, an <see cref="InvalidOperationException"/> will be thrown.</remarks>
        public XmlWriterSettings XmlWriterSettings { get; } = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment, Indent = true };

        /// <summary>
        /// A <see cref="PropertyFilter"/> applied on all Serilog properties.
        /// <para>The default implementation always returns <c>true</c>, i.e. it doesn't filter out any property.</para>
        /// </summary>
        /// <remarks>If an exception is thrown while executing the filter, the default filter will be applied, i.e. the Serilog property will be included in the log4net properties.</remarks>
        public PropertyFilter FilterProperty { get; set; } = (logEvent, propertyName) => true;

        /// <summary>
        /// An <see cref="ExceptionFormatter"/> controlling how all exceptions are formatted.
        /// <para>The default implementation calls <c>Exception.ToString()</c>.</para>
        /// <para>If the formatter returns <see langref="null"/>, the exception will not be written to the log4net event.</para>
        /// </summary>
        /// <remarks>If an exception is thrown while executing the formatter, the default formatter will be used, i.e. <c>Exception.ToString()</c>.</remarks>
        public ExceptionFormatter FormatException { get; set; } = exception => exception.ToString();
    }

    /// <summary>
    /// Represents the method that determines whether a Serilog property must be included in the log4net properties.
    /// </summary>
    /// <param name="logEvent">The <see cref="LogEvent"/> associated with the Serilog property.</param>
    /// <param name="propertyName">The Serilog property name.</param>
    /// <returns><see langref="true"/> to include the Serilog property in the log4net properties or <see langref="false"/> to ignore the Serilog property.</returns>
    public delegate bool PropertyFilter(LogEvent logEvent, string propertyName);

    /// <summary>
    /// Represents the method that formats an <see cref="Exception"/>.
    /// </summary>
    /// <param name="exception">The exception to be formatted.</param>
    public delegate string ExceptionFormatter(Exception exception);
}