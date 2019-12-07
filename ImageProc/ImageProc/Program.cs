using System;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Numerics;

namespace ImageProc
{
    class Program
    {
        static void Main(string[] args)
        {            
            using (var image = (Bitmap)Image.FromFile(args[0]))
            {
                EncoderParameters encoderParameters;

                #region BmpRle
                var bmpRlePath = "bmpRle.bmp";

                encoderParameters = new EncoderParameters
                {
                    Param = new[]
                    {
                        new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionRle)
                    }
                };

                var (compRle, encodeT, decodeT, readT, writeT) = GetCompressedWithStats(image,
                    ImageFormat.Bmp,
                    encoderParameters,
                    bmpRlePath);

                ShowStats(image, compRle,
                    encodeT, decodeT,
                    readT, writeT,
                    "BMP TO BMP/RLE", args[0],
                    bmpRlePath);

                Console.WriteLine("\n\n\n");

                #endregion

                #region TiffLzw
                var tiffLzwPath = "encLzw.tif";
                Bitmap compTiff;

                encoderParameters = new EncoderParameters
                {
                    Param = new[]
                    {
                        new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW)
                    }
                };

                (compTiff, encodeT, decodeT, readT, writeT) = GetCompressedWithStats(
                    image,
                    ImageFormat.Tiff,
                    encoderParameters,
                    tiffLzwPath);                

                ShowStats(image, compTiff,
                    encodeT, decodeT,
                    readT, writeT,
                    "Bmp to tiff/lzw", args[0],
                    tiffLzwPath);

                Console.WriteLine("\n\n\n");
                #endregion

                #region Jpeg  
                var jpegPath = "jpeg.jpeg";
                Bitmap compJpeg;

                encoderParameters = new EncoderParameters
                {
                    Param = new[]
                    {
                        new EncoderParameter(Encoder.Quality, 0L)
                    }
                };

                (compJpeg, encodeT, decodeT, readT, writeT) = GetCompressedWithStats(
                    image,
                    ImageFormat.Jpeg,
                    encoderParameters,
                    jpegPath);

                ShowStats(image, compJpeg,
                    encodeT, decodeT,
                    readT, writeT,
                    "Bmp to jpeg", args[0],
                    jpegPath);

                Console.WriteLine("\n\n\n");

                #endregion

                Console.WriteLine($"\t\tColor loss stats \n\n".ToUpper());

                BigInteger init, compressRle, compressLzw, compressJpeg;

                init = GetPixelColorSumNew(image);

                compressRle = GetPixelColorSumNew(compRle);
                Console.WriteLine($"\tBmp/rle color loss : {init - compressRle}\n");

                compressLzw = GetPixelColorSumNew(compTiff);
                Console.WriteLine($"\tTiff color loss : {init - compressLzw}\n");

                compressJpeg = GetPixelColorSumNew(compJpeg);
                Console.WriteLine($"\tJpeg color loss : {init - compressJpeg}\n");

                var (initR, initG, initB) = GetPixelRgbSums(image);

                Console.WriteLine($"\n\n\t\tColor loss RGB stats \n\n");

                var (rleR, rleG, rleB) = GetPixelRgbSums(compRle);
                Console.WriteLine($"\tBmp/rle color loss\t  R:{initR - rleR}; G:{initG-rleG}; B:{initB-rleB}\n");

                var (tiffR, tiffG, tiffB) = GetPixelRgbSums(compTiff);
                Console.WriteLine($"\tTiff color loss\t  R:{initR - tiffR}; G:{initG-tiffG}; B:{initB - tiffB}\n");

                var (jpegR, jpegG, jpegB) = GetPixelRgbSums(compJpeg);
                Console.WriteLine($"\tJpeg color loss\t  R:{initR - jpegR}; G:{initG - jpegG}; B:{initB - jpegB}\n");

                GenerateRgbDiffImages(image, compRle, "Rle");

                GenerateRgbDiffImages(image, compTiff, "Tiff");

