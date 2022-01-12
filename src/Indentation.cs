namespace Serilog.Formatting.Log4Net;

/// <summary>
/// Possible values for XML elements indentation.
/// </summary>
public enum Indentation
{
    /// <summary>
    /// Indent with the space character, i.e. unicode U+0020.
    /// </summary>
    Space,
    /// <summary>
    /// Indent with the tabulation character, i.e. unicode U+0009.
    /// </summary>
    Tab,
}