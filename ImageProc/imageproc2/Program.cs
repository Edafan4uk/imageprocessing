using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace imageproc2
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var image = (Bitmap)Image.FromFile(args[0]))
            {
                var hists = GetColorFrequenciesRgb(image);

                CreateHistBmp(hists[0], "redHist.bmp", Pens.Red);

                CreateHistBmp(hists[1], "greenHist.bmp", Pens.Green);

                CreateHistBmp(hists[2], "blueHist.bmp", Pens.Blue);

                var equalized = Equalize(image);

                hists = GetColorFrequenciesRgb(equalized);

                CreateHistBmp(hists[0], "redHistEqualized.bmp", Pens.Red);

                CreateHistBmp(hists[1], "greenHistEqualized.bmp", Pens.Green);

                CreateHistBmp(hists[2], "blueHistEqualized.bmp", Pens.Blue);

                equalized.Save("Equalized.bmp");

                var robertsImage = Roberts(image);
                robertsImage.Save("roberts.bmp");

                var sobelImage = Sobel(image);
                sobelImage.Save("sobel.bmp");

                var previtImage = Previt(image);
                previtImage.Save("previt.bmp");
            }
        }
        public static Bitmap Roberts(Bitmap original)
        {
            Bitmap result = new Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int width = original.Width;
            int height = original.Height;

            int[,] GX = new int[,]
            {
                { 0, -1 },
                { 1,  0 }
            };
            int[,] GY = new int[,]
            {
                { -1, 0 },
                {  0, 1 }
            };

            int[,] R = new int[width, height];
            int[,] G = new int[width, height];
            int[,] B = new int[width, height];
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    R[i, j] = original.GetPixel(i, j).R;
                    G[i, j] = original.GetPixel(i, j).G;
                    B[i, j] = original.GetPixel(i, j).B;
                }
            }

            int Rx = 0, Ry = 0;
            int Gx = 0, Gy = 0;
            int Bx = 0, By = 0;
            int RChannel, GChannel, BChannel;
            for (int i = 1; i < original.Width - 1; ++i)
            {
                for (int j = 1; j < original.Height - 1; ++j)
                {

                    Rx = 0;
                    Ry = 0;
                    Gx = 0;
                    Gy = 0;
                    Bx = 0;
                    By = 0;
                    RChannel = 0;
                    GChannel = 0;
                    BChannel = 0;
                    for (int x = -1; x < 1; ++x)
                    {
                        for (int y = -1; y < 1; ++y)
                        {
                            RChannel = R[i + y, j + x];
                            Rx += GX[x + 1, y + 1] * RChannel;
                            Ry += GY[x + 1, y + 1] * RChannel;

                            GChannel = G[i + y, j + x];
                            Gx += GX[x + 1, y + 1] * GChannel;
                            Gy += GY[x + 1, y + 1] * GChannel;

                            BChannel = B[i + y, j + x];
                            Bx += GX[x + 1, y + 1] * BChannel;
                            By += GY[x + 1, y + 1] * BChannel;
                        }
                    }
                    result.SetPixel(i, j, Color.FromArgb(SatureCast(Rx + Ry), SatureCast(Gx + Gy), SatureCast(Bx + By)));
                }
            }
            return result;

        }

        public static Bitmap Previt(Bitmap original)
        {
            Bitmap result = new Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int width = original.Width;
            int height = original.Height;

            int[,] GX = new int[,]
            {
                { 1, 0, -1 },
                { 1, 0, -1 },
                { 1, 0, -1 }
            };
            int[,] GY = new int[,]
            {
                { -1, -1, -1 },
                { 0, 0, 0 },
                { 1, 1, 1 }
            };

            int[,] R = new int[width, height];
            int[,] G = new int[width, height];
            int[,] B = new int[width, height];
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    R[i, j] = original.GetPixel(i, j).R;
                    G[i, j] = original.GetPixel(i, j).G;
                    B[i, j] = original.GetPixel(i, j).B;
                }
            }

            int Rx = 0, Ry = 0;
            int Gx = 0, Gy = 0;
            int Bx = 0, By = 0;
            int RChannel, GChannel, BChannel;
            for (int i = 1; i < original.Width - 1; ++i)
            {
                for (int j = 1; j < original.Height - 1; ++j)
                {

                    Rx = 0;
                    Ry = 0;
                    Gx = 0;
                    Gy = 0;
                    Bx = 0;
                    By = 0;
                    RChannel = 0;
                    GChannel = 0;
                    BChannel = 0;
                    for (int x = -1; x < 2; ++x)
                    {
                        for (int y = -1; y < 2; ++y)
                        {
                            RChannel = R[i + y, j + x];
                            Rx += GX[x + 1, y + 1] * RChannel;
                            Ry += GY[x + 1, y + 1] * RChannel;

                            GChannel = G[i + y, j + x];
                            Gx += GX[x + 1, y + 1] * GChannel;
                            Gy += GY[x + 1, y + 1] * GChannel;

                            BChannel = B[i + y, j + x];
                            Bx += GX[x + 1, y + 1] * BChannel;
                            By += GY[x + 1, y + 1] * BChannel;
                        }
                    }
                    result.SetPixel(i, j, Color.FromArgb(SatureCast(Rx + Ry), SatureCast(Gx + Gy), SatureCast(Bx + By)));
                }
            }
            return result;
        }

        public static Bitmap Sobel(Bitmap original)
        {
            Bitmap result = new Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int width = original.Width;
            int height = original.Height;

            int[,] GX = new int[,]
            {
                { 1, 0, -1 },
                { 2, 0, -2 },
                { 1, 0, -1 }
            };
            int[,] GY = new int[,]
            {
                { -1, -2, -1 },
                {  0,  0,  0 },
                {  1,  2,  1 }
            };

            int[,] R = new int[width, height];
            int[,] G = new int[width, height];
            int[,] B = new int[width, height];
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    R[i, j] = original.GetPixel(i, j).R;
                    G[i, j] = original.GetPixel(i, j).G;
                    B[i, j] = original.GetPixel(i, j).B;
                }
            }

            int Rx = 0, Ry = 0;
            int Gx = 0, Gy = 0;
            int Bx = 0, By = 0;
            int RChannel, GChannel, BChannel;
            for (int i = 1; i < original.Width - 1; ++i)
            {
                for (int j = 1; j < original.Height - 1; ++j)
                {

                    Rx = 0;
                    Ry = 0;
                    Gx = 0;
                    Gy = 0;
                    Bx = 0;
                    By = 0;
                    RChannel = 0;
                    GChannel = 0;
                    BChannel = 0;
                    for (int x = -1; x < 2; ++x)
                    {
                        for (int y = -1; y < 2; ++y)
                        {
                            RChannel = R[i + y, j + x];
                            Rx += GX[x + 1, y + 1] * RChannel;
                            Ry += GY[x + 1, y + 1] * RChannel;

                            GChannel = G[i + y, j + x];
                            Gx += GX[x + 1, y + 1] * GChannel;
                            Gy += GY[x + 1, y + 1] * GChannel;

                            BChannel = B[i + y, j + x];
                            Bx += GX[x + 1, y + 1] * BChannel;
                            By += GY[x + 1, y + 1] * BChannel;
                        }
                    }
                    result.SetPixel(i, j, Color.FromArgb(SatureCast(Rx + Ry), SatureCast(Gx + Gy), SatureCast(Bx + By)));
                }
            }
            return result;
        }

        static int SatureCast(double number)
        {
            if (number < 0)
            {
                return 0;
            }
            if (number > 255)
            {
                return 255;
            }
            return (int)number;
        }

        static Bitmap Equalize(Bitmap image)
        {            
            var height = image.Height;
            var width = image.Width;

            var outputImage = new Bitmap(width, height, image.PixelFormat);

            var hists = GetColorFrequenciesRgb(image);

            var cdfR = Cdf(hists[0]);
            var cdfG = Cdf(hists[1]);
            var cdfB = Cdf(hists[2]);

            var cdfRMin = cdfR.Min(e => e.Value);
            var cdfGMin = cdfG.Min(e => e.Value);
            var cdfBMin = cdfB.Min(e => e.Value);

            var bitmapData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height), 
                ImageLockMode.ReadOnly, 
                PixelFormat.Format24bppRgb);

            var bytes = new byte[bitmapData.Stride * bitmapData.Height];

            Marshal.Copy(bitmapData.Scan0, bytes, 0, bytes.Length);

            image.UnlockBits(bitmapData);

            var offset = bitmapData.Stride - bitmapData.Width * 3;

            var index = 0;

            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++, index += 3)
                {
                    bytes[index + 2] = H(bytes[index + 2], cdfR, cdfRMin, width, height);
                    bytes[index + 1] = H(bytes[index + 1], cdfG, cdfGMin, width, height);
                    bytes[index] = H(bytes[index], cdfB, cdfBMin, width, height);
                }

                index += offset;
            }

            bitmapData = outputImage.LockBits(
                new Rectangle(0, 0, outputImage.Width, outputImage.Height),
                ImageLockMode.WriteOnly,
                image.PixelFormat);

            Marshal.Copy(bytes, 0, bitmapData.Scan0, bytes.Length);

            outputImage.UnlockBits(bitmapData);

            return outputImage;
        }

        static byte H(int v, Dictionary<int, int> cdf, int cdfMin, int w, int h)
        {
            var range = 256;

            var hh = ((cdf[v] - cdfMin) / (float)(w * h - cdfMin)) * (range - 1);

            return (byte)Math.Floor(hh);
        }

        static Dictionary<int, int> Cdf(Dictionary<int, int> colorFreq)
        {
            var dict = new Dictionary<int, int>();

            var newDictFreq = new Dictionary<int, int>();

            foreach (var kvPair in colorFreq.Where(e => e.Value != 0))
            {
                newDictFreq.Add(kvPair.Key, kvPair.Value);
            }

            var firstElem = newDictFreq.ElementAt(0);

            dict.Add(firstElem.Key, firstElem.Value);

            for (int i = 1; i < newDictFreq.Keys.Count; i++)
            {
                var currEl = newDictFreq.ElementAt(i);

                dict.Add(currEl.Key, currEl.Value + dict.ElementAt(i - 1).Value);
            }

            return dict;
        }

        static void CreateHistBmp(Dictionary<int, int> histData, string filePath, Pen pen)
        {
            var max = histData.Max(e => e.Value);

            var sum = histData.Sum(e => e.Value);

            var width = 256;

            var height = 256;

            var bmp = new Bitmap(width, height);

            using(Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                foreach (var key in histData.Keys)
                {
                    float y2 = height - (histData[key] / (float)max) * (height - 5);
                           
                    g.DrawLine(pen, key, height, key, y2);
                }
            }

            using(MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Bmp);

                bmp = new Bitmap(Image.FromStream(ms));
            }

            bmp.Save(filePath);

            bmp.Dispose();
        }

        static List<Dictionary<int, int>> GetColorFrequenciesRgb(Bitmap image)
        {
            var listDictRgb = new List<Dictionary<int, int>>
            {
                new Dictionary<int, int>(),
                new Dictionary<int, int>(),
                new Dictionary<int, int>()
            };

            listDictRgb.ForEach(l =>
            {
                for (int i = 0; i < 256; i++)
                {
                    l.Add(i, 0);
                }
            });

            var bitmapData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height), 
                ImageLockMode.ReadOnly, 
                PixelFormat.Format24bppRgb);

            var bytes = new byte[bitmapData.Stride * bitmapData.Height];

            Marshal.Copy(bitmapData.Scan0, bytes, 0, bytes.Length);

            var offset = bitmapData.Stride - bitmapData.Width * 3;

            image.UnlockBits(bitmapData);

            var index = 0;

            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++, index += 3)
                {
                    listDictRgb[0][bytes[index + 2]] += 1;
                    listDictRgb[1][bytes[index + 1]] += 1;
                    listDictRgb[2][bytes[index]] += 1;
                }

                index += offset;
            }

            return listDictRgb;
        }
    }
}
