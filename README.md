**Serilog.Formatting.Log4Net** is an add-on for [Serilog](https://serilog.net/) to format log events as [log4net](https://logging.apache.org/log4net/) compatible XML format.

It is available on NuGet: [![NuGet Package](https://img.shields.io/nuget/v/Serilog.Formatting.Log4Net.svg)](https://www.nuget.org/packages/Serilog.Formatting.Log4Net/)

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

#### Exception formatting

By default, Log4NetTextFormatter formats exception by calling [ToString()](https://docs.microsoft.com/en-us/dotnet/api/system.exception.tostring). You can customise this behaviour by setting your own formatting delegate. For exemple, you could use [Ben.Demystifier](https://github.com/benaadams/Ben.Demystifier/) like this:

```c#
new Log4NetTextFormatter(c => c.UseExceptionFormatter(exception => exception.ToStringDemystified()))
```

#### CDATA

By default, Log4NetTextFormatter writes all messages and exceptions with a [CDATA](https://en.wikipedia.org/wiki/CDATA) section. It is possible to configure it to use CDATA sections only when the message or exception contain `&`, `<` or `>` by using `CDataMode.IfNeeded` or to never write CDATA sections by using `CDataMode.Never`:

```c#
new Log4NetTextFormatter(c => c.UseCDataMode(CDataMode.Never))
```

#### XML Namespace

You can remove the `log4net` XML namespace by setting the `Log4NetXmlNamespace` option to `null`. This is useful if you want to spare some bytes and your log reader supports log4net XML events without namespace, like [Log4View](https://www.log4view.com) does. You *could* also change the namespace to something else than the default `log4net:http://logging.apache.org/log4net/schemas/log4net-events-1.2/` but that would probably not be a good idea.

```c#
new Log4NetTextFormatter(c => c.UseLog4NetXmlNamespace(null))
```

#### Format provider

By default, Log4NetTextFormatter uses the invariant culture (Serilog's default) when formatting Serilog properties that implement the `IFormattable` interface. It can be configured to use culture-specific formatting information. For example, to use the Swiss French culture:

```c#
new Log4NetTextFormatter(c => c.UseFormatProvider(CultureInfo.GetCultureInfo("fr-CH")))
```

####  Property filter

By default, Log4NetTextFormatter serializes all Serilog properties. You can filter out some properties by configuring a a custom property filter delegate:

```c#
new Log4NetTextFormatter(c => c.UsePropertyFilter((_, name) => name != "MySecretProperty"))
```

### Combining options

You can also combine options, for example, both removing namespaces and using Ben.Demystifier for exception formatting:

```c#
new Log4NetTextFormatter(c => c
    .UseLog4NetXmlNamespace(null)
    .UseExceptionFormatter(exception => exception.ToStringDemystified())
);
```

## Enrichers

The log4Net XML format defines some special attributes which are not included by default in Serilog events. They can be added by using the appropriate Serilog enrichers.

#### Thread Id

Include the thread id in log4net events by using [Serilog.Enrichers.Thread](https://www.nuget.org/packages/Serilog.Enrichers.Thread/):

```c#
var loggerConfiguration = new LoggerConfiguration().Enrich.WithThreadId();
```

#### Domain and User Name

Include the domain and user name in log4net events by using [Serilog.Enrichers.Environment](https://www.nuget.org/packages/Serilog.Enrichers.Environment/):

```c#
var loggerConfiguration = new LoggerConfiguration().Enrich.WithEnvironmentUserName();
```

#### Machine Name

Include the machine name in log4net events by using [Serilog.Enrichers.Environment](https://www.nuget.org/packages/Serilog.Enrichers.Environment/):

```c#
var loggerConfiguration = new LoggerConfiguration().Enrich.WithMachineName();
```

