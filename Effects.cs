using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System;
using System.Windows;
//using Accord.Imaging.Filters;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;

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
            Bitmap img = GetBitmap(photo.bmpFrame);
            ColorMatrix colorMatrix = new ColorMatrix(new float[][] {
                //           r    g    b    a    t
                new float[] {-1,  0,   0,   0,   0}, // red
                new float[] {0,  -1,   0,   0,   0}, // green
                new float[] {0,   0,  -1,   0,   0}, // blue
                new float[] {0,   0,   0,   1,   0}, // alpha 
                new float[] {1,   1,   1,   1,   1}  // three translations
            });
            photo.bmpFrame = BitmapFrame.Create(GetBitmapSource(ApplyColorMatrix(img, colorMatrix)));
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
    }
}
