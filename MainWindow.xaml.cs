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
using System.Runtime.InteropServices;

namespace PhotoEditor
{
    public partial class MainWindow : Window
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();
        [DllImport("Kernel32")]
        public static extern void FreeConsole();
        [DllImport("Kernel32")]
        static extern IntPtr GetConsoleWindow();


        public MainWindow()
        {
            InitializeComponent();
            LayersWidgets = new ObservableCollection<LayerWidget>();
            widgetsCanvas.DataContext = this;
            MouseMove += new MouseEventHandler(mainCanvas_MouseMove);
        }

        public static ObservableCollection<LayerWidget> LayersWidgets { get; set; }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            text_2.Text = "" + mainCanvas.ActualHeight + " " + mainCanvas.ActualWidth;

            newLayer(1, 350, 350);

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


        // LAYER NEW / DELETE


        public void UpdateLayersZIndex()
        {
            if (mainCanvas.Children.Count > 0)
            {
                for(int i = LayersWidgets.Count - 1; i >= 0; i--)
                    Panel.SetZIndex(LayersWidgets[i].ThisLayer, LayersWidgets.Count - 1 - i);
            }
        }

        public void newLayer(double opacity, double PixelHeight, double PixelWidth)
        {
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


        // LAYERS SWAP / OPACITY


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
                if (x==90 || x==180 || x == 360) { Effects.Rotate(layer, x); } else { Effects.RotateBilinear(layer, x); }
                
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


        // MainCanvas actions (DRAWING / RESIZING)


        public Point clickPosition;

        private void mainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int index = GlobalState.currentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;

            if (GlobalState.CurrentTool == GlobalState.Instruments.Resize)
            {
                clickPosition = e.GetPosition(layer);
                GlobalState.MousePressed = true;
                GlobalState.isResizing = true;
            }

            if (e.ButtonState == MouseButtonState.Pressed && GlobalState.CurrentTool == GlobalState.Instruments.Brush)
            {
                Polyline polyLine = new Polyline();
                polyLine.Stroke = VisualHost.BrushColor;
                polyLine.StrokeThickness = VisualHost.BrushSize;

                layer.Children.Add(polyLine);

                GlobalState.MousePressed = true;
            }
            Console.WriteLine("mousedown " + Cursor + " " + clickPosition.X + " " + clickPosition.Y);
        }
        
        private void mainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int index = GlobalState.currentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;

            if (GlobalState.CurrentTool == GlobalState.Instruments.Resize)
            {
                GlobalState.MousePressed = false;
                GlobalState.isResizing = false;
            }

            if (e.ButtonState == MouseButtonState.Released && GlobalState.CurrentTool == GlobalState.Instruments.Brush)
            {
                GlobalState.MousePressed = false;
            }
        }

        private void mainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (LayersWidgets.Count > 0)
            {
                int index = GlobalState.currentLayerIndex;
                var layer = LayersWidgets[index].ThisLayer;
                Point newPosition = new Point();

                // Setting Cursor
                if (GlobalState.CurrentTool == GlobalState.Instruments.Resize)
                {
                    newPosition.X = e.GetPosition(layer).X;
                    newPosition.Y = e.GetPosition(layer).Y;
                    var width = layer.ActualWidth;
                    var height = layer.ActualHeight;
                    var xPos = layer.LayerPosition.X;
                    var yPos = layer.LayerPosition.Y;
                    TranslatePoint(newPosition, layer);
                    bool stretchWidth = (newPosition.X >= 0 && newPosition.X <= width) ? true : false;
                    bool stretchHeight = (newPosition.Y >= 0 && newPosition.Y <= height) ? true : false;
                    bool stretchRight = (newPosition.X > width - 5 && newPosition.X < width && stretchHeight && stretchWidth) ? true : false;
                    bool stretchBottom = (newPosition.Y > height - 5 && newPosition.Y < height && stretchHeight && stretchWidth) ? true : false;
                    
                    Console.WriteLine(e.GetPosition(mainCanvas) + " " + (xPos + width));

                    if (stretchRight && stretchBottom && !GlobalState.isResizing)
                    {
                        Cursor = Cursors.SizeNWSE;
                    }
                    else if (stretchRight && !stretchBottom && !GlobalState.isResizing)
                    {
                        // <->
                        Cursor = Cursors.SizeWE;
                    }
                    else if (stretchBottom && !stretchRight && !GlobalState.isResizing)
                    {
                        Cursor = Cursors.SizeNS;
                    }
                    else if (!stretchBottom && !stretchRight && !GlobalState.isResizing)
                    {
                        Cursor = Cursors.Arrow;
                    }
                }

                // Resizing Layer
                if (GlobalState.MousePressed && GlobalState.isResizing)
                {
                    Text_2(layer);
                    double mousePosX = e.GetPosition(layer).X;
                    double mousePosY = e.GetPosition(layer).Y;

                    double xDiff = mousePosX - clickPosition.X;
                    double yDiff = mousePosY - clickPosition.Y;
                    
                    xDiff = (layer.Width + xDiff) > layer.MinWidth ? xDiff : layer.MinWidth;
                    yDiff = (layer.Height + yDiff) > layer.MinHeight ? yDiff : layer.MinHeight;
                   
                    if (Cursor == Cursors.SizeNWSE)
                    {
                        layer.Width += xDiff;
                        layer.Height += yDiff;
                    }
                    else if (Cursor == Cursors.SizeWE)
                        layer.Width += xDiff;
                    else if (Cursor == Cursors.SizeNS)
                        layer.Height += yDiff;
                    
                    clickPosition.X = mousePosX;
                    clickPosition.Y = mousePosY;
                }

                // Drawing
                if (GlobalState.MousePressed && GlobalState.CurrentTool == GlobalState.Instruments.Brush)
                {
                    var polyLine = (Polyline)layer.Children[layer.Children.Count - 1];
                    Point currentPoint = e.GetPosition(layer);
                    polyLine.Points.Add(currentPoint);
                }
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
                + GlobalState.currentLayerIndex
                + "\npos "
                + Mouse.GetPosition(layer);

        }

        private void OpenConsole(object sender, RoutedEventArgs e)
        {
            if (GetConsoleWindow() != IntPtr.Zero)
            {
                FreeConsole();
            }
            else
            {
                AllocConsole();
            }
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
            ArrowButton.BorderThickness = new Thickness(0.5);
            BrushButton.BorderThickness = new Thickness(1);
            ResizeButton.BorderThickness = new Thickness(0.5);
        }

        private void Resize_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = GlobalState.Instruments.Resize;
            ArrowButton.BorderThickness = new Thickness(0.5);
            BrushButton.BorderThickness = new Thickness(0.5);
            ResizeButton.BorderThickness = new Thickness(1);
        }

        private void Arrow_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = GlobalState.Instruments.Arrow;
            ArrowButton.BorderThickness = new Thickness(1);
            BrushButton.BorderThickness = new Thickness(0.5);
            ResizeButton.BorderThickness = new Thickness(0.5);
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