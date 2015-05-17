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
        public KerbalCraftForm()
        {
            InitializeComponent();
            RestApi.SetConfig("localhost:10412", "huso", "lolol");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var handle = RestApi.GetCraft(0, 5, delegate(IRestResponse response)
            {
                if (response.ErrorException != null) throw response.ErrorException;
                var crafts = RestApi.Deserialize<List<Craft>>(response);
                Debugger.Break();
                if (crafts != null && crafts.Count > 0) textBox1.Text = crafts[0]._id;
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var craftData = CLZF2.Compress(File.ReadAllBytes("Munlander.craft"));
            var thumbnail = File.ReadAllBytes("Munlander.png");
            var handle = RestApi.PostCraft(craftData, thumbnail, delegate(IRestResponse response)
            {
                if (response.ErrorException != null) throw response.ErrorException;
                var craft = RestApi.Deserialize<Craft>(response);
                Debugger.Break();
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var handle = RestApi.GetCraftData(textBox1.Text, delegate(IRestResponse response)
            {
                var craft = Encoding.UTF8.GetString(CLZF2.Decompress(response.RawBytes));
                Debugger.Break();
            });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var handle = RestApi.GetCraftThumbnail(textBox1.Text, delegate(IRestResponse response)
            {
                File.WriteAllBytes("download.png", response.RawBytes);
                Debugger.Break();
            });
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RestApi.HandleResponses();
        }
    }
}
