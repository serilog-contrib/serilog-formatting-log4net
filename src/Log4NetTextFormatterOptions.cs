using System;
using System.IO;
using System.Xml;

namespace Serilog.Formatting.Log4Net;

/// <summary>
/// Options for configuring the XML format produced by <see cref="Log4NetTextFormatter"/>.
/// </summary>
internal sealed class Log4NetTextFormatterOptions
{
    internal Log4NetTextFormatterOptions(IFormatProvider? formatProvider, CDataMode cDataMode, XmlQualifiedName? xmlNamespace, LineEnding lineEnding, IndentationSettings? indentationSettings, PropertyFilter filterProperty, ExceptionFormatter formatException)
    {
        FormatProvider = formatProvider;
        CDataMode = cDataMode;
        XmlNamespace = xmlNamespace;
        NewLineChars = lineEnding.ToCharacters();
        IndentationSettings = indentationSettings;
        FilterProperty = filterProperty;
        FormatException = formatException;
    }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseFormatProvider"/></summary>
    internal IFormatProvider? FormatProvider { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseCDataMode"/></summary>
    internal CDataMode CDataMode { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseNoXmlNamespace"/></summary>
    internal XmlQualifiedName? XmlNamespace { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseLineEnding"/></summary>
    internal string NewLineChars { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseIndentationSettings"/></summary>
    private IndentationSettings? IndentationSettings { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UsePropertyFilter"/></summary>
    internal PropertyFilter FilterProperty { get; }

    /// <summary>See <see cref="Log4NetTextFormatterOptionsBuilder.UseExceptionFormatter"/></summary>
    internal ExceptionFormatter FormatException { get; }

    internal XmlWriter CreateXmlWriter(TextWriter output, bool useLog4JCompatibility)
    {
        if (useLog4JCompatibility)
        {
            var xmlWriter = new NoNamespaceXmlWriter(output, Log4NetTextFormatterOptionsBuilder.Log4JXmlNamespace);
            if (IndentationSettings != null)
            {
                xmlWriter.Formatting = System.Xml.Formatting.Indented;
                xmlWriter.IndentChar = IndentationSettings.Character;
                xmlWriter.Indentation = IndentationSettings.Size;
            }
            return xmlWriter;
        }

        var settings = new XmlWriterSettings
        {
            Indent = IndentationSettings is not null,
            NewLineChars = NewLineChars,
            ConformanceLevel = ConformanceLevel.Fragment,
        };
        if (IndentationSettings is not null)
        {
            settings.IndentChars = IndentationSettings.ToString();
        }
        return XmlWriter.Create(output, settings);
    }
}