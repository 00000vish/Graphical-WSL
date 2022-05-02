using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GWSL
{
    public partial class Form2 : Form
    {
        int value = 10;
        public Form2()
        {
            InitializeComponent();
        }
        private void Form2_Load(object sender, EventArgs e)
        {

        }
        public void Show(string title, string dis)
        {
            this.Text = title;
            label1.Text = dis;
            Show();
        }
        public void completeTask()
        {
            progressBar1.Value = progressBar1.Maximum;
            progressBar1.Update();
            progressBar1.Refresh();
            MessageBox.Show(label1.Text + " complete!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);     
        }
        public void updateProgress()
        {
            value +=10;
            progressBar1.Value = progressBar1.Maximum - ((500 / value)+2 );
            progressBar1.Update();
            progressBar1.Refresh();
        }
    }
}
