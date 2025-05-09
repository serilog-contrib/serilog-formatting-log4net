using System;
using System.Xml;
using Serilog.Events;

namespace Serilog.Formatting.Log4Net;

/// <summary>
/// A fluent builder for the options controlling how <see cref="Log4NetTextFormatter"/> writes log4net events.
/// </summary>
public class Log4NetTextFormatterOptionsBuilder
{
    /// <summary>
    /// The XML namespace used for log4net events.
    /// </summary>
    /// <remarks>https://github.com/apache/logging-log4net/blob/rel/2.0.8/src/Layout/XmlLayout.cs#L49</remarks>
    private static readonly XmlQualifiedName Log4NetXmlNamespace = new("log4net", "http://logging.apache.org/log4net/schemas/log4net-events-1.2/");

    /// <summary>
    /// The XML namespace used for Log4j events.
    /// </summary>
    /// <remarks>https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L137</remarks>
    internal static readonly XmlQualifiedName Log4JXmlNamespace = new("log4j", "http://jakarta.apache.org/log4j/");

    /// <summary>
    /// Initialize a new instance of the <see cref="Log4NetTextFormatterOptionsBuilder"/> class.
    /// </summary>
    internal Log4NetTextFormatterOptionsBuilder()
    {
    }

    /// <summary>See <see cref="UseFormatProvider"/></summary>
    private IFormatProvider? _formatProvider;

    /// <summary> See <see cref="UseCDataMode"/></summary>
    private CDataMode _cDataMode = CDataMode.Always;

    /// <summary>See <see cref="UseNoXmlNamespace"/></summary>
    private XmlQualifiedName? _xmlNamespace = Log4NetXmlNamespace;

    /// <summary>See <see cref="UseLineEnding"/></summary>
    private LineEnding _lineEnding = LineEnding.LineFeed;

    /// <summary>See <see cref="UseIndentationSettings"/></summary>
    private IndentationSettings? _indentationSettings = new(Indentation.Space, size: 2);

    /// <summary>See <see cref="UsePropertyFilter"/></summary>
    private PropertyFilter _filterProperty = (_, _) => true;

    /// <summary>See <see cref="UseMessageFormatter"/></summary>
    private MessageFormatter _formatMessage = Log4NetTextFormatter.DefaultMessageFormatter;

    /// <summary>See <see cref="UseExceptionFormatter"/></summary>
    private ExceptionFormatter _formatException = Log4NetTextFormatter.DefaultExceptionFormatter;

