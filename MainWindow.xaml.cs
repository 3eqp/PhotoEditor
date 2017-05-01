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
            LayersWidgets = new ObservableCollection<LayerWidget>();
            widgetsCanvas.DataContext = this;
        }

        // Layer -> Widget
        public static ObservableCollection<LayerWidget> LayersWidgets { get; set; }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalState.setLayerSize(mainCanvas.ActualWidth, mainCanvas.ActualHeight);
            text_2.Text = "" + mainCanvas.ActualHeight + " " + mainCanvas.ActualWidth;

            newLayer(1,350,350);

            text.Text = "" + GlobalState.LayersCount + widgetsCanvas.Items.Count + GlobalState.currentLayerIndex;
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
                BitmapFrame bmpFrame = BitmapFrame.Create(new Uri(op.FileName));
                int x = bmpFrame.PixelHeight;
                int y = bmpFrame.PixelWidth;
                newLayer(1,x,y);

                int index = GlobalState.currentLayerIndex;
                var layer = (Layer)mainCanvas.Children[index];


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

        public void UpdateLayersZIndex()
        {
            if (mainCanvas.Children.Count > 0)
            {
                var first = (Layer)mainCanvas.Children[0];
                Panel.SetZIndex(first, 0);
                int count = mainCanvas.Children.Count;
                for (int i = 0; i < count; i++)
                {
                    var layer = (Layer)mainCanvas.Children[i];
                    int layerInd = Panel.GetZIndex(layer);
                    if (layerInd != i)
                        for (int j = count - mainCanvas.Children.IndexOf(layer); j >= 0; j--)
                        {
                            int layerPrevInd = Panel.GetZIndex(mainCanvas.Children[j]);
                            if (layerInd != layerPrevInd)
                                Panel.SetZIndex(layer, layerInd--);
                        }
                }
            }
        }

        public void newLayer(double opacity, int PixelHeight, int PixelWidth)
        {
            double Width = GlobalState.layerWidth;
            double Height = GlobalState.layerHeight;
            string layerName = "NewLayer" + LayersWidgets.Count;
            var layer = new Layer(layerName, PixelWidth, PixelHeight, opacity, 1, 2, 1);

            mainCanvas.Children.Add(layer);
            LayersWidgets.Add(layer.Widget);

            // Перемещение элемента в самый верх списка, для наглядности отображения верхних слоев пользователю
            LayerWidget last = LayersWidgets.Last();
            for (int i = LayersWidgets.Count - 1; i > 0; i--)
            {
                LayersWidgets[i] = LayersWidgets[i - 1];
             
            }
            LayersWidgets[0] = last;


            if (widgetsCanvas.Items.Count > 0)
                widgetsCanvas.SelectedIndex = 0;
            
            GlobalState.currentLayerIndex = widgetsCanvas.SelectedIndex;
            layer.Background = new SolidColorBrush(Colors.White);
            
            text.Text = ""  + widgetsCanvas.Items.Count + LayersWidgets.IndexOf(layer.Widget) + GlobalState.currentLayerIndex;
        }

        private void btnNewLayer_Click(object sender, RoutedEventArgs e)
        {
            newLayer(1,350,350);
        }

        private void btnDeleteLayer_Click(object sender, RoutedEventArgs e)
        {
            int index = GlobalState.currentLayerIndex;
            int count = mainCanvas.Children.Count - 1;
            if (LayersWidgets.Count > 0 && index <= LayersWidgets.Count)
            {
                var layer = (Layer)mainCanvas.Children[count - index];
                LayerWidget widget = layer.Widget;

                layer.Children.Clear();
                mainCanvas.Children.Remove(layer);
                LayersWidgets.Remove(widget);
                widgetsCanvas.Items.Refresh();

                widgetsCanvas.SelectedIndex = GlobalState.currentLayerIndex = index - 1;
            }
            GlobalState.LayersCount = mainCanvas.Children.Count;
            UpdateLayersZIndex();

            text.Text = "" + GlobalState.LayersCount + widgetsCanvas.Items.Count + GlobalState.currentLayerIndex;
        }

        // EFFECTS


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


        // DRAWING


        Point currentPoint = new Point();
        Point nextPoint = new Point();

        private void mainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                GlobalState.MousePressed = true;
                currentPoint = e.GetPosition(this);
                text_2.Text += "\npressed leftButtonDown";
            }
        }
        
        private void mainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Released)
            {
                GlobalState.MousePressed = false;
                text_2.Text += "\nreleased";
            }
        }

        private void mainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (GlobalState.MousePressed == true)
            {
                int index = GlobalState.currentLayerIndex;
                var layer = (Layer)mainCanvas.Children[index];
                Line line = new Line();
                currentPoint = TranslatePoint(currentPoint, mainCanvas);

                line.Stroke = SystemColors.WindowFrameBrush;
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;

                nextPoint.X = e.GetPosition(this).X;
                nextPoint.Y = e.GetPosition(this).Y;
                nextPoint = TranslatePoint(nextPoint, mainCanvas);

                line.X2 = nextPoint.X;
                line.Y2 = nextPoint.Y;

                currentPoint = e.GetPosition(this);
                
                layer.Children.Add(line);
            }
        }

        // TEST OUTPUT
        static public void Text_2(Layer layer)
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).text_2.Text = "ln "
                + layer.LayerName
                + " zi "
                + Panel.GetZIndex(layer)
                + " wi "
                + LayersWidgets.IndexOf(layer.Widget)
                + " cur " 
                + GlobalState.currentLayerIndex;

        }
    }
}