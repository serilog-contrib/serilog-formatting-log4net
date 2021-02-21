using System;
using System.Globalization;
using System.Xml;
using Serilog.Events;

namespace Serilog.Formatting.Log4Net
{
    /// <summary>
    /// A fluent builder for the options controlling how <see cref="Log4NetTextFormatter"/> writes log4net events.
    /// </summary>
    public class Log4NetTextFormatterOptionsBuilder
    {
        /// <summary>
        /// The <see cref="IFormatProvider"/> used when formatting message and properties of log4net events.
        /// <para>The default value is <see langref="null"/>, meaning that the default Serilog provider is used, i.e. the <see cref="CultureInfo.InvariantCulture"/>.</para>
        /// </summary>
        public IFormatProvider? FormatProvider { get; private set; }

        /// <summary>
        /// The <see cref="CDataMode"/> controlling how <c>message</c> and <c>exception</c> XML elements of log4net events are written.
        /// <para>The default value is <see cref="Log4Net.CDataMode.Always"/>.</para>
        /// </summary>
        public CDataMode CDataMode { get; private set; } = CDataMode.Always;

        /// <summary>
        /// The XML namespace used for log4net events. No namespace is used if <see langref="null"/>.
        /// <para>The default value has prefix <c>log4net</c> and namespace <c>http://logging.apache.org/log4net/schemas/log4net-events-1.2/</c>.</para>
        /// </summary>
        /// <remarks>https://github.com/apache/logging-log4net/blob/rel/2.0.8/src/Layout/XmlLayout.cs#L49</remarks>
        public XmlQualifiedName? Log4NetXmlNamespace { get; private set; } = new("log4net", "http://logging.apache.org/log4net/schemas/log4net-events-1.2/");

        /// <summary>
        /// The line ending used for log4net events.
        /// <para>The default value is <see cref="Log4Net.LineEnding.LineFeed"/>.</para>
        /// </summary>
        public LineEnding LineEnding { get; private set; } = LineEnding.LineFeed;

        /// <summary>
        /// The indentation settings used for log4net events. No indentation is used if <see langref="null"/>.
        /// <para>The default value uses two spaces.</para>
        /// </summary>
        public IndentationSettings? IndentationSettings { get; private set; } = new(Indentation.Space, size: 2);

        /// <summary>
        /// The <see cref="PropertyFilter"/> applied on all Serilog properties.
        /// <para>The default implementation always returns <c>true</c>, i.e. it doesn't filter out any property.</para>
        /// </summary>
        /// <remarks>If an exception is thrown while executing the filter, the default filter will be applied, i.e. the Serilog property will be included in the log4net properties.</remarks>
        public PropertyFilter FilterProperty { get; private set; } = (_, _) => true;

        /// <summary>
        /// The <see cref="ExceptionFormatter"/> controlling how all exceptions are formatted.
        /// <para>The default implementation calls <c>Exception.ToString()</c>.</para>
        /// <para>If the formatter returns <see langref="null"/>, the exception will not be written to the log4net event.</para>
        /// </summary>
        /// <remarks>If an exception is thrown while executing the formatter, the default formatter will be used, i.e. <c>Exception.ToString()</c>.</remarks>
        public ExceptionFormatter FormatException { get; private set; } = exception => exception.ToString();

