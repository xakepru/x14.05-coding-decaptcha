using AForge.Imaging.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Tesseract;

namespace DeCaptcha
{
    /// <summary>
    /// Return all used Methods for Decaptcha
    /// </summary>
    public class Methods
    {
        // Event to send current index to Progress bar in FormMain
        public delegate void ProgBarEvent(int indeX);
        public event ProgBarEvent NewEvent;

        /// <summary>
        /// Zoom Bitmap to x2
        /// </summary>
        /// <param name="original">Image to Zoom</param>
        /// <param name="percentage">Percent</param>
        /// <returns></returns>
        public Image GenerateThumbnail(Image original, int percentage)
        {
            if (percentage < 1)
            {
                throw new Exception("Thumbnail size must be aat least 1% of the original size");
            }
            Bitmap tn = new Bitmap(Convert.ToInt32(original.Width * 0.01f * percentage), Convert.ToInt32(original.Height * 0.01f * percentage));
            Graphics g = Graphics.FromImage(tn);
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            //experiment with this...
            g.DrawImage(original, new Rectangle(0, 0, tn.Width, tn.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel);
            g.Dispose();
            return (Image)tn;
        }
        
        
        /// <summary>
        /// Invert Bitmap
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Bitmap Invert(Bitmap source)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(source.Width, source.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            // create the negative color matrix
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
            {
                new float[] {-1, 0, 0, 0, 0},
                new float[] {0, -1, 0, 0, 0},
                new float[] {0, 0, -1, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {1, 1, 1, 0, 1}
            });

            // create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            attributes.SetColorMatrix(colorMatrix);

            g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                        0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();

            return newBitmap;
        }
        
        /// <summary>
        /// Replace selected color to "col"
        /// </summary>
        /// <param name="scrBitmap"></param>
        /// <param name="col">New color</param>
        /// <param name="red"></param>
        /// <param name="grn"></param>
        /// <param name="blu"></param>
        /// <returns></returns>
        public Bitmap DeleteColor(Bitmap scrBitmap, Color col, int red, int grn, int blu)
        {
            try
            {
                //You can change your new color here. Red,Green,LawnGreen any..
                Color newColor = col;
                Color actulaColor;
                //make an empty bitmap the same size as scrBitmap
                for (int i = 0; i < scrBitmap.Width; i++)
                {
                    for (int j = 0; j < scrBitmap.Height; j++)
                    {
                        //get the pixel from the scrBitmap image
                        actulaColor = scrBitmap.GetPixel(i, j);
                        // > 150 because.. Images edges can be of low pixel colr. if we set all pixel color to new then there will be no smoothness left.
                        if (
                            actulaColor.R == red
                            || actulaColor.R == red - 1 || actulaColor.R == red - 2 || actulaColor.R == red - 3
                            || actulaColor.R == red - 4 || actulaColor.R == red - 5 || actulaColor.R == red - 6
                            || actulaColor.R == red + 1 || actulaColor.R == red + 2 || actulaColor.R == red + 3
                            || actulaColor.R == red + 4 || actulaColor.R == red + 5 || actulaColor.R == red + 6

                            && actulaColor.G == grn
                            || actulaColor.G == grn - 1 || actulaColor.G == grn - 2 || actulaColor.G == grn - 3
                            || actulaColor.G == grn - 4 || actulaColor.G == grn - 5 || actulaColor.G == grn - 6
                            || actulaColor.G == grn + 1 || actulaColor.G == grn + 2 || actulaColor.G == grn + 3
                            || actulaColor.G == grn + 4 || actulaColor.G == grn + 5 || actulaColor.G == grn + 6

                            && actulaColor.B == blu
                            || actulaColor.B == blu - 1 || actulaColor.B == blu - 2 || actulaColor.B == blu - 3
                            || actulaColor.B == blu - 4 || actulaColor.B == blu - 5 || actulaColor.B == blu - 6
                            || actulaColor.B == blu + 1 || actulaColor.B == blu + 2 || actulaColor.B == blu + 3
                            || actulaColor.B == blu + 4 || actulaColor.B == blu + 5 || actulaColor.B == blu + 6)

                            scrBitmap.SetPixel(i, j, newColor);
                        else
                            scrBitmap.SetPixel(i, j, actulaColor);
                    }
                }
            }
            catch (Exception fgasgds)
            {
                MessageBox.Show(fgasgds.Message);
            }
            return scrBitmap;
        }

        /// <summary>
        /// Check input numbers to 0 <= x <= 255; Else return 0
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public Int32 CheckForNumber(string num)
        {
            try
            {
                int a = Convert.ToInt32(num);
                if (a >= 0 && a <= 255)
                {
                    return a;
                }
                return 0;
            }
            catch (Exception)
            {
                MessageBox.Show("Введите правильное число");
                return 0;
            }
        }

        /// <summary>
        /// Contrast Bitmap
        /// </summary>
        /// <param name="sourceBitmap"></param>
        /// <param name="threshold">Power</param>
        /// <returns></returns>
        public Bitmap Contrast(Bitmap sourceBitmap, int threshold)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                        sourceBitmap.Width, sourceBitmap.Height),
                                        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);


