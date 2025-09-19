using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace Serilog.Formatting.Log4Net;

// Support for https://github.com/serilog/serilog-settings-configuration through dedicated constructor
public partial class Log4NetTextFormatter
{
    /// <summary>
    /// The default text used to represent <see langword="null"/>.
    /// </summary>
    internal const string DefaultNullText = "(null)";

    /// <summary>
    /// Do not use this constructor. It is only available for the <a href="https://github.com/serilog/serilog-settings-configuration">Serilog.Settings.Configuration</a> integration.
    /// The property filter, the message formatter and the exception formatter can only be configured through
    /// the <see cref="Log4NetTextFormatter(Action&lt;Log4NetTextFormatterOptionsBuilder&gt;)"/> constructor.
    /// </summary>
    /// <remarks>
    /// Binary compatibility across versions is not guaranteed. New optional parameters will be added in order to match new options.
    /// </remarks>
    [Obsolete("This constructor is only for use by the Serilog.Settings.Configuration package.", error: true)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by Serilog.Settings.Configuration through reflection.")]
    public Log4NetTextFormatter(
        string? formatProvider = null,
        CDataMode? cDataMode = null,
        string? nullText = DefaultNullText,
        // in order to support options.UseNullText(null) on .NET < 10, see https://learn.microsoft.com/en-us/dotnet/core/compatibility/extensions/10.0/configuration-null-values-preserved
        bool noNullText = false,
        bool noXmlNamespace = false,
        LineEnding? lineEnding = null,
        string? indentation = null,
        bool log4JCompatibility = false
    ) : this(options =>
        {
            if (formatProvider != null)
                options.UseFormatProvider(CultureInfo.GetCultureInfo(formatProvider));
            if (cDataMode != null)
                options.UseCDataMode(cDataMode.Value);
            if (nullText != DefaultNullText)
                options.UseNullText(nullText);
            if (noNullText)
                options.UseNullText(null);
            if (noXmlNamespace)
                options.UseNoXmlNamespace();
            if (lineEnding != null)
                options.UseLineEnding(lineEnding.Value);
            if (indentation != null)
                ConfigureIndentation(options, indentation);
            if (log4JCompatibility)
                options.UseLog4JCompatibility();
        })
    {
    }

    private static void ConfigureIndentation(Log4NetTextFormatterOptionsBuilder options, string indentation)
    {
        if (indentation.Length == 0)
        {
            options.UseNoIndentation();
            return;
        }

        if (!ConfigureIndentation(options, indentation, Indentation.Space) && !ConfigureIndentation(options, indentation, Indentation.Tab))
        {
            throw new ArgumentException("The indentation must contains only space or tab characters.", nameof(indentation));
        }
    }

    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "An uppercase word in the middle of a sentence would be weird")]
    private static bool ConfigureIndentation(Log4NetTextFormatterOptionsBuilder options, string indentation, Indentation indent)
    {
        var indentChar = indent == Indentation.Space ? ' ' : '\t';
        var count = indentation.Count(c => c == indentChar);
        if (count == indentation.Length)
        {
            if (count <= byte.MaxValue)
            {
                options.UseIndentationSettings(new IndentationSettings(indent, Convert.ToByte(count)));
                return true;
            }

            var indentDescription = $"{indent.ToString().ToLowerInvariant()}s";
            throw new ArgumentException($"The indentation exceeds the maximum number of allowed {indentDescription}. ({count} > {byte.MaxValue})", nameof(indentation));
        }

        return false;
    }
}