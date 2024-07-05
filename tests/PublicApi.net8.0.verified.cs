[assembly: System.CLSCompliant(true)]
[assembly: System.Reflection.AssemblyMetadata("IsTrimmable", "True")]
[assembly: System.Reflection.AssemblyMetadata("RepositoryUrl", "https://github.com/serilog-contrib/serilog-formatting-log4net")]
[assembly: System.Runtime.Versioning.TargetFramework(".NETCoreApp,Version=v8.0", FrameworkDisplayName=".NET 8.0")]
namespace Serilog.Formatting.Log4Net
{
    public enum CDataMode
    {
        Always = 0,
        Never = 1,
        IfNeeded = 2,
    }
    public delegate string ExceptionFormatter(System.Exception exception);
    public enum Indentation
    {
        Space = 0,
        Tab = 1,
    }
    public class IndentationSettings
    {
        public IndentationSettings(Serilog.Formatting.Log4Net.Indentation indentation, byte size) { }
        public override string ToString() { }
    }
    [System.Flags]
    public enum LineEnding
    {
        None = 0,
        LineFeed = 1,
        CarriageReturn = 2,
    }
    public class Log4NetTextFormatter : Serilog.Formatting.ITextFormatter
    {
        public Log4NetTextFormatter() { }
        public Log4NetTextFormatter(System.Action<Serilog.Formatting.Log4Net.Log4NetTextFormatterOptionsBuilder>? configureOptions) { }
        public static Serilog.Formatting.Log4Net.Log4NetTextFormatter Log4JFormatter { get; }
        public void Format(Serilog.Events.LogEvent logEvent, System.IO.TextWriter output) { }
    }
    public class Log4NetTextFormatterOptionsBuilder
    {
        public Serilog.Formatting.Log4Net.Log4NetTextFormatterOptionsBuilder UseCDataMode(Serilog.Formatting.Log4Net.CDataMode cDataMode) { }
        public Serilog.Formatting.Log4Net.Log4NetTextFormatterOptionsBuilder UseExceptionFormatter(Serilog.Formatting.Log4Net.ExceptionFormatter formatException) { }
        public Serilog.Formatting.Log4Net.Log4NetTextFormatterOptionsBuilder UseFormatProvider(System.IFormatProvider? formatProvider) { }
        public Serilog.Formatting.Log4Net.Log4NetTextFormatterOptionsBuilder UseIndentationSettings(Serilog.Formatting.Log4Net.IndentationSettings indentationSettings) { }
        public Serilog.Formatting.Log4Net.Log4NetTextFormatterOptionsBuilder UseLineEnding(Serilog.Formatting.Log4Net.LineEnding lineEnding) { }
        public void UseLog4JCompatibility() { }
        public Serilog.Formatting.Log4Net.Log4NetTextFormatterOptionsBuilder UseNoIndentation() { }
        public Serilog.Formatting.Log4Net.Log4NetTextFormatterOptionsBuilder UseNoXmlNamespace() { }
        public Serilog.Formatting.Log4Net.Log4NetTextFormatterOptionsBuilder UsePropertyFilter(Serilog.Formatting.Log4Net.PropertyFilter filterProperty) { }
    }
    public delegate bool PropertyFilter(Serilog.Events.LogEvent logEvent, string propertyName);
}