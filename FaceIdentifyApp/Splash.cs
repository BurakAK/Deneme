using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FaceIdentifyApp
{
    public partial class Splash : Form
    {
        public Splash()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Increment(3);

            if (progressBar1.Value == 100)
            {
                timer1.Stop();
                this.Close();
                Form1 form = new Form1();
                form.Show();
            }    }
    }
}
