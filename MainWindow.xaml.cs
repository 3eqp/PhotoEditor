using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Collections.Generic;
using PhotoEditor;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PhotoEditor.Controls;


namespace PhotoEditor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LayersWidgets = new List<LayerWidget>();
        }

        // Layer -> Widget
        public static List<LayerWidget> LayersWidgets { get; set; }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalState.setLayerSize(mainCanvas.ActualWidth, mainCanvas.ActualHeight);
            text_2.Text = "" + mainCanvas.ActualHeight + " " + mainCanvas.ActualWidth;

            newLayer(1);

            text.Text = "" + mainCanvas.Children.Count + layerCanvas.Children.Count + GlobalState.currentLayerIndex;
        }
        private void btnSavePng(object sender, RoutedEventArgs e)
        {
            btnSave_Click(new PngBitmapEncoder(), ".png");
        }
        private void SaveToJpg(object sender, RoutedEventArgs e)
        {
            btnSave_Click(new PngBitmapEncoder(), ".jpg");
        }
        private void SaveToBmp(object sender, RoutedEventArgs e)
        {
            btnSave_Click(new PngBitmapEncoder(), ".bmp");
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

                int index = GlobalState.currentLayerIndex;
                var layer = (Layer)mainCanvas.Children[index];

                BitmapFrame bmpFrame = BitmapFrame.Create(new Uri(op.FileName));
                layer.layerBmpFrame = bmpFrame;
                layer.refreshBrush();
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

            SaveAs(rtb, filename);
        }

        private static void SaveAs(RenderTargetBitmap bmp, string filename)
        {
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmp));

            using (FileStream stm = File.Create(filename))
            {
                enc.Save(stm);
            }
        }
       

        private void btnSave_Click(BitmapEncoder encoder, string format)
        {
            var saveDlg = new SaveFileDialog();
            switch (format) {
                case ".jpg":
                    saveDlg.Filter = "JPG|*.jpg";
                    break;
                case ".png":
                    saveDlg.Filter = "PNG|*.png";
                    break;
                case ".bmp":
                    saveDlg.Filter = "BMP|*.bmp";
                    break;
            }
            

            if (saveDlg.ShowDialog() == true)
            {
                SaveCanvas(mainCanvas, 96, saveDlg.FileName);
            }
        }

        public static void RefreshLayersWidgets()
        {
            int count = 0;
            foreach (LayerWidget widget in LayersWidgets)
            {
                widget.widgetIndex = LayersWidgets.IndexOf(widget);
                if (GlobalState.currentLayerIndex != count)
                    widget.Background = new SolidColorBrush(Colors.Transparent);
                else widget.Background = new SolidColorBrush(Colors.Red);
                count += 1;
            }
        }

        public void newLayer(double opacity)
        {
            double Width = GlobalState.layerWidth;
            double Height = GlobalState.layerHeight;
            string layerName = "NewLayer" + LayersWidgets.Count;
            var layer = new Layer(layerName, Width, Height, opacity, 1, 2, 1, layerCanvas);
            mainCanvas.Children.Add(layer);
            LayersWidgets.Add(layer.Widget);
            GlobalState.currentLayerIndex = LayersWidgets.Count - 1;
            layer.Background = new SolidColorBrush(Colors.White);


            RefreshLayersWidgets();
            text.Text = "" + mainCanvas.Children.Count + layerCanvas.Children.Count + GlobalState.currentLayerIndex;
        }

        private void btnNewLayer_Click(object sender, RoutedEventArgs e)
        {
            newLayer(1);
        }

        private void btnDeleteLayer_Click(object sender, RoutedEventArgs e)
        {
            int index = GlobalState.currentLayerIndex;
            if (LayersWidgets.Count > 0 && index <= LayersWidgets.Count)
            {
                var layer = (Layer)mainCanvas.Children[index];
                LayerWidget widget = layer.Widget;
                mainCanvas.Children.Remove(layer);
                LayersWidgets.Remove(widget);
                if (index > 0) GlobalState.currentLayerIndex = index - 1;
                layerCanvas.Children.Remove(widget);
            }

            RefreshLayersWidgets();
            text.Text = "" + mainCanvas.Children.Count + layerCanvas.Children.Count + GlobalState.currentLayerIndex;
        }

        private void btnEffect_Click(object sender, RoutedEventArgs e)
        {
            var layer = (Layer)mainCanvas.Children[GlobalState.currentLayerIndex];
            Effects.Negative(layer);
            layer.refreshBrush();
        }

        private void Grayscale(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)mainCanvas.Children[GlobalState.currentLayerIndex];
            Effects.Grayscale(layer);
            layer.refreshBrush();
        }

        private void GaussianBlur(object sender, RoutedEventArgs e)
        {
            Turn BoxWindow = new Turn();
            if (BoxWindow.ShowDialog() == true)
            {
                int x = int.Parse(BoxWindow.Turns);
                Layer layer = (Layer)mainCanvas.Children[GlobalState.currentLayerIndex];
                Effects.GaussianBlur(layer, x);
                layer.refreshBrush();
            }
        }

        private void SobelFilter(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)mainCanvas.Children[GlobalState.currentLayerIndex];
            Effects.SobelFilter(layer);
            layer.refreshBrush();
        }

        private void SobelFilterGrayscale(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)mainCanvas.Children[GlobalState.currentLayerIndex];
            Effects.SobelFilter(layer, true);
            layer.refreshBrush();
        }

        private void Rotate90(object sender, RoutedEventArgs e)
        {
            
            Turn BoxWindow = new Turn();
            if (BoxWindow.ShowDialog() == true)
            {
                
                double x = double.Parse(BoxWindow.Turns);
                Layer layer = (Layer)mainCanvas.Children[GlobalState.currentLayerIndex];
                Effects.Rotate(layer, x);
                layer.refreshBrush();
            }
        }


        // TEST OUTPUT
        static public void Text_2(Layer layer)
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).text_2.Text = "" + layer.LayerName + " " + GlobalState.currentLayerIndex;

        }

       

    }
}