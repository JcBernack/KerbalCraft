using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;

namespace CraftShareClient
{
    public partial class Form1
        : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var crafts = CraftGET();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var craft = new SharedCraft
            {
                Name = "Trollcraft",
                Author = "Client app",
                Craft = "lots of parts..",
                Thumbnail = "base64 schtrings"
            };
            craft = CraftPOST(craft);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var craft = CraftGET(textBox1.Text);
        }

        private IEnumerable<SharedCraft> CraftGET()
        {
            var request = WebRequest.Create("http://localhost:8000/api/crafts/");
            var jsonSerializer = new DataContractJsonSerializer(typeof(List<SharedCraft>));
            using (var response = request.GetResponse())
            using (var inStream = response.GetResponseStream())
            {
                if (inStream == null) return null;
                return (List<SharedCraft>)jsonSerializer.ReadObject(inStream);
            }
        }

        private SharedCraft CraftGET(string id)
        {
            var request = WebRequest.Create("http://localhost:8000/api/craft/" + id);
            var jsonSerializer = new DataContractJsonSerializer(typeof(SharedCraft));
            using (var response = request.GetResponse())
            using (var inStream = response.GetResponseStream())
            {
                if (inStream == null) return null;
                return (SharedCraft)jsonSerializer.ReadObject(inStream);
            }
        }

        private SharedCraft CraftPOST(SharedCraft craft)
        {
            var request = WebRequest.Create("http://localhost:8000/api/craft/");
            request.Method = "POST";
            request.ContentType = "application/json";
            var jsonSerializer = new DataContractJsonSerializer(typeof(SharedCraft));
            using (var outStream = request.GetRequestStream())
            {
                jsonSerializer.WriteObject(outStream, craft);
            }
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) return null;
                return (SharedCraft)jsonSerializer.ReadObject(stream);
            }
        }
    }

    [DataContract]
    public class SharedCraft
    {
        [DataMember(Name = "_id", EmitDefaultValue = false)]
        public string Id;

        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "author")]
        public string Author;
        
        [DataMember(Name = "date", EmitDefaultValue = false)]
        public string Date;

        [DataMember(Name = "craft")]
        public string Craft;

        [DataMember(Name = "thumbnail")]
        public string Thumbnail;
    }
}
