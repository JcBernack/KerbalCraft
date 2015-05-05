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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var crafts = RestApi.GetCraftList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var craft = new SharedCraft
            {
                Name = "Trollcraft",
                Author = "Client app",
                Craft = "lots of parts.."
            };
            craft = RestApi.CreateCraft(craft);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var craft = RestApi.GetCraft(textBox1.Text);
        }
    }
}
