namespace Serilog.Formatting.Log4Net
{
    /// <summary>
    /// Defines how XML elements are indented.
    /// </summary>
    public class IndentationSettings
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="IndentationSettings"/> class.
        /// </summary>
        /// <param name="indentation">The <see cref="Log4Net.Indentation"/> character to use for indenting XML elements.</param>
        /// <param name="size">The number of times the <see cref="Indentation"/> character is repeated.</param>
        public IndentationSettings(Indentation indentation, byte size)
        {
            Indentation = indentation;
            Size = size;
        }

        /// <summary>
        /// The <see cref="Log4Net.Indentation"/> character to use for indenting XML elements.
        /// </summary>
        public Indentation Indentation { get; }

        /// <summary>
        /// The number of times the <see cref="Indentation"/> character is repeated.
        /// </summary>
        public byte Size { get; }
    }
}