using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System;
using System.Windows;
//using Accord.Imaging.Filters;
using SimpleImageGallery; 
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using PhotoEditor.Controls;

namespace PhotoEditor
{
    public static class Effects
    {
        private static Bitmap ApplyColorMatrix(Bitmap source, ColorMatrix colorMatrix)
        {
            Bitmap dest = new Bitmap(source.Width, source.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(dest))
            {
                ImageAttributes bmpAttributes = new ImageAttributes();
                bmpAttributes.SetColorMatrix(colorMatrix);
                graphics.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                    0, 0, source.Width, source.Height, GraphicsUnit.Pixel, bmpAttributes);
            }
            source.Dispose();
            return dest;
        }

        public static void Negative(Layer photo)
        {
            Bitmap img = GetBitmap(photo.layerBmpFrame);
            ColorMatrix colorMatrix = new ColorMatrix(new float[][] {
                //           r    g    b    a    t
                new float[] {-1,  0,   0,   0,   0}, // red
                new float[] {0,  -1,   0,   0,   0}, // green
                new float[] {0,   0,  -1,   0,   0}, // blue
                new float[] {0,   0,   0,   1,   0}, // alpha 
                new float[] {1,   1,   1,   1,   1}  // three translations
            });
            photo.layerBmpFrame = BitmapFrame.Create(GetBitmapSource(ApplyColorMatrix(img, colorMatrix)));
        }

