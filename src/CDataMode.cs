namespace Serilog.Formatting.Log4Net
{
    /// <summary>
    /// Controls how <see cref="Log4NetTextFormatter"/> writes <c>message</c> and <c>exception</c> XML elements.
    /// </summary>
    public enum CDataMode
    {
        /// <summary>
        /// The XML element content is always written as a CDATA section.
        /// </summary>
        Always,
        /// <summary>
        /// The XML element content is never written as a CDATA section. I.e. the characters '&amp;', '&lt;' and '&gt;' are escaped.
        /// </summary>
        Never,
        /// <summary>
        /// The XML element content is written as CDATA section if its content contains the '&amp;', '&lt;' or '&gt;' character.
        /// </summary>
        IfNeeded,
    }
}