        /// <summary>
        /// Sets the <see cref="IFormatProvider"/> used when formatting message and properties of log4net events.
        /// <para>The default value is <see langref="null"/>, meaning that the default Serilog provider is used, i.e. the <see cref="CultureInfo.InvariantCulture"/>.</para>
        /// </summary>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use.</param>
        /// <returns>The builder in order to fluently chain all options.</returns>
        public Log4NetTextFormatterOptionsBuilder UseFormatProvider(IFormatProvider? formatProvider)
        {
            FormatProvider = formatProvider;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="CDataMode"/> controlling how <c>message</c> and <c>exception</c> XML elements of log4net events are written.
        /// <para>The default value is <see cref="Log4Net.CDataMode.Always"/>.</para>
        /// </summary>
        /// <param name="cDataMode">The <see cref="CDataMode"/> to use.</param>
        /// <returns>The builder in order to fluently chain all options.</returns>
        public Log4NetTextFormatterOptionsBuilder UseCDataMode(CDataMode cDataMode)
        {
            CDataMode = cDataMode;
            return this;
        }

        /// <summary>
        /// Sets the XML namespace used for log4net events. Set to <see langref="null"/> in order not to use a namespace.
        /// <para>The default value has prefix <c>log4net</c> and namespace <c>http://logging.apache.org/log4net/schemas/log4net-events-1.2/</c>.</para>
        /// </summary>
        /// <param name="log4NetXmlNamespace">The XML namespace to use.</param>
        /// <returns>The builder in order to fluently chain all options.</returns>
        public Log4NetTextFormatterOptionsBuilder UseLog4NetXmlNamespace(XmlQualifiedName? log4NetXmlNamespace)
        {
            Log4NetXmlNamespace = log4NetXmlNamespace;
            return this;
        }

        /// <summary>
        /// Sets the line ending used for log4net events.
        /// <para>The default value is <see cref="Log4Net.LineEnding.LineFeed"/>.</para>
        /// </summary>
        /// <param name="lineEnding">The <see cref="Log4Net.LineEnding"/> to use.</param>
        /// <returns>The builder in order to fluently chain all options.</returns>
        public Log4NetTextFormatterOptionsBuilder UseLineEnding(LineEnding lineEnding)
        {
            LineEnding = lineEnding;
            return this;
        }

        /// <summary>
        /// Sets the indentation settings used for log4net events.
        /// <para>The default value uses two spaces.</para>
        /// </summary>
        /// <param name="indentationSettings">The <see cref="IndentationSettings"/> to use.</param>
        /// <returns>The builder in order to fluently chain all options.</returns>
        public Log4NetTextFormatterOptionsBuilder UseIndentationSettings(IndentationSettings indentationSettings)
        {
            IndentationSettings = indentationSettings;
            return this;
        }

        /// <summary>
        /// Do not indent log4net events.
        /// </summary>
        /// <returns>The builder in order to fluently chain all options.</returns>
        public Log4NetTextFormatterOptionsBuilder UseNoIndentation()
        {
            IndentationSettings = null;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="PropertyFilter"/> applied on all Serilog properties.
        /// <para>The default implementation always returns <c>true</c>, i.e. it doesn't filter out any property.</para>
        /// </summary>
        /// <remarks>If an exception is thrown while executing the filter, the default filter will be applied, i.e. the Serilog property will be included in the log4net properties.</remarks>
        /// <returns>The builder in order to fluently chain all options.</returns>
        public Log4NetTextFormatterOptionsBuilder UsePropertyFilter(PropertyFilter filterProperty)
        {
            FilterProperty = filterProperty ?? throw new ArgumentNullException(nameof(filterProperty), $"The {nameof(FilterProperty)} option can not be null.");
            return this;
        }

        /// <summary>
        /// Sets the <see cref="ExceptionFormatter"/> controlling how all exceptions are formatted.
        /// <para>The default implementation calls <c>Exception.ToString()</c>.</para>
        /// <para>If the formatter returns <see langref="null"/>, the exception will not be written to the log4net event.</para>
        /// </summary>
        /// <remarks>If an exception is thrown while executing the formatter, the default formatter will be used, i.e. <c>Exception.ToString()</c>.</remarks>
        /// <returns>The builder in order to fluently chain all options.</returns>
        public Log4NetTextFormatterOptionsBuilder UseExceptionFormatter(ExceptionFormatter formatException)
        {
            FormatException = formatException ?? throw new ArgumentNullException(nameof(formatException), $"The {nameof(FormatException)} option can not be null.");
            return this;
        }

        /// <summary>
        /// Enables log4j compatibility mode. This tweaks the XML elements to match the log4j logging event specification.
        /// The DTD can be found at https://raw.githubusercontent.com/apache/log4j/v1_2_17/src/main/resources/org/apache/log4j/xml/log4j.dtd
        /// <para>
        /// Here is the list of differences between the log4net and the log4j XML layout:
        /// <list type="bullet">
        ///   <item>The log element uses <c>log4j</c> instead of <c>log4net</c> XML namespace.</item>
        ///   <item>The <c>timestamp</c> attribute uses milliseconds elapsed from 1/1/1970 instead of an ISO 8601 formatted date.</item>
        ///   <item>The exception element is named <c>throwable</c> instead of <c>exception</c>.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <remarks>You must not change other options after calling this method.</remarks>
        public void UseLog4JCompatibility()
        {
            // https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L135
            LineEnding = LineEnding.CarriageReturn | LineEnding.LineFeed;

            // https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L137
            Log4NetXmlNamespace = new XmlQualifiedName("log4j", "http://jakarta.apache.org/log4j/");

            // https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L147
            CDataMode = CDataMode.Always;
        }

        internal Log4NetTextFormatterOptions Build()
            => new(FormatProvider, CDataMode, Log4NetXmlNamespace, CreateXmlWriterSettings(LineEnding, IndentationSettings), FilterProperty, FormatException);

        private static XmlWriterSettings CreateXmlWriterSettings(LineEnding lineEnding, IndentationSettings? indentationSettings)
        {
            return new()
            {
                Indent = !(indentationSettings is null),
                IndentChars = indentationSettings?.ToString() ?? "",
                NewLineChars = lineEnding.ToCharacters(),
                ConformanceLevel = ConformanceLevel.Fragment,
            };
        }
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