    /// <summary>
    /// Sets the <see cref="IFormatProvider"/> used when formatting message and properties of log4net events.
    /// <para/>
    /// The default value is <see langword="null"/>, meaning that the default Serilog format provider is used, i.e. the invariant culture.
    /// </summary>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use.</param>
    /// <returns>The builder in order to fluently chain all options.</returns>
    public Log4NetTextFormatterOptionsBuilder UseFormatProvider(IFormatProvider? formatProvider)
    {
        _formatProvider = formatProvider;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="CDataMode"/> controlling how <c>message</c> and <c>exception</c> XML elements of log4net events are written.
    /// <para/>
    /// The default value is <see cref="Log4Net.CDataMode.Always"/>.
    /// </summary>
    /// <param name="cDataMode">The <see cref="CDataMode"/> to use.</param>
    /// <returns>The builder in order to fluently chain all options.</returns>
    public Log4NetTextFormatterOptionsBuilder UseCDataMode(CDataMode cDataMode)
    {
        _cDataMode = cDataMode;
        return this;
    }

    /// <summary>
    /// Do not use any XML namespace for log4net events.
    /// <para/>
    /// The default value has prefix <c>log4net</c> and namespace <c>http://logging.apache.org/log4net/schemas/log4net-events-1.2/</c>.
    /// </summary>
    /// <returns>The builder in order to fluently chain all options.</returns>
    public Log4NetTextFormatterOptionsBuilder UseNoXmlNamespace()
    {
        _xmlNamespace = null;
        return this;
    }

    /// <summary>
    /// Sets the line ending used for log4net events.
    /// <para/>
    /// The default value is <see cref="Log4Net.LineEnding.LineFeed"/>.
    /// </summary>
    /// <param name="lineEnding">The <see cref="Log4Net.LineEnding"/> to use.</param>
    /// <returns>The builder in order to fluently chain all options.</returns>
    public Log4NetTextFormatterOptionsBuilder UseLineEnding(LineEnding lineEnding)
    {
        _lineEnding = lineEnding;
        return this;
    }

    /// <summary>
    /// Sets the indentation settings used for log4net events.
    /// <para/>
    /// The default value uses two spaces.
    /// </summary>
    /// <param name="indentationSettings">The <see cref="IndentationSettings"/> to use.</param>
    /// <returns>The builder in order to fluently chain all options.</returns>
    public Log4NetTextFormatterOptionsBuilder UseIndentationSettings(IndentationSettings indentationSettings)
    {
        _indentationSettings = indentationSettings;
        return this;
    }

    /// <summary>
    /// Do not indent log4net events.
    /// </summary>
    /// <returns>The builder in order to fluently chain all options.</returns>
    public Log4NetTextFormatterOptionsBuilder UseNoIndentation()
    {
        _indentationSettings = null;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="PropertyFilter"/> applied on all Serilog properties.
    /// <para/>
    /// The default property filter always returns <c>true</c>, i.e. it doesn't filter out any property.
    /// </summary>
    /// <remarks>If an exception is thrown while executing the filter, the default filter will be applied, i.e. the Serilog property will be included in the log4net properties.</remarks>
    /// <returns>The builder in order to fluently chain all options.</returns>
    public Log4NetTextFormatterOptionsBuilder UsePropertyFilter(PropertyFilter filterProperty)
    {
        _filterProperty = filterProperty ?? throw new ArgumentNullException(nameof(filterProperty), "The property filter can not be null.");
        return this;
    }

    /// <summary>
    /// Sets the <see cref="MessageFormatter"/> controlling how all messages are formatted.
    /// <para/>
    /// The default message formatter has two formatting modes.
    /// <list type="bullet">
    ///   <item>If the log event comes from <c>Microsoft.Extensions.Logging</c> (detected by the presence of the <c>EventId</c> property) then the message is formatted by switching off quoting of strings.</item>
    ///   <item>If the log event comes from Serilog, then the message is formatted by calling <see cref="LogEvent.RenderMessage(System.IO.TextWriter,System.IFormatProvider)"/>.</item>
    /// </list>
    /// </summary>
    /// <remarks>If an exception is thrown while executing the formatter, the default formatter will be used.</remarks>
    /// <returns>The builder in order to fluently chain all options.</returns>
    public Log4NetTextFormatterOptionsBuilder UseMessageFormatter(MessageFormatter formatMessage)
    {
        _formatMessage = formatMessage ?? throw new ArgumentNullException(nameof(formatMessage), "The message formatter can not be null.");
        return this;
    }

    /// <summary>
    /// Sets the <see cref="ExceptionFormatter"/> controlling how all exceptions are formatted.
    /// <para/>
    /// If the formatter returns <see langword="null"/>, the exception will not be written to the log4net event.
    /// <para/>
    /// The default exception formatter calls <c>Exception.ToString()</c>.
    /// </summary>
    /// <remarks>If an exception is thrown while executing the formatter, the default formatter will be used, i.e. <c>Exception.ToString()</c>.</remarks>
    /// <returns>The builder in order to fluently chain all options.</returns>
    public Log4NetTextFormatterOptionsBuilder UseExceptionFormatter(ExceptionFormatter formatException)
    {
        _formatException = formatException ?? throw new ArgumentNullException(nameof(formatException), "The exception formatter can not be null.");
        return this;
    }

    /// <summary>
    /// Enables log4j compatibility mode. This tweaks the XML elements to match the log4j logging event specification.
    /// The DTD can be found at https://raw.githubusercontent.com/apache/log4j/v1_2_17/src/main/resources/org/apache/log4j/xml/log4j.dtd
    /// <para>
    /// Here is the list of differences between the log4net and the log4j XML layout:
    /// <list type="bullet">
    ///   <item>The <c>event</c> elements have <c>log4j</c> instead of <c>log4net</c> XML namespace.</item>
    ///   <item>The <c>timestamp</c> attribute uses milliseconds elapsed from 1/1/1970 instead of an ISO 8601 formatted date.</item>
    ///   <item>The exception element is named <c>throwable</c> instead of <c>exception</c>.</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <remarks>You must not change other options after calling this method.</remarks>
    public void UseLog4JCompatibility()
    {
        // https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L135
        _lineEnding = LineEnding.CarriageReturn | LineEnding.LineFeed;

        // https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L137
        _xmlNamespace = Log4JXmlNamespace;

        // https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L147
        _cDataMode = CDataMode.Always;
    }

    internal Log4NetTextFormatterOptions Build()
        => new(_formatProvider, _cDataMode, _xmlNamespace, CreateXmlWriterSettings(_lineEnding, _indentationSettings), _filterProperty, _formatMessage, _formatException);

    private static XmlWriterSettings CreateXmlWriterSettings(LineEnding lineEnding, IndentationSettings? indentationSettings)
    {
        var xmlWriterSettings = new XmlWriterSettings
        {
            Indent = indentationSettings is not null,
            NewLineChars = lineEnding.ToCharacters(),
            ConformanceLevel = ConformanceLevel.Fragment,
        };
        if (indentationSettings is not null)
        {
            xmlWriterSettings.IndentChars = indentationSettings.ToString();
        }
        return xmlWriterSettings;
    }
}

/// <summary>
/// Represents the method that determines whether a Serilog property must be included in the log4net properties.
/// </summary>
/// <param name="logEvent">The <see cref="LogEvent"/> associated with the Serilog property.</param>
/// <param name="propertyName">The Serilog property name.</param>
/// <returns><see langword="true"/> to include the Serilog property in the log4net properties or <see langword="false"/> to ignore the Serilog property.</returns>
public delegate bool PropertyFilter(LogEvent logEvent, string propertyName);

/// <summary>
/// Represents the method that formats the message for a <see cref="LogEvent"/>.
/// </summary>
/// <param name="logEvent">The log event to format.</param>
/// <param name="formatProvider">The <see cref="IFormatProvider"/> that supplies culture-specific formatting information, or <see langword="null"/>.</param>
/// <returns>The formatted message.</returns>
public delegate string MessageFormatter(LogEvent logEvent, IFormatProvider? formatProvider);

/// <summary>
/// Represents the method that formats an <see cref="Exception"/>.
/// </summary>
/// <param name="exception">The exception to format.</param>
/// <returns>The formatted exception.</returns>
public delegate string ExceptionFormatter(Exception exception);