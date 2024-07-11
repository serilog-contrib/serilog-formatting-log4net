using System.IO;
using System.Xml;

namespace Serilog.Formatting.Log4Net;

// Inspired by https://www.hanselman.com/blog/xmlfragmentwriter-omiting-the-xml-declaration-and-the-xsd-and-xsi-namespaces but does not actually work since the xmlns stack can't be manipulated that easily.
internal sealed class NoNamespaceXmlWriter : XmlTextWriter
{
    private readonly XmlQualifiedName _ns;
    private bool _skipAttribute;

    // log4j writes the XML "manually", see https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L137-L145
    // The resulting XML is impossible to write with a standard compliant XML writer such as XmlWriter.
    // That's why we write the event in a StringWriter then massage the output to remove the xmlns:log4j attribute to match log4j output.
    // The XML fragment becomes valid when surrounded by an external entity, see https://github.com/apache/log4j/blob/v1_2_17/src/main/java/org/apache/log4j/xml/XMLLayout.java#L31-L49
    public NoNamespaceXmlWriter(TextWriter output, XmlQualifiedName ns) : base(output)
    {
        _ns = ns;
    }

    public override void WriteEndAttribute()
    {
        if (_skipAttribute)
        {
            _skipAttribute = false;
        }
        else
        {
            base.WriteEndAttribute();
        }
    }

    public override void WriteStartAttribute(string? prefix, string localName, string? ns)
    {
        // Actually that's not how writing XML namespaces work...
        if (prefix == "xmlns" && localName == _ns.Name)
        {
            _skipAttribute = true;
        }
        else
        {
            base.WriteStartAttribute(prefix, localName, ns);
        }
    }

    public override void WriteString(string? text)
    {
        if (!_skipAttribute)
            base.WriteString(text);
    }
}