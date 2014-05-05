using AForge.Imaging.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Tesseract;

namespace DeCaptcha
{
    public partial class FormMain : Form
    {
        System.Drawing.Bitmap image;
        /// <summary>
        /// Return oll methods used in FormMain
        /// </summary>
        public static Methods meth = new Methods();
        public static OCRMethods ocrMethod = new OCRMethods();
        public static ArrayList FewImagesNames = new ArrayList();
        public static ArrayList Proxy = new ArrayList();
        Bitmap[] temp = new Bitmap[24];
        string[] symboltext = new string[24];

        /// <summary>
        /// Return padding level selected in Form1 TesseractORC numericUpDown
        /// </summary>
        public static byte paddingLevel = 3;
        /// <summary>
        /// Return yours language traindata, if not found it will be "eng"
        /// </summary>
        public static string langToForm2 = "";
        /// <summary>
        /// Return selected chars from Form1 listbox
        /// </summary>
        public static string charsToForm2 = "";

        /// <summary>
        /// Use in CCL1 method
        /// </summary>
        public static byte neighborX = 1;
        /// <summary>
        /// Use in CCL1 method
        /// </summary>
        public static byte neighborY = 1;

        public static byte AforgeMaxH = 30;
        public static byte AforgeMaxW = 30;
        public static byte AforgeMinH = 5;
        public static byte AforgeMinW = 5;
        public static byte AforgeFilterBlobs = 1;

        public static int ResizeW = 30;
        public static int ResizeH = 30;
        public static bool ResizeAuto = false;

        public static Tesseract.EngineMode engMode = Tesseract.EngineMode.Default;

        FUNNNNN fun = new FUNNNNN();

        Point pt = new Point();

        #region DLL Import

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        #endregion DLL Import

        public FormMain()
        {
            InitializeComponent();
            fun.ToLogEvent += fun_ToLogEvent; // Event to reciev text from FANN OCR to textboxLOG
        }

        //  ****************************       Buttons      ****************************
        #region Buttons

        // Tesseractv3 Letters read
        private void buttonTesseractLetters_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            t.Text = "";
            BitmapFormat24bppRgb();
            if (!(comboBoxChars.Text.Length > 0)) return;
            if (L1.Image == null)
            {
                t.Text = ocrMethod.WriteToTextBox_OCRtesseractengine3(image, comboBoxChars.Text, engMode, false);
            }
            else
            {
                byte i = 0;
                foreach (var item in temp)
                {
                    if (item != null)
                    {
                        if (i > 23) break;
                        symboltext[i] = ocrMethod.WriteToTextBox_OCRtesseractengine3(temp[i], comboBoxChars.Text, engMode, true);
                        i++;
                    }
                }
                GetFullString();
            }
        }

