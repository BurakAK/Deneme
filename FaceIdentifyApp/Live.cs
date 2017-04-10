using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Luxand;
using System.IO;
using NReco.VideoConverter;
using System.Threading;

namespace FaceIdentifyApp
{
    public struct TFaceRecord
    {
        public byte[] Template; //Face Template;
        public FSDK.TFacePosition FacePosition;
        public FSDK.TPoint[] FacialFeatures; //Facial Features;

        public string ImageFileName;
        public string suspectName;

        public FSDK.CImage image;
        public FSDK.CImage faceImage;
    }

    public partial class Live : UserControl
    {
        private string folderName;
        private string fileName;
        private string selected;

        public float FaceDetectionThreshold = 5;
        public static float FARValue = 100;

        private List<TFaceRecord> FaceSearchList;
        static ImageList FaceSearchImageList;

        static ImageList resultImagelist;

        private List<TFaceRecord> SubjectList;
        private DbConnection db = new DbConnection();

        public Live()
        {
            InitializeComponent();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            OpenDialog dl = new OpenDialog();
            dl.ShowDialog();

            folderName = dl.folderName;
            fileName = dl.fileName;
            selected = dl.selectedTab;

            if (folderName != null && selected.ToString() == "Video")
            {
                MessageBox.Show("Video  folder tara");
            }
            else if (fileName != null && selected.ToString() == "Video")
            {
                SubjectList = db.LoadSubject(Constants.conString);
                loadVideo(fileName);
                Thread t = new Thread(delegate () { matchesFace(); });
                t.Start();
            }
            else if (folderName != null && selected.ToString() == "Image")
            {
                SubjectList =db.LoadSubject(Constants.conString);
                loadSubjectFolder(folderName);
                Thread t = new Thread(delegate () { matchesFace(); });
                t.Start();
            }
            else if (fileName != null && selected.ToString() == "Image")
            {
                 SubjectList = db.LoadSubject(Constants.conString);
                loadSubject(fileName);
                Thread t = new Thread(delegate () { matchesFace(); });
                t.Start();
            }
        }

        private void loadSubject(string fileName)
        {
            FSDK.SetFaceDetectionParameters(false, true, 384);
            FSDK.SetFaceDetectionThreshold((int)FaceDetectionThreshold);

            TFaceRecord fr = new TFaceRecord();
            fr.ImageFileName = fileName;

            fr.FacePosition = new FSDK.TFacePosition();
            fr.FacialFeatures = new FSDK.TPoint[2];
            fr.Template = new byte[FSDK.TemplateSize];

            fr.image = new FSDK.CImage(fileName);

            fr.FacePosition = fr.image.DetectFace();
            if (0 == fr.FacePosition.w)
            {
                if (fileName.Length <= 1)
                    MessageBox.Show("No faces found. Try to lower the Minimal Face Quality parameter in the Options dialog box.", "Enrollment error");
            }
            else
            {
                fr.faceImage = fr.image.CopyRect((int)(fr.FacePosition.xc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.xc + Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc + Math.Round(fr.FacePosition.w * 0.5)));

                try
                {
                    fr.FacialFeatures = fr.image.DetectEyesInRegion(ref fr.FacePosition);
                }
                catch (Exception ex2)
                {
                    MessageBox.Show(ex2.Message, "Error detecting eyes.");
                }

                try
                {
                    fr.Template = fr.image.GetFaceTemplateInRegion(ref fr.FacePosition); // get template with higher precision
                }
                catch (Exception ex2)
                {
                    MessageBox.Show(ex2.Message, "Error retrieving face template.");
                }

                FaceSearchList.Add(fr);
                FaceSearchImageList.Images.Add(fr.faceImage.ToCLRImage());
                listView1.Items.Add((FaceSearchImageList.Images.Count - 1).ToString(), fileName.Split('\\')[fileName.Split('\\').Length - 1], FaceSearchImageList.Images.Count - 1);

                using (Image img = fr.image.ToCLRImage())
                {
                    pictureBox1.Image = img;
                    pictureBox1.Refresh();
                }
            }
            pictureBox1.Image = null;
        }

