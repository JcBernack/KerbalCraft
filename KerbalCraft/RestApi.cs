using System;
using System.Collections.Generic;
using System.Net;
using KerbalCraft.Models;
using RestSharp;
using RestSharp.Deserializers;

namespace KerbalCraft
{
    /// <summary>
    /// Handles all the communication with the server.
    /// </summary>
    public class RestApi
    {
        public string HostAddress { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        private readonly IDeserializer _deserializer;
        private readonly List<Action> _callbacks;
        
        public RestApi()
        {
            //TODO: get a proper certificate and stop preventing the validity check
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            _callbacks = new List<Action>();
            _deserializer = new JsonDeserializer { DateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ" };
        }

        public T Deserialize<T>(IRestResponse response)
        {
            return _deserializer.Deserialize<T>(response);
        }

        public void SetConfig(string hostAddress, string username, string password)
        {
            HostAddress = hostAddress;
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Calls all pending callbacks from finished requests.
        /// </summary>
        public void HandleResponses()
        {
            // Invoke all callbacks in a FIFO manner, but let exceptions bubble up
            // as there should not be any and we want to know otherwise.
            while (_callbacks.Count > 0)
            {
                try
                {
                    _callbacks[0].Invoke();
                }
                finally
                {
                    // Make sure the callback is removed, even if an exception occured.
                    // Otherwise it might be invoked over and over again as HandleResponses is most likely called from OnGUI.
                    _callbacks.RemoveAt(0);
                }
            }
        }

        private RestRequest CreateRequest(string resource, Method method)
        {
            return new RestRequest(resource, method)
            {
                Timeout = 5000,
                RequestFormat = DataFormat.Json
            };
        }

        private RestRequestAsyncHandle Execute(IRestRequest request, Action<IRestResponse> callback, bool authentication = false)
        {
            var client = new RestClient(string.Format("https://{0}/api/", HostAddress));
            if (authentication) client.Authenticator = new HttpBasicAuthenticator(Username, Password);
            return client.ExecuteAsync(request, _ => _callbacks.Add(() => callback(_)));
        }

        /// <summary>
        /// Creates a new user on the server or validates an existing one.
        /// The server will respond with HTTP 204 if the user credentials are ok, otherwise 400.
        /// </summary>
        /// <param name="username">Specifies the username of a new user.</param>
        /// <param name="password">Specifies the password of a new user.</param>
        /// <param name="callback">A callback receiving the response from the server.</param>
        /// <returns>A handle to the asynchronous request.</returns>
        public RestRequestAsyncHandle PostUser(string username, string password, Action<IRestResponse> callback)
        {
            var request = CreateRequest("user/", Method.POST);
            request.AddBody(new CraftUser { username = username, password = password });
            return Execute(request, callback);
        }

        /// <summary>
        /// Creates a new shared craft with the given data.
        /// </summary>
        /// <param name="craftData">Raw bytes of the (compressed) craft file.</param>
        /// <param name="thumbnail">Raw bytes of the thumbnail.</param>
        /// <param name="callback">A callback receiving the response from the server, containing the newly created craft with updated information returned from the server.</param>
        /// <returns>A handle to the asynchronous request.</returns>
        public RestRequestAsyncHandle PostCraft(byte[] craftData, byte[] thumbnail, Action<IRestResponse> callback)
        {
            var request = CreateRequest("craft/", Method.POST);
            request.AddFile("craftData", craftData, "craftData.craft");
            request.AddFile("thumbnail", thumbnail, "thumbnail.png");
            return Execute(request, callback, true);
        }

        /// <summary>
        /// Retrieves a list of shared craft. Use the arguments to apply paging.
        /// </summary>
        /// <param name="skip">Specifies the number elements to skip from the start of the list.</param>
        /// <param name="limit">Specifies the number of elements to request from the server. The server does not necessarily respond with the given number of items.</param>
        /// <param name="callback">A callback receiving the response from the server, containing the elements of the craft list.</param>
        /// <returns>A handle to the asynchronous request.</returns>
        public RestRequestAsyncHandle GetCraft(int skip, int limit, Action<IRestResponse> callback)
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
        public RestRequestAsyncHandle GetCraftData(string id, Action<IRestResponse> callback)
        {
            var request = CreateRequest("craft/{id}/data", Method.GET);
            request.AddUrlSegment("id", id);
            return Execute(request, callback);
        }

        /// <summary>
        /// Retrieves the thumbnail for the given id.
        /// </summary>
        /// <param name="id">Specifies the id of a shared craft.</param>
        /// <param name="callback">A callback receiving the response from the server, containing the raw bytes of the thumbnail.</param>
        /// <returns>A handle to the asynchronous request.</returns>
        public RestRequestAsyncHandle GetCraftThumbnail(string id, Action<IRestResponse> callback)
        {
            var request = CreateRequest("craft/{id}/thumbnail", Method.GET);
            request.AddUrlSegment("id", id);
            return Execute(request, callback);
        }

        /// <summary>
        /// Deletes the shared craft identified by the given id.
        /// </summary>
        /// <param name="id">Specifies the id of a shared craft.</param>
        /// <param name="callback">A callback receiving the response from the server.</param>
        /// <returns>A handle to the asynchronous request.</returns>
        public RestRequestAsyncHandle DeleteCraft(string id, Action<IRestResponse> callback)
        {
            var request = CreateRequest("craft/" + id, Method.DELETE);
            return Execute(request, callback, true);
        }
    }
}