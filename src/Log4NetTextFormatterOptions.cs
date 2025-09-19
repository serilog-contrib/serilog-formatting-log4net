using System;
using System.Xml;

namespace Serilog.Formatting.Log4Net;

/// <summary>
/// Options for configuring the XML format produced by <see cref="Log4NetTextFormatter"/>.
/// </summary>
/// <param name="FormatProvider">See <see cref="Log4NetTextFormatterOptionsBuilder.UseFormatProvider"/></param>
/// <param name="CDataMode">See <see cref="Log4NetTextFormatterOptionsBuilder.UseCDataMode"/></param>
/// <param name="NullText">See <see cref="Log4NetTextFormatterOptionsBuilder.UseNullText"/></param>
/// <param name="XmlNamespace">See <see cref="Log4NetTextFormatterOptionsBuilder.UseNoXmlNamespace"/></param>
/// <param name="XmlWriterSettings">See <see cref="Log4NetTextFormatterOptionsBuilder.CreateXmlWriterSettings"/></param>
/// <param name="FilterProperty">See <see cref="Log4NetTextFormatterOptionsBuilder.UsePropertyFilter"/></param>
/// <param name="FormatMessage">See <see cref="Log4NetTextFormatterOptionsBuilder.UseMessageFormatter"/></param>
/// <param name="FormatException">See <see cref="Log4NetTextFormatterOptionsBuilder.UseExceptionFormatter"/></param>
internal record struct Log4NetTextFormatterOptions(
    IFormatProvider? FormatProvider,
    CDataMode CDataMode,
    string? NullText,
    XmlQualifiedName? XmlNamespace,
    XmlWriterSettings XmlWriterSettings,
    PropertyFilter FilterProperty,
    MessageFormatter FormatMessage,
    ExceptionFormatter FormatException);