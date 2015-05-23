using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using KerbalCraft;
using KerbalCraft.Models;
using RestSharp;

namespace KerbalCraftTest
{
    public partial class KerbalCraftForm
        : Form
    {
        private readonly RestApi _api;

        public KerbalCraftForm()
        {
            InitializeComponent();
            _api = new RestApi();
            _api.SetConfig("localhost:10412", "huso", "lololol");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var handle = _api.GetCraft(0, 5, delegate(IRestResponse response)
            {
                if (response.ErrorException != null) throw response.ErrorException;
                var crafts = _api.Deserialize<List<Craft>>(response);
                Debugger.Break();
                if (crafts != null && crafts.Count > 0) textBox1.Text = crafts[0]._id;
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var craftData = CLZF2.Compress(File.ReadAllBytes("Munlander.craft"));
            var thumbnail = File.ReadAllBytes("Munlander.png");
            var handle = _api.PostCraft(craftData, thumbnail, delegate(IRestResponse response)
            {
                if (response.ErrorException != null) throw response.ErrorException;
                var craft = _api.Deserialize<Craft>(response);
                Debugger.Break();
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var handle = _api.GetCraftData(textBox1.Text, delegate(IRestResponse response)
            {
                var craft = Encoding.UTF8.GetString(CLZF2.Decompress(response.RawBytes));
                Debugger.Break();
            });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var handle = _api.GetCraftThumbnail(textBox1.Text, delegate(IRestResponse response)
            {
                File.WriteAllBytes("download.png", response.RawBytes);
                Debugger.Break();
            });
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _api.HandleResponses();
        }
    }
}
