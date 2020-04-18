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
        /// The <see cref="CDataMode"/> controlling how CData sections are emitted.
        /// The default value is <see cref="Log4Net.CDataMode.IfNeeded"/>.
        /// </summary>
        public CDataMode CDataMode { get; set; } = CDataMode.IfNeeded;

        /// <summary>
        /// The XML namespace used on log events. Set to <c>null</c> to remove namespaces.
        /// The default value has prefix <c>log4net</c> and namespace <c>http://logging.apache.org/log4net/schemas/log4net-events-1.2/</c>.
        /// </summary>
        /// <remarks>https://github.com/apache/logging-log4net/blob/rel/2.0.8/src/Layout/XmlLayout.cs#L49</remarks>
        public XmlQualifiedName? Log4NetXmlNamespace { get; set; } = new XmlQualifiedName("log4net", "http://logging.apache.org/log4net/schemas/log4net-events-1.2/");

        /// <summary>
        /// The <see cref="XmlWriterSettings"/> controlling the formatting of the XML fragments.
        /// The default value sets <see cref="System.Xml.XmlWriterSettings.ConformanceLevel"/> to <see cref="ConformanceLevel.Fragment"/> and <see cref="System.Xml.XmlWriterSettings.Indent"/> to <c>true</c>.
        /// </summary>
        /// <remarks>If the <see cref="System.Xml.XmlWriterSettings.ConformanceLevel"/> is changed, it will be reset to <see cref="ConformanceLevel.Fragment"/>.</remarks>
        public XmlWriterSettings XmlWriterSettings { get; } = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment, Indent = true };

        /// <summary>
        /// A filter applied on all Serilog properties. Return <c>true</c> to have the Serilog property written to the Log4Net properties or <c>false</c> to ignore the property.
        /// The first argument is the <see cref="LogEvent"/>.
        /// The second argument is the Serilog property name.
        /// The default value always returns <c>true</c>, i.e. it doesn't filter out any property.
        /// </summary>
        public Func<LogEvent, string, bool> FilterProperty { get; set; } = (logEvent, propertyName) => true;

        /// <summary>
        /// A function controlling how all exceptions are formatted.
        /// The default implementation calls <see cref="Exception.ToString"/>.
        /// </summary>
        public Func<Exception, string> ExceptionFormatter { get; set; } = exception => exception.ToString();
    }
}