using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace 酷狗音乐UWP.Class.ImageUtility
{
    public class MyImage
    {
        //original bitmap image
        public WriteableBitmap image;
        //public WriteableBitmap destImage;

        //format of image (jpg/png)
        private String formatName;
        //dimensions of image
        private int width, height;
        // RGB Array Color
        public int[] colorArray;

        public MyImage(WriteableBitmap img)
        {
            this.image = img;// img.Clone(); Silverlight中的 WriteableBitmap 有 Clone()方法，win runtime 中的 WriteableBitmap没有该方法。在调用该构造函数时，可以调用自定义的 Utility.BitmapClone(wb) 方法来实现对原对象的“拷贝”

            formatName = "jpg";
            width = img.PixelWidth;
            height = img.PixelHeight;
            updateColorArray();
        }

        public MyImage clone()
        {
            return new MyImage(this.image);
        }

        /**
         * Method to reset the image to a solid color
         * @param color - color to rest the entire image to
         */
        public void clearImage(int color)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    setPixelColor(x, y, color);
                }
            }
        }


        /**
         * Set color array for image - called on initialisation
         * by constructor
         * 
         * @param bitmap
         */
        private void updateColorArray()
        {
            #region 原
            //colorArray = image.Pixels;

            //int r, g, b;
            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {
            //        int index = y * width + x;
            //        r = (colorArray[index] >> 16) & 0xff;
            //        g = (colorArray[index] >> 8) & 0xff;
            //        b = colorArray[index] & 0xff;
            //        colorArray[index] = (255 << 24) | (r << 16) | (g << 8) | b;
            //    }
            //}
            #endregion

            #region 段光磊

            // 戴震军 ： https://github.net/daizhenjun/ImageFilterForWindowsPhone

            // WriteableBitmap.PixelBuffer property ：https://msdn.microsoft.net/en-us/library/windows/apps/windows.ui.xaml.media.imaging.writeablebitmap.pixelbuffer.aspx

            // WriteableBitmap class : https://msdn.microsoft.net/en-us/library/windows/apps/windows.ui.xaml.media.imaging.writeablebitmap.aspx

            // WriteableBitmapEx : http://writeablebitmapex.codeplex.net/releases/view/612952

            // Getting Pixels of an Element(WriteableBitmap) // https://social.msdn.microsoft.net/Forums/en-US/39b3c702-caed-47e4-b7d3-b51d75cbca9b/getting-pixels-of-an-element-writeablebitmap?forum=winappswithcsharp

            // Windows 8 WriteableBitmap Pixel Arrays in C# and C++ : http://www.tuicool.net/articles/fQNvUz

            // 各种流转换 ： http://www.cnblogs.net/hebeiDGL/p/3428743.html

            // XAML images sample ： https://code.msdn.microsoft.net/windowsapps/0f5d56ae-5e57-48e1-9cd9-993115b027b9/

            // 重新想象 Windows 8 Store Apps (29) - 图片处理 ： http://www.cnblogs.net/webabcd/archive/2013/05/27/3101069.html

            // 把 uwp 的 WriteableBitmap 对象的 PixelBuffer属性（IBuffer）转换为  byte[] 数组
            byte[] colorBytes = BufferToBytes(image.PixelBuffer);

            // 转换为 silverlight 的 int[] 数组时，长度为 byte[] 数组的四分之一
            colorArray = new int[colorBytes.Length / 4];

            int a, r, g, b;
            int i = 0; // 通过 i 自加，来遍历 byte[] 整个数组

            // 通过图片的宽、高，分别设置 int[] 数组中每个像素的 ARGB 信息
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // int[] 数组的索引
                    int index = y * width + x;
                    b = colorBytes[i++]; // Blue
                    g = colorBytes[i++]; // Green
                    r = colorBytes[i++]; // Red
                    a = colorBytes[i++]; // Alpha
                    colorArray[index] = (a << 24) | (r << 16) | (g << 8) | b;  // 4个 byte值存储为一个 int 值
                }
            }


            // msdn : https://msdn.microsoft.net/en-us/library/windows/apps/windows.ui.xaml.media.imaging.writeablebitmap.aspx
            //Stream stream = image.PixelBuffer.AsStream();
            //using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            //{
            //    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
            //    // Scale image to appropriate size 
            //    BitmapTransform transform = new BitmapTransform()
            //    {
            //        ScaledWidth = Convert.ToUInt32(Scenario4WriteableBitmap.PixelWidth),
            //        ScaledHeight = Convert.ToUInt32(Scenario4WriteableBitmap.PixelHeight)
            //    };
            //    PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
            //        BitmapPixelFormat.Bgra8, // WriteableBitmap uses BGRA format 
            //        BitmapAlphaMode.Straight,
            //        transform,
            //        ExifOrientationMode.IgnoreExifOrientation, // This sample ignores Exif orientation 
            //        ColorManagementMode.DoNotColorManage
            //    );

            //    // An array containing the decoded image data, which could be modified before being displayed 
            //    byte[] sourcePixels = pixelData.DetachPixelData();

            //    // Open a stream to copy the image contents to the WriteableBitmap's pixel buffer 
            //    using (Stream stream = Scenario4WriteableBitmap.PixelBuffer.AsStream())
            //    {
            //        await stream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
            //    }
            //}


            //PixelDataProvider dp = await BitmapDecoder.CreateAsync(image.PixelBuffer.a.AsStream());


            //var length = image.PixelWidth * image.PixelHeight;
            //var data = image.PixelBuffer.ToArray();

            //fixed (byte* srcPtr = data)
            //{
            //    fixed (int* dstPtr = colorArray)
            //    {
            //        for (var i = 0; i < length; i++)
            //        {
            //            dstPtr[i] = (srcPtr[i * 4 + 3] << 24)
            //                      | (srcPtr[i * 4 + 2] << 16)
            //                      | (srcPtr[i * 4 + 1] << 8)
            //                      | srcPtr[i * 4 + 0];
            //        }
            //    }
            //}

            #endregion
        }

        static byte[] BufferToBytes(IBuffer buf)
        {
            using (var dataReader = DataReader.FromBuffer(buf))
            {
                var bytes = new byte[buf.Capacity];
                dataReader.ReadBytes(bytes);

                return bytes;
            }
        }

        /**
         * Method to set the color of a specific pixel
         * 
         * @param x
         * @param y
         * @param color
         */
        public void setPixelColor(int x, int y, int color)
        {
            colorArray[((y * image.PixelWidth + x))] = color;
            //image.setPixel(x, y, color);
            //destImage.setPixel(x, y, colorArray[((y*image.getWidth()+x))]);
        }

        /**
         * Get the color for a specified pixel
         * 
         * @param x
         * @param y
         * @return color
         */
        public int getPixelColor(int x, int y)
        {
            return colorArray[y * width + x];
        }

        private int[] RgbTranToHsl(int R, int G, int B)
        {
            int hVALUE, sVALUE, lVALUE;
            //   RGB空间到HSL空间的转换  
            double Delta, CMax, CMin;
            double Red, Green, Blue, Hue, Sat, Lum;
            Red = (double)R / 255;
            Green = (double)G / 255;
            Blue = (double)B / 255;
            CMax = Math.Max(Red, Math.Max(Green, Blue));
            CMin = Math.Min(Red, Math.Min(Green, Blue));
            //计算hsb
            //Lum = (CMax + CMin) / 2;
            if (CMax == CMin)
            {
                Sat = 0;
                Hue = 0;
            }
            else
            {
                //计算hsb
                //if (Lum < 0.5)
                //    Sat = (CMax - CMin) / (CMax + CMin);
                //else
                //    Sat = (CMax - CMin) / (2 - CMax - CMin);
                //计算hsv（彩色空间）
                if (CMax == 0)
                {
                    Sat = 0;
                }
                else
                {
                    Sat = 1 - CMin / CMax;
                }
                Delta = CMax - CMin;
                if (Red == CMax)
                {
                    Hue = (Green - Blue) / Delta;
                }
                else if (Green == CMax)
                {
                    Hue = 2 + (Blue - Red) / Delta;
                }
                else
                {
                    Hue = 4.0 + (Red - Green) / Delta;
                }
                Hue = Hue / 6;
                if (Hue < 0)
                    Hue = Hue + 1;
            }
            hVALUE = (int)Math.Round(Hue * 360);
            sVALUE = (int)Math.Round(Sat * 100);
            lVALUE = (int)Math.Round(CMax * 100);
            int[] HSL = new int[3] { hVALUE, sVALUE, lVALUE };
            return HSL;
            //lvalue = (int)Math.Round(Lum * 100);
        }
        public int getPixelHue(int x, int y)
        {
            int[] i = RgbTranToHsl(getRComponent(x, y), getGComponent(x, y), getBComponent(x, y));
            return i[0];
        }
        /**
         * Set the color of a specified pixel from an RGB combo
         * 
         * @param x
         * @param y
         * @param c0
         * @param c1
         * @param c2
         */
        public void setPixelColor(int x, int y, int c0, int c1, int c2)
        {
            int rgbcolor = (255 << 24) + (c0 << 16) + (c1 << 8) + c2;
            colorArray[((y * image.PixelWidth + x))] = rgbcolor;
            //int array = ((y*image.getWidth()+x));

            //vbb.order(ByteOrder.nativeOrder());
            //vertexBuffer = vbb.asFloatBuffer();
            //vertexBuffer.put(vertices);
            //vertexBuffer.position(0);

            //image.setPixel(x, y, colorArray[((y*image.getWidth()+x))]);
        }

        public void copyPixelsFromBuffer()
        { //从缓冲区中copy数据以加快像素处理速度
          // this.destImage.Dispatcher.BeginInvoke(() =>
          // {
          //var result = new WriteableBitmap(width, height);
          //Buffer.BlockCopy(colorArray, 0, destImage.Pixels, 0, colorArray.Length * 4); // 段光磊注释掉
          //return result;
          // });
        }

        /**
         * Method to get the RED color for the specified 
         * pixel 
         * @param x
         * @param y
         * @return color of R
         */
        public int getRComponent(int x, int y)
        {
            return (getColorArray()[((y * width + x))] & 0x00FF0000) >> 16;
        }


        /**
         * Method to get the GREEN color for the specified 
         * pixel 
         * @param x
         * @param y
         * @return color of G
         */
        public int getGComponent(int x, int y)
        {
            return (getColorArray()[((y * width + x))] & 0x0000FF00) >> 8;
        }


        /**
         * Method to get the BLUE color for the specified 
         * pixel 
         * @param x
         * @param y
         * @return color of B
         */
        public int getBComponent(int x, int y)
        {
            return (getColorArray()[((y * width + x))] & 0x000000FF);
        }




        /**
         * @return the image
         */
        //public WriteableBitmap getImage()
        //{
        //    //return image;
        //    return destImage;
        //}


        /**
         * @param image the image to set
         */
        public void setImage(WriteableBitmap image)
        {
            this.image = image;
        }


        /**
         * @return the formatName
         */
        public String getFormatName()
        {
            return formatName;
        }


        /**
         * @param formatName the formatName to set
         */
        public void setFormatName(String formatName)
        {
            this.formatName = formatName;
        }


        /**
         * @return the width
         */
        public int getWidth()
        {
            return width;
        }


        /**
         * @param width the width to set
         */
        public void setWidth(int width)
        {
            this.width = width;
        }


        /**
         * @return the height
         */
        public int getHeight()
        {
            return height;
        }


        /**
         * @param height the height to set
         */
        public void setHeight(int height)
        {
            this.height = height;
        }


        /**
         * @return the colorArray
         */
        public int[] getColorArray()
        {
            return colorArray;
        }


        /**
         * @param colorArray the colorArray to set
         */
        public void setColorArray(int[] colorArray)
        {
            this.colorArray = colorArray;
        }

        public static int SAFECOLOR(int a)
        {
            if (a < 0)
                return 0;
            else if (a > 255)
                return 255;
            else
                return a;
        }

        public static int rgb(int r, int g, int b)
        {
            return (255 << 24) + (r << 16) + (g << 8) + b;
        }


        public static MyImage LoadImage(string path)
        {
            //Uri uri = new Uri(path, UriKind.Relative);
            //StreamResourceInfo resourceInfo = Application.GetResourceStream(uri);
            //BitmapImage bmp = new BitmapImage();
            //bmp.SetSource(resourceInfo.Stream);
            //return new Image(new WriteableBitmap(bmp));

            return null;
        }

    }
    class Utility
    {
        /// <summary>
        /// WriteableBitmap 的拷贝
        /// </summary>
        /// <param name="bitmap">原</param>
        /// <returns></returns>
        public static async Task<WriteableBitmap> BitmapClone(WriteableBitmap bitmap)
        {
            WriteableBitmap result = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight);

            byte[] sourcePixels = Get_WriteableBitmap_bytes(bitmap);

            //byte[] resultPixels = new byte[sourcePixels.Length];

            //sourcePixels.CopyTo(resultPixels, 0);

            using (Stream resultStream = result.PixelBuffer.AsStream())
            {
                await resultStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
            }

            return result;
        }

        public static byte[] Get_WriteableBitmap_bytes(WriteableBitmap bitmap)
        {
            // 获取对直接缓冲区的访问，WriteableBitmap 的每个像素都写入直接缓冲区。
            IBuffer bitmapBuffer = bitmap.PixelBuffer;

            //byte[] sourcePixels = new byte[bitmapBuffer.Length];
            //Windows.Security.Cryptography.CryptographicBuffer.CopyToByteArray(bitmapBuffer, out sourcePixels);
            //bitmapBuffer.CopyTo(sourcePixels);

            using (var dataReader = DataReader.FromBuffer(bitmapBuffer))
            {
                var bytes = new byte[bitmapBuffer.Capacity];
                dataReader.ReadBytes(bytes);

                return bytes;
            }
        }
    }
    public interface IImageFilter
    {
        MyImage process(MyImage imageIn);
    }
    public class GaussianBlurFilter : IImageFilter
    {
        protected static int Padding = 3;

        /// <summary>
        /// The bluriness factor. 
        /// Should be in the range [0, 40].
        /// </summary>
        public float Sigma = 0.75f;

        protected float[] ApplyBlur(float[] srcPixels, int width, int height)
        {
            float[] destPixels = new float[srcPixels.Length];
            System.Array.Copy(srcPixels, 0, destPixels, 0, srcPixels.Length);

            int w = width + Padding * 2;
            int h = height + Padding * 2;

            // Calculate the coefficients
            float q = Sigma;
            float q2 = q * q;
            float q3 = q2 * q;

            float b0 = 1.57825f + 2.44413f * q + 1.4281f * q2 + 0.422205f * q3;
            float b1 = 2.44413f * q + 2.85619f * q2 + 1.26661f * q3;
            float b2 = -(1.4281f * q2 + 1.26661f * q3);
            float b3 = 0.422205f * q3;

            float b = 1.0f - ((b1 + b2 + b3) / b0);

            // Apply horizontal pass
            ApplyPass(destPixels, w, h, b0, b1, b2, b3, b);

            // Transpose the array
            float[] transposedPixels = new float[destPixels.Length];
            Transpose(destPixels, transposedPixels, w, h);

            // Apply vertical pass
            ApplyPass(transposedPixels, h, w, b0, b1, b2, b3, b);

            // transpose back
            Transpose(transposedPixels, destPixels, h, w);

            return destPixels;
        }

        protected void ApplyPass(float[] pixels, int width, int height, float b0, float b1, float b2, float b3, float b)
        {
            float num = 1f / b0;
            int triplewidth = width * 3;
            for (int i = 0; i < height; i++)
            {
                int steplength = i * triplewidth;
                for (int j = steplength + 9; j < (steplength + triplewidth); j += 3)
                {
                    pixels[j] = (b * pixels[j]) + ((((b1 * pixels[j - 3]) + (b2 * pixels[j - 6])) + (b3 * pixels[j - 9])) * num);
                    pixels[j + 1] = (b * pixels[j + 1]) + ((((b1 * pixels[(j + 1) - 3]) + (b2 * pixels[(j + 1) - 6])) + (b3 * pixels[(j + 1) - 9])) * num);
                    pixels[j + 2] = (b * pixels[j + 2]) + ((((b1 * pixels[(j + 2) - 3]) + (b2 * pixels[(j + 2) - 6])) + (b3 * pixels[(j + 2) - 9])) * num);
                }
                for (int k = ((steplength + triplewidth) - 9) - 3; k >= steplength; k -= 3)
                {
                    pixels[k] = (b * pixels[k]) + ((((b1 * pixels[k + 3]) + (b2 * pixels[k + 6])) + (b3 * pixels[k + 9])) * num);
                    pixels[k + 1] = (b * pixels[k + 1]) + ((((b1 * pixels[(k + 1) + 3]) + (b2 * pixels[(k + 1) + 6])) + (b3 * pixels[(k + 1) + 9])) * num);
                    pixels[k + 2] = (b * pixels[k + 2]) + ((((b1 * pixels[(k + 2) + 3]) + (b2 * pixels[(k + 2) + 6])) + (b3 * pixels[(k + 2) + 9])) * num);
                }
            }
        }


        protected void Transpose(float[] input, float[] output, int width, int height)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int index = (j * height) * 3 + (i * 3);
                    int pos = (i * width) * 3 + (j * 3);
                    output[index] = input[pos];
                    output[index + 1] = input[pos + 1];
                    output[index + 2] = input[pos + 2];
                }
            }
        }


        protected float[] ConvertImageWithPadding(MyImage imageIn, int width, int height)
        {
            int newheight = height + Padding * 2;
            int newwidth = width + Padding * 2;
            float[] numArray = new float[(newheight * newwidth) * 3];
            int index = 0;
            int num = 0;
            for (int i = -3; num < newheight; i++)
            {
                int y = i;
                if (i < 0)
                {
                    y = 0;
                }
                else if (i >= height)
                {
                    y = height - 1;
                }
                int count = 0;
                int negpadding = -1 * Padding;
                while (count < newwidth)
                {
                    int x = negpadding;
                    if (negpadding < 0)
                    {
                        x = 0;
                    }
                    else if (negpadding >= width)
                    {
                        x = width - 1;
                    }
                    numArray[index] = imageIn.getRComponent(x, y) * 0.003921569f;
                    numArray[index + 1] = imageIn.getGComponent(x, y) * 0.003921569f;
                    numArray[index + 2] = imageIn.getBComponent(x, y) * 0.003921569f;

                    count++; negpadding++;
                    index += 3;
                }
                num++;
            }
            return numArray;
        }

        //@Override
        public virtual MyImage process(MyImage imageIn)
        {
            int width = imageIn.getWidth();
            int height = imageIn.getHeight();
            float[] imageArray = ConvertImageWithPadding(imageIn, width, height);
            imageArray = ApplyBlur(imageArray, width, height);
            int newwidth = width + Padding * 2;
            for (int i = 0; i < height; i++)
            {
                int num = ((i + 3) * newwidth) + 3;
                for (int j = 0; j < width; j++)
                {
                    int pos = (num + j) * 3;
                    imageIn.setPixelColor(j, i, (byte)(imageArray[pos] * 255f), (byte)(imageArray[pos + 1] * 255f), (byte)(imageArray[pos + 2] * 255f));
                }
            }
            return imageIn;
        }

    }

    public class BlurEffect
    {
        WriteableBitmap wb;
        public BlurEffect(WriteableBitmap Image)
        {
            wb = Image;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level">[0,40]</param>
        public async Task<WriteableBitmap> ApplyFilter(float level)
        {

            #region old
            /*
            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                // Scale image to appropriate size 
                BitmapTransform transform = new BitmapTransform()
                {
                    ScaledWidth = Convert.ToUInt32(wb.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(wb.PixelHeight)
                };
                PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8, // WriteableBitmap uses BGRA format 
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation, // This sample ignores Exif orientation 
                    ColorManagementMode.DoNotColorManage
                );



                // An array containing the decoded image data, which could be modified before being displayed 
                byte[] sourcePixels = pixelData.DetachPixelData();
                using (Stream stream = wb.PixelBuffer.AsStream())
                {
                    await stream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                }
            }
            */
            #endregion

            // 拷贝
            WriteableBitmap new_bitmap = await Utility.BitmapClone(wb);

            // 添加高斯滤镜效果
            MyImage mi = new MyImage(new_bitmap);
            GaussianBlurFilter filter = new GaussianBlurFilter();
            filter.Sigma = level;
            filter.process(mi);

            // 图片添加完滤镜的 int[] 数组
            int[] array = mi.colorArray;

            // byte[] 数组的长度是 int[] 数组的 4倍
            byte[] result = new byte[array.Length * 4];

            // 通过自加，来遍历 byte[] 数组中的值
            int j = 0;
            for (int i = 0; i < array.Length; i++)
            {
                // 同时把 int 值中 a、r、g、b 的排列方式，转换为 byte数组中 b、g、r、a 的存储方式 
                result[j++] = (byte)(array[i]);       // Blue
                result[j++] = (byte)(array[i] >> 8);  // Green
                result[j++] = (byte)(array[i] >> 16); // Red
                result[j++] = (byte)(array[i] >> 24); // Alpha
            }



            // Open a stream to copy the image contents to the WriteableBitmap's pixel buffer 
            using (Stream stream = new_bitmap.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(result, 0, result.Length);
            }

            return new_bitmap;// 把 WriteableBitmap 对象赋值给 Image 控件

            // 用像素缓冲区的数据绘制图片
            //new_bitmap.Invalidate();
        }
    }

    public class GetColor
    {
        public static Color GetMajorColor(WriteableBitmap bitmap)
        {
            MyImage mi = new MyImage(bitmap);


            //色调的总和
            var sum_hue = 0d;
            //色差的阈值
            var threshold = 30;
            //计算色调总和
            for (int h = 0; h < bitmap.PixelHeight; h++)
            {
                for (int w = 0; w < bitmap.PixelWidth; w++)
                {

                    var hue = mi.getPixelHue(w, h);
                    sum_hue += hue;
                }
            }
            var avg_hue = sum_hue / (bitmap.PixelWidth * bitmap.PixelHeight);

            //色差大于阈值的颜色值
            var rgbs = new List<Color>();
            for (int h = 0; h < bitmap.PixelHeight; h++)
            {
                for (int w = 0; w < bitmap.PixelWidth; w++)
                {
                    var hue = mi.getPixelHue(w, h);
                    //如果色差大于阈值，则加入列表
                    if (Math.Abs(hue - avg_hue) > threshold)
                    {
                        rgbs.Add(Color.FromArgb(255, Convert.ToByte(mi.getRComponent(w, h)), Convert.ToByte(mi.getGComponent(w, h)), Convert.ToByte(mi.getBComponent(w, h))));
                    }
                }
            }
            if (rgbs.Count == 0)
                return Colors.Black;
            //计算列表中的颜色均值，结果即为该图片的主色调
            int sum_r = 0, sum_g = 0, sum_b = 0;
            foreach (var rgb in rgbs)
            {
                sum_r += rgb.R;
                sum_g += rgb.G;
                sum_b += rgb.B;
            }
            return Color.FromArgb(255, Convert.ToByte(sum_r / rgbs.Count),
                Convert.ToByte(sum_g / rgbs.Count),
                Convert.ToByte(sum_b / rgbs.Count));
        }

    }

}