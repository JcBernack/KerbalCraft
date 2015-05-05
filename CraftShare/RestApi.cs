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
        private static RestClient _client;

        public static void SetHostAddress(string host)
        {
            _client = new RestClient(string.Format("http://{0}/api/", host));
        }

        private static RestRequest CreateRequest(string ressource, Method method)
        {
            return new RestRequest(ressource, method)
            {
                Timeout = 2000,
                RequestFormat = DataFormat.Json,
                DateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ"
            };
        }

        public static List<SharedCraft> GetCraftList()
        {
            var request = CreateRequest("crafts/", Method.GET);
            return _client.Execute<List<SharedCraft>>(request).Data;
        }

        public static string GetCraft(string id)
        {
            var request = CreateRequest("craft/{id}", Method.GET);
            request.AddUrlSegment("id", id);
            return _client.Execute<SharedCraft>(request).Data.craft;
        }

        public static SharedCraft CreateCraft(SharedCraft craft)
        {
            var request = CreateRequest("craft/", Method.POST);
            request.AddBody(craft);
            return _client.Execute<SharedCraft>(request).Data;
        }

        public static bool DeleteCraft(string id)
        {
            var request = CreateRequest("craft/" + id, Method.DELETE);
            var status = _client.Execute(request).StatusCode;
            return status == HttpStatusCode.OK || status == HttpStatusCode.NoContent;
        }
    }
}