        private void loadSubjectFolder(string folderName)
        {
            FaceSearchList.Clear();
            string DirectoryUrl = folderName;
            DirectoryInfo di = new DirectoryInfo(DirectoryUrl);
            FileInfo[] Images = di.GetFiles("*.jpg", SearchOption.AllDirectories);

            for (int i = 0; i < Images.Length; i++)
            {
                string fn = Images[i].FullName;

                TFaceRecord fr = new TFaceRecord();
                fr.ImageFileName = fn;
                fr.FacePosition = new FSDK.TFacePosition();
                fr.FacialFeatures = new FSDK.TPoint[FSDK.FSDK_FACIAL_FEATURE_COUNT];
                fr.Template = new byte[FSDK.TemplateSize];

                try
                {
                    fr.image = new FSDK.CImage(fn);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error loading file");
                }

                fr.FacePosition = fr.image.DetectFace();
                if (0 == fr.FacePosition.w)
                { }
                else
                {
                    fr.faceImage = fr.image.CopyRect((int)(fr.FacePosition.xc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.xc + Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc + Math.Round(fr.FacePosition.w * 0.5)));

                    bool eyesDetected = false;
                    try
                    {
                        fr.FacialFeatures = fr.image.DetectEyesInRegion(ref fr.FacePosition);
                        eyesDetected = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error detecting eyes.");
                    }

                    if (eyesDetected)
                    {
                        fr.Template = fr.image.GetFaceTemplateInRegion(ref fr.FacePosition); // get template with higher precision
                    }

                    FaceSearchList.Add(fr);
                    FaceSearchImageList.Images.Add(fr.faceImage.ToCLRImage());

                    listView1.Items.Add((FaceSearchImageList.Images.Count - 1).ToString(), folderName.Split('\\')[folderName.Split('\\').Length - 1], FaceSearchImageList.Images.Count - 1);

                    using (Image img = fr.image.ToCLRImage())
                    {
                        pictureBox1.Image = img;
                        pictureBox1.Refresh();
                        listView1.Refresh();
                    }
                }
            }
            pictureBox1.Image = null;
        }

        private void loadVideo(string fileName)
        {
            if (FaceSearchList.Count > 0)
            {
                FaceSearchList.Clear();
                FaceSearchImageList.Images.Clear();
                listView1.Clear();
            }

            string username = Environment.UserName;
            string framePath = @"C:\Users\" + username + @"\Desktop\Frames";

            if (!Directory.Exists(framePath))
                Directory.CreateDirectory(framePath);

            FFMpegConverter ffmpeg = new FFMpegConverter();
            TFaceRecord fr = new TFaceRecord();
            for (int i = 0; i < Duration(fileName); i++)
            {
                string fn = framePath + @"\" + i + ".jpeg";
                ffmpeg.GetVideoThumbnail(fileName.ToString(), fn, i);

                fr.ImageFileName = fn;
                fr.FacePosition = new FSDK.TFacePosition();
                fr.FacialFeatures = new FSDK.TPoint[2];
                fr.Template = new byte[FSDK.TemplateSize];
                fr.image = new FSDK.CImage(fn);

                fr.FacePosition = fr.image.DetectFace();

                fr.faceImage = fr.image.CopyRect((int)(fr.FacePosition.xc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.xc + Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc + Math.Round(fr.FacePosition.w * 0.5)));
                try
                {
                    fr.FacialFeatures = fr.image.DetectEyesInRegion(ref fr.FacePosition);
                }
                catch (Exception ex2)
                {
                    MessageBox.Show(ex2.Message, "Error detecting eyes.");
                }

                try
                {
                    fr.Template = fr.image.GetFaceTemplateInRegion(ref fr.FacePosition); // get template with higher precision
                }
                catch (Exception ex2)
                {
                    MessageBox.Show(ex2.Message, "Error retrieving face template.");
                }

                FaceSearchList.Add(fr);
                FaceSearchImageList.Images.Add(fr.faceImage.ToCLRImage());

                listView1.Items.Add((FaceSearchImageList.Images.Count - 1).ToString(), fn.Split('\\')[fn.Split('\\').Length - 1], FaceSearchImageList.Images.Count - 1);
                using (Image img = fr.image.ToCLRImage())
                {

                    pictureBox1.Image = img;
                    pictureBox1.Refresh();
                }
                listView1.Refresh();
            }
            pictureBox1.Image = null;
        }

