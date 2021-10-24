# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

* Replace `UseLog4NetXmlNamespace(null)` with `UseNoXmlNamespace()`
* Reduce the public API surface
  * Removed all property getters on `Log4NetTextFormatterOptionsBuilder`
  * Converted the `LineEndingExtensions` class from public to internal
* Improve log4j compatibility mode: don't write the `xmlns:log4j` attribute to be [exactly compatible](https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L137-L145) with log4j

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


## [1.0.0-rc.2] - 2021-03-25

* Handle non Serilog provided `LogEventPropertyValue` subclasses
* The `Log4NetTextFormatterOptionsBuilder` constructor is now internal
* Include the index in the property name when formatting a SequenceValue

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

* Documentation has been improved

## [1.0.0-rc.1] - 2021-02-05

This release contains the same code as 1.0.0-alpha.0.110.

Still trying to figure out how to make everything fit together with [MinVer](https://github.com/adamralph/minver), **annotated** tags and GitHub actions.

## [1.0.0-alpha.0.110] - 2021-02-04

* Implement log4j compatibility mode.

[Unreleased]: https://github.com/serilog-contrib/serilog-formatting-log4net/compare/1.0.0-rc.2...HEAD
[1.0.0-rc.2]: https://github.com/serilog-contrib/serilog-formatting-log4net/compare/1.0.0-rc.1...1.0.0-rc.2
[1.0.0-rc.1]: https://github.com/serilog-contrib/serilog-formatting-log4net/compare/1.0.0-alpha.0.110...1.0.0-rc.1
[1.0.0-alpha.0.110]: https://github.com/serilog-contrib/serilog-formatting-log4net/releases/tag/1.0.0-alpha.0.110

