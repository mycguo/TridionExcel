using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Text;

[Serializable]
[XmlRoot(ElementName = "Content", Namespace = "uuid:24B83862-D136-42F0-A7C8-7A9F785489CF")]
public class ProductPrice
{
    private ComponentLink _product;

    public ProductPrice()
    {
        Product = new ComponentLink();
    }

    [XmlElement("product")]
    public ComponentLink Product
    {
        get { return _product ?? (_product = new ComponentLink()); }
        set { _product = value; }
    }

    [XmlIgnore]
    public string TcmId { get; set; }

    [XmlElement("pricing_id")]
    public string PricingId { get; set; }

    [XmlElement("setup_fee")]
    public decimal SetupFee { get; set; }

    [XmlElement("three_month_rate")]
    public decimal ThreeMonthRate { get; set; }

    [XmlElement("twelve_month_rate")]
    public decimal TwelveMonthRate { get; set; }

    public string Serialize()
    {
        XmlWriterSettings settings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true, Encoding = Encoding.ASCII };

        using (MemoryStream stream = new MemoryStream())
        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "uuid:24B83862-D136-42F0-A7C8-7A9F785489CF");

            XmlSerializer serializer = new XmlSerializer(typeof(ProductPrice));
            serializer.Serialize(writer, this, ns);

            string xml = Encoding.ASCII.GetString(stream.ToArray());
            return xml;
        }
    }

    public static ProductPrice Deserialize(string xml)
    {
        ProductPrice result;

        using (StringReader stringReader = new StringReader(xml))
        using (XmlReader xmlReader = XmlReader.Create(stringReader))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ProductPrice));
            result = (ProductPrice)serializer.Deserialize(xmlReader);
        }

        return result;
    }
}