using System;
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
            RestApi.SetHostAddress("localhost:8000");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var crafts = RestApi.GetCraftList(20, 5);
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
            craft = RestApi.CreateCraft(craft);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var craft = RestApi.GetCraft(textBox1.Text);
        }
    }
}
