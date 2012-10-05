using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

/// <summary>
/// Summary description for ComponentLink
/// </summary>
[Serializable]
public class ComponentLink
{
    [XmlNamespaceDeclarations]
    public XmlSerializerNamespaces Namespaces;

    public ComponentLink()
    {
        Namespaces = new XmlSerializerNamespaces();
        Namespaces.Add("xlink", "http://www.w3.org/1999/xlink");
    }

    [XmlAttribute("href", Namespace = "http://www.w3.org/1999/xlink")]
    public string TcmId { get; set; }

    [XmlText]
    public string Text { get; set; }

    [XmlAttribute("title", Namespace = "http://www.w3.org/1999/xlink")]
    public string Title { get; set; }

}