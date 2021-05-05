using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

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

        public string AddOrder(string instanceUrl, string authToken, string quantity, string unitPrice, string effectiveDate, string billingCity, string accountId, string pricebookId, string pricebookEntryId)
        {
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

            //create order item record object
            //var requestBodyOrderItemRecord = new OrderItemRecords
            //{
            //    Attributes = requestBodyOrderItemAttributes,
            //    PricebookEntryId = pricebookEntryId,
            //    Quantity = "1",
            //    UnitPrice = "10"
            //};

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

            //create order
            //var requestBodyOrder = new Order
            //{
            //    Attributes = requestBodyOrderAttributes,
            //    EffectiveDate = "2021-04-07",
            //    Status = "Draft",
            //    BillingCity = "San Diego",
            //    AccountId = accountId,
            //    Pricebook2Id = pricebookId,
            //    OrderItems = requestBodyOrderItems
            //};

            //create array of orders

            //create array of orders,
            var requestBodyOrderArray = new List<Order>
            {
                new Order {
                attributes = requestBodyOrderAttributes,
                EffectiveDate = effectiveDate,
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

            //String jsonResponse;
            //using (var client = new HttpClient())
            //{

            //    var request = new FormUrlEncodedContent(new Dictionary<string, string>
            //    {
            //        { "order", requestBodyStr }
            //    });
            //    request.Headers.Add("Authorization", "Bearer " + authToken);
            //    request.Headers.Add("X-PrettyPrint", "1");
            //    var response = client.PostAsync(instanceUrl + API_ENDPOINT, request).Result;
            //    jsonResponse = response.Content.ReadAsStringAsync().Result;
            //}

            Console.WriteLine(instanceUrl + API_ENDPOINT + "commerce/sale/order");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, instanceUrl + API_ENDPOINT + "commerce/sale/order");
            request.Headers.Add("Authorization", "Bearer " + authToken);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(requestBodyStr, Encoding.UTF8, "application/json");
            HttpClient postClient = new HttpClient();

            var result = postClient.SendAsync(request).Result;
            var jsonResponse = result.Content.ReadAsStringAsync().Result;

            //var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
            return jsonResponse;
        }

        public string ExtractId(string jsonResponse)
        {
            QueryResponse response = JsonConvert.DeserializeObject<QueryResponse>(jsonResponse);
            return response.records[0].Id;
        }
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

    //creating class for query responses
    public class QueryResponse
    {
        public int totalSize { get; set; }
        public bool done { get; set; }
        public List<ReturnRecords> records { get; set; }
    }

    public class ReturnRecords
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }

    //public string Describe(string instanceUrl, string apiEndpoint, string authToken, string sObject)
    //{
    //    using (var client = new HttpClient())
    //    {
    //        string restQuery = instanceUrl + API_ENDPOINT + "sobjects/" + sObject;
    //        var request = new HttpRequestMessage(HttpMethod.Get, restQuery);
    //        request.Headers.Add("Authorization", "Bearer " + authToken);
    //        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //        request.Headers.Add("X-PrettyPrint", "1");
    //        var response = client.SendAsync(request).Result;
    //        return response.Content.ReadAsStringAsync().Result;
    //    }
    //}

    //public string QueryEndpoints(string instanceUrl, string apiEndpoint, string authToken)
    //{
    //    using (var client = new HttpClient())
    //    {
    //        string restQuery = instanceUrl + API_ENDPOINT;
    //        var request = new HttpRequestMessage(HttpMethod.Get, restQuery);
    //        request.Headers.Add("Authorization", "Bearer " + authToken);
    //        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //        request.Headers.Add("X-PrettyPrint", "1");
    //        var response = client.SendAsync(request).Result;
    //        return response.Content.ReadAsStringAsync().Result;
    //    }
    //}
}