using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

using TotalAgility.Sdk;

namespace SalesforceApiLib
{
    public class SalesforceApi
    {
        private const string TEST_LOGIN_ENDPOINT = "https://test.salesforce.com/services/oauth2/token";
        private const string PROD_LOGIN_ENDPOINT = "https://login.salesforce.com/services/oauth2/token";
        private const string API_ENDPOINT = "/services/data/v51.0/";

        static SalesforceApi()
        {
            // SF requires TLS 1.1 or 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
        }

        // TODO: use RestSharps
        public string[] Login(string clientId, string clientSecret, string username, string password, string token, bool prodInstance)
        {
            String jsonResponse;
            using (var client = new HttpClient())
            {
                var request = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        {"grant_type", "password"},
                        {"client_id", clientId},
                        {"client_secret", clientSecret},
                        {"username", username},
                        {"password", password + token}
                    }
                );

                request.Headers.Add("X-PrettyPrint", "1");

                //added if statement to handle test/prod
                var LOGIN_ENDPOINT = TEST_LOGIN_ENDPOINT;
                if (prodInstance == true)
                {
                    LOGIN_ENDPOINT = PROD_LOGIN_ENDPOINT;
                }

                var response = client.PostAsync(LOGIN_ENDPOINT, request).Result;
                jsonResponse = response.Content.ReadAsStringAsync().Result;
            }
            Console.WriteLine($"Response: {jsonResponse}");
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
            string[] loginInfoArr = { values["access_token"], values["instance_url"] };
            return loginInfoArr;
        }

        public string Query(string instanceUrl, string authToken, string soqlQuery)
        {
            using (var client = new HttpClient())
            {
                string restRequest = instanceUrl + API_ENDPOINT + "query/?q=" + soqlQuery;
                var request = new HttpRequestMessage(HttpMethod.Get, restRequest);
                request.Headers.Add("Authorization", "Bearer " + authToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("X-PrettyPrint", "1");
                var response = client.SendAsync(request).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        public string AddOrder(string instanceUrl, string authToken, string quantity, string unitPrice, DateTime effectiveDate, string billingCity, string accountId, string pricebookId, string pricebookEntryId)
        {
            //convert date object to formatted string
            String effectiveDateString = effectiveDate.ToString("yyyy-MM-dd");

            //create attributes object for the order
            var requestBodyOrderAttributes = new Attributes
            {
                type = "Order"
            };

            //create attributes object for order item
            var requestBodyOrderItemAttributes = new Attributes
            {
                type = "OrderItem"
            };

            //create records array of order items
            var requestBodyOrderRecord = new List<OrderItemRecords>
            {
                new OrderItemRecords {
                attributes = requestBodyOrderItemAttributes,
                PricebookEntryId = pricebookEntryId,
                quantity = quantity,
                UnitPrice = unitPrice
                },
            };

            //create order items from that array of order items
            var requestBodyOrderItems = new OrderItems
            {
                records = requestBodyOrderRecord
            };

            //create array of orders,
            var requestBodyOrderArray = new List<Order>
            {
                new Order {
                attributes = requestBodyOrderAttributes,
                EffectiveDate = effectiveDateString,
                Status = "Draft",
                billingCity = billingCity,
                accountId = accountId,
                Pricebook2Id = pricebookId,
                OrderItems = requestBodyOrderItems
                },
            };

            //add the order, then serialize
            var requestBody = new OrderArray
            {
                order = requestBodyOrderArray,
            };
            Console.WriteLine("Created request body");
            Console.WriteLine("Serializing request body");
            var requestBodyStr = JsonConvert.SerializeObject(requestBody);
            Console.WriteLine(requestBodyStr);

            Console.WriteLine(instanceUrl + API_ENDPOINT + "commerce/sale/order");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, instanceUrl + API_ENDPOINT + "commerce/sale/order");
            request.Headers.Add("Authorization", "Bearer " + authToken);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(requestBodyStr, Encoding.UTF8, "application/json");
            HttpClient postClient = new HttpClient();

            var result = postClient.SendAsync(request).Result;
            var jsonResponse = result.Content.ReadAsStringAsync().Result;

            return jsonResponse;
        }

        public string ExtractId(string jsonResponse)
        {
            FlatQueryResponse response = JsonConvert.DeserializeObject<FlatQueryResponse>(jsonResponse);
            return response.records[0].Id;
        }

        public string[] ExtractPricebookIds(string jsonResponse)
        {
            PricebookQueryResponse response = JsonConvert.DeserializeObject<PricebookQueryResponse>(jsonResponse);
            string[] pricebookIdArr = { response.records[0].Id, response.records[0].Pricebook2.Id };
            return pricebookIdArr;
        }

        public string AttachFile(string instanceUrl, string authToken, string sessionId, string docId, string docFileType, string recordId, string docName)
        {
            CaptureDocumentService cds = new CaptureDocumentService();
            string[] docFileTypeArr = docFileType.Split('/');
            string docFileTypeFormatted = docFileTypeArr[1];
            Stream docStream = cds.GetDocumentFile(sessionId, null, docId, docFileTypeFormatted);
            byte[] docStreamBytes = new byte[docStream.Length];
            docStream.Read(docStreamBytes, 0, docStreamBytes.Length);
            String docBase64 = Convert.ToBase64String(docStreamBytes);

            StringBuilder jsonData = new StringBuilder("{");
            jsonData.Append("\"Name\" : \"" + docName + "\",");
            jsonData.Append("\"Body\" : \"" + docBase64 + "\",");
            jsonData.Append("\"parentId\" : \"" + recordId + "\"");
            jsonData.Append("}");

            HttpContent addAttachmentBody = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");

            HttpClient apiCallClient = new HttpClient();
            String restCallUrl = instanceUrl + API_ENDPOINT + "sobjects/attachment/";

            HttpRequestMessage apiRequest = new HttpRequestMessage(HttpMethod.Post, restCallUrl);
            apiRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            apiRequest.Headers.Add("Authorization", "Bearer " + authToken /*Your OAuth token*/);
            apiRequest.Content = addAttachmentBody;

            HttpResponseMessage apiCallResponse = apiCallClient.SendAsync(apiRequest).Result;

            String requestResponse = apiCallResponse.Content.ReadAsStringAsync().Result;
            return requestResponse;
        }

        public string AddCase(string instanceUrl, string authToken, string priority, string subject, string description, string contactId, string accountId, string suppliedName, string suppliedEmail, string suppliedPhone, string suppliedCompany)
        {
            var requestBody = new Dictionary<string, string>
                {
                    {"Status", "New"},
                    {"Origin", "Web"},
                    {"Priority", priority},
                    {"Subject", subject},
                    {"Description", description},
                    {"ContactId", contactId},
                    {"AccountId", accountId},
                    {"SuppliedName", suppliedName},
                    {"SuppliedEmail", suppliedEmail},
                    {"SuppliedPhone", suppliedPhone},
                    {"SuppliedCompany", suppliedCompany},
                    {"Comments", null},
                    {"Type", null},
                    {"Reason", null}
                };

            var requestBodyStr = JsonConvert.SerializeObject(requestBody);
            Console.WriteLine(requestBodyStr);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, instanceUrl + API_ENDPOINT + "sobjects/case");
            request.Headers.Add("Authorization", "Bearer " + authToken);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(requestBodyStr, Encoding.UTF8, "application/json");
            HttpClient postClient = new HttpClient();

            var result = postClient.SendAsync(request).Result;
            var jsonResponse = result.Content.ReadAsStringAsync().Result;

            return jsonResponse;
        }

        //creating classes for the payload
        public class OrderArray
        {
            public List<Order> order { get; set; }
        }

        public class Order
        {
            public Attributes attributes { get; set; }
            //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string EffectiveDate { get; set; }
            //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string Status { get; set; }
            //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string billingCity { get; set; }
            //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string accountId { get; set; }
            //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string Pricebook2Id { get; set; }
            //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public OrderItems OrderItems { get; set; }
            //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        }

        public class Attributes
        {
            public string type { get; set; }
        }

        public class OrderItems
        {
            public List<OrderItemRecords> records { get; set; }
        }

        public class OrderItemRecords
        {
            public Attributes attributes { get; set; }
            public string PricebookEntryId { get; set; }
            public string quantity { get; set; }
            public string UnitPrice { get; set; }
        }

        //creating class for flat query responses
        public class FlatQueryResponse
        {
            public int totalSize { get; set; }
            public bool done { get; set; }
            public List<ReturnRecords> records { get; set; }
        }

        //creating class for complex query responses (pricebooks)
        public class PricebookQueryResponse
        {
            public int totalSize { get; set; }
            public bool done { get; set; }
            public List<ReturnRecordsWithPricebook> records { get; set; }
        }

        //used only for flat responses
        public class ReturnRecords
        {
            public string Name { get; set; }
            public string Id { get; set; }
        }


        //used for response with pricebook info
        public class ReturnRecordsWithPricebook
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public Pricebook2Record Pricebook2 { get; set; }
        }

        //class for pricebook2 info
        public class Pricebook2Record
        {
            public string Name { get; set; }
            public string Id { get; set; }
        }
    }
}