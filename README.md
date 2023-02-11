**Serilog.Formatting.Log4Net** is an add-on for [Serilog](https://serilog.net/) to format log events as [log4net](https://logging.apache.org/log4net/) or [log4j](https://logging.apache.org/log4j/) compatible XML format.

[![NuGet](https://img.shields.io/nuget/v/Serilog.Formatting.Log4Net.svg?label=NuGet&logo=NuGet)](https://www.nuget.org/packages/Serilog.Formatting.Log4Net/) [![Continuous Integration](https://img.shields.io/github/actions/workflow/status/serilog-contrib/serilog-formatting-log4net/continuous-integration.yml?branch=main&label=Continuous%20Integration&logo=GitHub)](https://github.com/serilog-contrib/serilog-formatting-log4net/actions/workflows/continuous-integration.yml) [![Coverage](https://img.shields.io/codecov/c/github/serilog-contrib/serilog-formatting-log4net?label=Coverage&logo=Codecov&logoColor=f5f5f5)](https://codecov.io/gh/serilog-contrib/serilog-formatting-log4net) [![Code Quality](https://img.shields.io/codacy/grade/d53b6a7664504544bc6638da641bf979?label=Code%20Quality&logo=Codacy)](https://app.codacy.com/gh/serilog-contrib/serilog-formatting-log4net)

You can use [Log4View](https://www.log4view.com) to look at log files produced with this formatter.

## Getting started

Add the [Serilog.Formatting.Log4Net](https://www.nuget.org/packages/Serilog.Formatting.Log4Net/) NuGet package to your project using the NuGet Package Manager or run the following command:

```sh
dotnet add package Serilog.Formatting.Log4Net
```

**Serilog.Formatting.Log4Net** provides the `Log4NetTextFormatter` class which implements Serilog's [ITextFormatter](https://github.com/serilog/serilog/blob/v2.0.0/src/Serilog/Formatting/ITextFormatter.cs#L20-L31) interface.

Here's how to use it with a [file sink](https://www.nuget.org/packages/Serilog.Sinks.File/) in a simple *Hello World* app:

```c#
using System;
using Serilog;
using Serilog.Formatting.Log4Net;

static class Program
{
    static void Main(string[] args)
    {
        var logger = new LoggerConfiguration()
            .Enrich.WithProperty("AppName", "Program")
            .WriteTo.File(new Log4NetTextFormatter(c => c.UseCDataMode(CDataMode.Never)), "logs.xml")
            .CreateLogger();

        logger.Information("Start app with {Args}", args);
        Console.WriteLine("Hello World!");
        logger.Information("Stop app");
    }
}
```

Running this app writes the following XML events into the `logs.xml` file in the current working directory:

```xml
<log4net:event timestamp="2021-02-24T18:23:40.4496605+01:00" level="INFO" xmlns:log4net="http://logging.apache.org/log4net/schemas/log4net-events-1.2/">
  <log4net:properties>
    <log4net:data name="Args[0]" value="--first-argument" />
    <log4net:data name="Args[1]" value="--second-argument" />
    <log4net:data name="AppName" value="Program" />
  </log4net:properties>
  <log4net:message>Start app with ["--first-argument", "--second-argument"]</log4net:message>
</log4net:event>
<log4net:event timestamp="2021-02-24T18:23:40.5086666+01:00" level="INFO" xmlns:log4net="http://logging.apache.org/log4net/schemas/log4net-events-1.2/">
  <log4net:properties>
    <log4net:data name="AppName" value="Program" />
  </log4net:properties>
  <log4net:message>Stop app</log4net:message>
</log4net:event>
```

## Configuration

You can configure `Log4NetTextFormatter` in multiple ways, the fluent options builder will help you discover all the possibilities.

### Exception formatting

By default, Log4NetTextFormatter formats exception by calling [ToString()](https://docs.microsoft.com/en-us/dotnet/api/system.exception.tostring). You can customise this behaviour by setting your own formatting delegate. For example, you could use [Ben.Demystifier](https://github.com/benaadams/Ben.Demystifier/) like this:

```c#
new Log4NetTextFormatter(c => c.UseExceptionFormatter(exception => exception.ToStringDemystified()))
```

### CDATA

By default, Log4NetTextFormatter writes all messages and exceptions with a [CDATA](https://en.wikipedia.org/wiki/CDATA) section. It is possible to configure it to use CDATA sections only when the message or exception contain `&`, `<` or `>` by using `CDataMode.IfNeeded` or to never write CDATA sections by using `CDataMode.Never`:

```c#
new Log4NetTextFormatter(c => c.UseCDataMode(CDataMode.Never))
```

### XML Namespace

You can remove the `log4net` XML namespace by calling `UseNoXmlNamespace()` on the options builder. This is useful if you want to spare some bytes and your log reader supports log4net XML events without namespace, like [Log4View](https://www.log4view.com) does.

```c#
new Log4NetTextFormatter(c => c.UseNoXmlNamespace())
```

### Line ending

By default, Log4NetTextFormatter uses the line feed (LF) character for line ending between XML elements. You can choose to use CRLF if you need to:

```c#
new Log4NetTextFormatter(c => c.UseLineEnding(LineEnding.CarriageReturn | LineEnding.LineFeed))
```

### Indentation

By default, Log4NetTextFormatter indents XML elements with two spaces. You can configure it to use either spaces or tabs. For example, indent XML elements with one tab:

```c#
new Log4NetTextFormatter(c => c.UseIndentationSettings(new IndentationSettings(Indentation.Tab, 1)))
```

Or you can use no indentation at all, having log4net events written on a single line:

```c#
new Log4NetTextFormatter(c => c.UseNoIndentation())
```

### Format provider

By default, Log4NetTextFormatter uses the invariant culture (Serilog's default) when formatting Serilog properties that implement the `IFormattable` interface. It can be configured to use culture-specific formatting information. For example, to use the Swiss French culture:

```c#
new Log4NetTextFormatter(c => c.UseFormatProvider(CultureInfo.GetCultureInfo("fr-CH")))
```

### Property filter

By default, Log4NetTextFormatter serializes all Serilog properties. You can filter out some properties by configuring a custom property filter delegate:

```c#
new Log4NetTextFormatter(c => c.UsePropertyFilter((_, name) => name != "MySecretProperty"))
```

### Log4j compatibility mode

The formatter also supports a log4j compatibility mode. Log4Net and Log4j XML formats are very similar but have a few key differences.

*   The `event` elements are in different XML namespaces
*   The `timestamp` attribute is a number of milliseconds (log4j) vs an ISO 8061 formatted date (log4net)
*   Exception elements are named `throwable` vs `exception`

In order to enable the compatibility mode, call `UseLog4JCompatibility()`:

```c#
new Log4NetTextFormatter(c => c.UseLog4JCompatibility())
```

Note that unlike other fluent configuration methods, this one can not be chained because you should not change options after enabling the log4j compatibility mode.

Alternatively, you can use the `Log4NetTextFormatter.Log4JFormatter` static property which is configured for the log4j XML layout. This static property is also useful when using the [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration/) package where it can be used with the following accessor:

```
Serilog.Formatting.Log4Net.Log4NetTextFormatter::Log4JFormatter, Serilog.Formatting.Log4Net
```

### Combining options

You can also combine options, for example, using [Ben.Demystifier](https://www.nuget.org/packages/Ben.Demystifier/) for exception formatting, filtering properties and using the log4j compatibility mode. This sample configuration sends logs as UDP packages over the network with [Serilog.Sinks.Udp](https://www.nuget.org/packages/Serilog.Sinks.Udp/) and are viewable with [Loginator](https://github.com/dabeku/Loginator):

```c#
var appFileName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
var processId = Process.GetCurrentProcess().Id;
var formatter = new Log4NetTextFormatter(c => c
    .UseExceptionFormatter(exception => exception.ToStringDemystified())
    .UsePropertyFilter((_, name) => name.StartsWith("log4j"))
    .UseLog4JCompatibility()
);
Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("log4japp", $"{appFileName}({processId})")
    .Enrich.WithProperty("log4jmachinename", Environment.MachineName)
    .Enrich.WithThreadId()
    .WriteTo.Udp("localhost", 7071, AddressFamily.InterNetwork, formatter)
    .CreateLogger();
```

## Enrichers

The log4Net XML format defines some special attributes which are not included by default in Serilog events. They can be added by using the appropriate Serilog [enrichers](https://github.com/serilog/serilog/wiki/Enrichment).

### Thread Id

Include the thread id in log4net events by using [Serilog.Enrichers.Thread](https://www.nuget.org/packages/Serilog.Enrichers.Thread/):

```c#
var loggerConfiguration = new LoggerConfiguration().Enrich.WithThreadId();
```

### Domain and User Name

Include the domain and user name in log4net events by using [Serilog.Enrichers.Environment](https://www.nuget.org/packages/Serilog.Enrichers.Environment/):

```c#
var loggerConfiguration = new LoggerConfiguration().Enrich.WithEnvironmentUserName();
```

### Machine Name

Include the machine name in log4net events by using [Serilog.Enrichers.Environment](https://www.nuget.org/packages/Serilog.Enrichers.Environment/):

```c#
var loggerConfiguration = new LoggerConfiguration().Enrich.WithMachineName();
```

Combining these three enrichers will produce a log event including `thread`, `domain` and `username` attributes plus a `log4net:HostName` property containing the machine name:

```xml
<event timestamp="2020-06-28T10:07:33.314159+02:00" level="INFO" thread="1" domain="TheDomainName" username="TheUserName">
  <properties>
    <data name="log4net:HostName" value="TheMachineName" />
  </properties>
  <message>The message</message>
</event>
```

## Related projects

The [Serilog.Sinks.Log4Net](https://github.com/serilog/serilog-sinks-log4net) project is similar but depends on the log4net NuGet package whereas Serilog.Formatting.Log4Net does not. Also, Serilog.Sinks.Log4Net is a sink so you have to configure log4net in addition to configuring Serilog.

The [Serilog.Sinks.Udp](https://github.com/FantasticFiasco/serilog-sinks-udp) project also provides a [Log4Net formatter](https://github.com/FantasticFiasco/serilog-sinks-udp/blob/v7.1.0/src/Serilog.Sinks.Udp/Sinks/Udp/TextFormatters/Log4netTextFormatter.cs) but it writes XML *manually* (without using an [XmlWriter](https://docs.microsoft.com/en-us/dotnet/api/System.Xml.XmlWriter)), completely ignores Serilog properties, is not configurable at all (indentation, newlines, namespaces etc.) and is not documented.
