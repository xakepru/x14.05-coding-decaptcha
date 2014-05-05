using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DeCaptcha
{
    public class CCL1 : IConnectedComponentLabeling
    {
        #region Member Variables

        private int[,] _board;
        private Bitmap _input;
        private int _width;
        private int _height;

        #endregion

        // MODIF ************************* MODIF ************************* MODIF
        int xXx = 2;
        int yYy = 2;
        // MODIF ************************* MODIF ************************* MODIF

        #region IConnectedComponentLabeling

        public IDictionary<int, Bitmap> Process(Bitmap Input, int x, int y)
        {
            // MODIF ************************* MODIF ************************* MODIF
            this.xXx = x; this.yYy = y;
            Bitmap input = new Bitmap(Input);
            input.RotateFlip(RotateFlipType.Rotate90FlipNone);
            // MODIF ************************* MODIF ************************* MODIF

            _input = input; // _input = input;
            _width = input.Width;
            _height = input.Height;
            _board = new int[_width, _height];

            Dictionary<int, List<Pixel>> patterns = Find();
            var images = new Dictionary<int, Bitmap>();

            foreach (KeyValuePair<int, List<Pixel>> pattern in patterns)
            {
                Bitmap bmp = CreateBitmap(pattern.Value);
                // MODIF ************************* MODIF ************************* MODIF
                bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                // MODIF ************************* MODIF ************************* MODIF
                images.Add(pattern.Key, bmp);
            }

            return images; // return images;
        }

        #endregion

        #region Protected Methods

        protected virtual bool CheckIsBackGround(Pixel currentPixel)
        {
            return currentPixel.color.A == 255 && currentPixel.color.R == 255 && currentPixel.color.G == 255 && currentPixel.color.B == 255;
        }

        #endregion

        #region Private Methods

        private Dictionary<int, List<Pixel>> Find()
        {
            int labelCount = 1;
            var allLabels = new Dictionary<int, Label>();

            for (int i = 0; i < _height; i++) // _height
            {
                for (int j = 0; j < _width; j++) // _width
                {
                    Pixel currentPixel = new Pixel(new Point(j, i), _input.GetPixel(j, i));

                    if (CheckIsBackGround(currentPixel))
                    {
                        continue;
                    }

                    IEnumerable<int> neighboringLabels = GetNeighboringLabels(currentPixel);
                    int currentLabel;

                    if (!neighboringLabels.Any())
                    {
                        currentLabel = labelCount;
                        allLabels.Add(currentLabel, new Label(currentLabel));
                        labelCount++;
                    }
                    else
                    {
                        currentLabel = neighboringLabels.Min(n => allLabels[n].GetRoot().Name);
                        Label root = allLabels[currentLabel].GetRoot();

                        foreach (var neighbor in neighboringLabels)
                        {
                            if (root.Name != allLabels[neighbor].GetRoot().Name)
                            {
                                allLabels[neighbor].Join(allLabels[currentLabel]);
                            }
                        }
                    }

                    _board[j, i] = currentLabel;
                }
            }


            Dictionary<int, List<Pixel>> patterns = AggregatePatterns(allLabels);

            return patterns;
        }

        private IEnumerable<int> GetNeighboringLabels(Pixel pix)
        {
            var neighboringLabels = new List<int>();

            // MODIF ************************* MODIF ************************* MODIF

            for (int i = pix.Position.Y - yYy; i <= pix.Position.Y + yYy && i < _height - yYy; i++) // _height
            {
                for (int j = pix.Position.X - xXx; j <= pix.Position.X + xXx && j < _width - xXx; j++) // _width
                {
                    if (i > -1 && j > -1 && _board[j, i] != 0)
                    {
                        neighboringLabels.Add(_board[j, i]);
                    }
                }
            }

            // MODIF ************************* MODIF ************************* MODIF

            /* ORIGINAL
            for (int i = pix.Position.Y - 1; i <= pix.Position.Y + 2 && i < _height - 1; i++)
            {
                for (int j = pix.Position.X - 1; j <= pix.Position.X + 2 && j < _width - 1; j++)
                {
                    if (i > -1 && j > -1 && _board[j, i] != 0)
                    {
                        neighboringLabels.Add(_board[j, i]);
                    }
                }
            }
            */
            return neighboringLabels;
        }

        private Dictionary<int, List<Pixel>> AggregatePatterns(Dictionary<int, Label> allLabels)
        {
            var patterns = new Dictionary<int, List<Pixel>>();

            for (int i = 0; i < _height; i++) //_height
            {
                for (int j = 0; j < _width; j++) //_width
                {
                    int patternNumber = _board[j, i];

                    if (patternNumber != 0)
                    {
                        patternNumber = allLabels[patternNumber].GetRoot().Name;

                        if (!patterns.ContainsKey(patternNumber))
                        {
                            patterns[patternNumber] = new List<Pixel>();
                        }

                        patterns[patternNumber].Add(new Pixel(new Point(j, i), Color.Black));
                    }
                }
            }

            return patterns;
        }

        private Bitmap CreateBitmap(List<Pixel> pattern)
        {
            int minX = pattern.Min(p => p.Position.X);
            int maxX = pattern.Max(p => p.Position.X);

            int minY = pattern.Min(p => p.Position.Y);
            int maxY = pattern.Max(p => p.Position.Y);

            int width = maxX + 1 - minX;
            int height = maxY + 1 - minY;

            var bmp = new Bitmap(width, height);

            foreach (Pixel pix in pattern)
            {
                bmp.SetPixel(pix.Position.X - minX, pix.Position.Y - minY, pix.color);//shift position by minX and minY
            }

            return bmp;
        }

        #endregion
    }

    public class Pixel
    {
        #region Public Properties

        public Point Position { get; set; }
        public Color color { get; set; }

        #endregion

        #region Constructor

        public Pixel(Point Position, Color color)
        {
            this.Position = Position;
            this.color = color;
        }

        #endregion
    }

    internal class Label
    {
        #region Public Properties

        public int Name { get; set; }

        public Label Root { get; set; }

        public int Rank { get; set; }
        #endregion

        #region Constructor

        public Label(int Name)
        {
            this.Name = Name;
            this.Root = this;
            this.Rank = 0;
        }

        #endregion

        #region Public Methods

        internal Label GetRoot()
        {
            if (this.Root != this)
            {
                this.Root = this.Root.GetRoot();//Compact tree
            }

            return this.Root;
        }

        internal void Join(Label root2)
        {
            if (root2.Rank < this.Rank)//is the rank of Root2 less than that of Root1 ?
            {
                root2.Root = this;//yes! then Root1 is the parent of Root2 (since it has the higher rank)
            }
            else //rank of Root2 is greater than or equal to that of Root1
            {
                this.Root = root2;//make Root2 the parent<br />
                if (this.Rank == root2.Rank)//both ranks are equal ?
                {
                    root2.Rank++;//increment Root2, we need to reach a single root for the whole tree
                }
            }
        }

        #endregion
    }

    public interface IConnectedComponentLabeling
    {
        IDictionary<int, Bitmap> Process(Bitmap input, int x, int y);
    }
}