                GenerateRgbDiffImages(image, compJpeg, "Jpeg");
            }
        }

        static void GenerateRgbDiffImages(Bitmap init, Bitmap compressed, string compName)
        {
            var rDiff = new Bitmap(init);
            var gDiff = new Bitmap(init);
            var bDiff = new Bitmap(init);
            var rgbDiff = new Bitmap(init);

            for (int i = 0; i < rDiff.Height; i++)
            {
                for (int j = 0; j < rDiff.Width; j++)
                {
                    var compr = compressed.GetPixel(j, i);

                    var rgb = rDiff.GetPixel(j, i);
                    rgb = Color.FromArgb(Math.Abs(rgb.R - compr.R), rgb.G, rgb.B);
                    rDiff.SetPixel(j, i, rgb);

                    rgb = gDiff.GetPixel(j, i);
                    rgb = Color.FromArgb(rgb.R, Math.Abs(rgb.G - compr.G), rgb.B);
                    gDiff.SetPixel(j, i, rgb);

                    rgb = bDiff.GetPixel(j, i);
                    rgb = Color.FromArgb(rgb.R, rgb.G, Math.Abs(rgb.B - compr.B));
                    bDiff.SetPixel(j, i, rgb);

                    rgb = rgbDiff.GetPixel(j, i);
                    rgb = Color.FromArgb(Math.Abs(rgb.R - compr.R), Math.Abs(rgb.G - compr.G), Math.Abs(rgb.B - compr.B));
                    rgbDiff.SetPixel(j, i, rgb);
                }
            }

            using (var ms = new MemoryStream())
            {
                rDiff.Save(ms, init.RawFormat);
                rDiff = (Bitmap)Image.FromStream(ms);
                rDiff.Save($"rgbDiff/rDif{compName}.bmp");

                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                gDiff.Save(ms, init.RawFormat);
                gDiff = (Bitmap)Image.FromStream(ms);
                gDiff.Save($"rgbDiff/gDif{compName}.bmp");

                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                bDiff.Save(ms, init.RawFormat);
                bDiff = (Bitmap)Image.FromStream(ms);
                bDiff.Save($"rgbDiff/bDif{compName}.bmp");

                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                rgbDiff.Save(ms, init.RawFormat);
                rgbDiff = (Bitmap)Image.FromStream(ms);
                rgbDiff.Save($"rgbDiff/rgbDif{compName}.bmp");
            }

            rDiff.Dispose();
        } 

        static (BigInteger r, BigInteger g, BigInteger b) GetPixelRgbSums(Bitmap image) =>
            (GetPixelColorSumNew(image, 2), GetPixelColorSumNew(image, 1), GetPixelColorSumNew(image, 0));        
        
        static void ShowStats(Bitmap initial, Bitmap compressed, 
            double encodeTime, double readTime, 
            double writeTime, double decodeTime, 
            string compressionName, string pathInit, 
            string pathComp)
        {
            Console.WriteLine($"\t\t{compressionName}\n");

            long initS, compS;

            using (var fss = File.OpenRead(pathComp))
            using (var fs = File.OpenRead(pathInit))
            {
                initS = fs.Length;
                compS = fss.Length;
            }

            Console.WriteLine($"\tSize comparison - ${compressionName}\n");
            Console.WriteLine($"Initial size: {initS}\t Size after compression {compS}\n");
            Console.WriteLine($"\tCompression stats(in milliseconds) \n\n EncodeTime : {encodeTime}; Decode time: {decodeTime}; " +
                $"Read time : {readTime}; Write time : {writeTime}\n");
        }

        static (Bitmap image, double encodeTimeMs, double decodeTimeMs, double readTimeMs, double writeTimeMs) GetCompressedWithStats(
            Bitmap image,
            ImageFormat imageFormat,
            EncoderParameters encoderParameters,
            string writePath)
        {
            Bitmap compressed;
            double encodeMs, decodeMs, readMs, writeMs;
            DateTime start, end;

            //encode
            (compressed, encodeMs) = Compress(image, imageFormat, encoderParameters);
            
            //write
            start = DateTime.Now;
            compressed.Save(writePath);
            end = DateTime.Now;
            writeMs = (end - start).TotalMilliseconds;

            //read
            start = DateTime.Now;
            compressed = (Bitmap)Image.FromFile(writePath);
            end = DateTime.Now;
            readMs = (end - start).TotalMilliseconds;

            //decode
            start = DateTime.Now;
            compressed.Save($"decoded{imageFormat.ToString()}.bmp", image.RawFormat);
            end = DateTime.Now;
            decodeMs = (end - start).TotalMilliseconds;

            return (compressed, encodeMs, decodeMs, readMs, writeMs);
        }

        static BigInteger GetPixelColorSum(Bitmap image, int? ind = null)
        {
            var bitmapData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height), 
                ImageLockMode.ReadOnly, 
                image.PixelFormat);

            var byteArr = new byte[bitmapData.Stride * bitmapData.Height];

            Marshal.Copy(bitmapData.Scan0, byteArr, 0, byteArr.Length);

            var sum = new BigInteger();

            if (!ind.HasValue)
            {
                for(int i = 0; i < byteArr.Length; 
                    i += Bitmap.GetPixelFormatSize(image.PixelFormat)/8)
                {
                    sum += Color.FromArgb(byteArr[i+2], byteArr[i+1], byteArr[i]).ToArgb();
                }
            }
            else
            {
                int indd = ind.Value;
                for (int i = 0; i < byteArr.Length;
                    i += Bitmap.GetPixelFormatSize(image.PixelFormat) / 8)
                {
                    sum += byteArr[i + indd];
                }
            }
                        

            image.UnlockBits(bitmapData);

            return BigInteger.Abs(sum);
        }

        static BigInteger GetPixelColorSumNew(Bitmap image, int? ind = null)
        {
            var sum = new BigInteger();


            if (!ind.HasValue)
            {
                Color temp;
                for (int i = 0; i < image.Height; i++)
                {
                    for (int j = 0; j < image.Width; j++)
                    {
                        temp = image.GetPixel(j, i);

                        sum += Color.FromArgb(temp.R, temp.G, temp.B).ToArgb();
                    }
                }
            }
            else
            {
                Color temp;
                for (int i = 0; i < image.Height; i++)
                {
                    for (int j = 0; j < image.Width; j++)
                    {
                        temp = image.GetPixel(j, i);

                        switch(ind.Value)
                        {
                            case 2:
                                sum += temp.R;
                                break;
                            case 1:
                                sum += temp.G;
                                break;
                            case 0:
                                sum += temp.B;
                                break;
                        }
                    }
                }
            }

            return sum;
        }

        static (Bitmap image, double timeMs) Compress(Bitmap image, ImageFormat format, EncoderParameters encoderParameters)
        {
            Bitmap compressed;
            DateTime start, end;
            double compressTime;

            var imageType = ImageCodecInfo.GetImageEncoders().FirstOrDefault(e => e.FormatID == format.Guid);

            using(var stream = new MemoryStream())            
            {
                start = DateTime.Now;
                image.Save(stream, imageType, encoderParameters);
                end = DateTime.Now;
                compressTime = (end - start).TotalMilliseconds;
                compressed = new Bitmap(Image.FromStream(stream));
            }

            return (compressed, compressTime);
        }        
    }
}