        private void matchesFace()
        {
            if (dataGridView1.Rows.Count > 0)
            {
                this.dataGridView1.Invoke(new MethodInvoker(() =>
                                this.dataGridView1.Rows.Clear()));
                this.dataGridView1.Invoke(new MethodInvoker(() =>
                                this.dataGridView1.Refresh()));
            }

            for (int i = 0; i < SubjectList.Count; i++)
            {
                if (SubjectList.Count >= 1)
                {
                    FSDK.GetMatchingThresholdAtFAR(FARValue / 100, ref FaceDetectionThreshold);

                    TFaceRecord DbSubject = SubjectList[i];
                    int MatchedCount = 0;
                    int FaceCount = FaceSearchList.Count;
                    float[] Similarities = new float[FaceCount];
                    float[] Smile = new float[FaceCount];
                    float[] EyesOpen = new float[FaceCount];
                    float[] Male = new float[FaceCount];
                    float[] Female = new float[FaceCount];

                    int[] Numbers = new int[FaceCount];

                    for (int k = 0; k < FaceSearchList.Count; k++)
                    {
                        float Similarity = 0.0f;
                        float ConfidenceSmile = 0.0f;
                        float ConfidenceEyesOpen = 0.0f;
                        float ConfidenceMale = 0.0f;
                        float ConfidenceFemale = 0.0f;

                        TFaceRecord SearchFace = FaceSearchList[k];
                        FSDK.MatchFaces(ref DbSubject.Template, ref SearchFace.Template, ref Similarity);

                        long MaxSizeInBytes = 100000;
                        string ExpressionValues = "";
                        string GenderValues = "";
                        FSDK.CImage CurrentImage = SearchFace.image;
                        FSDK.TPoint[] Facefeatures = null;

                        FSDK.DetectFacialFeatures(SearchFace.faceImage.ImageHandle, out Facefeatures);

                        if (Facefeatures != null)
                        {
                            FSDK.DetectFacialAttributeUsingFeatures(SearchFace.faceImage.ImageHandle, ref Facefeatures, "Expression", out ExpressionValues, MaxSizeInBytes);
                            FSDK.GetValueConfidence(ExpressionValues, "Smile", ref ConfidenceSmile);
                            FSDK.GetValueConfidence(ExpressionValues, "EyesOpen", ref ConfidenceEyesOpen);

                            FSDK.DetectFacialAttributeUsingFeatures(SearchFace.faceImage.ImageHandle, ref Facefeatures, "Gender", out GenderValues, MaxSizeInBytes);
                            FSDK.GetValueConfidence(GenderValues, "Male", ref ConfidenceMale);
                            FSDK.GetValueConfidence(GenderValues, "Female", ref ConfidenceFemale);

                            if (Similarity >= FaceDetectionThreshold)
                            {
                                Similarities[MatchedCount] = Similarity;
                                Smile[MatchedCount] = ConfidenceSmile;
                                EyesOpen[MatchedCount] = ConfidenceEyesOpen;
                                Male[MatchedCount] = ConfidenceMale;
                                Female[MatchedCount] = ConfidenceFemale;

                                Numbers[MatchedCount] = k;
                                ++MatchedCount;
                            }
                        }
                        else
                        {
                            if (Similarity >= FaceDetectionThreshold)
                            {
                                Similarities[MatchedCount] = Similarity;
                                Smile[MatchedCount] = 0;
                                EyesOpen[MatchedCount] = 0;
                                Male[MatchedCount] = 0;
                                Female[MatchedCount] = 0;

                                Numbers[MatchedCount] = k;
                                ++MatchedCount;
                            }
                        }
                    }

                    if (MatchedCount == 0)
                        MessageBox.Show("No matches found. You can try to increase the FAR parameter in the Options dialog box.", "No matches");
                    else
                    {
                        for (int j = 0; j < MatchedCount; j++)
                        {
                            if ((Similarities[j] * 100.0f) >= 30.0f)
                            {
                                resultImagelist.Images.Add(FaceSearchList[j].faceImage.ToCLRImage());

                                Image img1 = FaceSearchList[Numbers[j]].faceImage.ToCLRImage();
                                img1 = (Image)(new Bitmap(img1, new Size(100, 100)));

                                Image img2 = Image.FromFile(SubjectList[i].ImageFileName);
                                img2 = (Image)(new Bitmap(img2, new Size(100, 100)));

                                string feature = DbSubject.suspectName + " \r\n\nSimilarity = " + (Similarities[j] * 100).ToString() + " Smile:" + Smile[j] * 100 + " Eyes Open: " + EyesOpen[j] * 100
                                            + " Male:" + Male[j] * 100 + " Female: " + Female[j] * 100;
                                Object[] row = new Object[] { img1, img2, feature };

                                this.dataGridView1.Invoke(new MethodInvoker(() =>
                                     this.dataGridView1.Rows.Add(row)));
                            }
                        }
                    }
                }
            }
        }

        private int Duration(string path)
        {
            var ffProbe = new NReco.VideoInfo.FFProbe();
            var videoInfo = ffProbe.GetMediaInfo(path);

            int count = videoInfo.Duration.Seconds + videoInfo.Duration.Minutes * 60 + videoInfo.Duration.Hours * 60 * 60;
            return count;
        }

        private void Live_Load(object sender, EventArgs e)
        {
            FaceSearchList = new List<TFaceRecord>();

            pictureBox1.Dock = DockStyle.None;

            FaceSearchImageList = new ImageList();
            Size size = new Size();
            size.Height = 150;
            size.Width = 150;
            FaceSearchImageList.ImageSize = size;
            FaceSearchImageList.ColorDepth = ColorDepth.Depth24Bit;

            listView1.View = View.LargeIcon;
            listView1.LargeImageList = FaceSearchImageList;

            resultImagelist = new ImageList();
            Size size100 = new Size();
            size100.Height = 150;
            size100.Width = 150;
            resultImagelist.ImageSize = size100;
            resultImagelist.ColorDepth = ColorDepth.Depth24Bit;

            DataGridViewImageColumn imgCool = new DataGridViewImageColumn();
            imgCool.HeaderText = "Macthes";
            imgCool.Name = "Macthes";
            dataGridView1.Columns.Add(imgCool);

            DataGridViewImageColumn imgCool1 = new DataGridViewImageColumn();
            imgCool1.HeaderText = "Subject";
            imgCool1.Name = "Subject";
            dataGridView1.Columns.Add(imgCool1);

            dataGridView1.ColumnCount = 3;
            dataGridView1.Columns[2].Name = "Features";
        }
    }
}
  