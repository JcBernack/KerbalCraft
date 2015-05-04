using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Json;

namespace CraftShare
{
    /// <summary>
    /// TODO: Make the whole API async?
    /// </summary>
    public static class RestApi
    {
        public static string HostAddress = "localhost:8000";

        private static WebRequest CreateRequest(string url)
        {
            var request = WebRequest.Create(string.Format("http://{0}/api/{1}", HostAddress, url));
            request.Timeout = 2000;
            return request;
        }

        public static IList<SharedCraft> GetCraftList()
        {
            return Get<IList<SharedCraft>>("crafts/");
        }

        public static string GetThumbnail(string id)
        {
            return Get<string>("thumbnail/" + id);
        }

        public static SharedCraft CreateCraft(SharedCraft craft)
        {
            return Post(craft, "craft/");
        }

        private static T Get<T>(string url)
        {
            var request = CreateRequest(url);
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));
            using (var response = request.GetResponse())
            using (var inStream = response.GetResponseStream())
            {
                if (inStream == null) return default(T);
                return (T)jsonSerializer.ReadObject(inStream);
            }
        }

        private static T Post<T>(T craft, string url)
        {
            var request = CreateRequest(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));
            using (var outStream = request.GetRequestStream())
            {
                jsonSerializer.WriteObject(outStream, craft);
            }
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) return default(T);
                return (T)jsonSerializer.ReadObject(stream);
            }
        }
    }
}