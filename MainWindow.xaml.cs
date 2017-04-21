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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            newLayer(1);
            LayerList.layersList[0].Background = new SolidColorBrush(Colors.White);

            text.Text = "" + mainCanvas.Children.Count;
            GlobalState.refreshGlobal();
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
                newLayer(0.5);

                int index = GlobalState.currentLayerIndex;
                Layer layer = (Layer)LayerList.layersList[index];

                BitmapFrame bmpFrame = BitmapFrame.Create(new Uri(op.FileName));
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = bmpFrame;
                layer.layerImageBrush = brush;

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
            string layerName = "NewLayer" + GlobalState.LayersCount;
            Layer layer = new Layer(layerName, GlobalState.currentLayerIndex);
            LayerList.layersList.Add(layer);
            GlobalState.refreshGlobal();
            GlobalState.currentLayerIndex = GlobalState.LayersCount - 1;
            mainCanvas.Children.Add(layer);

            layer.Name = layerName;
            layer.Width = 500;
            layer.Height = 264;
            layer.Opacity = opacity;
            Grid.SetColumn(layer, 1);
            Grid.SetRow(layer, 1);

            text.Text = "" + mainCanvas.Children.Count;
        }

        private void btnNewLayer_Click(object sender, RoutedEventArgs e)
        {
            newLayer(0.5);
        }

        private void btnDeleteLayer_Click(object sender, RoutedEventArgs e)
        {
            int index = GlobalState.currentLayerIndex;
            if (GlobalState.LayersCount > 0)
            {
                Layer layer = (Layer)LayerList.layersList[index];
                mainCanvas.Children.Remove(layer);
                LayerList.layersList.Remove(layer);
                GlobalState.refreshGlobal();
                GlobalState.currentLayerIndex = GlobalState.LayersCount - 1;
            }

            text.Text = "" + mainCanvas.Children.Count;
        }
    }
}