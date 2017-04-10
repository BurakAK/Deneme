using Luxand;
using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FaceIdentifyApp
{
    class DbConnection
    {
        private SqlCeCommand cmd;
        private SqlCeConnection conn;

        public ImageList SubjectImageList;
        public void CreateDB(string conString)
        {
            using (SqlCeEngine en = new SqlCeEngine(conString))
            {
                if (!File.Exists("Subjects.sdf"))
                {
                    en.CreateDatabase();
                    CreateTable(conString);
                    CreateSettingsTable(conString);
                    InsertInitialDBSettings(conString);
                }
            }
        }

        private void InitializeSDK()
        {

            if (FSDK.FSDKE_OK != FSDK.ActivateLibrary("VBsVmYmHr/5JxUlk3q0KHjILz7R3Hb5OEhCQ7KdCg/tPbQqJfAaz8ok/9+iTgDp/KjGjkBi23HeCaUq8KKtKeXXN3xbe+bKfQ8q/3mfG6sad3AGUYDj6E+Qi2pzCWFgb4vqWDB3pLzUw+hnOZ7///CBV63IaB1kh7XF6VCaGtNw="))
            {
                MessageBox.Show("Please run the License Key Wizard (Start - Luxand - FaceSDK - License Key Wizard)", "Error activating FaceSDK", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (FSDK.InitializeLibrary() != FSDK.FSDKE_OK)
                MessageBox.Show("Error initializing FaceSDK!", "Error");
        }

        public void CreateTable(string conString)
        {
            using (conn = new SqlCeConnection(conString))
            {
                conn.Open();

                using (cmd = new SqlCeCommand(@"create table FaceList(
                     ImageFileName nvarchar(256) primary key,
                     SubjectName nvarchar(256) not null,
                     FacePositionXc int not null,
                     FacePositionYc int not null,
                     FacePositionW int not null,
                     FacePositionAngle real not null,
                     Eye1X int not null,
                     Eye1Y int not null,
                     Eye2X int not null,
                     Eye2Y int not null,
                     Template image not null,
                     Image image not null,
                     FaceImage image not null
                    )", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }
        public void  SaveSubject(TFaceRecord fr , string conString)
        {
            using (conn = new SqlCeConnection(conString))
            {
                conn.Open();
                TFaceRecord tfr = new TFaceRecord();
                tfr = fr;

                Image img = null;
                Image img_face = null;
                MemoryStream strm = new MemoryStream();
                MemoryStream strm_face = new MemoryStream();
                img = tfr.image.ToCLRImage();
                img_face = tfr.faceImage.ToCLRImage();
                img.Save(strm, System.Drawing.Imaging.ImageFormat.Jpeg);
                img_face.Save(strm_face, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] img_array = new byte[strm.Length];
                byte[] img_face_array = new byte[strm_face.Length];
                strm.Position = 0;
                strm.Read(img_array, 0, img_array.Length);
                strm_face.Position = 0;
                strm_face.Read(img_face_array, 0, img_face_array.Length);

                using (cmd = new SqlCeCommand("insert into FaceList (ImageFileName,SubjectName,FacePositionXc,FacePositionYc" +
                   ",FacePositionW,FacePositionAngle,Eye1X,Eye1Y,Eye2X,Eye2Y,Template,Image,FaceImage) values " +
                   "(@IFName,@SName,@FPXc,@FPYc,@FPW,@FPA,@Eye1X,@Eye1Y,@Eye2X,@Eye2Y,@Template,@Image,@FaceImage)", conn))
                {
                    cmd.Parameters.AddWithValue(@"IFName", tfr.ImageFileName);
                    cmd.Parameters.AddWithValue(@"SName", tfr.suspectName);
                    cmd.Parameters.AddWithValue(@"FPXc", tfr.FacePosition.xc);
                    cmd.Parameters.AddWithValue(@"FPYc", tfr.FacePosition.yc);
                    cmd.Parameters.AddWithValue(@"FPW", tfr.FacePosition.w);
                    cmd.Parameters.AddWithValue(@"FPA", tfr.FacePosition.angle);
                    cmd.Parameters.AddWithValue(@"Eye1X", tfr.FacialFeatures[0].x);
                    cmd.Parameters.AddWithValue(@"Eye1Y", tfr.FacialFeatures[0].y);
                    cmd.Parameters.AddWithValue(@"Eye2X", tfr.FacialFeatures[1].x);
                    cmd.Parameters.AddWithValue(@"Eye2Y", tfr.FacialFeatures[1].y);
                    cmd.Parameters.AddWithValue(@"Template", tfr.Template);
                    cmd.Parameters.AddWithValue(@"Image", img_array);
                    cmd.Parameters.AddWithValue(@"FaceImage", img_face_array);

                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
            MessageBox.Show(" Subject added successfully  !!");
        }
        public List<TFaceRecord> LoadSubject(string conString)
        {
            InitializeSDK();
            SubjectImageList = new ImageList();

            List<TFaceRecord> SubjectList = new List<TFaceRecord>();
            try
            {
                using (conn = new SqlCeConnection(conString))
                {
                    conn.Open();

                    using (cmd = new SqlCeCommand(@"Select * From FaceList", conn))
                    {
                        SqlCeDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            TFaceRecord fr = new TFaceRecord();
                            fr.ImageFileName = reader.GetString(0);
                            fr.suspectName = reader.GetString(1);

                            fr.FacePosition = new FSDK.TFacePosition();
                            fr.FacePosition.xc = reader.GetInt32(2);
                            fr.FacePosition.yc = reader.GetInt32(3);
                            fr.FacePosition.w = reader.GetInt32(4);
                            fr.FacePosition.angle = reader.GetFloat(5);

                            fr.FacialFeatures = new FSDK.TPoint[2];
                            fr.FacialFeatures[0] = new FSDK.TPoint();
                            fr.FacialFeatures[0].x = reader.GetInt32(6);
                            fr.FacialFeatures[0].y = reader.GetInt32(7);

                            fr.FacialFeatures[1] = new FSDK.TPoint();
                            fr.FacialFeatures[1].x = reader.GetInt32(8);
                            fr.FacialFeatures[1].y = reader.GetInt32(9);

                            fr.Template = new byte[FSDK.TemplateSize];
                            reader.GetBytes(10, 0, fr.Template, 0, FSDK.TemplateSize);

                            Image img = Image.FromStream(new System.IO.MemoryStream(reader.GetSqlBinary(11).Value));
                            Image img_face = Image.FromStream(new System.IO.MemoryStream(reader.GetSqlBinary(12).Value));
                            fr.image = new FSDK.CImage(img);
                            fr.faceImage = new FSDK.CImage(img_face);

                            SubjectList.Add(fr);
                            SubjectImageList.Images.Add(fr.faceImage.ToCLRImage());

                            img.Dispose();
                            img_face.Dispose();
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception on loading database");
            }

            return SubjectList;
        }

        public void ClearDB(string conString)
        {
            using (conn = new SqlCeConnection(conString))
            {
                conn.Open();

                using (cmd = new SqlCeCommand("DELETE FROM FaceList", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }


        public void CreateSettingsTable(string conString)
        {
            using (conn = new SqlCeConnection(Constants.conString))
            {
                conn.Open();

                using (cmd = new SqlCeCommand(@"create table Settings(
                     ThresholdValue nvarchar(256),
                     DBName nvarchar(256),
                     DBPassword nvarchar(256),
                     DBPort nvarchar(256)
                    )", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }

        }


        public void UpdateDBSettings(String conString, String DBProvider, String DBname, String DBpassword, String DBport, 
            int ThresholdValue)
        {
            using (conn = new SqlCeConnection(conString))
            {
                conn.Open();

                using (cmd = new SqlCeCommand(@"INSERT INTO Settings(DBNAME, DBPassword, DBPort, ThresholdValue) VALUES(
                    @DBname,@DBpassword, @DBport, @ThresholdValue
                    )", conn))
                {
                    cmd.Parameters.AddWithValue(@"DBname", DBname);
                    cmd.Parameters.AddWithValue(@"DBpassword", DBpassword);
                    cmd.Parameters.AddWithValue(@"DBport", DBport);
                    cmd.Parameters.AddWithValue(@"ThresholdValue", ThresholdValue);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }


        public void InsertInitialDBSettings(string conString)
        {
            using (conn = new SqlCeConnection(conString))
            {
                conn.Open();

                using (cmd = new SqlCeCommand(@"INSERT INTO Settings(DBNAME, DBPassword, DBPort, ThresholdValue) VALUES(
                    @DBname,@DBpassword, @DBport, @ThresholdValue
                    )", conn))
                {
                    cmd.Parameters.AddWithValue(@"DBname", "SQL Server Compact");
                    cmd.Parameters.AddWithValue(@"DBpassword", "1234");
                    cmd.Parameters.AddWithValue(@"DBport", "1433");
                    cmd.Parameters.AddWithValue(@"ThresholdValue", "5");
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }







        public void UpdateDBProvider(String conString, String DBProvider, String DBname, String DBpassword, String DBport)
        {
            using (conn = new SqlCeConnection(conString))
            {
                conn.Open();

                using (cmd = new SqlCeCommand(@"UPDATE Settings(DBProvider, DBNAME, DBPassword, DBPort, ThresholdValue) VALUES(
                    @DBProvider, @DBname,@DBpassword, @DBport, @ThresholdValue
                    )", conn))
                {
                    cmd.Parameters.AddWithValue(@"DBProvider", DBProvider);
                    cmd.Parameters.AddWithValue(@"DBname", DBname);
                    cmd.Parameters.AddWithValue(@"DBpassword", DBpassword);
                    cmd.Parameters.AddWithValue(@"DBport", DBport);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public void RestoreDefaultSettings(string conString)
        {
            using (conn = new SqlCeConnection(conString))
            {
                conn.Open();

                using (cmd = new SqlCeCommand(@"UPDATE Settings SET DBName = @DBname, DBPassword = @DBpassword, 
               DBPort = @DBport, ThresholdValue = @ThresholdValue 
                    )", conn))
                {
                    cmd.Parameters.AddWithValue(@"DBname", "SQL Server Compact");
                    cmd.Parameters.AddWithValue(@"DBpassword", "1234");
                    cmd.Parameters.AddWithValue(@"DBport", "1433");
                    cmd.Parameters.AddWithValue(@"ThresholdValue", "5");
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }


        public void UpdateThresholdValue(string ThresholdValue, String conString)
        {
            using (conn = new SqlCeConnection(conString))
            {
                conn.Open();

                using (cmd = new SqlCeCommand(@"UPDATE Settings SET ThresholdValue = @ThresholdValue
                    ", conn))
                {
                    cmd.Parameters.AddWithValue(@"ThresholdValue", ThresholdValue);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }





















        public string GetDBSetting(string conString)
        {
            string trasHold = null ;
            using (conn = new SqlCeConnection(conString))
            {
                conn.Open();

                using (cmd = new SqlCeCommand(@"Select * from Settings", conn))
                {
                    SqlCeDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        trasHold = reader.GetString(0);
                    }
                }
                conn.Close();
            }
            return trasHold;
        }

        public void TruncateDBSettings(string conString)
        {
            using (conn = new SqlCeConnection(conString))
            {
                conn.Open();

                using (cmd = new SqlCeCommand(@"DELETE FROM Settings", conn))
                {

                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }
    }
}
