using Luxand;
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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (FSDK.FSDKE_OK != FSDK.ActivateLibrary("VBsVmYmHr/5JxUlk3q0KHjILz7R3Hb5OEhCQ7KdCg/tPbQqJfAaz8ok/9+iTgDp/KjGjkBi23HeCaUq8KKtKeXXN3xbe+bKfQ8q/3mfG6sad3AGUYDj6E+Qi2pzCWFgb4vqWDB3pLzUw+hnOZ7///CBV63IaB1kh7XF6VCaGtNw="))
            {
                MessageBox.Show("Please run the License Key Wizard (Start - Luxand - FaceSDK - License Key Wizard)", "Error activating FaceSDK", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (FSDK.InitializeLibrary() != FSDK.FSDKE_OK)
                MessageBox.Show("Error initializing FaceSDK!", "Error");
        }
    }
}
