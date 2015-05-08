using System;
using System.IO;
using System.Windows.Forms;
using CraftShare;

namespace CraftShareClient
{
    public partial class CraftShareForm
        : Form
    {
        public CraftShareForm()
        {
            InitializeComponent();
            RestApi.SetHostAddress("localhost:10412");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var crafts = RestApi.GetCraft(0, 5);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var craft = new SharedCraft
            {
                name = "Trollcraft",
                facility = "VAB",
                author = "Client app",
                craft = "lots of parts.."
            };
            var data = File.ReadAllBytes("thumbnail.png");
            craft = RestApi.PostCraft(craft, data);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var craft = RestApi.GetCraft(textBox1.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var result = RestApi.GetThumbnail(textBox1.Text);
            File.WriteAllBytes("download.png", result);
        }
    }
}
