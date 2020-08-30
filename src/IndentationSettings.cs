using System;

namespace Serilog.Formatting.Log4Net
{
    /// <summary>
    /// Defines how XML elements are indented.
    /// </summary>
    public class IndentationSettings
    {
        private readonly string _indentationString;

        /// <summary>
        /// Initialize a new instance of the <see cref="IndentationSettings"/> class.
        /// </summary>
        /// <param name="indentation">The <see cref="Log4Net.Indentation"/> character to use for indenting XML elements.</param>
        /// <param name="size">The number of times the <see cref="Indentation"/> character is repeated.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="size"/> is zero or if the <paramref name="indentation"/> is an invalid value for the enum type <see cref="Indentation"/>.</exception>
        public IndentationSettings(Indentation indentation, byte size)
        {
            if (size == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size, $"The value of argument '{nameof(size)}' must be greater than 0.");
            }
            _indentationString = indentation switch
            {
                Indentation.Space => new string(c: ' ', size),
                Indentation.Tab => new string(c: '\t', size),
                _ => throw new ArgumentOutOfRangeException(nameof(indentation), indentation, $"The value of argument '{nameof(indentation)}' ({indentation}) is invalid for enum type '{nameof(Indentation)}'.")
            };
        }

        /// <summary>
        /// Returns a string representation of the indentation settings.
        /// </summary>
        public override string ToString() => _indentationString;
    }
}