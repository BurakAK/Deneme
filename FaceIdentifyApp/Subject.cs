﻿using Luxand;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FaceIdentifyApp
{
    public partial class Subject : UserControl
    {

        public static float FaceDetectionThreshold = 3;
        public static float FARValue = 100;

        public static List<TFaceRecord> FaceList;

        static ImageList imageList1;
        DbConnection db = new DbConnection();
        List<TFaceRecord> SubjectList = new List<TFaceRecord>();

        public Subject()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

       

        private void Subject_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
           SubjectList = db.LoadSubject(Constants.conString);

            FaceList = new List<TFaceRecord>();

            imageList1 = new ImageList();
            Size size100x100 = new Size();
            size100x100.Height = 100;
            size100x100.Width = 100;
            imageList1.ImageSize = size100x100;
            imageList1.ColorDepth = ColorDepth.Depth24Bit;

            imageList1 = db.SubjectImageList;

            listView1.OwnerDraw = false;
            listView1.View = View.LargeIcon;
            listView1.LargeImageList = imageList1;


            for (int i = 0; i < SubjectList.Count; i++)
            {
                FaceList.Add(SubjectList[i]);
                listView1.Items.Add((imageList1.Images.Count - (imageList1.Images.Count - i)).ToString()
                    , SubjectList[i].suspectName
                    , (imageList1.Images.Count - (imageList1.Images.Count - i)));

                listView1.SelectedIndices.Clear();
                listView1.SelectedIndices.Add(listView1.Items.Count - 1);
                listView1.Refresh();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

           
            db.ClearDB(Constants.conString);

            var confirmResult = MessageBox.Show("Are you sure to delete this database ??",
                                    "Confirm Delete!!",
                                    MessageBoxButtons.YesNo);

            if (confirmResult == DialogResult.Yes)
            {
                FaceList.Clear();
                listView1.Items.Clear();
                imageList1.Images.Clear();
                pictureBox1.Width = 0;
                pictureBox1.Height = 0;
                db.CreateDB(Constants.conString);
                GC.Collect();
                MessageBox.Show("The database was delete successfully!!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                MessageBox.Show("Please Enter the Subject Name");
                textBox1.Focus();
            }
            else
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "JPEG (*.jpg)|*.jpg|Windows bitmap (*.bmp)|*.bmp|All files|*.*";
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {

                        FSDK.SetFaceDetectionParameters(false, true, 384);
                        FSDK.SetFaceDetectionThreshold((int)FaceDetectionThreshold);


                        foreach (string fn in dlg.FileNames)
                        {
                            FaceList.Clear();
                            TFaceRecord fr = new TFaceRecord();
                            fr.ImageFileName = fn;
                            fr.suspectName = textBox1.Text.Trim();
                            fr.FacePosition = new FSDK.TFacePosition();
                            fr.FacialFeatures = new FSDK.TPoint[2];
                            fr.Template = new byte[FSDK.TemplateSize];

                            fr.image = new FSDK.CImage(fn);


                            fr.FacePosition = fr.image.DetectFace();
                            if (0 == fr.FacePosition.w)
                                if (dlg.FileNames.Length <= 1)
                                    MessageBox.Show("No faces found. Try to lower the Minimal Face Quality parameter in the Options dialog box.", "Enrollment error");
                                else
                                { }
                            else
                            {
                                fr.faceImage = fr.image.CopyRect((int)(fr.FacePosition.xc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.xc + Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc + Math.Round(fr.FacePosition.w * 0.5)));
                                fr.FacialFeatures = fr.image.DetectEyesInRegion(ref fr.FacePosition);
                                fr.Template = fr.image.GetFaceTemplateInRegion(ref fr.FacePosition); // get template with higher precision



                                FaceList.Add(fr);

                                imageList1.Images.Add(fr.faceImage.ToCLRImage());
                                listView1.Items.Add((imageList1.Images.Count - 1).ToString(), fn.Split('\\')[fn.Split('\\').Length - 1], imageList1.Images.Count - 1);


                            }

                            listView1.SelectedIndices.Clear();
                            // listView1.SelectedIndices.Add(listView1.Items.Count - 1);

                            db.SaveSubject(fr, Constants.conString);
                        }
                        
                     
                        listView1.Refresh();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Can't open image(s) with error: " + ex.Message.ToString(), "Error");
                    }

                }

            }
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (this.listView1.SelectedIndices.Count > 0)
            {

                //pictureBox1.Image = null;
                Image img = Image.FromFile(FaceList[listView1.SelectedIndices[0]].ImageFileName);
                pictureBox1.Height = img.Height;
                pictureBox1.Width = img.Width;
                pictureBox1.Image = img;

                pictureBox1.Refresh();
                Graphics gr = pictureBox1.CreateGraphics();
                gr.DrawRectangle(Pens.LightGreen, FaceList[listView1.SelectedIndices[0]].FacePosition.xc - FaceList[listView1.SelectedIndices[0]].FacePosition.w / 2, FaceList[listView1.SelectedIndices[0]].FacePosition.yc - FaceList[listView1.SelectedIndices[0]].FacePosition.w / 2, FaceList[listView1.SelectedIndices[0]].FacePosition.w, FaceList[listView1.SelectedIndices[0]].FacePosition.w);

                for (int i = 0; i < 2; ++i)
                {
                    FSDK.TPoint tp = FaceList[listView1.SelectedIndices[0]].FacialFeatures[i];
                    gr.DrawEllipse(Pens.Blue, tp.x, tp.y, 3, 3);
                }
            }
        }
    }
}
