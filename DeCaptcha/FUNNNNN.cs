using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FANN.Net;
using System.IO;
using System.Security.Cryptography;
using System.Drawing;
using System.Windows.Forms;

namespace DeCaptcha
{
    class FUNNNNN
    {
        NeuralNet net = new NeuralNet();

        int SUMMPIX = 0; // count of pixels in images, will be set next..
        int layerS = 120; // Layers of Neural Network
        int outputs = 0; // count of dirs, will be set next..

        string tempanswer = ""; // for outputs label names

        string[] DirNamebyNumber;

        string SaveFilesPath = Environment.GetEnvironmentVariable("userprofile") + "\\Desktop\\NEURALNETWORK\\";

        public delegate void DelegateToLog(double[] a, string b, string answer);
        public event DelegateToLog ToLogEvent;

        public void CreateTrainData()
        {
            int countOfALLImages = 0; // Count of train images, will be set next..

            List<GenPapers> genpaperlist = new List<GenPapers> { };
            GenPapers tempgenpaper = new GenPapers();

            string[] dirs = Directory.GetDirectories(Environment.GetEnvironmentVariable("userprofile") + "\\Desktop\\TESTDATA\\");
            if (dirs.Length < 1) return;

            foreach (var item in dirs) // Create object's, with FOLDERS and FILES in these
            {
                string bb = item.Substring(item.LastIndexOf(@"\") + 1);
                if (bb == "Images" || bb == "Garbage") continue;
                genpaperlist.Add(new GenPapers { dir = item.ToString(), files = Directory.GetFiles(item.ToString(), "*.png") });
            }

            // SET OUTPUT size (count of folders == searched symbols)
            outputs = genpaperlist.Count;

            foreach (var item in genpaperlist) // GET count of all images in all Folders.
            {
                countOfALLImages += item.RetrunFilesCount();
            }

            // GET/SET count of pixels in images. They must be SAME SIZE, ALL!
            Bitmap btmtemp = new Bitmap(genpaperlist[0].files[0]);
            int wxx = btmtemp.Width;
            int hxx = btmtemp.Height;
            SUMMPIX = wxx * hxx;
            btmtemp.Dispose();

            // Check is directory already Exist
            bool isExists = System.IO.Directory.Exists(SaveFilesPath);
            if (!isExists)
                System.IO.Directory.CreateDirectory(SaveFilesPath);

            double[,] input = new double[countOfALLImages, SUMMPIX]; // 
            double[,] output = new double[countOfALLImages, outputs];
            string[] hashes = new string[countOfALLImages]; // HASH array for each file *.png HASH

            DirNamebyNumber = new string[genpaperlist.Count]; // Save symbol == number of dir.

            // "Load bitmaps"
            int tmpINT = 0; // Count of train images
            for (int i = 0; i < genpaperlist.Count; i++)
            {
                tempgenpaper = (GenPapers)genpaperlist[i];

                // Get directory name, and use it for name new .tif file
                DirNamebyNumber[i] = tempgenpaper.dir.Substring(tempgenpaper.dir.LastIndexOf(@"\") + 1);

                foreach (var item in tempgenpaper.files)
                {
                    // Get current file *.png HASH
                    string hash = GetMD5HashFromFile(item);

                    if (hashes.Contains(hash))
                    {
                        continue; // IF new Hash already exist quit from current iteration, and go to next file *.png
                    }

                    // ADD new HASH to array
                    hashes[tmpINT] = hash;

                    Bitmap bit = new Bitmap(item);

                    int index = 0;
                    for (int y = 0; y < bit.Height; y++)
                        for (int x = 0; x < bit.Width; x++)
                            input[tmpINT, index++] = (bit.GetPixel(x, y) == Color.FromArgb(0, 0, 0)) ? 1 : 0;
                    for (int j = 0; j < outputs; j++)
                        output[tmpINT, j] = (j == i) ? 1 : 0;
                    tmpINT++;
                    bit.Dispose();
                }
            }

            try
            {
                //"Create train.tr"
                string train_data = tmpINT.ToString() + " " + SUMMPIX.ToString() + " " + outputs.ToString() + Environment.NewLine;
                for (int i = 0; i < tmpINT; i++)
                {
                    for (int x = 0; x < SUMMPIX; x++)
                        train_data += input[i, x].ToString() + ((x < SUMMPIX - 1) ? " " : Environment.NewLine);
                    for (int x = 0; x < outputs; x++)
                        train_data += output[i, x].ToString() + ((x < outputs - 1) ? " " : Environment.NewLine);
                    if (i % 40 == 0)
                    {
                        File.AppendAllText(SaveFilesPath + "train.tr", train_data);
                        train_data = "";
                    }
                }

                File.AppendAllText(SaveFilesPath + "train.tr", train_data);

                string abcd = "";
                for (int i = 0; i < DirNamebyNumber.Length; i++)
                {
                    abcd += DirNamebyNumber[i];
                }
                File.AppendAllText(SaveFilesPath + "CONFIG.txt", SUMMPIX.ToString() + " " + abcd);
            }
            catch (Exception gdgdggdgdgd)
            {
                MessageBox.Show(gdgdggdgdgd.Message + " FUNNNNN.CreateTrainData()");
            }
        }
        static string GetMD5HashFromFile(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }


        public void Learning()
        {
            try
            {
                string a = File.ReadAllText(SaveFilesPath + "CONFIG.txt");
                string[] b = a.Split(' ');
                int SumPix = Convert.ToInt32(b[0]);
                int Outpt = Convert.ToInt32(b[1].Length);

                uint[] layers = { (uint)SumPix, (uint)layerS, (uint)Outpt };
                net.CreateStandardArray(layers);
                net.RandomizeWeights(-0.1, 0.1);
                net.SetLearningRate(0.7f);
                TrainingData data = new TrainingData();
                data.ReadTrainFromFile(SaveFilesPath + "train.tr");
                net.TrainOnData(data, 1000, 0, 0.001f);
                net.Save(SaveFilesPath + "FANNLearning.ann");
            }
            catch (Exception gsgsd)
            {
                MessageBox.Show(gsgsd.Message + " FUNNNNN.Learning()");
            }
        }



        int ttt = 0;
        public string FUNNOCR(Bitmap img)
        {
            if (ttt == 0)
            {
                try
                {
                    net.CreateFromFile(SaveFilesPath + "FANNLearning.ann");

                    string a = File.ReadAllText(SaveFilesPath + "CONFIG.txt");
                    string[] b = a.Split(' ');
                    SUMMPIX = Convert.ToInt32(b[0]);
                    tempanswer = b[1];

                    ttt += 1;
                }
                catch (Exception gsdgdgs)
                {
                    MessageBox.Show(gsdgdgs.Message);
                }
                return OCR(img);
            }
            else
            {
                return OCR(img);
            }
        }

        private string OCR(Bitmap img)
        {
            string answer = "";
            try
            {
                int whx = img.Width * img.Height;

                if (SUMMPIX != whx)
                {
                    MessageBox.Show("Количество точек в изображениях не сходится, в нейросети сейчас на вход требуется " +
                        SUMMPIX + " а на вход подаются " + whx);
                    return "0";
                }
                else
                {
                    double[] input = new double[whx];
                    int index = 0;
                    for (int y = 0; y < img.Height; y++)
                        for (int x = 0; x < img.Width; x++)
                            input[index++] = (img.GetPixel(x, y) == Color.FromArgb(0, 0, 0)) ? 1 : 0;

                    double[] result = net.Run(input);
                    double max = result[0];

                    if (tempanswer.Length != result.Length)
                    {
                        MessageBox.Show("Количество выходов не сходится, в нейросети сейчас требуется " +
                            result.Length + ", а подается " + tempanswer.Length);
                        return "0";
                    }

                    int max_num = 0;
                    for (int i = 0; i < result.Length; i++)
                        if (result[i] > max)
                        {
                            max_num = i;
                            max = result[i];
                        }
                    answer = Convert.ToString(tempanswer[max_num]);
                    if (ToLogEvent != null)
                        ToLogEvent(result, tempanswer, answer);
                }
            }
            catch (Exception gdsgsdgsdgsg)
            {
                MessageBox.Show(gdsgsdgsdgsg.Message + " FUNNNNN.FUNNOCR.OCR");
            }
            return answer;
        }    
    }        
}
