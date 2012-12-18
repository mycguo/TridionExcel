using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tridion;
using log4net;
using System.Reflection;
using System.Configuration;
using System.Xml.Linq;

/// <summary>
/// Summary description for ProductUtilites
/// </summary>
public static class ProductUtilites
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public static string GetProductTcmId(CoreService2010Client client, string productId)
    {
        string productTcmId = "";
        try
        {
            //get the XML list of component from the folder
            var productsXML = client.GetListXml(ConfigurationManager.AppSettings["ProductFolderTcmId"], 
                new OrganizationalItemItemsFilterData { ItemTypes = new[] { ItemType.Component } });

            //loop through each item and find out if it is the product we want
            foreach (var product in productsXML.Elements())
            {
                var productData = client.Read(product.Attribute("ID").Value, null) as ComponentData;
                var schemaFields = client.ReadSchemaFields(productData.Schema.IdRef, false, null);
                var content = XDocument.Parse(productData.Content);
                XNamespace ns = schemaFields.NamespaceUri;

                //check if the product id's match
                if (productId == content.Root.Element(ns + "product_id").Value)
                {
                    //return the TcmId
                    productTcmId = product.Attribute("ID").Value;
                    //exit the foreach
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("Error in GetProductTcmId()", ex);
        }
        return productTcmId;
    }
}