        // Tesseractv3 Russian Letters read
        private void buttonTesseract302RUS_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            t.Text = "";
            BitmapFormat24bppRgb();
            if (L1.Image == null)
            {
                t.Text = ocrMethod.WriteToTextBox_OCRtesseractengine3(image, "rus", engMode, false);
            }
            else
            {
                byte i = 0;
                foreach (var item in temp)
                {
                    if (item != null)
                    {
                        if (i > 23) break;
                        symboltext[i] = ocrMethod.WriteToTextBox_OCRtesseractengine3(temp[i], "rus", engMode, true);
                        i++;
                    }
                }
                GetFullString();
            }
        }

        // Tesseractv3 Math
        private void buttonTesseract302Math_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            t.Text = "";
            BitmapFormat24bppRgb();
            if (L1.Image == null)
            {
                t.Text = ocrMethod.WriteToTextBox_OCRtesseractengine3(image, "math", engMode, false);
            }
            else
            {
                byte i = 0;
                foreach (var item in temp)
                {
                    if (item != null)
                    {
                        if (i > 23) break;
                        symboltext[i] = ocrMethod.WriteToTextBox_OCRtesseractengine3(temp[i], "math", engMode, true);
                        i++;
                    }
                }
                GetFullString();
            }
        }

        // Tesseractv3 Yours traindata file name
        private void buttonTesseract302Random_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            t.Text = "";
            BitmapFormat24bppRgb();
            FindLangMethod((byte) 0);
        }

        // Download Images from URL
        private void buttonDownloadImages_Click(object sender, EventArgs e)
        {
            if (textBoxURLtoDownloadImages.Text.Contains("http") && textBoxURLtoDownloadImages.Text.Length > 15)
            {
                progressBar1.Value = 0;
                progressBar1.Maximum = (int) numericUpDownURLImagesNum.Value;
                meth.NewEvent += meth_NewEvent;
                Thread downloadImagesThread = new Thread(DownLoadImageThreadMethod);
                downloadImagesThread.IsBackground = true; // If app closed, thread wil close too;
                downloadImagesThread.Start();
                buttonDownloadImages.Enabled = false;
                buttonProxyList.Enabled = false;
            }
            else
            {
                MessageBox.Show("Paste correct URL first!");
            }
        }
        void meth_NewEvent(int indeX)
        {
            try
            {
                progressBar1.Invoke(new Action(() => { progressBar1.Value = indeX; }));
            }
            catch (Exception ggg) { MessageBox.Show(ggg.Message); }
            if (progressBar1.Value == numericUpDownURLImagesNum.Value)
            {
                progressBar1.Invoke(new Action(() => { buttonDownloadImages.Enabled = true; buttonProxyList.Enabled = true; }));
            }
        }
        private void DownLoadImageThreadMethod()
        {
            meth.DownloadRemoteImageFile(textBoxURLtoDownloadImages.Text, (int)numericUpDownURLImagesNum.Value, Proxy, (int) numericUpDownProxyTimeOut.Value);
        }

        // Proxy Add
        private void buttonProxyList_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Proxy.Clear();
                    Proxy.AddRange(File.ReadAllLines(ofd.FileName));
                    labelProxyCount.Text = "num - " + Proxy.Count.ToString();
                }
                catch (Exception bbb)
                {
                    MessageBox.Show(bbb.Message);
                }
            }
        }

        // Check for Image in pictureBox
        private bool DoCheck()
        {
            if (Engine.FULLPATH == "")
            {
                MessageBox.Show("Нужно выбрать файл картинку");
                buttonOpen.PerformClick();
                return false;
            }
            else
            {
                return true;
            }
        }

        // Open
        private void button1_Click(object sender, EventArgs e)
        {
            ClearImages();
            SetImager(temp);
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files(*.jpg; *.jpeg; *.bmp; *.png; *.tif; *.gif)|*.jpg; *.jpeg; *.bmp; *.png; *.tif; *.gif";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Engine.FULLPATH = ofd.FileName.ToString();
                    System.Drawing.Bitmap image = (Bitmap)Bitmap.FromFile(Engine.FULLPATH);
                    AForge.Imaging.Image.Clone(image, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    pb1.Image = image;
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
        }

        // Reload
        private void button2_Click(object sender, EventArgs e)
        {
            ClearImages();
            SetImager(temp);
            if (!DoCheck()) return;
            System.Drawing.Bitmap image = (Bitmap)Bitmap.FromFile(Engine.FULLPATH);
            pb1.Image = image;
        }

        // Save
        private void button3_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Image Files (*.jpg)|*.jpg";
            save.FilterIndex = 4;
            save.RestoreDirectory = true;
            if (save.ShowDialog() == DialogResult.OK)
            {
                pb1.Image.Save(save.FileName);
            }
        }

        // Open Few Images
        private void buttonOpenFewImages_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files(*.jpg; *.jpeg; *.bmp; *.png; *.tif; *.gif)|*.jpg; *.jpeg; *.bmp; *.png; *.tif; *.gif";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FewImagesNames.Clear(); // Delete old ArrayList, not summ it.
                foreach (var item in ofd.FileNames)
                {
                    FewImagesNames.Add(item);
                }
                labelArrayListImagesCount.Text = FewImagesNames.Count.ToString();
            }
        }

        // Run if Filter
        private void buttonRunINFilter_Click(object sender, EventArgs e)
        {
            if (FewImagesNames.Count <= 0) return;
            paddingLevel = (byte) numericUpDownORCPaddingLevel.Value;
            langToForm2 = FindLangMethod((byte) 1);
            charsToForm2 = (string) comboBoxChars.Text;

            progressBar1.Value = 0;
            progressBar1.Maximum = FewImagesNames.Count;

            if (checkBoxUseOtherCCL.Checked == true)
            {
                Form2.CCLTYPE = comboBoxUseOtherCCL.Text;
                neighborX = (byte)numericUpDownCCL1X.Value;
                neighborY = (byte)numericUpDownCCL1Y.Value;
            }
            else
            {
                Form2.CCLTYPE = "";
            }

            ResizeW = (int)numericUpDownResizeW.Value;
            ResizeH = (int)numericUpDownResizeH.Value;

            Form2 fm2 = new Form2();
            fm2.NewEvent += fm2_NewEvent;
            fm2.ShowDialog();
        }
        void fm2_NewEvent(int indeX)
        {
            try
            {
                progressBar1.Value = indeX;
            }
            catch (Exception ggg) { MessageBox.Show(ggg.Message); }
        }

        // Read TESTDATA for folders, and all *.png in this folders; create new image, name - folder, content - all images in this folder
        private void buttonGenerateCharsPapers_Click(object sender, EventArgs e)
        {
            try
            {
                List<GenPapers> genpaperlist = new List<GenPapers> { };
                GenPapers tempgenpaper = new GenPapers();
                string[] dirs = Directory.GetDirectories(Environment.GetEnvironmentVariable("userprofile") + "\\Desktop\\TESTDATA\\");

                if (dirs.Length < 1) return;

                // Create objects genpaperlist with 1 - folder name, 2 - all *.png files in this folder
                foreach (var item in dirs)
                {
                    string bb = item.Substring(item.LastIndexOf(@"\") + 1);
                    if (bb == "Images" || bb == "Garbage") continue;
                    genpaperlist.Add(new GenPapers { dir = item.ToString(), files = Directory.GetFiles(item.ToString(), "*.png") });
                }

                // Check is directory already Exist
                string SaveFilesPath = Environment.GetEnvironmentVariable("userprofile") + "\\Desktop\\TESTREADY\\";
                bool isExists = System.IO.Directory.Exists(SaveFilesPath);
                if (!isExists)
                    System.IO.Directory.CreateDirectory(SaveFilesPath);

                progressBar1.Value = 0;
                progressBar1.Maximum = genpaperlist.Count;

                //////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////

                // Получить общее число картинок
                uint pixsum = 0;
                foreach (var item in genpaperlist)
                {
                    pixsum += (uint) item.RetrunFilesCount();
                }

                // Получить высоту и ширину первой картинки
                int pixW = 0;
                int pixH = 0;
                string nameF1 = genpaperlist[0].files[0].ToString();
                Bitmap bmapTemp = new Bitmap(nameF1);
                pixW = bmapTemp.Width;
                pixH = bmapTemp.Height;

                // Расчитать размер требуемого полотна дла всех картинок
                int xxxx = 10, MaxHeight = 10;
                for (int i = 0; i < pixsum; i++)
                {
                    if (xxxx < 1950) { }
                    else
                    {
                        if (MaxHeight > 18950)
                        {
                            break;
                        }
                        xxxx = -50; MaxHeight += 60;
                    }
                    xxxx += 60;
                }

                MaxHeight += 150;

                Bitmap BIGbit = new Bitmap(2000, MaxHeight); //9000
                List<string> TempBoxFile = new List<string>(); // Box file variable
                int x = 10, y = 10;

                for (int i = 0; i < genpaperlist.Count; i++)
                {
                    tempgenpaper = (GenPapers)genpaperlist[i];

                    //// Get directory name, and use it for name new .tif file
                    string aaa = tempgenpaper.dir.Substring(tempgenpaper.dir.LastIndexOf(@"\") + 1);

                    using (Graphics g = Graphics.FromImage(BIGbit))
                    {
                        foreach (var item in tempgenpaper.files)
                        {
                            if (x < 1950)
                            {
                                Bitmap filesbitmap = new Bitmap(item);
                                g.DrawImage(filesbitmap, x, y);
                                TempBoxFile.Add(aaa + " " + x + " " + (MaxHeight - y - filesbitmap.Height) + " " + (x + filesbitmap.Width) + " " + (MaxHeight - y) + " 0"); //9000
                                //tesseract makebox returns co-ordinates as (left,bottom, right, top) with reference of X-Y axis = 0,0 
                                //being at the bottom-left of the image, whereas GIMP use top-left.
                            }
                            else
                            {
                                if (y > MaxHeight) //8950
                                {
                                    break;
                                }
                                x = -50;
                                y += 60;
                            }
                            x += 60;
                        }
                    }
                    progressBar1.PerformStep();
                }
                BIGbit.Save(SaveFilesPath + "test.arial.exp0.tif", System.Drawing.Imaging.ImageFormat.Tiff);
                File.WriteAllLines(SaveFilesPath + "test.arial.exp0.box", TempBoxFile.ToArray());
                File.WriteAllLines(SaveFilesPath + "test.arialexp0", TempBoxFile.ToArray());
            }
            catch (Exception excep)
            {
                MessageBox.Show(excep.Message);
            }
            MessageBox.Show("Work done!");
        }

        // Tesseract 302 find Char, draw in bitmap
        private void buttonFindCharsTesseract302_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            BitmapFormat24bppRgb();
            temp = meth.FindCharsByTesseract302(image, (int) numericUpDownORCPaddingLevel.Value);
            SetImager(temp);
        }

        // Filter
        private void button5_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            BitmapFormat24bppRgb();

            int red1 = Convert.ToInt32(r1.Value);
            int red2 = Convert.ToInt32(r2.Value);

            int grn1 = Convert.ToInt32(g1.Value);
            int grn2 = Convert.ToInt32(g2.Value);

            int blu1 = Convert.ToInt32(b1.Value);
            int blu2 = Convert.ToInt32(b2.Value);

            Engine.Cfilter.Red = new AForge.IntRange(red1, red2);
            Engine.Cfilter.Green = new AForge.IntRange(grn1, grn2);
            Engine.Cfilter.Blue = new AForge.IntRange(blu1, blu2);

            Bitmap nImage = Engine.Cfilter.Apply(image);
            pb1.Image = nImage;
        }

        // Dilate \
        private void buttonDilate1_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            BitmapFormat24bppRgb();

            Engine.Dfilter1.ApplyInPlace(image);

            pb1.Image = image;
        }

        // Dilate /
        private void buttonDilate2_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            BitmapFormat24bppRgb();

            Engine.Dfilter2.ApplyInPlace(image);

            pb1.Image = image;
        }

        // Skeleton
        private void Skeleton_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            BitmapFormat24bppRgb();

            Engine.FilterX.ApplyInPlace(image);

            Bitmap grayImage = Engine.FilterGrayscale.Apply(image);

            pb1.Image = grayImage;

            Engine.CED.ApplyInPlace(grayImage);

            pb1.Image = grayImage;
        }

        // Binarize
        private void buttonBinarize_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            try
            {
                System.Drawing.Bitmap imageX = new Bitmap(pb1.Image);

                FiltersSequence SEQLOC = new FiltersSequence();
                SEQLOC.Add(Grayscale.CommonAlgorithms.BT709);
                SEQLOC.Add(new OtsuThreshold());
                imageX = SEQLOC.Apply(imageX);
                pb1.Image = imageX;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        // Grayscale
        private void buttonGrayScale_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            BitmapFormat24bppRgb();

            Bitmap grayImage = Engine.FilterGrayscale.Apply(image);
            pb1.Image = grayImage;
        }

        // Median
        private void buttonMedian_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            BitmapFormat24bppRgb();

            System.Drawing.Bitmap newImage = Engine.FilterMedian.Apply(image);

            pb1.Image = newImage;
        }

        // Fill
        private void buttonFill_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            BitmapFormat24bppRgb();
            Engine.FilterX.Apply(image);
            Engine.FilterCFF.Tolerance = Color.FromArgb(150, 92, 92);
            Engine.FilterCFF.FillColor = Color.FromArgb(255, 255, 255);
            Engine.FilterCFF.StartingPoint = new AForge.IntPoint(5, 5);
            Engine.FilterCFF.Apply(image);

            pb1.Image = image;
        }

        // Zoom
        private void buttonZoom_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            pb1.Image = meth.GenerateThumbnail(pb1.Image, 200);
        }

        // Zoom Out
        private void buttonZoomOut_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            pb1.Image = meth.GenerateThumbnail(pb1.Image, 50);
        }

        // Invert
        private void buttonInvert_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            System.Drawing.Bitmap scrBitmap = (Bitmap)pb1.Image;
            pb1.Image = meth.Invert(scrBitmap);
        }

        // Delete color
        private void buttonColorsNum_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            Color col = Color.Black;
            int red = (int)r2.Value;
            int grn = (int)g2.Value;
            int blu = (int)b2.Value;
            pb1.Image = meth.DeleteColor((Bitmap)pb1.Image, col, red, grn, blu);
        }

        // Contrast
        private void buttonContrast_Click(object sender, EventArgs e)
        {
            if (DoCheck())
            {
                System.Drawing.Bitmap scrBitmap = (Bitmap)pb1.Image;
                pb1.Image = meth.Contrast(scrBitmap, 50);
            }
        }

        // Kill Noise
        private void buttonFloodFill_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            System.Drawing.Bitmap scrBitmap = (Bitmap)pb1.Image;
            int[, ,] ma = new int[3, scrBitmap.Width, scrBitmap.Height];
            meth.refreshMatrix(0, ref ma, scrBitmap);
            pb1.Image = meth.killNoise(0, ref ma, scrBitmap, trackBarKillNoise.Value);
        }

        // Clear Noise
        private void buttonClearNoise_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            System.Drawing.Bitmap scrBitmap = (Bitmap)pb1.Image;
            int[, ,] ma = new int[3, scrBitmap.Width, scrBitmap.Height];
            meth.refreshMatrix(0, ref ma, scrBitmap);
            pb1.Image = meth.clearNoise(0, ref ma, scrBitmap, trackBarClearNoise.Value);
        }

        // Get symbols
        private void buttonGetSymbols_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            BitmapFormat24bppRgb();
            byte num = 0;
            AforgeMaxH = (byte)numericUpDownAFrorgeMaxH.Value;
            AforgeMaxW = (byte)numericUpDownAFrorgeMaxW.Value;
            AforgeMinH = (byte)numericUpDownAFrorgeMinH.Value;
            AforgeMinW = (byte)numericUpDownAFrorgeMinW.Value;
            AforgeFilterBlobs = (byte)numericUpDownAFrorgeFilterBlobs.Value;
            temp = meth.CCLCutLetters((Bitmap)pb1.Image, ref num);
            SetImager(temp);
        }

        // Colorize
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            BitmapFormat24bppRgb();
            pb1.Image = meth.CCLFilterColorize((Bitmap)pb1.Image);
        }

        // FUNN CREATE TRAIN DATA
        private void button4_Click(object sender, EventArgs e)
        {
            fun.CreateTrainData();
        }

        // FUNN LEARNING
        private void OCRNumber_Click(object sender, EventArgs e)
        {
            fun.Learning();
        }

        // FUNN OCR
        private void OCRMath_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            t.Text = "";
            BitmapFormat24bppRgb();
            textBoxLOG.Clear();
            byte iiiiii = 0;
            foreach (var item in temp)
            {
                if (item != null)
                {
                    if (iiiiii > 23) break;
                    symboltext[iiiiii] = fun.FUNNOCR(temp[iiiiii]);
                    iiiiii++;
                }
            }
            GetFullString();
        }
        void fun_ToLogEvent(double[] a, string b, string answer)
        {
            textBoxLOG.Text += "**********************> " + answer + Environment.NewLine;
            for (int i = 0; i < a.Length; i++)
            {
                textBoxLOG.Text += b[i] + " : " + a[i] + Environment.NewLine;
            }
        }

        // CCL1 Button
        private void buttonOtherCCL1_Click(object sender, EventArgs e)
        {
            if (!DoCheck()) return;
            temp = CCLMethods.CCL1Method((Bitmap)pb1.Image, (int)numericUpDownCCL1X.Value, (int)numericUpDownCCL1Y.Value);
            SetImager(temp);
        }

        // Resize all temp[] images to one size
        private void buttonResize_Click(object sender, EventArgs e)
        {
            int x = (int) numericUpDownResizeW.Value, y = (int) numericUpDownResizeH.Value;
            if (ResizeAuto)
	        {
                x = 0; y = 0;
                // Search for maximum W and H
                foreach (var item in temp)
                {
                    if (item != null)
                    {
                        if (x < item.Width) x = item.Width;
                        if (y < item.Height) y = item.Height;
                    }
                }
	        }
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] != null)
                {
                    temp[i] = meth.ResizeBitmap(temp[i], x, y); // x + 10, y + 10);
                }
            }
            SetImager(temp);
        }

        // GetBrightness kill
        private void buttonBrightness_Click(object sender, EventArgs e)
        {
            try
            {
                double a = Convert.ToDouble(textBoxGetBrightness.Text);
                pb1.Image = meth.GetBrightness((Bitmap)pb1.Image, a);
            }
            catch (Exception)
            {

            }
        }

        #endregion Buttons

        //  ****************************       Methods      ****************************
        #region Methods
        
        // Change Image to Format24bppRgb Method
        private void BitmapFormat24bppRgb()
        {
            System.Drawing.Bitmap imageX = new Bitmap(pb1.Image);
            this.image = AForge.Imaging.Image.Clone(imageX, System.Drawing.Imaging.PixelFormat.Format24bppRgb); //Format24bppRgb
        }

        // Get color from pictureBox
        private void pb1_MouseDown(object sender, MouseEventArgs e)
        {
            if (DoCheck())
            {
                try
                {
                    if (e.X <= pb1.Image.Width && e.Y <= pb1.Image.Height)
                    {
                        pt.X = e.X; pt.Y = e.Y;
                        Color pixelColor = GetPixelColor(pt);
                        r2.Value = pixelColor.R;
                        g2.Value = pixelColor.G;
                        b2.Value = pixelColor.B;
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// Return validate language traindata name
        /// </summary>
        /// <param name="FewIm">0 - is single image; 1 - is Few Images Form2</param>
        /// <returns></returns>
        private string FindLangMethod(byte FewIm)
        {
            var filenames4 = Directory
                .EnumerateFiles(Environment.CurrentDirectory + "\\tessdata\\", "*.traineddata", SearchOption.TopDirectoryOnly);
            string a = "";
            bool goOrnot = false;
            foreach (var item in filenames4)
            {
                if (Path.GetFileNameWithoutExtension(item) == textBoxYourLangTrainData.Text) goOrnot = true;
                a += Path.GetFileNameWithoutExtension(item) + " ";
            }
            if (goOrnot)
            {
                if (FewIm == 0)
                {
                    if (L1.Image == null)
                    {
                        t.Text = ocrMethod.WriteToTextBox_OCRtesseractengine3YourTrainData(image, textBoxYourLangTrainData.Text, engMode, false);
                    }
                    else
                    {
                        byte i = 0;
                        foreach (var item in temp)
                        {
                            if (item != null)
                            {
                                if (i > 23) break;
                                symboltext[i] = ocrMethod.WriteToTextBox_OCRtesseractengine3YourTrainData(temp[i], textBoxYourLangTrainData.Text, engMode, true);
                                i++;
                            }
                        }
                        GetFullString();
                    }
                }
                return textBoxYourLangTrainData.Text;
            }
            else
            {
                ToolTip ToolTip1 = new ToolTip();
                ToolTip1.SetToolTip(this.buttonTesseract302Random, "В tessdata найдены только следующие словари: " + Environment.NewLine + a);
                return "eng";
            }
        }

        /// <summary>
        /// Take temp[11], and set each to picturebox1,2,3...12
        /// </summary>
        /// <param name="img"></param>
        private void SetImager(Bitmap[] img)
        {
            int a = 0;
            foreach (var item in img)
            {
                if (item != null)
                {
                    a++;
                }
                groupBox3.Text = "OUT " + a.ToString();
            }
            try
            {
                L1.Image = img[0];
                L2.Image = img[1];
                L3.Image = img[2];
                L4.Image = img[3];
                L5.Image = img[4];
                L6.Image = img[5];
                L7.Image = img[6];
                L8.Image = img[7];
                L9.Image = img[8];
                L10.Image = img[9];
                L11.Image = img[10];
                L12.Image = img[11];
                L13.Image = img[12];
                L14.Image = img[13];
                L15.Image = img[14];
                L16.Image = img[15];
                L17.Image = img[16];
                L18.Image = img[17];
                L19.Image = img[18];
                L20.Image = img[19];
                L21.Image = img[20];
                L22.Image = img[21];
                L23.Image = img[22];
                L24.Image = img[23];
                labelL1.Text = img[0].Width + "x" + img[0].Height;
                labelL2.Text = img[1].Width + "x" + img[1].Height;
                labelL3.Text = img[2].Width + "x" + img[2].Height;
                labelL4.Text = img[3].Width + "x" + img[3].Height;
                labelL5.Text = img[4].Width + "x" + img[4].Height;
                labelL6.Text = img[5].Width + "x" + img[5].Height;
                labelL7.Text = img[6].Width + "x" + img[6].Height;
                labelL8.Text = img[7].Width + "x" + img[7].Height;
                labelL9.Text = img[8].Width + "x" + img[8].Height;
                labelL10.Text = img[9].Width + "x" + img[9].Height;
                labelL11.Text = img[10].Width + "x" + img[10].Height;
                labelL12.Text = img[11].Width + "x" + img[11].Height;
                labelL13.Text = img[12].Width + "x" + img[12].Height;
                labelL14.Text = img[13].Width + "x" + img[13].Height;
                labelL15.Text = img[14].Width + "x" + img[14].Height;
                labelL16.Text = img[15].Width + "x" + img[15].Height;
                labelL17.Text = img[16].Width + "x" + img[16].Height;
                labelL18.Text = img[17].Width + "x" + img[17].Height;
                labelL19.Text = img[18].Width + "x" + img[18].Height;
                labelL20.Text = img[19].Width + "x" + img[19].Height;
                labelL21.Text = img[20].Width + "x" + img[20].Height;
                labelL22.Text = img[21].Width + "x" + img[21].Height;
                labelL23.Text = img[22].Width + "x" + img[22].Height;
                labelL24.Text = img[23].Width + "x" + img[23].Height;
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// Clear contet of temp[]
        /// </summary>
        private void ClearImages()
        {
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = null;
            }
            labelL1.Text = "0x0"; labelL7.Text = "0x0"; labelL13.Text = "0x0"; labelL19.Text = "0x0";
            labelL2.Text = "0x0"; labelL8.Text = "0x0"; labelL14.Text = "0x0"; labelL20.Text = "0x0";
            labelL3.Text = "0x0"; labelL9.Text = "0x0"; labelL15.Text = "0x0"; labelL21.Text = "0x0";
            labelL4.Text = "0x0"; labelL10.Text = "0x0"; labelL16.Text = "0x0"; labelL22.Text = "0x0";
            labelL5.Text = "0x0"; labelL11.Text = "0x0"; labelL17.Text = "0x0"; labelL23.Text = "0x0";
            labelL6.Text = "0x0"; labelL12.Text = "0x0"; labelL18.Text = "0x0"; labelL24.Text = "0x0";
        }

        /// <summary>
        /// Set all content of symboltext[] to textbox Main Form
        /// </summary>
        private void GetFullString()
        {
            try
            {
                for (int i = 0; i < symboltext.Length; i++)
                {
                    t.Text += symboltext[i];
                }
                labelNumOfChars.Text = t.Text.Length.ToString();
                for (int y = 0; y < symboltext.Length; y++)
                {
                    symboltext[y] = "";
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void checkBoxResizeAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxResizeAuto.Checked == true)
            {
                ResizeAuto = true;
            }
            else
            {
                ResizeAuto = false;
            }
        }

        public static Color GetPixelColor(Point pt)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, pt.X, pt.Y);
            ReleaseDC(IntPtr.Zero, hdc);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                         (int)(pixel & 0x0000FF00) >> 8,
                         (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }

        #endregion Methods

        //  ****************************       Templates      ****************************
        #region Templates

        private void buttonPixel_Click(object sender, EventArgs e)
        {
            if (DoCheck())
            {
                System.Drawing.Bitmap imageX = new Bitmap(pb1.Image);
                System.Drawing.Bitmap image =
                    AForge.Imaging.Image.Clone(imageX, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                Engine.Cfilter.Red = new AForge.IntRange(0, 255);
                Engine.Cfilter.Green = new AForge.IntRange(0, 75);
                Engine.Cfilter.Blue = new AForge.IntRange(0, 75);

                Bitmap nImage = Engine.Cfilter.Apply(image);

                System.Drawing.Bitmap ImageXX = AForge.Imaging.Image.Clone
                    (nImage, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                Engine.Dfilter1.ApplyInPlace(ImageXX);

                System.Drawing.Bitmap imageXXX = AForge.Imaging.Image.Clone
                    (ImageXX, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                Engine.Dfilter2.ApplyInPlace(imageXXX);

                pb1.Image = imageXXX;

                //ocrMethod.WriteToTextBox_OCRtessnet2(imageXXX, 0);
            }
        }

        private void buttonSoftodrom_Click(object sender, EventArgs e)
        {

        }

        private void buttonMegafon_Click(object sender, EventArgs e)
        {
            
        }

        #endregion

        // Event change Tesseract Engine Mode, and buttons visible value
        private void trackBarEngineMode_Scroll(object sender, EventArgs e)
        {
            switch (trackBarEngineMode.Value)
            {
                case 0: labelEngineMode.Text = "EngineMode: CubeOnly"; engMode = Tesseract.EngineMode.CubeOnly;
                    buttonTesseract302Math.Enabled = false;
                    break;
                case 1: labelEngineMode.Text = "EngineMode: Default"; engMode = Tesseract.EngineMode.Default;
                    buttonTesseract302RUS.Enabled = true; buttonTesseract302Math.Enabled = true;
                    break;
                case 2: labelEngineMode.Text = "EngineMode: TesseractAndCube"; engMode = Tesseract.EngineMode.TesseractAndCube;
                    buttonTesseract302RUS.Enabled = false; buttonTesseract302Math.Enabled = false;
                    break;
                case 3: labelEngineMode.Text = "EngineMode: TesseractOnly"; engMode = Tesseract.EngineMode.TesseractOnly;
                    buttonTesseract302RUS.Enabled = true; buttonTesseract302Math.Enabled = true;
                    break;
            }
        }

        private void pb1_MouseMove(object sender, MouseEventArgs e)
        {
            labelMainXY.Text = "X: " + e.X + " Y: " + e.Y;
            this.labelMainPictureColorsToMousse.Visible = true;
            this.labelMainPictureColorsToMousse.Left = e.X + 20;
            this.labelMainPictureColorsToMousse.Top = e.Y + 20;
        }

        private void pb1_MouseLeave(object sender, EventArgs e)
        {
            this.labelMainPictureColorsToMousse.Visible = false;
        }
    }

    /// <summary>
    /// Класс для хранения FullName Folders with images.
    /// </summary>
    public class GenPapers : List<string>
    {
        public String dir { get; set; }
        public String[] files { get; set; }
        public int RetrunFilesCount()
        {
            return files.Length;
        }
    }
}