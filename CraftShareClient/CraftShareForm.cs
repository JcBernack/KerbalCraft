using System;
using System.IO;
using System.Text;
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
                author = "Client app"
            };
            var thumbnail = File.ReadAllBytes("thumbnail.png");
            var craftData = File.ReadAllBytes("landerOriginal.txt");
            craft = RestApi.PostCraft(craft, craftData, thumbnail);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var data = RestApi.GetCraft(textBox1.Text);
            var craft = Encoding.UTF8.GetString(data);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var result = RestApi.GetThumbnail(textBox1.Text);
            File.WriteAllBytes("download.png", result);
        }
    }
}
