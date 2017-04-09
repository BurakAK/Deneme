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
    public partial class OpenDialog : Form
    {
        public string fileName;
        public string folderName;
        public string selectedTab;

        public OpenDialog()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textBox4.Text = dlg.FileName;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    textBox3.Text = fbd.SelectedPath;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "JPEG (*.jpg)|*.jpg|Windows bitmap (*.bmp)|*.bmp|All files|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dlg.FileName;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    textBox2.Text = fbd.SelectedPath;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab.Text == "Video")
            {
                selectedTab = tabControl1.SelectedTab.Text;
                if (textBox1.Text.Length >= 1)
                {
                    fileName = textBox4.Text;
                }
                else
                {
                    folderName = textBox3.Text;
                }
            }
            else if (tabControl1.SelectedTab.Text == "Image")
            {
                selectedTab = tabControl1.SelectedTab.Text;
                if (textBox3.Text.Length >= 1)
                {
                    fileName = textBox1.Text;
                }
                else
                {
                    folderName = textBox2.Text;
                }
            }
            this.Close();
        }
    }
}
