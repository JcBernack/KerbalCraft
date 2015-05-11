using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;

namespace CraftShare
{
    /// <summary>
    /// Handles all the communication with the server.
    /// </summary>
    public static class RestApi
    {
        /// <summary>
        /// Holds all pending callbacks.
        /// </summary>
        private static readonly List<Action> Callbacks = new List<Action>();

        private static RestClient _client;

        static RestApi()
        {
            //TODO: get a proper certificate and stop preventing the validity check
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        /// <summary>
        /// Applies a new host address to use for all upcoming request.
        /// </summary>
        public static void SetHostAddress(string host)
        {
            _client = new RestClient(string.Format("https://{0}/api/", host));
        }

        private static RestRequest CreateRequest(string resource, Method method)
        {
            return new RestRequest(resource, method)
            {
                Timeout = 5000,
                RequestFormat = DataFormat.Json,
                DateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ"
            };
        }

        /// <summary>
        /// Calls all pending callbacks from finished requests.
        /// </summary>
        public static void HandleResponses()
        {
            // Invoke all callbacks in a FIFO manner, but let exceptions bubble up
            // as there should not be any and we want to know otherwise.
            while (Callbacks.Count > 0)
            {
                try
                {
                    Callbacks[0].Invoke();
                }
                finally
                {
                    // Make sure the callback is removed, even if an exception occured.
                    // Otherwise it might be invoked over and over again as HandleResponses is most likely called from OnGUI.
                    Callbacks.RemoveAt(0);
                }
            }
        }

        private static RestRequestAsyncHandle Execute<T>(IRestRequest request, Action<IRestResponse<T>> callback)
            where T : new()
        {
            return _client.ExecuteAsync<T>(request, _ => Callbacks.Add(() => callback(_)));
        }

        private static RestRequestAsyncHandle Execute(IRestRequest request, Action<IRestResponse> callback)
        {
            return _client.ExecuteAsync(request, _ => Callbacks.Add(() => callback(_)));
        }

        /// <summary>
        /// Retrieves a list of shared craft. Use the arguments to apply paging.
        /// </summary>
        /// <param name="skip">Specifies the number elements to skip from the start of the list.</param>
        /// <param name="limit">Specifies the number of elements to request from the server. The server does not necessarily respond with the given number of items.</param>
        /// <param name="callback">A callback receiving the response from the server, containing the elements of the craft list.</param>
        /// <returns>A handle to the asynchronous request.</returns>
        public static RestRequestAsyncHandle GetCraft(int skip, int limit, Action<IRestResponse<List<SharedCraft>>> callback)
        {
            var request = CreateRequest("craft/", Method.GET);
            request.AddQueryParameter("skip", skip.ToString());
            request.AddQueryParameter("limit", limit.ToString());
            return Execute(request, callback);
        }

        /// <summary>
        /// Retrieves the craft string for the given id.
        /// </summary>
        /// <param name="id">Specifies the id of a shared craft.</param>
        /// <param name="callback">A callback receiving the response from the server, containing the (compressed) raw bytes of the craft.</param>
        /// <returns>A handle to the asynchronous request.</returns>
        public static RestRequestAsyncHandle GetCraft(string id, Action<IRestResponse> callback)
        {
            var request = CreateRequest("craft/data/{id}", Method.GET);
            request.AddUrlSegment("id", id);
            return Execute(request, callback);
        }

        /// <summary>
        /// Retrieves the thumbnail for the given id.
        /// </summary>
        /// <param name="id">Specifies the id of a shared craft.</param>
        /// <param name="callback">A callback receiving the response from the server, containing the raw bytes of the thumbnail.</param>
        /// <returns>A handle to the asynchronous request.</returns>
        public static RestRequestAsyncHandle GetThumbnail(string id, Action<IRestResponse> callback)
        {
            var request = CreateRequest("craft/thumbnail/{id}", Method.GET);
            request.AddUrlSegment("id", id);
            return Execute(request, callback);
        }

        /// <summary>
        /// Deletes the shared craft identified by the given id.
        /// </summary>
        /// <param name="id">Specifies the id of a shared craft.</param>
        /// <param name="callback">A callback receiving the response from the server.</param>
        /// <returns>A handle to the asynchronous request.</returns>
        public static RestRequestAsyncHandle DeleteCraft(string id, Action<IRestResponse> callback)
        {
            var request = CreateRequest("craft/" + id, Method.DELETE);
            return Execute(request, callback);
        }

        /// <summary>
        /// Creates a new shared craft with the given data.
        /// </summary>
        /// <param name="craft">Information about the craft to be shared.</param>
        /// <param name="craftData">Raw bytes of the (compressed) craft file.</param>
        /// <param name="thumbnail">Raw bytes of the thumbnail.</param>
        /// <param name="callback">A callback receiving the response from the server, containing the newly created craft with updated information returned from the server.</param>
        /// <returns>A handle to the asynchronous request.</returns>
        public static RestRequestAsyncHandle PostCraft(SharedCraft craft, byte[] craftData, byte[] thumbnail, Action<IRestResponse<SharedCraft>> callback)
        {
            var request = CreateRequest("craft/", Method.POST);
            request.AddObject(craft);
            request.AddFile("craftData", CLZF2.Compress(craftData), "craftData.craft");
            request.AddFile("thumbnail", thumbnail, "thumbnail.png");
            return Execute(request, callback);
        }
    }
}