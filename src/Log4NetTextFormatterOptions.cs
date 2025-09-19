using System;
using System.Xml;

namespace Serilog.Formatting.Log4Net;

/// <summary>
/// Options for configuring the XML format produced by <see cref="Log4NetTextFormatter"/>.
/// </summary>
internal sealed class Log4NetTextFormatterOptions
{
    internal Log4NetTextFormatterOptions(IFormatProvider? formatProvider, CDataMode cDataMode, string? nullText, XmlQualifiedName? xmlNamespace, XmlWriterSettings xmlWriterSettings, PropertyFilter filterProperty, MessageFormatter formatMessage, ExceptionFormatter formatException)
    {
        FormatProvider = formatProvider;
        CDataMode = cDataMode;
        NullText = nullText;
        XmlNamespace = xmlNamespace;
        XmlWriterSettings = xmlWriterSettings;
        FilterProperty = filterProperty;
        FormatMessage = formatMessage;
        FormatException = formatException;
    }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseFormatProvider"/></summary>
    public IFormatProvider? FormatProvider { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseCDataMode"/></summary>
    public CDataMode CDataMode { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseNullText"/></summary>
    public string? NullText { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseNoXmlNamespace"/></summary>
    public XmlQualifiedName? XmlNamespace { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.CreateXmlWriterSettings"/></summary>
    public XmlWriterSettings XmlWriterSettings { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UsePropertyFilter"/></summary>
    public PropertyFilter FilterProperty { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseMessageFormatter"/></summary>
    public MessageFormatter FormatMessage { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseExceptionFormatter"/></summary>
    public ExceptionFormatter FormatException { get; }
}