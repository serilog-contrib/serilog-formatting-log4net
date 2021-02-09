**Serilog.Formatting.Log4Net** is an add-on for [Serilog](https://serilog.net/) to format log events as [log4net](https://logging.apache.org/log4net/) compatible XML format.

[![NuGet](https://img.shields.io/nuget/v/Serilog.Formatting.Log4Net.svg?label=NuGet&logo=NuGet)](https://www.nuget.org/packages/Serilog.Formatting.Log4Net/) [![Continuous Integration](https://img.shields.io/github/workflow/status/0xced/serilog-formatting-log4net/Continuous%20Integration?label=Continuous%20Integration&logo=GitHub)](https://github.com/0xced/serilog-formatting-log4net/actions?query=workflow%3A%22Continuous+Integration%22) [![Coverage](https://img.shields.io/codecov/c/github/0xced/serilog-formatting-log4net?label=Coverage&logo=Codecov&logoColor=f5f5f5)](https://codecov.io/gh/0xced/serilog-formatting-log4net)

You can use [Log4View](https://www.log4view.com) to look at log files produced with this formatter.

## Getting started

**Serilog.Formatting.Log4Net** provides the `Log4NetTextFormatter` class which implements Serilog's [ITextFormatter](https://github.com/serilog/serilog/blob/v2.0.0/src/Serilog/Formatting/ITextFormatter.cs#L20-L31) interface.

Here's how to use it with a file sink in a simple *Hello World* app:

```c#
using System;
using Serilog;
using Serilog.Formatting.Log4Net;

static class Program
{
    static void Main()
    {
        var logger = new LoggerConfiguration()
            .WriteTo.File(new Log4NetTextFormatter(), "logs.xml")
            .CreateLogger();

        logger.Information("Start app");
        Console.WriteLine("Hello World!");
        logger.Information("Stop app");
    }
}
```

Running this app writes the following XML events into the `logs.xml` file in the current working directory:

```xml
<log4net:event timestamp="2020-06-28T10:03:31.685165+02:00" level="INFO" xmlns:log4net="http://logging.apache.org/log4net/schemas/log4net-events-1.2/">
  <log4net:message><![CDATA[Start app]]></log4net:message>
</log4net:event>
<log4net:event timestamp="2020-06-28T10:03:31.705216+02:00" level="INFO" xmlns:log4net="http://logging.apache.org/log4net/schemas/log4net-events-1.2/">
  <log4net:message><![CDATA[Stop app]]></log4net:message>
</log4net:event>
```

## Configuration

You can configure `Log4NetTextFormatter` in multiple ways, the fluent options builder will help you discover all the possibilities.

### Exception formatting

By default, Log4NetTextFormatter formats exception by calling [ToString()](https://docs.microsoft.com/en-us/dotnet/api/system.exception.tostring). You can customise this behaviour by setting your own formatting delegate. For exemple, you could use [Ben.Demystifier](https://github.com/benaadams/Ben.Demystifier/) like this:

```c#
new Log4NetTextFormatter(c => c.UseExceptionFormatter(exception => exception.ToStringDemystified()))
```

### CDATA

By default, Log4NetTextFormatter writes all messages and exceptions with a [CDATA](https://en.wikipedia.org/wiki/CDATA) section. It is possible to configure it to use CDATA sections only when the message or exception contain `&`, `<` or `>` by using `CDataMode.IfNeeded` or to never write CDATA sections by using `CDataMode.Never`:

```c#
new Log4NetTextFormatter(c => c.UseCDataMode(CDataMode.Never))
```

### XML Namespace

You can remove the `log4net` XML namespace by setting the `Log4NetXmlNamespace` option to `null`. This is useful if you want to spare some bytes and your log reader supports log4net XML events without namespace, like [Log4View](https://www.log4view.com) does. You *could* also change the namespace to something else than the default `log4net:http://logging.apache.org/log4net/schemas/log4net-events-1.2/` but that would probably not be a good idea.

```c#
new Log4NetTextFormatter(c => c.UseLog4NetXmlNamespace(null))
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

By default, Log4NetTextFormatter serializes all Serilog properties. You can filter out some properties by configuring a a custom property filter delegate:

```c#
new Log4NetTextFormatter(c => c.UsePropertyFilter((_, name) => name != "MySecretProperty"))
```

### Log4j compatibility mode

The formatter also supports a log4j compatibility mode. Log4Net and Log4j XML formats are very similar but have a few key differences.

*   Events are in different XML namespaces
*   The `timestamp` is a number of milliseconds (log4j) vs an ISO 8061 formatted date (log4net)
*   Exception elements are named `throwable` vs `exception`

In order to enable the compatibility mode, call `UseLog4JCompatibility()`:

```c#
new Log4NetTextFormatter(c => c.UseLog4JCompatibility())
```

Note that unlike other fluent configuration methods, this one can not be chained because you should not change options after enabling the log4j compatibility mode.

### Combining options

You can also combine options, for example, both removing namespaces and using Ben.Demystifier for exception formatting:

```c#
var formatter = new Log4NetTextFormatter(c => c
    .UseLog4NetXmlNamespace(null)
    .UseExceptionFormatter(exception => exception.ToStringDemystified())
);
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

Combining these three enrichers wil produce a log event like this, including `thread`, `domain` and `username` attributes plus a `log4net:HostName` property containing the machine name:

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

## Creating a release

**Serilog.Formatting.Log4Net** uses [MinVer](https://github.com/adamralph/minver) for its versioning, so a tag must exist with the chosen semantic version number in order to create an official release.

1. Create an **[annotated](https://stackoverflow.com/questions/11514075/what-is-the-difference-between-an-annotated-and-unannotated-tag/25996877#25996877)** tag, the (multi-line) message of the annotated tag will be the content of the GitHub release. Markdown can be used.

    `git tag --annotate 1.0.0-rc.1`

2. [Push the tag](https://stackoverflow.com/questions/5195859/how-do-you-push-a-tag-to-a-remote-repository-using-git/26438076#26438076)

    `git push --follow-tags`

Once pushed, the GitHub [Continuous Integration](https://github.com/0xced/serilog-formatting-log4net/blob/main/.github/workflows/continuous-integration.yml) workflow takes care of building, running the tests, creating the NuGet package and publishing the produced NuGet package.

