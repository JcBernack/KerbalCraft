using System;
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

        private static void ThrowOnError(IRestResponse response)
        {
            if (response.ErrorException != null)
            {
                throw new Exception("Request failed", response.ErrorException);
            }
        }

        public static List<SharedCraft> GetCraftList(int skip, int limit)
        {
            var request = CreateRequest("craft/", Method.GET);
            request.AddQueryParameter("skip", skip.ToString());
            request.AddQueryParameter("limit", limit.ToString());
            var response = _client.Execute<List<SharedCraft>>(request);
            ThrowOnError(response);
            return response.Data;
        }

        public static string GetCraft(string id)
        {
            var request = CreateRequest("craft/{id}", Method.GET);
            request.AddUrlSegment("id", id);
            var response = _client.Execute<SharedCraft>(request);
            ThrowOnError(response);
            return response.Data.craft;
        }

        public static SharedCraft CreateCraft(SharedCraft craft)
        {
            var request = CreateRequest("craft/", Method.POST);
            request.AddBody(craft);
            var response = _client.Execute<SharedCraft>(request);
            ThrowOnError(response);
            return response.Data;
        }

        public static bool DeleteCraft(string id)
        {
            var request = CreateRequest("craft/" + id, Method.DELETE);
            var response = _client.Execute(request);
            ThrowOnError(response);
            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
        }
    }
}