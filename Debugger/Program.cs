using System;
using SalesforceApiLib;

namespace Debugger
{
    class Program
    {
        static void Main(string[] args)
        {
            string client_id = "3MVG9Nk1FpUrSQHd6HOW6WiNA_KSUd3SQXlx_qV_2c7mKygbdtMyegIUohc_OdBsWy6O3DTIVQoK9M49h9Ah9";
            string client_secret = "C38C29A240EC5826B2C8EBFE2131F9DF54C01FD83D5C58C761CAD46330C2805C";
            string username = "adam.sawyers@resilient-wolf-b9s6tc.com";
            string password = "Hippomilk-00";
            string token = "Zk5c9wVErrUuWIfZ3qtubu56";

            string acctName = "Adams Adventures";
            //string pricebookName = "Adams Test Pricebook";
            string pricebookEntry = "Adams Test Product";

            DateTime today = DateTime.Now;
            String todayString = today.ToString("yyyy-MM-dd");
            Console.WriteLine(todayString);

            var client = new SalesforceApi();

            Console.WriteLine("Logging in");
            var loginResponse = client.Login(client_id, client_secret, username, password, token, true);

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

            //no longer needed
            //var queryPricebookResult = client.Query(instanceUrl, authToken, $"SELECT Name,Id FROM Pricebook2 WHERE Name='{pricebookName}'");
            //Console.WriteLine(queryPricebookResult);

            var queryPricebookEntryResult = client.Query(instanceUrl, authToken, $"SELECT Name,Id,Pricebook2.Name,Pricebook2.Id FROM PricebookEntry WHERE Name='{pricebookEntry}'");
            Console.WriteLine(queryPricebookEntryResult);

            //pull the ids out of each query, too lazy to do it here (also unecessary since KTA will do it)
            var acctId = "00102000008UmtiAAC";
            var pricebookId = "01s1N000007VuGbQAK";
            var pricebookEntryId = "01u020000017GBJAA2";

            //var addOrderResult = client.AddOrder(instanceUrl, authToken, "1", "5", DateTime.Now, "San Diego", acctId, pricebookId, pricebookEntryId);
            //Console.WriteLine(addOrderResult);

            //var newRecordId = client.ExtractId(addOrderResult);
            //Console.WriteLine(newRecordId);

            String filePath = "C:/Users/adam.sawyers/OneDrive - Kofax, Inc/Documents/Sample Images/Order 1.tif";
            String recordId = "8010200000064NM"; //can be an order, account, anything that accepts attachments
            String fileName = "Test TIF File.tif";
            //var attachFileResult = client.AttachFile(instanceUrl, authToken, filePath, recordId, fileName);
            //WriteLine(attachFileResult);

            //add case
            var addCaseResult = client.AddCase(instanceUrl, authToken, "High", "Its broken", "KTA is really broken", null, null, "Adam", "adam@adam.com", "555-555-5555", "Adams Adventures");
            Console.WriteLine(addCaseResult);

            var caseId = client.ExtractId(addCaseResult);
            Console.WriteLine(caseId);

            Console.ReadLine();            
        }
    }
}
