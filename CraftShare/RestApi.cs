using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;

namespace CraftShare
{
    /// <summary>
    /// Handles all the communication with the server.
    /// TODO: Make the whole API async?
    /// </summary>
    public static class RestApi
    {
        private static RestClient _client;

        /// <summary>
        /// Applies a new host address to use for all upcoming request.
        /// </summary>
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

        /// <summary>
        /// Retrieves a list of shared craft. Use the arguments to apply paging.
        /// </summary>
        /// <param name="skip">Specifies the number elements to skip from the start of the list.</param>
        /// <param name="limit">Specifies the number of elements to request from the server. The server does not necessarily respond with the given number of items.</param>
        /// <returns>The elements returned from the server.</returns>
        public static List<SharedCraft> GetCraftList(int skip, int limit)
        {
            var request = CreateRequest("craft/", Method.GET);
            request.AddQueryParameter("skip", skip.ToString());
            request.AddQueryParameter("limit", limit.ToString());
            var response = _client.Execute<List<SharedCraft>>(request);
            ThrowOnError(response);
            return response.Data;
        }

        /// <summary>
        /// Retrieves the craft string for the given id.
        /// </summary>
        /// <param name="id">Specifies the id of a shared craft.</param>
        /// <returns>The craft string returned from the server.</returns>
        public static string GetCraft(string id)
        {
            var request = CreateRequest("craft/{id}", Method.GET);
            request.AddUrlSegment("id", id);
            var response = _client.Execute<SharedCraft>(request);
            ThrowOnError(response);
            return response.Data.craft;
        }

        /// <summary>
        /// Creates a new shared craft with the given data.
        /// </summary>
        /// <param name="craft">Spcifies the data to upload.</param>
        /// <returns>The newly created element with updated information returned from the server.</returns>
        public static SharedCraft CreateCraft(SharedCraft craft)
        {
            var request = CreateRequest("craft/", Method.POST);
            request.AddBody(craft);
            var response = _client.Execute<SharedCraft>(request);
            ThrowOnError(response);
            return response.Data;
        }

        /// <summary>
        /// Deletes the shared craft identified by the given id.
        /// </summary>
        /// <param name="id">Specifies the id of a shared craft.</param>
        /// <returns>True if the server responded with HTTP code 200 (OK) or 204 (NoContent), otherwise false.</returns>
        public static bool DeleteCraft(string id)
        {
            var request = CreateRequest("craft/" + id, Method.DELETE);
            var response = _client.Execute(request);
            ThrowOnError(response);
            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
        }
    }
}