        public static Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(source));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        public static BitmapSource GetBitmapSource(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  source.GetHbitmap(),
                  IntPtr.Zero,
                  Int32Rect.Empty,
                  BitmapSizeOptions.FromEmptyOptions());
        }

        private static Bitmap Get24bppRgb(Image image)
        {
            var bitmap = new Bitmap(image);
            var bitmap24 = new Bitmap(bitmap.Width, bitmap.Height,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bitmap24))
            {
                gr.DrawImage(bitmap, new Rectangle(0, 0, bitmap24.Width, bitmap24.Height));
            }
            return bitmap24;
        }

        private static Bitmap Get32bppArgb(Image image)
        {
            Bitmap bitmap32 = new Bitmap(image.Width, image.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics gr = Graphics.FromImage(bitmap32))
            {
                gr.DrawImage(image, new Rectangle(0, 0, bitmap32.Width, bitmap32.Height));
            }
            return bitmap32;
        }
        public static void Grayscale(Layer photo)
        {
            Bitmap img = GetBitmap(photo.layerBmpFrame);
            ColorMatrix colorMatrix = new ColorMatrix(new float[][] {
                //           r     g     b     a     t
                new float[] {.3f,  .3f,  .3f,  0,    0}, // red
                new float[] {.59f, .59f, .59f, 0,    0}, // green
                new float[] {.11f, .11f, .11f, 0,    0}, // blue
                new float[] {0,    0,    0,    1,    0}, // alpha 
                new float[] {0,    0,    0,    0,    1}  // three translations
            });
            photo.layerBmpFrame = BitmapFrame.Create(GetBitmapSource(ApplyColorMatrix(img, colorMatrix)));
        }


        //------------------------ Начало: Фильтр по Гауссу -------------------------
        public static void GaussianBlur(Layer photo, int radial)
        {
            Bitmap img = GetBitmap(photo.layerBmpFrame);
            var gaussianBlur = new GaussianBlur(img);
            img = gaussianBlur.Process(radial);
            photo.layerBmpFrame = BitmapFrame.Create(GetBitmapSource(img));
        }
        //--------------------- Конец: Фильтр по Гауссу  --------------------
        public static void SobelFilter(Layer photo, bool grayscale = false)
        {
            Bitmap source = GetBitmap(photo.layerBmpFrame);
            Bitmap resultBitmap = ConvolutionFilter(source,
                /*Горизонталь*/ new double[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } },
                /*Вертикаль*/ new double[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } },
                1.0, 0, grayscale);
            photo.layerBmpFrame = BitmapFrame.Create(GetBitmapSource(resultBitmap));
        }

        public static Bitmap ConvolutionFilter
        (
            this Bitmap sourceBitmap,
            double[,] xFilterMatrix,
            double[,] yFilterMatrix,
            double factor = 1,
            int bias = 0,
            bool grayscale = false
        )
        {
            BitmapData sourceData = sourceBitmap.LockBits(
                new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];
            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);
            if (grayscale == true)
            {
                float rgb = 0;
                for (int k = 0; k < pixelBuffer.Length; k += 4)
                {
                    rgb = pixelBuffer[k] * 0.11f;
                    rgb += pixelBuffer[k + 1] * 0.59f;
                    rgb += pixelBuffer[k + 2] * 0.3f;
                    pixelBuffer[k] = (byte)rgb;
                    pixelBuffer[k + 1] = pixelBuffer[k];
                    pixelBuffer[k + 2] = pixelBuffer[k];
                    pixelBuffer[k + 3] = 255;
                }
            }
            double blueX = 0.0;
            double greenX = 0.0;
            double redX = 0.0;
            double blueY = 0.0;
            double greenY = 0.0;
            double redY = 0.0;
            double blueTotal = 0.0;
            double greenTotal = 0.0;
            double redTotal = 0.0;
            int filterOffset = 1;
            int calcOffset = 0;
            int byteOffset = 0;
            for (int offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
                {
                    blueX = greenX = redX = 0;
                    blueY = greenY = redY = 0;
                    blueTotal = greenTotal = redTotal = 0.0;
                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;
                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);
                            blueX += (double)(pixelBuffer[calcOffset]) *
                                xFilterMatrix[filterY + filterOffset, filterX + filterOffset];
                            greenX += (double)(pixelBuffer[calcOffset + 1]) *
                                xFilterMatrix[filterY + filterOffset, filterX + filterOffset];
                            redX += (double)(pixelBuffer[calcOffset + 2]) *
                                xFilterMatrix[filterY + filterOffset, filterX + filterOffset];
                            blueY += (double)(pixelBuffer[calcOffset]) *
                                yFilterMatrix[filterY + filterOffset, filterX + filterOffset];
                            greenY += (double)(pixelBuffer[calcOffset + 1]) *
                                yFilterMatrix[filterY + filterOffset, filterX + filterOffset];
                            redY += (double)(pixelBuffer[calcOffset + 2]) *
                                yFilterMatrix[filterY + filterOffset, filterX + filterOffset];
                        }
                    }
                    blueTotal = Math.Sqrt((blueX * blueX) + (blueY * blueY));
                    greenTotal = Math.Sqrt((greenX * greenX) + (greenY * greenY));
                    redTotal = Math.Sqrt((redX * redX) + (redY * redY));
                    if (blueTotal > 255) { blueTotal = 255; }
                    else if (blueTotal < 0) { blueTotal = 0; }
                    if (greenTotal > 255) { greenTotal = 255; }
                    else if (greenTotal < 0) { greenTotal = 0; }
                    if (redTotal > 255) { redTotal = 255; }
                    else if (redTotal < 0) { redTotal = 0; }
                    resultBuffer[byteOffset] = (byte)(blueTotal);
                    resultBuffer[byteOffset + 1] = (byte)(greenTotal);
                    resultBuffer[byteOffset + 2] = (byte)(redTotal);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }
            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            BitmapData resultData = resultBitmap.LockBits(
                new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);
            return resultBitmap;
        }

        private static Bitmap ConvolutionFilter
        (
            Bitmap sourceBitmap,
            double[,] filterMatrix,
            double factor = 1,
            int bias = 0,
            bool grayscale = false
        )
        {
            BitmapData sourceData = sourceBitmap.LockBits(
                new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];
            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);
            if (grayscale == true)
            {
                float rgb = 0;
                for (int k = 0; k < pixelBuffer.Length; k += 4)
                {
                    rgb = pixelBuffer[k] * 0.11f;
                    rgb += pixelBuffer[k + 1] * 0.59f;
                    rgb += pixelBuffer[k + 2] * 0.3f;
                    pixelBuffer[k] = (byte)rgb;
                    pixelBuffer[k + 1] = pixelBuffer[k];
                    pixelBuffer[k + 2] = pixelBuffer[k];
                    pixelBuffer[k + 3] = 255;
                }
            }
            double blue = 0.0;
            double green = 0.0;
            double red = 0.0;
            int filterWidth = filterMatrix.GetLength(1);
            int filterHeight = filterMatrix.GetLength(0);
            int filterOffset = (filterWidth - 1) / 2;
            int calcOffset = 0;
            int byteOffset = 0;
            for (int offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
                {
                    blue = 0; green = 0; red = 0;
                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;
                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);
                            blue += (double)(pixelBuffer[calcOffset]) *
                                filterMatrix[filterY + filterOffset, filterX + filterOffset];
                            green += (double)(pixelBuffer[calcOffset + 1]) *
                                filterMatrix[filterY + filterOffset, filterX + filterOffset];
                            red += (double)(pixelBuffer[calcOffset + 2]) *
                                filterMatrix[filterY + filterOffset, filterX + filterOffset];
                        }
                    }
                    blue = factor * blue + bias;
                    green = factor * green + bias;
                    red = factor * red + bias;
                    if (blue > 255) { blue = 255; }
                    else if (blue < 0) { blue = 0; }
                    if (green > 255) { green = 255; }
                    else if (green < 0) { green = 0; }
                    if (red > 255) { red = 255; }
                    else if (red < 0) { red = 0; }
                    resultBuffer[byteOffset] = (byte)(blue);
                    resultBuffer[byteOffset + 1] = (byte)(green);
                    resultBuffer[byteOffset + 2] = (byte)(red);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }
            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            BitmapData resultData = resultBitmap.LockBits(
                new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);
            return resultBitmap;
        }


        //------------- Конец: Фильтры обнаружения края --------------
        //-----Resize image----
        
        //----end resize


        //-------------------- Начало: Поворот ---------------------


        public static void Rotate(Layer photo, double angle)
        {
            BitmapSource img = photo.layerBmpFrame;
            CachedBitmap cache = new CachedBitmap(img, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            photo.layerBmpFrame = BitmapFrame.Create(new TransformedBitmap(cache, new RotateTransform(angle)));
        }

        public static void RotateBilinear(Layer photo, double angle)
        {
            Bitmap img = GetBitmap(photo.layerBmpFrame);
            if (angle > 180) angle -= 360;
            System.Drawing.Color bkColor = System.Drawing.Color.Transparent;
            System.Drawing.Imaging.PixelFormat pf = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            float sin = (float)Math.Abs(Math.Sin(angle * Math.PI / 180.0)); // в радианы
            float cos = (float)Math.Abs(Math.Cos(angle * Math.PI / 180.0)); // тоже
            float newImgWidth = sin * img.Height + cos * img.Width;
            float newImgHeight = sin * img.Width + cos * img.Height;
            float originX = 0f; float originY = 0f;
            if (angle > 0)
            {
                if (angle <= 90)
                    originX = sin * img.Height;
                else
                {
                    originX = newImgWidth;
                    originY = newImgHeight - sin * img.Width;
                }
            }
            else
            {
                if (angle >= -90)
                    originY = sin * img.Width;
                else
                {
                    originX = newImgWidth - sin * img.Height;
                    originY = newImgHeight;
                }
            }
            Bitmap newImg = new Bitmap((int)newImgWidth*2, (int)newImgHeight*2, pf);
            Graphics g = Graphics.FromImage(newImg);
            g.Clear(bkColor);
            g.TranslateTransform(originX*2, originY*2); // смещение начала координат
            g.RotateTransform((float)angle); // начало поворота
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.DrawImageUnscaled(img, 0, 0); // Рисую изображение  0, 0
            g.Dispose();
            photo.layerBmpFrame = BitmapFrame.Create(GetBitmapSource(newImg));
        }
        //----------------- Конец: Поворот -----------------

        //---sizeImage
        public static BitmapFrame CreateResizedImage(ImageSource source, int height, int width, int margin)
        {

            var rect = new Rect(margin, margin, width - margin * 2, height - margin * 2);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);


            var resizedImage = new RenderTargetBitmap(
                width, height,         // Resized dimensions
                96, 96,                // Default DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            return BitmapFrame.Create(resizedImage);
        }
        //--
        
        
    }
}
