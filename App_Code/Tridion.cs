using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tridion;
using System.IO;
using System.Configuration;

/// <summary>
/// Summary description for Tridion
/// http://cms.devjp.oic.fujitsu.com/webservices/CoreService.svc
/// </summary>
///  
[TestClass()]
public class TestTridion
{

        [TestMethod()]
        static void TestUpload()
        {
            log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            Tridion.CoreService2010Client client = new Tridion.CoreService2010Client();

            Log.Info("API Version Test:" + client.GetApiVersion());
            // Use the 'client' variable to call operations on the service.
            //tcm:88-7204-2 for charles/test folder http://cms.devjp.oic.fujitsu.com/WebUI/Editors/CME/Views/Dashboard/Dashboard.aspx#locationId=tcm:88-7204-2
            UploadImages(@"D:\My Documents\My Pictures\sdl", "tcm:88-7204-2", client, Log);

            // Always close the client.
            client.Close();
        }

        //http://blog.building-blocks.com/uploading-images-using-the-core-service-in-sdl-tridion-2011
        public static void UploadImages(string location, string folderTcmId, CoreService2010Client client, log4net.ILog Log)
        {
            //create a reference to the directory of where the images are
            DirectoryInfo directory = new DirectoryInfo(location);
            //create global Tridion Read Options
            ReadOptions readOptions = new ReadOptions();
            //use Expanded so that Tridion exposes the TcmId of the newly created component
            readOptions.LoadFlags = LoadFlags.Expanded;
            try
            {
                //loop through the files
                foreach (FileInfo fileInfo in directory.GetFiles())
                {
                    //only allow images
                    if (IsAllowedFileType(fileInfo.Extension))
                    {
                        try
                        {
                            //create a new multimedia component in the folder specified
                            ComponentData multimediaComponent = (ComponentData)client.GetDefaultData(Tridion.ItemType.Component, folderTcmId);
                            multimediaComponent.Title = fileInfo.Name.ToLower();
                            multimediaComponent.ComponentType = ComponentType.Multimedia;
                            multimediaComponent.Schema.IdRef = ConfigurationManager.AppSettings["MultimediaSchemaId"];

                             //create a string to hold the temporary location of the image to use later
                            string tempLocation = "";

                            //use the StreamUpload2010Client to upload the image into Tridion
                            UploadResponse us = new UploadResponse();
                            using (Tridion.StreamUpload2010Client streamClient = new StreamUpload2010Client())
                            {
                                FileStream objfilestream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
                                tempLocation = streamClient.UploadBinaryContent(fileInfo.Name.ToLower(), objfilestream);
                            }

                            //creat a new binary component
                            BinaryContentData binaryContent = new BinaryContentData();
                            //set this temporary upload location to the source of this binary
                            binaryContent.UploadFromFile = tempLocation;
                            binaryContent.Filename = fileInfo.Name.ToLower();

                            //get the multimedia type id
                            binaryContent.MultimediaType = new LinkToMultimediaTypeData() { IdRef = GetMultimediaTypeId(fileInfo.Extension) };
                            
                            multimediaComponent.BinaryContent = binaryContent;

                            //save the image into a new object
                            IdentifiableObjectData savedComponent = client.Save(multimediaComponent, readOptions);

                            //check in using the Id of the new object
                            client.CheckIn(savedComponent.Id, null);
                        }
                        catch (Exception ex)
                        {
                            Log.Debug("Error creating image " + fileInfo.Name, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error processing images", ex);
            }
            finally
            { 
                //clean up temp objects
            }
        }

        private static string GetMultimediaTypeId(string fileExtension)
        {
            string tcmId = "";
            switch (fileExtension.TrimStart('.').ToLower())
            {
                case "jpg":
                case "jpeg":
                case "jpe":
                    tcmId = ConfigurationManager.AppSettings["JpegId"];
                    break;
                case "gif":
                    tcmId = ConfigurationManager.AppSettings["GifId"];
                    break;
                case "png":
                    tcmId = ConfigurationManager.AppSettings["PngId"];
                    break;
            }
            return tcmId;
        }

        private static bool IsAllowedFileType(string fileExtension)
        {
            bool allowed = false;
            switch (fileExtension.TrimStart('.').ToLower())
            {
                case "jpg":
                case "jpeg":
                case "jpe":
                case "gif":
                case "png":
                    allowed = true;
                    break;
            }
            return allowed;
        }
} 