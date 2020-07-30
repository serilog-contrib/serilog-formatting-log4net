using System;

namespace Serilog.Formatting.Log4Net
{
    /// <summary>
    /// Possible XML line endings.
    /// <para>Both <see cref="CarriageReturn"/> and <see cref="LineFeed"/> can be combined, i.e. <c>LineEnding.CarriageReturn | LineEnding.LineFeed</c> in order to produce the CR+LF characters.</para>
    /// <para>See also [End-of-Line Handling](https://www.w3.org/TR/xml/#sec-line-ends) in the XML specification.</para>
    /// </summary>
    [Flags]
    public enum LineEnding
    {
        /// <summary>
        /// No line ending.
        /// </summary>
        None = 0,
        /// <summary>
        /// End lines with the line feed (LF) character, i.e. unicode U+000A.
        /// </summary>
        LineFeed = 1,
        /// <summary>
        /// End lines with the carriage return (CR) character, i.e. unicode U+000D.
        /// </summary>
        CarriageReturn = 2,
    }

    /// <summary>
    /// Extensions to the <see cref="LineEnding"/> enum type.
    /// </summary>
    public static class LineEndingExtensions
    {
        /// <summary>
        /// Returns a string representation of the specified <paramref name="lineEnding"/>.
        /// </summary>
        /// <param name="lineEnding">The <see cref="LineEnding"/> to convert to a string.</param>
        /// <returns>A string representation of the <see cref="LineEnding"/>.</returns>
        public static string ToCharacters(this LineEnding lineEnding) => lineEnding switch
        {
            LineEnding.None => "",
            LineEnding.LineFeed => "\n",
            LineEnding.CarriageReturn => "\r",
            LineEnding.CarriageReturn | LineEnding.LineFeed => "\r\n",
            _ => throw new ArgumentOutOfRangeException(nameof(lineEnding), lineEnding, $"The value of argument '{nameof(lineEnding)}' ({lineEnding}) is invalid for Enum type '{nameof(LineEnding)}'.")
        };
    }
}