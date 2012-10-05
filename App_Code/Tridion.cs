using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Tridion
/// http://cms.devjp.oic.fujitsu.com/webservices/CoreService.svc
/// </summary>
public class TestTridion
{

        static void Main()
        {
            log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            Tridion.CoreService2010Client client = new Tridion.CoreService2010Client();

            Log.Info("API Version:" + client.GetApiVersion());
            // Use the 'client' variable to call operations on the service.

            // Always close the client.
            client.Close();
        }

} 