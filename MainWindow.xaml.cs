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

        public static ObservableCollection<LayerWidget> LayersWidgets { get; set; }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //?????GlobalState.setLayerSize(mainCanvas.ActualWidth, mainCanvas.ActualHeight);
            text_2.Text = "" + mainCanvas.ActualHeight + " " + mainCanvas.ActualWidth;

            //newLayer(1,350,350);

            text.Text = "" + mainCanvas.Children.Count + widgetsCanvas.Items.Count + GlobalState.currentLayerIndex;
        }


        // SAVE/OPEN


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
                int x = bmpFrame.PixelHeight; //размер изображения
                int y = bmpFrame.PixelWidth;
                int ind = 0;

                if (x > 300 || y > 400) // если большое, уменьшается в 2 раза 
                {
                    BitmapFrame img = Effects.CreateResizedImage(bmpFrame, x/2, y/2, 20);
                    ind = 1;
                } else
                {
                    BitmapFrame img = Effects.CreateResizedImage(bmpFrame, x , y , 20);
                }

                if (mainCanvas.Children.Count == 0)
                    if (ind == 0) newLayer(1, x, y);
                    else newLayer(1, x / 2, y / 2);
                int index = GlobalState.currentLayerIndex;
                var layer = LayersWidgets[index].ThisLayer;

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


        // NEW/DELETE


        public void UpdateLayersZIndex()
        {
            if (mainCanvas.Children.Count > 0)
            {
                for(int i = LayersWidgets.Count - 1; i >= 0; i--)
                    Panel.SetZIndex(LayersWidgets[i].ThisLayer, LayersWidgets.Count - 1 - i);
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

            layer.Background = new SolidColorBrush(Colors.White);

            // Перемещение элемента в самый верх списка, для наглядности отображения верхних слоев пользователю
            LayerWidget last = LayersWidgets.Last();
            for (int i = LayersWidgets.Count - 1; i > 0; i--)
            {
                LayersWidgets[i] = LayersWidgets[i - 1];
             
            }
            LayersWidgets[0] = last;

            if (widgetsCanvas.Items.Count > 0)
                widgetsCanvas.SelectedIndex = 0;

            GlobalState.LayersCount = mainCanvas.Children.Count;
            GlobalState.currentLayerIndex = widgetsCanvas.SelectedIndex;
            
            text.Text = ""  + widgetsCanvas.Items.Count + LayersWidgets.IndexOf(layer.Widget) + GlobalState.currentLayerIndex;
        }

        private void btnNewLayer_Click(object sender, RoutedEventArgs e)
        {
            newLayer(1,350,350);
        }

        private void btnDeleteLayer_Click(object sender, RoutedEventArgs e)
        {
            int index = GlobalState.currentLayerIndex;
            if (LayersWidgets.Count > 0 && index <= LayersWidgets.Count)
            {
                var layer = LayersWidgets[index].ThisLayer;
                LayerWidget widget = layer.Widget;

                layer.Children.Clear();
                mainCanvas.Children.Remove(layer);
                LayersWidgets.Remove(widget);
                widgetsCanvas.Items.Refresh();

                if (index != 0) widgetsCanvas.SelectedIndex = GlobalState.currentLayerIndex = index - 1;
                else widgetsCanvas.SelectedIndex = GlobalState.currentLayerIndex = 0;
            }
            GlobalState.LayersCount = mainCanvas.Children.Count;
            UpdateLayersZIndex();

            text.Text = "" + mainCanvas.Children.Count + widgetsCanvas.Items.Count + GlobalState.currentLayerIndex;
        }


        // LAYERS SWAP/OPACITY


        private void SwapLayers(int curIndx, int nextIndx)
        {
            LayerWidget curWidget = LayersWidgets[curIndx];
            LayerWidget nextWidget = LayersWidgets[nextIndx];

            LayersWidgets[curIndx] = LayersWidgets[nextIndx];
            LayersWidgets[nextIndx] = curWidget;

            UpdateLayersZIndex();

            GlobalState.currentLayerIndex = nextIndx;
            widgetsCanvas.SelectedIndex = GlobalState.currentLayerIndex;
        }

        private void MoveLayerUp(object sender, RoutedEventArgs e)
        {
            if (GlobalState.currentLayerIndex > 0)
                SwapLayers(GlobalState.currentLayerIndex, GlobalState.currentLayerIndex - 1);
        }

        private void MoveLayerDown(object sender, RoutedEventArgs e)
        {
            if (GlobalState.currentLayerIndex < widgetsCanvas.Items.Count - 1)
               SwapLayers(GlobalState.currentLayerIndex, GlobalState.currentLayerIndex + 1);
        }
        
        private void sliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int index = GlobalState.currentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;
            layer.Opacity = sliderOpacity.Value / 100;
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

        //изминение размера 
        private void SizeImg(object sender, RoutedEventArgs e)
        {

            sizeimage Size = new sizeimage();

            if (Size.ShowDialog() == true)
            {

                double SizeW = double.Parse(Size.SizeWs);
                double SizeH = double.Parse(Size.SizeHs);
                Layer layer = (Layer)mainCanvas.Children[GlobalState.currentLayerIndex];
               int  SizeHi = (int)SizeH;
                int SizeWi = (int)SizeW;
                BitmapFrame img = Effects.CreateResizedImage(layer.layerImageBrush.ImageSource, SizeWi, SizeHi, 20); 
             
                layer.refreshBrush();
            }
        }
        //--


        //private void Resize(object sender, RoutedEventArgs e)
        //{
        //    SizeImage BoxWindow = new SizeImage(); 
        //    if (BoxWindow.ShowDialog() == true)
        //    {
        //        int x = int.Parse(BoxWindow.SizeImagesHeight);
        //        int y = int.Parse(BoxWindow.SizeImageWidth);
        //        Layer layer = (Layer)mainCanvas.Children[GlobalState.currentLayerIndex];
        //        Effects.Resized(layer, y, x, BitmapScalingMode.LowQuality);
        //        layer.refreshBrush();

        //    }
        //}


        // DRAWING


       // Point currentPoint = new Point();
        //Point nextPoint = new Point();

        private void mainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && GlobalState.CurrentTool == GlobalState.Instruments.Brush)
            {
                int index = GlobalState.currentLayerIndex;
                var layer = LayersWidgets[index].ThisLayer;
                Polyline polyLine = new Polyline();
                polyLine.Stroke = VisualHost.BrushColor;
                polyLine.StrokeThickness = VisualHost.BrushSize;

                layer.Children.Add(polyLine);

                GlobalState.MousePressed = true;
                text_2.Text += "\npressed leftButtonDown";
            }
        }
        
        private void mainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Released && GlobalState.CurrentTool == GlobalState.Instruments.Brush)
            {
                GlobalState.MousePressed = false;
                text_2.Text += "\nreleased";
            }
        }

        private void mainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (GlobalState.MousePressed == true && GlobalState.CurrentTool == GlobalState.Instruments.Brush)
            {
                int index = GlobalState.currentLayerIndex;
                var layer = LayersWidgets[index].ThisLayer;
                var polyLine = (Polyline)layer.Children[layer.Children.Count - 1];
                Point currentPoint = e.GetPosition(layer);
                polyLine.Points.Add(currentPoint);
            }
        }

        private void sliderBrushSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VisualHost.BrushSize = sliderBrushSize.Value;
        }


        // TEST OUTPUT


        static public void Text_2(Layer layer)
        {
            ((MainWindow)Application.Current.MainWindow).text_2.Text = "ln "
                + layer.LayerName
                + " zi "
                + Panel.GetZIndex(layer)
                + " wi "
                + LayersWidgets.IndexOf(layer.Widget)
                + " cur "
                + GlobalState.currentLayerIndex;

        }


        // LISTBOX KEY SELECT RESTRICTION


        private void listBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }


        // INSTRUMENTS


        private void Brush_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = GlobalState.Instruments.Brush;
            ArrowButton.BorderThickness = new Thickness(1);
            BrushButton.BorderThickness = new Thickness(0.5);
        }

        private void Arrow_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = GlobalState.Instruments.Arrow;
            BrushButton.BorderThickness = new Thickness(1);
            ArrowButton.BorderThickness = new Thickness(0.5);
        }

        private void colorRedSelected(object sender, RoutedEventArgs e)
        {
            VisualHost.BrushColor = Brushes.Red;
        }

        private void colorBlackSelected(object sender, RoutedEventArgs e)
        {
            VisualHost.BrushColor = Brushes.Black;
        }
    }
}