using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;
using log4net;
using Tridion;
using System.Configuration;

public partial class _Default : System.Web.UI.Page
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly Regex ValidExtensions = new Regex(".xls|.xlsx", RegexOptions.IgnoreCase);

    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void btnUpload_Click(object sender, EventArgs e)
    {
        // Check file has been selected
        if (!fileUpload.HasFile || string.IsNullOrEmpty(fileUpload.PostedFile.FileName))
        {
            badMessage.Text = "Please select a file to upload.";
            badMessage.Visible = true;
            return;
        }

        // Check file has valid extension
        var fileExt = Path.GetExtension(fileUpload.PostedFile.FileName);
        if (string.IsNullOrEmpty(fileExt) || !ValidExtensions.IsMatch(fileExt))
        {
            badMessage.Text = "The uploaded file has an invalid name or extension.";
            badMessage.Visible = true;
            return;
        }

        try
        {
            // Save upload to disk
            string virtualPath = "~\\Bin\\" + fileUpload.FileName;
            string absPath = Server.MapPath(virtualPath);
            Log.Info("save file to " + absPath);
            fileUpload.SaveAs(absPath);

            // Parse XLS
            Log.Debug("Start XLS read");
            var productPrices = ReadXlsFile(virtualPath);
            int count = productPrices.Count;
            Log.Info("End XLS read " + count);

            using (var client = new CoreService2010Client())
            {
                Log.Debug("Start processing pricing components");
                CreateComponents(client, productPrices);
                Log.Debug("End processing pricing components");
            }
        }
        catch (Exception ex)
        {
            Log.Error("PricingManagement.UploadFile: Error retrieving and setting XLS", ex);
            
            badMessage.Text = "An occurred while processing the product prices." + ex.StackTrace ;
            badMessage.Visible = true;
        }
    }

    /// <summary>
    /// Excel to List of VOProductPrice
    /// </summary>
    public List<ProductPrice> ReadXlsFile(string fileLocation)
    {
        Application app = null;
        Workbook book = null;
        Worksheet sheet = null;

        var result = new List<ProductPrice>();

        try
        {
            app = new ApplicationClass();
            app.DisplayAlerts = false;
            book = app.Workbooks.Open(Server.MapPath(fileLocation), Missing.Value, Missing.Value, Missing.Value,
                                      Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                                      Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                                      Missing.Value);

            sheet = (Worksheet)book.Sheets[1];

            var rowCount = sheet.UsedRange.Rows.Count;
            Log.Info("rowCount " + rowCount);
            for (var i = 2; i <= rowCount; i++)
            {
                var price = new ProductPrice();

                var priceId = (Range)sheet.Cells[i, 1];
                if (priceId == null || string.IsNullOrEmpty(priceId.Text.ToString())) continue;
                price.PricingId = priceId.Text.ToString();
                Marshal.FinalReleaseComObject(priceId);

                var productId = (Range)sheet.Cells[i, 3];
                if (productId == null || string.IsNullOrEmpty(productId.Text.ToString())) continue;
                //store the product ID from the 3rd party system temporarily in the TcmId field. this will be converted later
                price.Product.TcmId = productId.Text.ToString();
                Marshal.FinalReleaseComObject(productId);

                var threeRate = (Range)sheet.Cells[i, 4];
                if (threeRate == null || string.IsNullOrEmpty(threeRate.Text.ToString())) continue;
                price.ThreeMonthRate = Convert.ToDecimal(threeRate.Text.ToString());
                Marshal.FinalReleaseComObject(threeRate);

                var twelveRate = (Range)sheet.Cells[i, 7];
                if (twelveRate == null || string.IsNullOrEmpty(twelveRate.Text.ToString())) continue;
                price.TwelveMonthRate = Convert.ToDecimal(twelveRate.Text.ToString());
                Marshal.FinalReleaseComObject(twelveRate);

                result.Add(price);
            }
        }
        catch (Exception ex)
        {
            Log.Error("PricingManagement.ReadXlsFile: Error Reading XLS File", ex);
            return null;
        }
        finally
        {
            //all COM objects must be explicitly destroyed otherwise EXCEL.EXE stays open
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (sheet != null)
            {
                Marshal.FinalReleaseComObject(sheet);
            }
            if (book != null)
            {
                book.Close(Missing.Value, Missing.Value, Missing.Value);
                Marshal.FinalReleaseComObject(book);
            }
            if (app != null)
            {
                app.Quit();
                Marshal.FinalReleaseComObject(app);
            }
        }
        return result;
    }

    public void CreateComponents(CoreService2010Client client, List<ProductPrice> productPrices)
    {
        try
        {
            int count = 1;
            foreach (var productPrice in productPrices)
            {
                // Component name
                Log.Debug("Start Processing " + productPrice.PricingId);
                ComponentData component;

                // Create Component in the correct folder
                component = client.GetDefaultData(ItemType.Component, ConfigurationManager.AppSettings["PriceFolderTcmId"]) as ComponentData;
                
                //get the correct TcmId for the Product
                productPrice.Product.TcmId = ProductUtilites.GetProductTcmId(client, productPrice.Product.TcmId);

                //set the component information
                component.Title = productPrice.PricingId;
                component.Id = "tcm:0-0-0";

                //serialize the object to XML for Tridion
                component.Content = productPrice.Serialize();
                component.Schema.IdRef = ConfigurationManager.AppSettings["PriceSchemaTcmId"];

                try
                {
                    //create the component
                    client.Create(component, null);
                }
                catch (Exception ex)
                {
                    Log.Error("PricingManagement.CreateComponents: Unable to save new component", ex);
                }

                Log.Debug("Finished Processing " + productPrice.PricingId + ". Count = " + count);
                count++;
            }

            goodMessage.Text = "Processed " + productPrices.Count + " product prices";
            goodMessage.Visible = true;
        }
        catch (Exception ex)
        {
            Log.Error("PricingManagement.CreateComponents: Could not create components", ex);
            throw;
        }
    }

}