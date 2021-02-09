using System;
using System.IO;
using Serilog.Events;

namespace Serilog.Formatting.Log4Net.Tests
{
    internal class CustomLogEventPropertyValue : LogEventPropertyValue
    {
        private readonly string _renderedText;

        public CustomLogEventPropertyValue(string renderedText)
        {
            _renderedText = renderedText;
        }

        public override void Render(TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
        {
            output.Write(_renderedText);
        }
    }
}