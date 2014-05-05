using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DeCaptcha
{
    public partial class Form2 : Form
    {
        string SaveFilesPath = Environment.GetEnvironmentVariable("userprofile") + "\\Desktop\\TESTDATA\\";
        string MainImageSave = "";
        int ItemIndex = 0;
        Bitmap[] temp = new Bitmap[24];

        // Event to send current index to Progress bar in FormMain
        public delegate void ProgBarEvent(int indeX);
        public event ProgBarEvent NewEvent;

        // Will use selected CCL engine for find chars in image
        public static string CCLTYPE = "";

        public Form2()
        {
            InitializeComponent();
            Bitmap image = (Bitmap)Bitmap.FromFile((string)FormMain.FewImagesNames[0]);
            pictureBoxMain.Image = image;
            AnalyzeImage(image);
        }

        //  ****************************       Buttons      ****************************
        #region Buttons

        // OK button
        private void buttonOk_Click(object sender, EventArgs e)
        {
            SaveToFiles();
            ProcessNextItem();
            if (NewEvent != null) NewEvent(ItemIndex);
        }

        // Cancel button
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            ProcessNextItem();
            if (NewEvent != null) NewEvent(ItemIndex);
        }

        #endregion Buttons



        //  ****************************       Methods      ****************************
        #region Methods

        // Then click "OK" or "Cancel" button
        private void ProcessNextItem()
        {
            textBoxUserInputFullImageName.Clear();
            ItemIndex++;
            if (ItemIndex <= FormMain.FewImagesNames.Count -1)
            {
                Bitmap image = (Bitmap)Bitmap.FromFile((string)FormMain.FewImagesNames[ItemIndex]);
                pictureBoxMain.Image = image;
                AnalyzeImage(image);
                t1.Focus(); // SetFocus to t.1 then user press SAVE, for faster input
            }
            else
            {
                MessageBox.Show("Work done!", "That's all", MessageBoxButtons.OK);
                this.Close();
            }
        }

        // Analyze images and show result in Form2
        private void AnalyzeImage(Bitmap image)
        {
            ClearTextBoxAndLabels();

            pictureBoxOriginal.Image = image;
            // FILTERS TO IMAGE BEFORE OCR or CCL (Blob)
            System.Drawing.Bitmap imageX = new Bitmap(image);


            /////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////
               /////////////////////////// ВСЕ ВАШИ ФИЛЬТРЫ ЗДЕСЬ ///////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////

            //BINARIZE
            FiltersSequence SEQLOC = new FiltersSequence();
            SEQLOC.Add(Grayscale.CommonAlgorithms.BT709);
            SEQLOC.Add(new OtsuThreshold());
            image = SEQLOC.Apply(imageX);           

            //INVERT
            //image = FormMain.meth.Invert(image);
            
            //Brightness
            //image = FormMain.meth.GetBrightness(imageX, 0.4);

            /////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////

            pictureBoxMain.Image = image;

            ClearImages();

            #region finde rectangles
            if (CCLTYPE == "")
            {
                // Get symblos images
                temp = FormMain.meth.FindCharsByTesseract302(image, (int)FormMain.paddingLevel);
                GetResizeImages();
                SetImager(temp);
            }
            else
            {
                temp = CCLMethods.CCL1Method(image, FormMain.neighborX, FormMain.neighborY);
                GetResizeImages();
                SetImager(temp);
            }
            #endregion finde rectangles

            #region symblos
            string[] symboltext = new string[24];
            if (FormMain.langToForm2 != "eng")
            {
                // Get symblos string in textboxes
                byte i = 0;
                foreach (var item in temp)
                {
                    if (item != null)
                    {
                        if (i > 23) break;
                        symboltext[i] = FormMain.ocrMethod.WriteToTextBox_OCRtesseractengine3YourTrainData(item, FormMain.langToForm2, FormMain.engMode, true);
                        i++;
                    }
                }
                SetTexter(symboltext);
            }
            // Else it will use eng.traindata with selected chars in combobox
            else
            {
                // Get symblos string in textboxes
                byte i = 0;
                foreach (var item in temp)
                {
                    if (item != null)
                    {
                        if (i > 23) break;
                        symboltext[i] = FormMain.ocrMethod.WriteToTextBox_OCRtesseractengine3(item, FormMain.charsToForm2, FormMain.engMode, true);
                        i++;
                    }
                }
                SetTexter(symboltext);
            }
            #endregion symblos
        }

        private void ClearTextBoxAndLabels()
        {
            t1.Clear(); t2.Clear(); t3.Clear(); t4.Clear();
            t5.Clear(); t6.Clear(); t7.Clear(); t8.Clear();
            t9.Clear(); t10.Clear(); t11.Clear(); t12.Clear();
            t13.Clear(); t14.Clear(); t15.Clear(); t16.Clear();
            t17.Clear(); t18.Clear(); t19.Clear(); t20.Clear();
            t21.Clear(); t22.Clear(); t23.Clear(); t24.Clear();

            L1.Text = "0x0"; L2.Text = "0x0"; L3.Text = "0x0"; L4.Text = "0x0";
            L5.Text = "0x0"; L6.Text = "0x0"; L7.Text = "0x0"; L8.Text = "0x0";
            L9.Text = "0x0"; L10.Text = "0x0"; L11.Text = "0x0"; L12.Text = "0x0";
            L13.Text = "0x0"; L14.Text = "0x0"; L15.Text = "0x0"; L16.Text = "0x0";
            L17.Text = "0x0"; L18.Text = "0x0"; L19.Text = "0x0"; L20.Text = "0x0";
            L21.Text = "0x0"; L22.Text = "0x0"; L23.Text = "0x0"; L24.Text = "0x0";
        }

        private void GetResizeImages()
        {
            int x = FormMain.ResizeW, y = FormMain.ResizeH;
            if (FormMain.ResizeAuto)
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
                    temp[i] = FormMain.meth.ResizeBitmap(temp[i], x, y); // x + 10, y + 10);
                }
            }
        }

        /// <summary>
        /// Check string for some rule...
        /// </summary>
        /// <param name="symb"></param>
        /// <returns></returns>
        private string CheckSymbolRule(string symb)
        {
            if (Char.IsWhiteSpace(symb[0]))
            {
                return ""; // Delete all space
            }
            return symb.ToString();
        }

        private void ClearImages()
        {
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = null;
            }
        }

        /// <summary>
        /// Take temp[11], and set each to picturebox1,2,3...23
        /// </summary>
        /// <param name="img"></param>
        private void SetImager(Bitmap[] img)
        {
            try
            {
                p1.Image = img[0];
                p2.Image = img[1];
                p3.Image = img[2];
                p4.Image = img[3];
                p5.Image = img[4];
                p6.Image = img[5];
                p7.Image = img[6];
                p8.Image = img[7];
                p9.Image = img[8];
                p10.Image = img[9];
                p11.Image = img[10];
                p12.Image = img[11];
                p13.Image = img[12];
                p14.Image = img[13];
                p15.Image = img[14];
                p16.Image = img[15];
                p17.Image = img[16];
                p18.Image = img[17];
                p19.Image = img[18];
                p20.Image = img[19];
                p21.Image = img[20];
                p22.Image = img[21];
                p23.Image = img[22];
                p24.Image = img[23];
                L1.Text = img[0].Width + "x" + img[0].Height;
                L2.Text = img[1].Width + "x" + img[1].Height;
                L3.Text = img[2].Width + "x" + img[2].Height;
                L4.Text = img[3].Width + "x" + img[3].Height;
                L5.Text = img[4].Width + "x" + img[4].Height;
                L6.Text = img[5].Width + "x" + img[5].Height;
                L7.Text = img[6].Width + "x" + img[6].Height;
                L8.Text = img[7].Width + "x" + img[7].Height;
                L9.Text = img[8].Width + "x" + img[8].Height;
                L10.Text = img[9].Width + "x" + img[9].Height;
                L11.Text = img[10].Width + "x" + img[10].Height;
                L12.Text = img[11].Width + "x" + img[11].Height;
                L13.Text = img[12].Width + "x" + img[12].Height;
                L14.Text = img[13].Width + "x" + img[13].Height;
                L15.Text = img[14].Width + "x" + img[14].Height;
                L16.Text = img[15].Width + "x" + img[15].Height;
                L17.Text = img[16].Width + "x" + img[16].Height;
                L18.Text = img[17].Width + "x" + img[17].Height;
                L19.Text = img[18].Width + "x" + img[18].Height;
                L20.Text = img[19].Width + "x" + img[19].Height;
                L21.Text = img[20].Width + "x" + img[20].Height;
                L22.Text = img[21].Width + "x" + img[21].Height;
                L23.Text = img[22].Width + "x" + img[22].Height;
                L24.Text = img[23].Width + "x" + img[23].Height;
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// get string[], and set each to textboxSymbol1,2,3...24
        /// </summary>
        /// <param name="str"></param>
        private void SetTexter(string[] str)
        {
            try
            {
                t1.Text = CheckSymbolRule(str[0]);
                t2.Text = CheckSymbolRule(str[1]);
                t3.Text = CheckSymbolRule(str[2]);
                t4.Text = CheckSymbolRule(str[3]);
                t5.Text = CheckSymbolRule(str[4]);
                t6.Text = CheckSymbolRule(str[5]);
                t7.Text = CheckSymbolRule(str[6]);
                t8.Text = CheckSymbolRule(str[7]);
                t9.Text = CheckSymbolRule(str[8]);
                t10.Text = CheckSymbolRule(str[9]);
                t11.Text = CheckSymbolRule(str[10]);
                t12.Text = CheckSymbolRule(str[11]);
                t13.Text = CheckSymbolRule(str[12]);
                t14.Text = CheckSymbolRule(str[13]);
                t15.Text = CheckSymbolRule(str[14]);
                t16.Text = CheckSymbolRule(str[15]);
                t17.Text = CheckSymbolRule(str[16]);
                t18.Text = CheckSymbolRule(str[17]);
                t19.Text = CheckSymbolRule(str[18]);
                t20.Text = CheckSymbolRule(str[19]);
                t21.Text = CheckSymbolRule(str[20]);
                t22.Text = CheckSymbolRule(str[21]);
                t23.Text = CheckSymbolRule(str[22]);
                t24.Text = CheckSymbolRule(str[23]);
            }
            catch (Exception)
            {

            }
        }

        // Save Image and point symbols in files
        private void SaveToFiles()
        {
            string tempString = GetCorrectedString().Replace("\n", string.Empty);
            if (tempString == "") return;

            if (textBoxUserInputFullImageName.Text.Length > 0)
            {
                MainImageSave = SaveFilesPath + "\\Images\\" + textBoxUserInputFullImageName.Text + ".png";
            }
            else
            {
                MainImageSave = SaveFilesPath + "\\Images\\" + tempString + ".png";
            }

            try
            {
                // Create MAIN folder
                bool isExists = System.IO.Directory.Exists(SaveFilesPath);
                if (!isExists)
                    System.IO.Directory.CreateDirectory(SaveFilesPath);

                // Create IMAGES folder
                bool isExists2 = System.IO.Directory.Exists(SaveFilesPath + "\\Images\\");
                if (!isExists2)
                    System.IO.Directory.CreateDirectory(SaveFilesPath + "\\Images\\");

                // Create Garbage folder
                bool bb2 = System.IO.Directory.Exists(SaveFilesPath + "\\Garbage\\");
                if (!bb2)
                    System.IO.Directory.CreateDirectory(SaveFilesPath + "\\Garbage\\");

                // Saving MainPage
                pictureBoxOriginal.Image.Save(MainImageSave, System.Drawing.Imaging.ImageFormat.Png);
                
                // Create Chars folders and save there images
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i] != null)
                    {
                        if (GetTextBoxText(i) != "") // Save to normal dir //(i < tempString.Length && tempString[i].ToString() != "")
                        {
                            bool bb1 = System.IO.Directory.Exists(SaveFilesPath + "\\" + tempString[i].ToString() + "\\");
                            if (!bb1)
                                System.IO.Directory.CreateDirectory(SaveFilesPath + "\\" + tempString[i].ToString() + "\\");
                            temp[i].Save(SaveFilesPath + "\\" + tempString[i].ToString() + "\\"
                                + System.IO.Path.GetRandomFileName() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                        else // All over save to Garbage dir, if one symbol already exist
                        {
                            temp[i].Save(SaveFilesPath + "\\Garbage\\"
                                + System.IO.Path.GetRandomFileName() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(e.Message + Environment.NewLine + "Form2 SaveToFiles()");
            }
        }

        private string GetCorrectedString()
        {
            string a = t1.Text + t2.Text + t3.Text + t4.Text
                + t5.Text + t6.Text + t7.Text + t8.Text
                + t9.Text + t10.Text + t11.Text + t12.Text
                + t13.Text + t14.Text + t15.Text + t16.Text
                + t17.Text + t18.Text + t19.Text + t20.Text
                + t21.Text + t22.Text + t23.Text + t24.Text;
            return a;
        }

        private string GetTextBoxText(int num)
        {
            try
            {
                if (num == 0) return t1.Text;
                if (num == 1) return t2.Text;
                if (num == 2) return t3.Text;
                if (num == 3) return t4.Text;
                if (num == 4) return t5.Text;
                if (num == 5) return t6.Text;
                if (num == 6) return t7.Text;
                if (num == 7) return t8.Text;
                if (num == 8) return t9.Text;
                if (num == 9) return t10.Text;
                if (num == 10) return t11.Text;
                if (num == 11) return t12.Text;
                if (num == 12) return t13.Text;
                if (num == 13) return t14.Text;
                if (num == 14) return t15.Text;
                if (num == 15) return t16.Text;
                if (num == 16) return t17.Text;
                if (num == 17) return t18.Text;
                if (num == 18) return t19.Text;
                if (num == 19) return t20.Text;
                if (num == 20) return t21.Text;
                if (num == 21) return t22.Text;
                if (num == 22) return t23.Text;
                if (num == 23) return t24.Text;
            }catch (Exception){}
            return "";
        }

        #endregion Methods

        // Handle user input from all t1...t24
        private void t1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                SendKeys.Send("+{TAB}"); // simualte a shift-tab press
            }
            else
            {
                SendKeys.Send("{TAB}"); // Go to next "t." for faster input.
            }  
        }

        // Проверка условия при закрытии окна
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ( e.CloseReason == CloseReason.None )
            { e.Cancel = true; }
            else
            { e.Cancel = false; if (NewEvent != null) NewEvent(0); }
        }
    }
}
