using System.Collections.Generic;
using System.Net;
using RestSharp;

namespace CraftShare
{
    /// <summary>
    /// TODO: Make the whole API async?
    /// </summary>
    public static class RestApi
    {
        public static string HostAddress = "localhost:8000";
        private static readonly RestClient Client;

        static RestApi()
        {
            Client = new RestClient(string.Format("http://{0}/api/", HostAddress));
        }

        private static RestRequest CreateRequest(string ressource, Method method)
        {
            return new RestRequest(ressource, method)
            {
                Timeout = 2000,
                RequestFormat = DataFormat.Json
            };
        }

        public static List<SharedCraft> GetCraftList()
        {
            var request = CreateRequest("crafts/", Method.GET);
            return Client.Execute<List<SharedCraft>>(request).Data;
        }

        public static string GetCraft(string id)
        {
            var request = CreateRequest("craft/{id}", Method.GET);
            request.AddUrlSegment("id", id);
            return Client.Execute<SharedCraft>(request).Data.craft;
        }

        public static SharedCraft CreateCraft(SharedCraft craft)
        {
            var request = CreateRequest("craft/", Method.POST);
            request.AddBody(craft);
            return Client.Execute<SharedCraft>(request).Data;
        }

        public static bool DeleteCraft(string id)
        {
            var request = CreateRequest("craft/" + id, Method.DELETE);
            var status = Client.Execute(request).StatusCode;
            return status == HttpStatusCode.OK || status == HttpStatusCode.NoContent;
        }
    }
}