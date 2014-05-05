using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math;
using AForge.Math.Geometry;


namespace DeCaptcha
{
    public static class Engine
    {
        public static string FULLPATH = "";

        // ********************************     AForge      ********************************
        public static ColorFiltering Cfilter = new ColorFiltering();

        public static short[,] se1 = new short[,]
                {
                    { 1, 0, 0 },
                    { 0, 1, 0 },
                    { 0, 0, 1 }
                };
        public static short[,] se2 = new short[,]
                {
                    { 0, 0, 1 },
                    { 0, 1, 0 },
                    { 1, 0, 0 }
                };

        public static Dilatation Dfilter1 = new AForge.Imaging.Filters.Dilatation(se1);
        public static Dilatation Dfilter2 = new AForge.Imaging.Filters.Dilatation(se2);

        public static ConservativeSmoothing FilterX = new ConservativeSmoothing();
        public static Grayscale FilterGrayscale = new AForge.Imaging.Filters.Grayscale(0.2125, 0.7154, 0.0721);
        public static CannyEdgeDetector CED = new CannyEdgeDetector(0, 70);

        public static AForge.Imaging.Filters.Median FilterMedian = new AForge.Imaging.Filters.Median();
        public static PointedColorFloodFill FilterCFF = new PointedColorFloodFill();

        public static BlobCounter bc = new BlobCounter();
        public static ConnectedComponentsLabeling CCLfilter = new ConnectedComponentsLabeling();
    }
}
