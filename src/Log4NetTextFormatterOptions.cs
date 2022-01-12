using System;
using System.Xml;

namespace Serilog.Formatting.Log4Net;

/// <summary>
/// Options for configuring the XML format produced by <see cref="Log4NetTextFormatter"/>.
/// </summary>
internal class Log4NetTextFormatterOptions
{
    internal Log4NetTextFormatterOptions(IFormatProvider? formatProvider, CDataMode cDataMode, XmlQualifiedName? xmlNamespace, XmlWriterSettings xmlWriterSettings, PropertyFilter filterProperty, ExceptionFormatter formatException)
    {
        FormatProvider = formatProvider;
        CDataMode = cDataMode;
        XmlNamespace = xmlNamespace;
        XmlWriterSettings = xmlWriterSettings;
        FilterProperty = filterProperty;
        FormatException = formatException;
    }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseFormatProvider"/></summary>
    internal IFormatProvider? FormatProvider { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseCDataMode"/></summary>
    internal CDataMode CDataMode { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseNoXmlNamespace"/></summary>
    internal XmlQualifiedName? XmlNamespace { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.CreateXmlWriterSettings"/></summary>
    internal XmlWriterSettings XmlWriterSettings { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UsePropertyFilter"/></summary>
    internal PropertyFilter FilterProperty { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseExceptionFormatter"/></summary>
    internal ExceptionFormatter FormatException { get; }
}