            double contrastLevel = Math.Pow((100.0 + threshold) / 100.0, 2);


            double blue = 0;
            double green = 0;
            double red = 0;


            for (int k = 0; k + 4 < pixelBuffer.Length; k += 4)
            {
                blue = ((((pixelBuffer[k] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                green = ((((pixelBuffer[k + 1] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                red = ((((pixelBuffer[k + 2] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                if (blue > 255)
                { blue = 255; }
                else if (blue < 0)
                { blue = 0; }


                if (green > 255)
                { green = 255; }
                else if (green < 0)
                { green = 0; }


                if (red > 255)
                { red = 255; }
                else if (red < 0)
                { red = 0; }


                pixelBuffer[k] = (byte)blue;
                pixelBuffer[k + 1] = (byte)green;
                pixelBuffer[k + 2] = (byte)red;
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);


            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                        resultBitmap.Width, resultBitmap.Height),
                                        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);


            Marshal.Copy(pixelBuffer, 0, resultData.Scan0, pixelBuffer.Length);
            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }

        /// <summary>
        /// Refresh Matrix (Need before Kill Noise)
        /// </summary>
        /// <param name="k"></param>
        /// <param name="a"></param>
        /// <param name="bmp"></param>
        public void refreshMatrix(int k, ref int[, ,] a, Bitmap bmp)
        {

            for (int i = 1; i < bmp.Width - 1; i++)
            {
                for (int j = 1; j < bmp.Height - 1; j++)
                {
                    int tmp = 0;
                    if (bmp.GetPixel(i, j).GetBrightness() == 0) tmp++;
                    if (bmp.GetPixel(i, j + 1).GetBrightness() == 0) tmp++;
                    if (bmp.GetPixel(i, j - 1).GetBrightness() == 0) tmp++;
                    if (bmp.GetPixel(i + 1, j).GetBrightness() == 0) tmp++;
                    if (bmp.GetPixel(i + 1, j + 1).GetBrightness() == 0) tmp++;
                    if (bmp.GetPixel(i + 1, j - 1).GetBrightness() == 0) tmp++;
                    if (bmp.GetPixel(i - 1, j).GetBrightness() == 0) tmp++;
                    if (bmp.GetPixel(i - 1, j + 1).GetBrightness() == 0) tmp++;
                    if (bmp.GetPixel(i - 1, j - 1).GetBrightness() == 0) tmp++;
                    a[k, i, j] = tmp;
                }
            }
        }
        
        /// <summary>
        /// Kill Noise
        /// </summary>
        /// <param name="k"></param>
        /// <param name="ma"></param>
        /// <param name="bmp"></param>
        /// <param name="Power">Numbers of pixels to be together</param>
        /// <returns></returns>
        public Bitmap killNoise(int k, ref int[, ,] ma, Bitmap bmp, int Power)
        {
            Bitmap newBitmap = new Bitmap(bmp.Width, bmp.Height);
            for (int i = 1; i < bmp.Width - 1; i++)
            {
                for (int j = 1; j < bmp.Height - 1; j++)
                {
                    if (ma[k, i, j] >= Power)
                    {
                        newBitmap.SetPixel(i, j, Color.Black);
                    }
                    else
                    {
                        newBitmap.SetPixel(i, j, Color.White);
                    }
                }
            }
            return newBitmap;
        }

        /// <summary>
        /// Clear Noise (not work)
        /// </summary>
        /// <param name="k"></param>
        /// <param name="ma"></param>
        /// <param name="bmp"></param>
        /// <param name="Power">Number of Pixels</param>
        /// <returns></returns>
        public Bitmap clearNoise(int k, ref int[, ,] ma, Bitmap bmp, int Power)
        {
            Bitmap newBitmap = new Bitmap(bmp.Width, bmp.Height);
            for (int i = 1; i < bmp.Width - 1; i++)
            {
                for (int j = 1; j < bmp.Height - 1; j++)
                {
                    if (ma[k, i, j] <= Power) newBitmap.SetPixel(i, j, Color.White);
                }
            }
            return newBitmap;
        }

        /// <summary>
        /// Cut Letters with use CCL AForgeNET
        /// </summary>
        /// <param name="image"></param>
        /// <param name="num">Ref number point of rectangles</param>
        /// <returns></returns>
        public Bitmap[] CCLCutLetters(Bitmap image, ref byte num)
        {
            Bitmap[] temp = new Bitmap[24];

            Engine.bc.MaxHeight = FormMain.AforgeMaxH;
            Engine.bc.MaxWidth = FormMain.AforgeMaxW;

            Engine.bc.MinHeight = FormMain.AforgeMinH;
            Engine.bc.MinWidth = FormMain.AforgeMinW;

            if (FormMain.AforgeFilterBlobs == 1)
            {
                Engine.bc.FilterBlobs = true;
            }
            else
            {
                Engine.bc.FilterBlobs = false;
            }

            Engine.bc.ProcessImage(image);
            Rectangle[] rects = Engine.bc.GetObjectsRectangles();
            // process blobs
            int a = 0;
            foreach (Rectangle rect in rects)
            {
                if (a > 23) break;
                Crop crop = new AForge.Imaging.Filters.Crop(new Rectangle(rect.Location, rect.Size));
                temp[a] = crop.Apply(image);
                a++;
            }
            num = (byte) a;
            return temp;
        }

        /// <summary>
        /// Colorise with use CCL Filter
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public Bitmap CCLFilterColorize(Bitmap image)
        {
            Bitmap newImage = Engine.CCLfilter.Apply(image);
            return newImage;
        }

        /// <summary>
        /// Download images from pasted text in \DESTOP\TEST\
        /// </summary>
        /// <param name="getUrl"></param>
        /// <param name="num">count of images to get</param>
        /// <param name="proxy">proxy ArrayList</param>
        /// <param name="timeout">timeout for each request</param>
        public void DownloadRemoteImageFile(string getUrl, int num, ArrayList proxy, int timeout)
        {
            string[] filesnames = Directory.GetFiles(Environment.GetEnvironmentVariable("userprofile") + "\\Desktop\\TEST\\");
            int filescoun = filesnames.Length;
            for (int i = 0; i < num; i++)
            {
                // Send curretn index to progress bar in FormMain
                if (NewEvent != null) NewEvent(i + 1);

                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(getUrl);
                    request.Timeout = timeout;
                    // If have proxy set it to request properties
                    if (proxy.Count > 2)
                    {
                        Random rnd = new Random();
                        int proxyNum = rnd.Next(1, proxy.Count);
                        WebProxy myproxy = new WebProxy(proxy[proxyNum].ToString());
                        myproxy.BypassProxyOnLocal = false;
                        request.Proxy = myproxy;
                        request.Method = "GET";
                    }

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    // Check that the remote file was found. The ContentType
                    // check is performed since a request for a non-existent
                    // image file might be redirected to a 404-page, which would
                    // yield the StatusCode "OK", even though the image was not
                    // found.
                    if ((response.StatusCode == HttpStatusCode.OK ||
                        response.StatusCode == HttpStatusCode.Moved ||
                        response.StatusCode == HttpStatusCode.Redirect) &&
                        response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                    {
                        // Create folder if not exist
                        bool isExists = System.IO.Directory.Exists(Environment.GetEnvironmentVariable("userprofile") + "\\Desktop\\TEST\\");
                        if (!isExists)
                            System.IO.Directory.CreateDirectory(Environment.GetEnvironmentVariable("userprofile") + "\\Desktop\\TEST\\");

                        // take +1 number to save new image
                        filescoun++;
                        // if the remote file was found, download oit
                        using (Stream inputStream = response.GetResponseStream())
                        using (Stream outputStream = File.OpenWrite(Environment.GetEnvironmentVariable("userprofile") + "\\Desktop\\TEST\\" + filescoun.ToString() + ".png"))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            do
                            {
                                bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                                outputStream.Write(buffer, 0, bytesRead);
                            } while (bytesRead != 0);
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public Bitmap[] FindCharsByTesseract302(Bitmap img, int paddinglevel)
        {
            Bitmap[] temp = new Bitmap[24];
            int a = 0;
            Tesseract.PixToBitmapConverter conv = new PixToBitmapConverter();
            Pix p;

            int c, v;

            TesseractEngine OCRtesseractengine302 = new TesseractEngine("tessdata", "eng", FormMain.engMode);
            using (Tesseract.Page page = OCRtesseractengine302.Process(img))
            {
                using (var iter = page.GetIterator())
                // Block -> Para -> TextLine -> Word -> Symbol
                {
                    do
                    {
                        do
                        {
                            do
                            {
                                do 
                                {
                                    if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                                {
                                    // do whatever you need to do when a block (top most level result) is encountered.
                                }
                                if (iter.IsAtBeginningOf(PageIteratorLevel.Para))
                                {
                                    // do whatever you need to do when a paragraph is encountered.
                                }
                                if (iter.IsAtBeginningOf(PageIteratorLevel.TextLine))
                                {
                                    // do whatever you need to do when a line of text is encountered is encountered.
                                }
                                if (iter.IsAtBeginningOf(PageIteratorLevel.Word))
                                {
                                    // do whatever you need to do when a word is encountered is encountered.
                                }

                                // get bounding box for symbol
                                Rect symbolBounds;
                                if (iter.TryGetBoundingBox(PageIteratorLevel.Symbol, out symbolBounds))
                                {
                                    // do whatever you want with bounding box for the symbol
                                    if (a > 23) break;
                                    p = iter.GetImage(PageIteratorLevel.Symbol, paddinglevel, out c, out v); // True image
                                    if (p.Height > 5 && p.Width > 5)
                                    {
                                        temp[a] = conv.Convert(p);
                                        a++;
                                    }
                                }
                                } while (iter.Next(PageIteratorLevel.Symbol));
                            } while (iter.Next(PageIteratorLevel.Word, PageIteratorLevel.Block));
                        } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));
                    } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                }
            }
            return temp;
        }

        /// <summary>
        /// Resize small images to 30x40 and Binarize it
        /// </summary>
        /// <param name="b"></param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <returns></returns>
        public Bitmap ResizeBitmap(Bitmap img, int nWidth, int nHeight)
        {
            Bitmap newImage = new Bitmap(nWidth, nHeight);
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.FillRectangle(myBrush, 0, 0, nWidth, nHeight);
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height)); //Width, nHeight));
            }
            // NOT CORRECT WITH CCL1 images! Thats why need to use BRUSH, and FillRectangle
            FiltersSequence SEQLOC = new FiltersSequence();
            SEQLOC.Add(Grayscale.CommonAlgorithms.BT709);
            SEQLOC.Add(new OtsuThreshold());
            newImage = SEQLOC.Apply(newImage);
            return newImage;
        }

        /// <summary>
        /// Replace all pixels that have Brightness less than given to method (0 - 1)
        /// </summary>
        /// <param name="img"></param>
        /// <param name="GetBrightness"></param>
        /// <returns></returns>
        public Bitmap GetBrightness(Bitmap img, double GetBrightness)
        {
            using (Graphics g = Graphics.FromImage(img))
                g.DrawImage(img, 0, 0, img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
                for (int x = 0; x < img.Width; x++)
                    if (img.GetPixel(x, y).GetBrightness() < GetBrightness)         //GetBrightness() < GetBrightness
                        img.SetPixel(x, y, Color.White);                            // Color.White);
                    else
                        img.SetPixel(x, y, Color.Black);                            // Color.Black);
            return img;
        }
    }

    public class OCRMethods
    {
        //Engine.ocr.SetVariable("tessedit_char_whitelist", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.,$-/#&=()\"':?");

        // Net 4.0
        public string WriteToTextBox_OCRtesseractengine3(Bitmap image, string chars, EngineMode engMode, bool SingleChar)
        {
            string answer = "";

            // eng      - English language data for Tesseract 3.02
            // rus      - Russian
            // equ      - Math

            //OCRtesseractengine302.SetVariable("tessedit_pageseg_mode", "3"); // TesseractPageSegMode: PSM_SINGLE_LINE
            TesseractEngine OCRtesseractengine302;
            
            if (chars.Length == 0 || chars == "rus")
            {
                OCRtesseractengine302 =
                new TesseractEngine("tessdata", "rus", engMode);
            }
            else if (chars == "math")
            {
                OCRtesseractengine302 =
                new TesseractEngine("tessdata", "equ", engMode);
            }
            else
            {
                OCRtesseractengine302 =
                new TesseractEngine("tessdata", "eng", engMode);
                OCRtesseractengine302.SetVariable("tessedit_char_whitelist", chars);
            }

            if (SingleChar) OCRtesseractengine302.DefaultPageSegMode = PageSegMode.SingleChar;
            using (Tesseract.Page page = OCRtesseractengine302.Process(image))
            {
                answer = page.GetText();
            }

            return answer;
        }

        /// <summary>
        /// For button with Your own lang traindata copyed in to "tessdata"
        /// </summary>
        /// <param name="image"></param>
        /// <param name="lang">Write only first chars, before dote.</param>
        /// <param name="engMode"></param>
        /// <returns></returns>
        public string WriteToTextBox_OCRtesseractengine3YourTrainData(Bitmap image, string lang, EngineMode engMode, bool SingleChar)
        {
            string answer = "";

            TesseractEngine OCRtesseractengine302 = new TesseractEngine("tessdata", lang, engMode);

            if (SingleChar) OCRtesseractengine302.DefaultPageSegMode = PageSegMode.SingleChar;
            using (Tesseract.Page page = OCRtesseractengine302.Process(image))
            {
                answer = page.GetText();
            }

            return answer;
        }
    }

    /// <summary>
    /// Static class for use CCL methods (blobs); Connected-component labeling algorithm
    /// </summary>
    public static class CCLMethods
    {
        /// <summary>
        /// This method use CCL1 algorithm, return  Bitmap[] (Width > 5 && Height > 5)
        /// </summary>
        /// <param name="img">Source image</param>
        /// <param name="xXx">Pixels in X</param>
        /// <param name="yYy">Pixels in Y</param>
        /// <returns></returns>
        public static Bitmap[] CCL1Method(Bitmap img, int xXx, int yYy)
        {
            CCL1 ccl1 = new CCL1();
            Bitmap[] temp = new Bitmap[24];
            try
            {
                var imagess = ccl1.Process(img, xXx, yYy);
                int i = 0;
                foreach (var item in imagess)
                {
                    // Тут можно проверить, если объект слишком большой по отношению к другим,
                    // то можно попытаться его еще раз разбить с более жесткими условиями.
                    if (item.Value != null)
                    {
                        if (i > 23) break;
                        temp[i] = item.Value;
                        i++;
                    }
                }
            }
            catch (Exception)
            {

            }
            return temp;
        }
    }
}