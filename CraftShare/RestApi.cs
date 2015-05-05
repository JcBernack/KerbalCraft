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
            var request = (HttpWebRequest)WebRequest.Create(string.Format("http://{0}/api/{1}", HostAddress, url));
            //request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Timeout = 2000;
            request.ContentType = "application/json";
            return request;
        }

        public static List<SharedCraft> GetCraftList()
        {
            return Get<List<SharedCraft>>("crafts/");
        }

        public static string GetCraft(string id)
        {
            return Get<SharedCraft>("craft/" + id).Craft;
        }

        public static SharedCraft CreateCraft(SharedCraft craft)
        {
            return Post(craft, "craft/");
        }

        public static bool DeleteCraft(string id)
        {
            var status = Delete("craft/" + id);
            return status == HttpStatusCode.OK;
        }

        private static T Get<T>(string url)
        {
            var request = CreateRequest(url);
            request.Method = "GET";
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var inStream = response.GetResponseStream())
            {
                if (inStream == null) return default(T);
                var jsonSerializer = new DataContractJsonSerializer(typeof(T));
                return (T)jsonSerializer.ReadObject(inStream);
            }
        }

        private static T Post<T>(T data, string url)
        {
            var request = CreateRequest(url);
            request.Method = "POST";
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));
            using (var outStream = request.GetRequestStream())
            {
                jsonSerializer.WriteObject(outStream, data);
            }
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) return default(T);
                return (T)jsonSerializer.ReadObject(stream);
            }
        }

        //private static T Delete<T>(string url)
        //{
        //    var request = CreateRequest(url);
        //    request.Method = "DELETE";
        //    using (var response = (HttpWebResponse)request.GetResponse())
        //    {
        //        if (response.StatusCode == HttpStatusCode.NoContent) return default(T);
        //        using (var inStream = response.GetResponseStream())
        //        {
        //            if (inStream == null) return default(T);
        //            var jsonSerializer = new DataContractJsonSerializer(typeof(T));
        //            return (T)jsonSerializer.ReadObject(inStream);
        //        }
        //    }
        //}

        private static HttpStatusCode Delete(string url)
        {
            var request = CreateRequest(url);
            request.Method = "DELETE";
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                return response.StatusCode;
            }
        }
    }
}