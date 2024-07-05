# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased][Unreleased]

- Add support for .NET 8 and mark `Serilog.Formatting.Log4Net` as [trimmable](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained) for [AOT compatibility](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

## [1.1.0][1.1.0] - 2023-05-02

- Add support for caller information (class, method, file, line) through the [Serilog.Enrichers.WithCaller](https://www.nuget.org/packages/Serilog.Enrichers.WithCaller/) package.

## [1.0.2][1.0.2] - 2023-02-11

- Add a new `Log4NetTextFormatter.Log4JFormatter` static property which is configured for the log4j XML layout. This static property is also useful when using the [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration/) package where it can be used with the following accessor:

```text
Serilog.Formatting.Log4Net.Log4NetTextFormatter::Log4JFormatter, Serilog.Formatting.Log4Net
```

## [1.0.1][1.0.1] - 2023-01-17

- Add support for .NET 6. It will be beneficial for [string interpolation](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-6/#arrays-strings-spans) which is used extensively through the project. See [Are there benefits in producing a .NET 6.0 version of a .NET Standard 2.0 library?](https://stackoverflow.com/questions/70778797/are-there-benefits-in-producing-a-net-6-0-version-of-a-net-standard-2-0-librar/72266562#72266562) on Stack Overflow.

## [1.0.0][1.0.0] - 2022-03-08

- First final version (i.e. non pre-release) which is identical to 1.0.0-rc.4

## [1.0.0-rc.4][1.0.0-rc.4] - 2021-11-02

- Add README and release notes (link to https://github.com/serilog-contrib/serilog-formatting-log4net/blob/main/CHANGELOG.md) in the NuGet package

## [1.0.0-rc.3][1.0.0-rc.3] - 2021-10-25

- Replace `UseLog4NetXmlNamespace(null)` with `UseNoXmlNamespace()`
- Reduce the public API surface
  - Removed all property getters on `Log4NetTextFormatterOptionsBuilder`
  - Converted the `LineEndingExtensions` class from public to internal
- Improve log4j compatibility mode: don't write the `xmlns:log4j` attribute to be [exactly compatible](https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L137-L145) with log4j

Before (1.0.0-rc.2):

```xml
<log4j:event timestamp="1041689366535" level="INFO" xmlns:log4j="http://jakarta.apache.org/log4j/"> 
  <log4j:message><![CDATA[Hello from Serilog]]></log4j:message> 
</log4j:event> 
```

After (1.0.0-rc.3)

```xml
<log4j:event timestamp="1041689366535" level="INFO"> 
  <log4j:message><![CDATA[Hello from Serilog]]></log4j:message> 
</log4j:event> 
```

## [1.0.0-rc.2][1.0.0-rc.2] - 2021-03-25

- Handle non Serilog provided `LogEventPropertyValue` subclasses
- The `Log4NetTextFormatterOptionsBuilder` constructor is now internal
- Include the index in the property name when formatting a SequenceValue

Before (1.0.0-rc.1):

```xml
<log4net:data name="Args" value="--first-argument" />
<log4net:data name="Args" value="--second-argument" />
```

After (1.0.0-rc.2)

```xml
<log4net:data name="Args[0]" value="--first-argument" />
<log4net:data name="Args[1]" value="--second-argument" />
```

- Documentation has been improved

## [1.0.0-rc.1][1.0.0-rc.1] - 2021-02-05

This release contains the same code as 1.0.0-alpha.0.110.

Still trying to figure out how to make everything fit together with [MinVer](https://github.com/adamralph/minver), **annotated** tags and GitHub actions.

## [1.0.0-alpha.0.110][1.0.0-alpha.0.110] - 2021-02-04

- Implement log4j compatibility mode.

[Unreleased]: https://github.com/serilog-contrib/serilog-formatting-log4net/compare/1.1.0...HEAD
[1.1.0]: https://github.com/serilog-contrib/serilog-formatting-log4net/compare/1.0.2...1.1.0
[1.0.2]: https://github.com/serilog-contrib/serilog-formatting-log4net/compare/1.0.1...1.0.2
[1.0.1]: https://github.com/serilog-contrib/serilog-formatting-log4net/compare/1.0.0...1.0.1
[1.0.0]: https://github.com/serilog-contrib/serilog-formatting-log4net/compare/1.0.0-rc.4...1.0.0
[1.0.0-rc.4]: https://github.com/serilog-contrib/serilog-formatting-log4net/compare/1.0.0-rc.3...1.0.0-rc.4
[1.0.0-rc.3]: https://github.com/serilog-contrib/serilog-formatting-log4net/compare/1.0.0-rc.2...1.0.0-rc.3
[1.0.0-rc.2]: https://github.com/serilog-contrib/serilog-formatting-log4net/compare/1.0.0-rc.1...1.0.0-rc.2
[1.0.0-rc.1]: https://github.com/serilog-contrib/serilog-formatting-log4net/compare/1.0.0-alpha.0.110...1.0.0-rc.1
[1.0.0-alpha.0.110]: https://github.com/serilog-contrib/serilog-formatting-log4net/releases/tag/1.0.0-alpha.0.110
