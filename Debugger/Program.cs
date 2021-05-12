using System;
using SalesforceApiLib;

namespace Debugger
{
    class Program
    {
        static void Main(string[] args)
        {
            string client_id = "3MVG9Eroh42Z9.iVnYylfw221tNItLyCkYT9V5SqGISPVMy8cSl0FsYhj13Pieh1TiYPJB7m3L_GF9jLFN.jw";
            string client_secret = "BE94011CC2E7144E9B7461E5EE56AF7DD25C8DADA8260CD7615193540B68CCB5";
            string username = "adam.sawyers18@kofax.com.strategy";
            string password = "Hippomilk-00";
            string token = "";

            string acctName = "Adams Adventures";
            string pricebookName = "Adams Test Pricebook";
            string pricebookEntry = "Adams Test Product";

            var client = new SalesforceApi();

            Console.WriteLine("Logging in");
            var loginResponse = client.Login(client_id, client_secret, username, password, token, false);

            Console.WriteLine("Write client object AuthToken prop");
            Console.WriteLine(loginResponse[0]);
            string authToken = loginResponse[0];

            Console.WriteLine("Write client object InstanceUrl prop");
            Console.WriteLine(loginResponse[1]);
            string instanceUrl = loginResponse[1];

            Console.WriteLine("Query Salesforce Accounts");

            var queryAcctResult = client.Query(instanceUrl, authToken, $"SELECT Name,Id FROM Account WHERE Name='{acctName}'");
            Console.WriteLine(queryAcctResult);

            var accountId = client.ExtractId(queryAcctResult);
            Console.WriteLine(accountId);

            //var queryPricebookResult = client.Query(instanceUrl, authToken, $"SELECT Name,Id FROM Pricebook2 WHERE Name='{pricebookName}'");
            //Console.WriteLine(queryPricebookResult);

            //var queryPricebookEntryResult = client.Query(instanceUrl, authToken, $"SELECT Name,Id FROM PricebookEntry WHERE Name='{pricebookEntry}'");
            //Console.WriteLine(queryPricebookEntryResult);

            //pull the ids out of each query, too lazy to do it here (also unecessary since KTA will do it)
            var acctId = "00102000008UmtiAAC";
            var pricebookId = "01s02000000DahoAAC";
            var pricebookEntryId = "01u020000017GBOAA2";

            //var addOrderResult = client.AddOrder(instanceUrl, authToken, "1", "10", "2021-04-30", "San Diego", acctId, pricebookId, pricebookEntryId);
            //Console.WriteLine(addOrderResult);

            String filePath = "C:/Users/adam.sawyers/OneDrive - Kofax, Inc/Documents/Sample Images/Order 1.tif";
            String recordId = "80102000000614v"; //can be an order, account, anything that accepts attachments
            String fileName = "Test TIF File.tif";
            var attachFileResult = client.AttachFile(instanceUrl, authToken, filePath, recordId, fileName);
            Console.WriteLine(attachFileResult);

            Console.ReadLine();            
        }
    }
}
