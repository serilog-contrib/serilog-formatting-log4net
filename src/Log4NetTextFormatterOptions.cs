using System;
using System.Xml;

namespace Serilog.Formatting.Log4Net
{
    /// <summary>
    /// Options for configuring the XML format produced by <see cref="Log4NetTextFormatter"/>.
    /// </summary>
    internal class Log4NetTextFormatterOptions
    {
        internal Log4NetTextFormatterOptions(IFormatProvider? formatProvider, CDataMode cDataMode, XmlQualifiedName? log4NetXmlNamespace, XmlWriterSettings xmlWriterSettings, PropertyFilter filterProperty, ExceptionFormatter formatException)
        {
            FormatProvider = formatProvider;
            CDataMode = cDataMode;
            Log4NetXmlNamespace = log4NetXmlNamespace;
            XmlWriterSettings = xmlWriterSettings ?? throw new ArgumentNullException(nameof(xmlWriterSettings));
            FilterProperty = filterProperty ?? throw new ArgumentNullException(nameof(filterProperty));
            FormatException = formatException ?? throw new ArgumentNullException(nameof(formatException));
        }

        /// See <see cref="Log4NetTextFormatterOptionsBuilder.FormatProvider"/>
        internal IFormatProvider? FormatProvider { get; }

        /// See <see cref="Log4NetTextFormatterOptionsBuilder.CDataMode"/>
        internal CDataMode CDataMode { get; }

        /// See <see cref="Log4NetTextFormatterOptionsBuilder.Log4NetXmlNamespace"/>
        internal XmlQualifiedName? Log4NetXmlNamespace { get; }

        /// See <see cref="Log4NetTextFormatterOptionsBuilder.CreateXmlWriterSettings"/>
        internal XmlWriterSettings XmlWriterSettings { get; }

        /// See <see cref="Log4NetTextFormatterOptionsBuilder.FilterProperty"/>
        internal PropertyFilter FilterProperty { get; }

        /// See <see cref="Log4NetTextFormatterOptionsBuilder.FormatException"/>
        internal ExceptionFormatter FormatException { get; }
    }
}