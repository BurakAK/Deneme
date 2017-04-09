﻿using Luxand;
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

        public void CreateDB(string conString)
        {
            using (SqlCeEngine en = new SqlCeEngine(conString))
            {
                if (!File.Exists("Subjects.sdf"))
                {
                    en.CreateDatabase();
                    CreateTable(conString);
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

        public List<TFaceRecord> LoadSubject(string conString)
        {
            InitializeSDK();

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
    }
}
