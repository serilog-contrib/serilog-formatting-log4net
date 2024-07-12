using System.Collections.Generic;
using System.Xml;

namespace Serilog.Formatting.Log4Net;

internal sealed class Log4JXmlNamespaceResolver : IXmlNamespaceResolver
{
    private const string Prefix = "log4j";
    private const string NamespaceName = "http://jakarta.apache.org/log4j/";

    public static readonly Log4JXmlNamespaceResolver Instance = new();

    /// <summary>
    /// The XML namespace used for Log4j events.
    /// </summary>
    /// <remarks>https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L137</remarks>
    public static readonly XmlQualifiedName Log4JXmlNamespace = new(Prefix, NamespaceName);

    private static readonly Dictionary<string, string> Namespaces = new() { [Prefix] = NamespaceName };

    IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope) => Namespaces;

    string? IXmlNamespaceResolver.LookupNamespace(string prefix) => prefix == Prefix ? NamespaceName : null;

    string? IXmlNamespaceResolver.LookupPrefix(string namespaceName) => namespaceName == NamespaceName ? Prefix : null;
}