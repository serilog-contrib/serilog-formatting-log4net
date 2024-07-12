using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using FluentAssertions;
using Xunit;

namespace Serilog.Formatting.Log4Net.Tests;

public class Log4JXmlNamespaceResolverTest
{
    private readonly IXmlNamespaceResolver _resolver;

    public Log4JXmlNamespaceResolverTest()
    {
        var log4JXmlNamespaceResolver = typeof(Log4NetTextFormatter).Assembly.GetType("Serilog.Formatting.Log4Net.Log4JXmlNamespaceResolver")
            ?? throw new MissingMemberException("Serilog.Formatting.Log4Net.Log4JXmlNamespaceResolver");
        var instance = log4JXmlNamespaceResolver.GetField("Instance", BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingFieldException("Serilog.Formatting.Log4Net.Log4JXmlNamespaceResolver", "Instance");
        _resolver = (IXmlNamespaceResolver)instance.GetValue(log4JXmlNamespaceResolver)!;
    }

    [Theory]
    [InlineData(XmlNamespaceScope.All)]
    [InlineData(XmlNamespaceScope.ExcludeXml)]
    [InlineData(XmlNamespaceScope.Local)]
    public void GetNamespacesInScope(XmlNamespaceScope scope)
    {
        var expected = new Dictionary<string, string> { ["log4j"] = "http://jakarta.apache.org/log4j/" };
        _resolver.GetNamespacesInScope(scope).Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void LookupLog4JNamespace()
    {
        _resolver.LookupNamespace("log4j").Should().Be("http://jakarta.apache.org/log4j/");
    }

    [Fact]
    public void LookupLog4JPrefix()
    {
        _resolver.LookupPrefix("http://jakarta.apache.org/log4j/").Should().Be("log4j");
    }

    [Theory]
    [InlineData("xmlns")]
    [InlineData("xml")]
    public void LookupStandardXmlNamespace(string prefix)
    {
        _resolver.LookupNamespace(prefix).Should().BeNull();
    }

    [Theory]
    [InlineData("http://www.w3.org/2000/xmlns/")]
    [InlineData("http://www.w3.org/XML/1998/namespace")]
    public void LookupStandardXmlPrefix(string namespaceName)
    {
        _resolver.LookupPrefix(namespaceName).Should().BeNull();
    }
}