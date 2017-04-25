using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;

namespace PhotoEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalState.setLayerSize(mainCanvas.ActualWidth, mainCanvas.ActualHeight);
            text_2.Text = "" + mainCanvas.ActualHeight + " " + mainCanvas.ActualWidth;

            newLayer(1);
            LayerList.layersList[0].Background = new SolidColorBrush(Colors.White);

            text.Text = "" + mainCanvas.Children.Count + layerCanvas.Children.Count;
        }
        
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";

            if (op.ShowDialog() == true)
            {
                newLayer(1);

                int index = LayerList.currentLayerIndex;
                Layer layer = (Layer)LayerList.layersList[index];

                BitmapFrame bmpFrame = BitmapFrame.Create(new Uri(op.FileName));
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = bmpFrame;
                layer.layerImageBrush = brush;
                layer.bmpFrame = bmpFrame;

                LayerList.layersList[index].Background = brush;
            }
        }

        private void SaveCanvas(Canvas canvas, int dpi, string filename)
        {
            var width = canvas.ActualWidth;
            var height = canvas.ActualHeight;

            var size = new Size(width, height);
            canvas.Measure(size);

            var rtb = new RenderTargetBitmap(
                (int)width,
                (int)height,
                dpi, //dpi x 
                dpi, //dpi y 
                PixelFormats.Pbgra32 // pixelformat 
                );
            rtb.Render(canvas);

            SaveAsPng(rtb, filename);
        }

        private static void SaveAsPng(RenderTargetBitmap bmp, string filename)
        {
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmp));

            using (FileStream stm = File.Create(filename))
            {
                enc.Save(stm);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var saveDlg = new SaveFileDialog
            {
                FileName = "Masterpiece",
                DefaultExt = ".png",
                Filter = "PNG (.png)|*.png"
            };

            if (saveDlg.ShowDialog() == true)
            {
                SaveCanvas(mainCanvas, 96, saveDlg.FileName);
            }
        }

        public void newLayer(double opacity)
        {
            double Width = GlobalState.layerWidth;
            double Height = GlobalState.layerHeight;
            string layerName = "NewLayer" + LayerList.layersList.Count;
            Layer layer = new Layer(layerName, LayerList.currentLayerIndex, Width, Height, opacity, 1, 2, 1, layerCanvas);
            LayerList.layersList.Add(layer);
            LayerList.currentLayerIndex = LayerList.layersList.Count - 1;
            mainCanvas.Children.Add(layer);
            
            text.Text = "" + mainCanvas.Children.Count + layerCanvas.Children.Count;
        }

        private void btnNewLayer_Click(object sender, RoutedEventArgs e)
        {
            newLayer(1);
        }

        private void btnDeleteLayer_Click(object sender, RoutedEventArgs e)
        {
            int index = LayerList.currentLayerIndex;
            if (LayerList.layersList.Count > 0)
            {
                Layer layer = (Layer)LayerList.layersList[index];
                LayerWidget widget = layer.widget;
                mainCanvas.Children.Remove(layer);
                LayerList.layersList.Remove(layer);
                LayerList.currentLayerIndex = LayerList.layersList.Count - 1;
                layerCanvas.Children.Remove(widget);
            }
            text.Text = "" + mainCanvas.Children.Count + layerCanvas.Children.Count;
        }

        private void btnEffect_Click(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)LayerList.layersList[LayerList.currentLayerIndex];
            Effects.Negative(layer);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = layer.bmpFrame;
            layer.Background = brush;
        }

        private void Grayscale(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)LayerList.layersList[LayerList.currentLayerIndex];
            Effects.Grayscale(layer);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = layer.bmpFrame;
            layer.Background = brush;
        }

        private void GaussianBlur(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)LayerList.layersList[LayerList.currentLayerIndex];
            Effects.GaussianBlur(layer, 4);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = layer.bmpFrame;
            layer.Background = brush;
        }

        private void SobelFilter(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)LayerList.layersList[LayerList.currentLayerIndex];
            Effects.SobelFilter(layer);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = layer.bmpFrame;
            layer.Background = brush;
        }

        private void SobelFilterGrayscale(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)LayerList.layersList[LayerList.currentLayerIndex];
            Effects.SobelFilter(layer, true);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = layer.bmpFrame;
            layer.Background = brush;
        }

        private void Rotate90(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)LayerList.layersList[LayerList.currentLayerIndex];
            Effects.Rotate(layer, 90);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = layer.bmpFrame;
            layer.Background = brush;
        }
    }
}