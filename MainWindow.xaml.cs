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
using System.Windows.Shapes;
using PhotoEditor.Controls;
using System.Runtime.InteropServices;
using Xceed.Wpf.Toolkit;
using System.Windows.Media.Effects;

namespace PhotoEditor
{
    public partial class MainWindow : Window
    {
        // Window Global Parameters
        public static double WindowTop { get; set; }
        public static double WindowLeft { get; set; }
        public static double WindowWidth { get; set; }
        public static double WindowHeight { get; set; }

        /// <summary>
        /// Trigger that Window gets from StartWindow
        /// 
        /// 1 - New File
        /// 2 - Open File
        /// 3 - Open Photo
        /// </summary>
        static public int WindowTrigger;

        public MainWindow()
        {
            InitializeComponent();

            LayersWidgets = new ObservableCollection<LayerWidget>();
            widgetsCanvas.DataContext = this;

            #region Button&Mouse Events
            // MainCanvas mouse events
            mainCanvas.MouseMove += new MouseEventHandler(MainCanvas_MouseMove);
            mainCanvas.MouseWheel += new MouseWheelEventHandler(MainCanvas_MouseWheel);
            // System buttons
            MinimizeButton_Black.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(MinimizeButtonUp), true);
            CloseButton_Black.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(CloseButtonUp), true);
            MaximizeButton_Black.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(MaximizeButtonUp), true);
            // Navigator buttons
            AddPhotoButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(ButtonOpenPhoto_Click), true);
            ArrowButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Arrow_Selected), true);
            ResizeButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Resize_Selected), true);
            RotateButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Rotate_Click), true);
            ColorButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(ColorButton_MouseDown), true);
            FillButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Fill_Selected), true);
            EraseButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Erase_Selected), true);
            BrushButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Brush_Selected), true);

            #endregion
            Hide();
            Start StartWindow = new Start();
            StartWindow.Show();
        }

        public static ObservableCollection<LayerWidget> LayersWidgets { get; set; }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindowState.IsOpen = true;
            
            WindowTop = EditorWindow.Top;
            WindowLeft = EditorWindow.Left;
            WindowHeight = EditorWindow.Height;
            WindowWidth = EditorWindow.Width;
        }

        // Actions from Start Window
        void WindowActions()
        {
            if (WindowTrigger == 1)
            {
                NewLayer(GlobalState.NewLayerHeight, GlobalState.NewLayerWidth);
                Show();
            }

            if (WindowTrigger == 2)
            {
                // Add Open File
                Show();
            }

            if (WindowTrigger == 3)
            {
                OpenPhoto();
                Show();
            }
        }
        
        // OPEN

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenPhoto();
        }

        private void ButtonOpenPhoto_Click(object sender, MouseButtonEventArgs e)
        {
            OpenPhoto();
        }

        private void OpenPhoto()
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";

            if (op.ShowDialog() == true)
            {
                double HeightCanvas, WidthCanvas;
                BitmapFrame bmpFrame = BitmapFrame.Create(new Uri(op.FileName));
                HeightCanvas = bmpFrame.PixelHeight;
                WidthCanvas = bmpFrame.PixelWidth;
                
                NewLayer(HeightCanvas, WidthCanvas);
                int index = GlobalState.CurrentLayerIndex;
                var layer = LayersWidgets[index].ThisLayer;

                layer.LayerBmpFrame = bmpFrame;
                layer.RefreshBrush();
            }
        }


        // SAVE


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
       
        private void ExportAs(BitmapEncoder encoder, string format)
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



        // LAYER EXPORT


        private void ExportAsPNG(object sender, RoutedEventArgs e)
        {
            ExportAs(new PngBitmapEncoder(), ".png");
        }

        private void ExportAsJPG(object sender, RoutedEventArgs e)
        {
            ExportAs(new PngBitmapEncoder(), ".jpg");
        }

        private void ExportAsBMP(object sender, RoutedEventArgs e)
        {
            ExportAs(new PngBitmapEncoder(), ".bmp");
        }


        // LAYER NEW / DELETE


        public void UpdateLayersZIndex()
        {
            if (mainCanvas.Children.Count > 0)
            {
                for (int i = LayersWidgets.Count - 1; i >= 0; i--)
                {
                    Panel.SetZIndex(LayersWidgets[i].ThisLayer, LayersWidgets.Count - 1 - i);
                }
            }
        }

        public void NewLayer(double PixelHeight, double PixelWidth)
        {
            string layerName = "NewLayer" + LayersWidgets.Count;
            var layer = new Layer(layerName, PixelWidth, PixelHeight, 1, 1, 1, 2, 1);

            mainCanvas.Children.Add(layer);
            LayersWidgets.Add(layer.Widget);

            layer.Background = new SolidColorBrush(Colors.White);

            // Replace layer to top of the list
            LayerWidget last = LayersWidgets.Last();
            for (int i = LayersWidgets.Count - 1; i > 0; i--)
            {
                LayersWidgets[i] = LayersWidgets[i - 1];
            }
            LayersWidgets[0] = last;

            if (widgetsCanvas.Items.Count > 0)
                widgetsCanvas.SelectedIndex = 0;

            GlobalState.LayersCount = mainCanvas.Children.Count;
            widgetsCanvas.SelectedIndex = GlobalState.LayersCount;
            GlobalState.CurrentLayerIndex = widgetsCanvas.SelectedIndex;
        }

        private void NewLayer_Click(object sender, RoutedEventArgs e)
        {
            NewLayer(GlobalState.DefaultLayerHeight, GlobalState.DefaultLayerWidth);
        }

        private void DeleteLayer_Click(object sender, RoutedEventArgs e)
        {
            int index = GlobalState.CurrentLayerIndex;
            if (LayersWidgets.Count > 0 && index <= LayersWidgets.Count)
            {
                var layer = LayersWidgets[index].ThisLayer;
                LayerWidget widget = layer.Widget;

                layer.Children.Clear();
                mainCanvas.Children.Remove(layer);
                LayersWidgets.Remove(widget);
                widgetsCanvas.Items.Refresh();

                if (index != 0) widgetsCanvas.SelectedIndex = GlobalState.CurrentLayerIndex = index - 1;
                else widgetsCanvas.SelectedIndex = GlobalState.CurrentLayerIndex = 0;
            }
            GlobalState.LayersCount = mainCanvas.Children.Count;
            UpdateLayersZIndex();
        }


        // LAYERS SWAP / OPACITY


        private void SwapLayers(int curIndx, int nextIndx)
        {
            LayerWidget curWidget = LayersWidgets[curIndx];
            LayerWidget nextWidget = LayersWidgets[nextIndx];

            LayersWidgets[curIndx] = LayersWidgets[nextIndx];
            LayersWidgets[nextIndx] = curWidget;

            UpdateLayersZIndex();

            GlobalState.CurrentLayerIndex = nextIndx;
            widgetsCanvas.SelectedIndex = GlobalState.CurrentLayerIndex;
        }

        private void MoveLayerUp(object sender, RoutedEventArgs e)
        {
            if (GlobalState.CurrentLayerIndex > 0)
                SwapLayers(GlobalState.CurrentLayerIndex, GlobalState.CurrentLayerIndex - 1);
        }

        private void MoveLayerDown(object sender, RoutedEventArgs e)
        {
            if (GlobalState.CurrentLayerIndex < widgetsCanvas.Items.Count - 1)
               SwapLayers(GlobalState.CurrentLayerIndex, GlobalState.CurrentLayerIndex + 1);
        }
        
        private void SliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int index = GlobalState.CurrentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;
            layer.Opacity = sliderOpacity.Value / 100;
        }


        // EFFECTS


        private void Negative_Click(object sender, RoutedEventArgs e)
        {
            int index = GlobalState.CurrentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;
            Effects.Negative(layer);
            layer.RefreshBrush();
        }

        private void Grayscale_Click(object sender, RoutedEventArgs e)
        {
            int index = GlobalState.CurrentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;
            Effects.Grayscale(layer);
            layer.RefreshBrush();
        }

        private void GaussianBlur_Click(object sender, RoutedEventArgs e)
        {
            Turn BoxWindow = new Turn();
            if (BoxWindow.ShowDialog() == true)
            {
                int x = int.Parse(BoxWindow.Turns);
                int index = GlobalState.CurrentLayerIndex;
                var layer = LayersWidgets[index].ThisLayer;
                Effects.GaussianBlur(layer, x);
                layer.RefreshBrush();
            }
        }

        private void SobelFilter_Click(object sender, RoutedEventArgs e)
        {
            int index = GlobalState.CurrentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;
            Effects.SobelFilter(layer);
            layer.RefreshBrush();
        }

        private void SobelFilterGrayscale_Click(object sender, RoutedEventArgs e)
        {
            int index = GlobalState.CurrentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;
            Effects.SobelFilter(layer, true);
            layer.RefreshBrush();
        }

        private void Rotate_Click(object sender, RoutedEventArgs e)
        {
            
            Turn BoxWindow = new Turn();
            if (BoxWindow.ShowDialog() == true)
            {
                
                double x = double.Parse(BoxWindow.Turns);
                int index = GlobalState.CurrentLayerIndex;
                var layer = LayersWidgets[index].ThisLayer;
                if (x==90 || x==180 || x == 360) { Effects.Rotate(layer, x); } else { Effects.RotateBilinear(layer, x); }
                
                layer.RefreshBrush();
            }
        }
        

        // LAYER BITMAP FUNCTIONS
        
            
        private BitmapFrame BmpFrameErase(Point pos, Layer layer)
        {
            var bmpFrame = layer.LayerBmpFrame;
            pos.X = pos.X * layer.LayerScale;
            pos.Y = pos.Y * layer.LayerScale;
            var source = new FormatConvertedBitmap(bmpFrame, PixelFormats.Bgra32, null, 0);
            TranslatePoint(pos, layer);

            int width = (int)source.Width;
            int height = (int)source.Height;
            int bytesperpixel = 4;
            int stride = width * bytesperpixel;

            uint transparentPixel = 0x7f;
            uint[] intPixelData = new uint[height * stride];

            source.CopyPixels(intPixelData, stride, 0);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    if (col >= pos.X - 10 && col <= pos.X + 10 && row >= pos.Y - 10 && row <= pos.Y + 10)
                        intPixelData[row * width + col] = transparentPixel;
                }
            }

            var bsCheckerboard = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, intPixelData, stride);
            bmpFrame = BitmapFrame.Create(bsCheckerboard);
            return bmpFrame;
        }

        private void BrushToBitmap()
        {
            int index = GlobalState.CurrentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;
            var size = new Size(layer.ActualWidth, layer.ActualHeight);
            var bitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Pbgra32);

            //fix for render position 
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                VisualBrush visualBrush = new VisualBrush(layer);
                drawingContext.DrawRectangle(visualBrush, null,
                    new Rect(new Point(0, 0), size));
            }
            bitmap.Render(drawingVisual);

            BitmapFrame bmpFrame = BitmapFrame.Create(bitmap);
            layer.LayerBmpFrame = bmpFrame;
            layer.RefreshBrush();
            layer.Children.Clear();
        }


        // MAIN CANVAS TRIGGERS (DRAW/RESIZE/ERASE)


        public Point clickPosition;

        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int index = GlobalState.CurrentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;

            // Erase
            if (GlobalState.CurrentTool == GlobalState.Instruments.Eraser)
            {
                clickPosition = e.GetPosition(layer);
                GlobalState.MousePressed = true;
                GlobalState.IsErasing = true;

                var bmpFrame = BmpFrameErase(clickPosition, layer);
                layer.LayerBmpFrame = bmpFrame;
                layer.RefreshBrush();
            }

            // Resize
            if (GlobalState.CurrentTool == GlobalState.Instruments.Resize)
            {
                clickPosition = e.GetPosition(layer);
                GlobalState.MousePressed = true;
                GlobalState.IsResizing = true;
            }

            // Brush
            if (e.ButtonState == MouseButtonState.Pressed && GlobalState.CurrentTool == GlobalState.Instruments.Brush)
            {
                Polyline polyLine = new Polyline();
                polyLine.Stroke = VisualHost.BrushColor;
                polyLine.StrokeThickness = VisualHost.BrushSize;

                layer.Children.Add(polyLine);

                GlobalState.MousePressed = true;
            }
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int index = GlobalState.CurrentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;

            // Erase
            if (GlobalState.CurrentTool == GlobalState.Instruments.Eraser)
            {
                GlobalState.MousePressed = false;
                GlobalState.IsErasing = false;
            }

            // Resize
            if (GlobalState.CurrentTool == GlobalState.Instruments.Resize)
            {
                GlobalState.MousePressed = false;
                GlobalState.IsResizing = false;
            }

            // Brush
            if (e.ButtonState == MouseButtonState.Released && GlobalState.CurrentTool == GlobalState.Instruments.Brush)
            {
                GlobalState.MousePressed = false;
                BrushToBitmap();
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (LayersWidgets.Count > 0)
            {
                int index = GlobalState.CurrentLayerIndex;
                var layer = LayersWidgets[index].ThisLayer;

                // Erase
                if (GlobalState.MousePressed && GlobalState.IsErasing)
                {
                    Point pos = new Point();
                    pos.X = e.GetPosition(layer).X;
                    pos.Y = e.GetPosition(layer).Y;

                    var bmpFrame = BmpFrameErase(pos, layer);

                    layer.LayerBmpFrame = bmpFrame;
                    layer.RefreshBrush();
                }

                #region Check_Cursor & Resize
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

                    if (stretchRight && stretchBottom && !GlobalState.IsResizing)
                    {
                        Cursor = Cursors.SizeNWSE;
                    }
                    else if (stretchRight && !stretchBottom && !GlobalState.IsResizing)
                    {
                        // <->
                        Cursor = Cursors.SizeWE;
                    }
                    else if (stretchBottom && !stretchRight && !GlobalState.IsResizing)
                    {
                        Cursor = Cursors.SizeNS;
                    }
                    else if (!stretchBottom && !stretchRight && !GlobalState.IsResizing)
                    {
                        Cursor = Cursors.Arrow;
                    }
                }

                // Resizing Layer
                if (GlobalState.MousePressed && GlobalState.IsResizing)
                {
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
                #endregion

                // Drawing
                if (GlobalState.MousePressed && GlobalState.CurrentTool == GlobalState.Instruments.Brush)
                {
                    var polyLine = (Polyline)layer.Children[layer.Children.Count - 1];
                    Point currentPoint = e.GetPosition(layer);
                    polyLine.Points.Add(currentPoint);
                }
            }
        }

        void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int index = GlobalState.CurrentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;

            if (GlobalState.CurrentTool == GlobalState.Instruments.Resize)
            {
                ScaleTransform scaletransform = new ScaleTransform();
                double height = layer.ActualHeight;
                double width = layer.ActualWidth;

                double zoom = e.Delta;

                if (zoom > 0)
                {
                    height = height * 2;
                    width = width * 2;
                    layer.LayerScale /= 2;
                }
                if (zoom < 0)
                {
                    height = height / 2;
                    width = width / 2;
                    layer.LayerScale *= 2;
                }

                layer.Height = height;
                layer.Width = width;
                layer.RenderTransform = scaletransform;
            }
        }


        // INSTRUMENTS


        private void Brush_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = GlobalState.Instruments.Brush;
        }

        private void Resize_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = GlobalState.Instruments.Resize;
        }

        private void Erase_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = GlobalState.Instruments.Eraser;
        }

        private void Arrow_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = GlobalState.Instruments.Arrow;
        }

        private void Fill_Selected(object sender, RoutedEventArgs e)
        {
            int index = GlobalState.CurrentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;
            layer.Background = new SolidColorBrush(VisualHost.BrushColor.Color);
            layer.Widget.previewCanvas.Background = new SolidColorBrush(VisualHost.BrushColor.Color);
        }
        

        // COLOR & OPACITY


        private void SliderBrushSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VisualHost.BrushSize = sliderBrushSize.Value;
        }
        
        private void ColorTranspSelected(object sender, RoutedEventArgs e)
        {
            VisualHost.BrushColor = Brushes.Transparent;
        }

        private void ColorButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ColorPreview.Fill = new SolidColorBrush(Colors.Red);
        }

        /*select color
        public Color? ColorName
        {
            get
            {
                return ColorPicker1.SelectedColor;
            }
        }

        private void SelectColor(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {


            Color Colorid = (Color)ColorName;
            var mySolidColorBrush = new SolidColorBrush(Colorid);

            VisualHost.BrushColor = mySolidColorBrush;
        }

        private void Pouring(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)mainCanvas.Children[GlobalState.currentLayerIndex];


            Color Colorid = (Color)ColorName;

            layer.Background = new SolidColorBrush(Colorid);

            // layer.refreshBrush();
        }
        */


        // SYSTEM BUTTONS


        private void MinimizeButtonUp(object sender, EventArgs e)
        {
            EditorWindow.WindowState = System.Windows.WindowState.Minimized;
        }

        private void MaximizeButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (EditorWindow.Top != 0 && EditorWindow.Left != 0)
            {
                EditorWindow.WindowState = System.Windows.WindowState.Normal;
                EditorWindow.Left = 0;
                EditorWindow.Top = 0;
                EditorWindow.Width = SystemParameters.PrimaryScreenWidth;
                EditorWindow.Height = SystemParameters.PrimaryScreenHeight;
            }
            else
            {
                EditorWindow.WindowState = System.Windows.WindowState.Normal;
                EditorWindow.Left = WindowLeft;
                EditorWindow.Top = WindowTop;
                EditorWindow.Width = WindowWidth;
                EditorWindow.Height = WindowHeight;
            }
        }

        private void CloseButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainWindowState.IsOpen = false;
            Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            WindowTop = EditorWindow.Top;
            WindowTop = EditorWindow.Left;
        }


        // MAIN WINDOW ACTIONS


        public static void CloseMainWindow()
        {
            ((MainWindow)Application.Current.MainWindow).Close();
            MainWindowState.IsOpen = false;
        }

        public static void ShowMainWindow()
        {
            if (!MainWindowState.IsOpen)
            {
                ((MainWindow)Application.Current.MainWindow).WindowActions();
                ((MainWindow)Application.Current.MainWindow).Show();
            }
        }


        // LISTBOX KEY SELECT RESTRICTION


        private void ListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}