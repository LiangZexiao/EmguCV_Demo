using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.Cuda;
using System.IO;

namespace CVDemo
{
    public partial class Form1 : Form
    {
        private Capture _capture = null;
        private bool _captureInProgress;
        public Form1()
        {
            InitializeComponent();
            
            CvInvoke.UseOpenCL = false;
            try
            {
                _capture = new Capture();
                _capture.ImageGrabbed += ProcessFrame;
            }
            catch (NullReferenceException excpt)
            {

                MessageBox.Show(excpt.Message);
            }
        }
        private void ProcessFrame(object sender, EventArgs arg)
        {
            Mat frame = new Mat();
            _capture.Retrieve(frame, 0);
            Mat grayFrame = new Mat();
            CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);
            Mat smallGrayFrame = new Mat();
            CvInvoke.PyrDown(grayFrame, smallGrayFrame);
            Mat smoothedGrayFrame = new Mat();
            CvInvoke.PyrUp(smallGrayFrame, smoothedGrayFrame);

            //Image<Gray, Byte> smallGrayFrame = grayFrame.PyrDown();
            //Image<Gray, Byte> smoothedGrayFrame = smallGrayFrame.PyrUp();
            Mat cannyFrame = new Mat();
            CvInvoke.Canny(smoothedGrayFrame, cannyFrame, 100, 60);

            //Image<Gray, Byte> cannyFrame = smoothedGrayFrame.Canny(100, 60);

            captureImageBox.Image = frame;
            /*grayscaleImageBox.Image = grayFrame;
            smoothedGrayscaleImageBox.Image = smoothedGrayFrame;
            cannyImageBox.Image = cannyFrame;*/

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(_capture!=null)
            {
                if(_captureInProgress)
                {
                    button1.Text = "Start Capture";
                    _capture.Pause();
                }
                else
                {
                    button1.Text = "Stop Capture";
                    _capture.Start();
                }
                _captureInProgress = !_captureInProgress;
            }
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Mat frame = new Mat();
            _capture.Retrieve(frame, 0);
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "BMP文件|*.bmp|JPG文件|*.jpg|JPEG文件|*.jpeg|所有文件|*.*";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show(saveDialog.FileName);
                frame.Save(saveDialog.FileName);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Mat frame = new Mat();
            _capture.Retrieve(frame, 0);
            frame.Save("C:\\Users\\psxyz\\Desktop\\" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".bmp");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(timer1.Enabled)
            {
                button3.Text = "Stop Timer";
                timer1.Stop();
            }
            else
            {
                button3.Text = "Start Timer";
                timer1.Start();
            }
        }


        //FaceDetection
        private void button4_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void Run()
        {
            //Mat image = new Mat(@"C:\Users\psxyz\Desktop\test1.jpg", LoadImageType.Color); //Read the files as an 8-bit Bgr image  
            Mat frame = new Mat();
            _capture.Retrieve(frame, 0);
            string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            frame.Save(path+"Detect_img.jpg");

            Mat image = new Mat(path + "Detect_img.jpg", LoadImageType.Color); //Read the files as an 8-bit Bgr image 
             
            long detectionTime;
            List<Rectangle> faces = new List<Rectangle>();
            List<Rectangle> eyes = new List<Rectangle>();

            //The cuda cascade classifier doesn't seem to be able to load "haarcascade_frontalface_default.xml" file in this release
            //disabling CUDA module for now
            bool tryUseCuda = false;

            FaceDetecter.Detect(
              image, "haarcascade_frontalface_default.xml", "haarcascade_eye.xml",
              faces, eyes,
              tryUseCuda,
              out detectionTime);

            foreach (Rectangle face in faces)
                CvInvoke.Rectangle(image, face, new Bgr(Color.Red).MCvScalar, 2);
            foreach (Rectangle eye in eyes)
                CvInvoke.Rectangle(image, eye, new Bgr(Color.Blue).MCvScalar, 2);

            //display the image 
            /*ImageViewer.Show(image, String.Format(
            "Completed face and eye detection using {0} in {1} milliseconds",
            (tryUseCuda && CudaInvoke.HasCuda) ? "GPU"
            : CvInvoke.UseOpenCL ? "OpenCL"
            : "CPU",
            detectionTime));
             */
            imageBox1.Image = image;
            image.Save(path + "Detect_result.jpg");
        }

        private void 打开摄像头ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_capture != null)
            {
                if (_captureInProgress)
                {
                    button1.Text = "Start Capture";
                    _capture.Pause();
                }
                else
                {
                    button1.Text = "Stop Capture";
                    _capture.Start();
                }
                _captureInProgress = !_captureInProgress;
            }
        }

        private void 获取截图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mat frame = new Mat();
            _capture.Retrieve(frame, 0);
            imageBox1.Image = frame;
        }

        private void 保存截图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Emgu.CV.IImage frame = imageBox1.Image;
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "BMP文件|*.bmp|JPG文件|*.jpg|JPEG文件|*.jpeg|所有文件|*.*";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show(saveDialog.FileName);
                frame.Save(saveDialog.FileName);
            }
        }

        private void 识别人脸ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Run();
        }


    }
}
