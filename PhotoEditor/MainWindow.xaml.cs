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
using System.Diagnostics;
using System.Windows.Navigation;
using System.Runtime.InteropServices;
using Xceed.Wpf.Toolkit;
using System.Windows.Media.Effects;
using System.Windows.Interop;
using System.Collections.Generic;

namespace PhotoEditor
{
    #region System Blur Parameters

    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }

    #endregion

    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

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

        private int keyCtrlDown = 0;

        public MainWindow()
        {
            InitializeComponent();
            SizeToContent = System.Windows.SizeToContent.Manual;
            MaxHeight = SystemParameters.WorkArea.Height;

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
            MaximizeButtonOFF.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(MaximizeButtonUp), true);
            // Navigator buttons
            AddPhotoButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(ButtonOpenPhoto_Click), true);
            ArrowButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Arrow_Selected), true);
            ResizeButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Resize_Selected), true);
            RotateButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Rotate_Click), true);

            FillButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Fill_Selected), true);
            EraseButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Erase_Selected), true);
            BrushButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Brush_Selected), true);
            // Layers
            LayerUpButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(MoveLayerUp), true);
            LayerDownButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(MoveLayerDown), true);
            AddLayerButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(NewLayer_Click), true);
            DeleteLayerButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(DeleteLayer_Click), true);
            // Effects
            GrayscaleButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Grayscale_Click), true);
            NegativeButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Negative_Click), true);
            GaussianBlurButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(GaussianBlur_Click), true);
            SobelEffectButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(SobelFilter_Click), true);
            SobelEffectGrayScaleButton.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(SobelFilterGrayscale_Click), true);
            #endregion

            Hide();
            Start StartWindow = new Start();
            StartWindow.Show();
            KeyDown += OnKeyDownHandler;
            KeyUp += OnKeyUpHandler;
        }

        public static ObservableCollection<LayerWidget> LayersWidgets { get; set; }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur(this);
            MainWindowState.IsOpen = true;

            WindowTop = EditorWindow.Top;
            WindowLeft = EditorWindow.Left;
            WindowHeight = EditorWindow.Height;
            WindowWidth = EditorWindow.Width;
        }

        public static void EnableBlur(Window win)
        {
            var windowHelper = new WindowInteropHelper(win);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
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
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select a picture";
            ofd.Filter = "All supported graphics|*.jpg;*.jpeg;*.png;*.bpe|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png|" +
              "Bstu Photo Editor (*.bpe)|*.bpe";

            if (ofd.ShowDialog() == true)
            {
                if (ofd.FileName.EndsWith(".bpe"))
                {
                    LoadBPE(ofd.FileName);
                }
                else
                {
                    double HeightCanvas, WidthCanvas;
                    BitmapFrame bmpFrame = BitmapFrame.Create(new Uri(ofd.FileName));
                    HeightCanvas = bmpFrame.PixelHeight;
                    WidthCanvas = bmpFrame.PixelWidth;

                    NewLayer(HeightCanvas, WidthCanvas);
                    int index = GlobalState.CurrentLayerIndex;
                    var layer = LayersWidgets[index].ThisLayer;

                    layer.LayerBmpFrame = bmpFrame;
                    layer.RefreshBrush();
                }
            }
        }


        // SAVE


        private void SaveCanvas(Canvas canvas, int dpi, string filename)
        {
            var width = ViewCanvas.ActualWidth;
            var height = ViewCanvas.ActualHeight;

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
            switch (format)
            {
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
            if (LayersWidgets.Count > 0)
            {
                int index = GlobalState.CurrentLayerIndex;
                var layer = LayersWidgets[index].ThisLayer;
                layer.Opacity = sliderOpacity.Value / 100;
            }
        }


        // EFFECTS


        private void Negative_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = GlobalState.CurrentLayerIndex;
                var layer = LayersWidgets[index].ThisLayer;
                Effects.Negative(layer);
                layer.RefreshBrush();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        private void Grayscale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = GlobalState.CurrentLayerIndex;
                var layer = LayersWidgets[index].ThisLayer;
                Effects.Grayscale(layer);
                layer.RefreshBrush();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        private void GaussianBlur_Click(object sender, RoutedEventArgs e)
        {
            Turn BoxWindow = new Turn();
            try
            {
                if (BoxWindow.ShowDialog() == true)
                {
                    int x = int.Parse(BoxWindow.Turns);
                    int index = GlobalState.CurrentLayerIndex;
                    var layer = LayersWidgets[index].ThisLayer;
                    Effects.GaussianBlur(layer, x);
                    layer.RefreshBrush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        private void SobelFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = GlobalState.CurrentLayerIndex;
                var layer = LayersWidgets[index].ThisLayer;
                Effects.SobelFilter(layer);
                layer.RefreshBrush();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        private void SobelFilterGrayscale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = GlobalState.CurrentLayerIndex;
                var layer = LayersWidgets[index].ThisLayer;
                Effects.SobelFilter(layer, true);
                layer.RefreshBrush();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        private void Rotate_Click(object sender, RoutedEventArgs e)
        {

            Turn BoxWindow = new Turn();
            try
            {
                if (BoxWindow.ShowDialog() == true)
                {

                    double x = double.Parse(BoxWindow.Turns);
                    int index = GlobalState.CurrentLayerIndex;
                    var layer = LayersWidgets[index].ThisLayer;
                    if (x == 90 || x == 180 || x == 360) { Effects.Rotate(layer, x); } else { Effects.RotateBilinear(layer, x); }

                    layer.RefreshBrush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }


        // LAYER BITMAP FUNCTIONS


        private BitmapFrame BmpFrameErase(Point pos, Layer layer)
        {
            var bmpFrame = layer.LayerBmpFrame;
            double brushSize = VisualHost.BrushSize;
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
                    if (col >= pos.X - brushSize && col <= pos.X + brushSize && row >= pos.Y - brushSize && row <= pos.Y + brushSize)
                        intPixelData[row * width + col] = transparentPixel;
                }
            }

            var bsCheckerboard = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, intPixelData, stride);
            bmpFrame = BitmapFrame.Create(bsCheckerboard);
            layer.RefreshBrush();
            return bmpFrame;
        }

        public void BrushToBitmap()
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
                    bool stretchRight = (newPosition.X > width - 10 && newPosition.X < width && stretchHeight && stretchWidth) ? true : false;
                    bool stretchBottom = (newPosition.Y > height - 10 && newPosition.Y < height && stretchHeight && stretchWidth) ? true : false;

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
                TranslateTransform translatetransform = new TranslateTransform();
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
                translatetransform.X = layer.LayerPosition.X;
                translatetransform.Y = layer.LayerPosition.Y;
                layer.RenderTransform = scaletransform;
                layer.RenderTransform = translatetransform;
            }
        }


        // INSTRUMENTS


        SolidColorBrush passiveColor = new SolidColorBrush(Color.FromArgb(0, (byte)255, (byte)72, (byte)31));
        SolidColorBrush activeColor = new SolidColorBrush(Color.FromArgb(255, (byte)255, (byte)72, (byte)31));

        private void Brush_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = GlobalState.Instruments.Brush;
            Arrow_Overlay.Fill = passiveColor;
            Resize_Overlay.Fill = passiveColor;
            Fill_Overlay.Fill = passiveColor;
            Erase_Overlay.Fill = passiveColor;
            Brush_Overlay.Fill = activeColor;
        }

        private void Resize_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = GlobalState.Instruments.Resize;
            Arrow_Overlay.Fill = passiveColor;
            Resize_Overlay.Fill = activeColor;
            Fill_Overlay.Fill = passiveColor;
            Erase_Overlay.Fill = passiveColor;
            Brush_Overlay.Fill = passiveColor;
        }

        private void Erase_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = GlobalState.Instruments.Eraser;
            Arrow_Overlay.Fill = passiveColor;
            Resize_Overlay.Fill = passiveColor;
            Fill_Overlay.Fill = passiveColor;
            Erase_Overlay.Fill = activeColor;
            Brush_Overlay.Fill = passiveColor;

            // Convert to Bitmap
            var index = GlobalState.CurrentLayerIndex;
            var layer = LayersWidgets[index].ThisLayer;
            if (layer.LayerBmpFrame == null)
            {
                mainCanvas.UpdateLayout();
                BrushToBitmap();
                layer.RefreshBrush();
            }
        }

        private void Arrow_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = GlobalState.Instruments.Arrow;
            Arrow_Overlay.Fill = activeColor;
            Resize_Overlay.Fill = passiveColor;
            Fill_Overlay.Fill = passiveColor;
            Erase_Overlay.Fill = passiveColor;
            Brush_Overlay.Fill = passiveColor;
        }

        private void Fill_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = GlobalState.Instruments.Fill;
            Arrow_Overlay.Fill = passiveColor;
            Resize_Overlay.Fill = passiveColor;
            Fill_Overlay.Fill = activeColor;
            Erase_Overlay.Fill = passiveColor;
            Brush_Overlay.Fill = passiveColor;
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
                MaximizeButton_Black.Visibility = Visibility.Hidden;
                MaximizeButtonOFF.Visibility = Visibility.Visible;
                MainWindowState.IsMaximized = true;
            }
            else
            {
                EditorWindow.WindowState = System.Windows.WindowState.Normal;
                EditorWindow.Left = WindowLeft;
                EditorWindow.Top = WindowTop;
                EditorWindow.Width = WindowWidth;
                EditorWindow.Height = WindowHeight;
                MaximizeButton_Black.Visibility = Visibility.Visible;
                MaximizeButtonOFF.Visibility = Visibility.Hidden;
                MainWindowState.IsMaximized = false;
            }
        }

        private void CloseButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainWindowState.IsOpen = false;
            Close();
        }

        private void DragCanvasButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!MainWindowState.IsMaximized)
            {
                DragMove();
                WindowTop = EditorWindow.Top;
                WindowTop = EditorWindow.Left;
            }
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

        private void HelpButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://google.com");
        }

        private void KeysEvent(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                    Arrow_Selected(sender, e);
                    break;
                case Key.R:
                    Resize_Selected(sender, e);
                    break;
                case Key.E:
                    Erase_Selected(sender, e);
                    break;
                case Key.B:
                    Brush_Selected(sender, e);
                    break;
                case Key.F:
                    Fill_Selected(sender, e);
                    break;
                case Key.OemPlus:
                    NewLayer_Click(sender, e);
                    break;
                case Key.OemMinus:
                    DeleteLayer_Click(sender, e);
                    break;
            }
            if ((e.Key == Key.R) && (Keyboard.IsKeyDown(Key.LeftCtrl)))
                Rotate_Click(sender, e);
        }
        //select color
        public Color? ColorName {
            get {

                return ColorPicker1.SelectedColor;
            }
        }
        private void ClrPcker(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Color Colorid = (Color)ColorName;
            var mySolidColorBrush = new SolidColorBrush(Colorid);

            VisualHost.BrushColor = mySolidColorBrush;
            ColorPreview.Fill = new SolidColorBrush(Colorid);
        }

        private void ColorButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ColorPreview.Fill = new SolidColorBrush(Colors.Red);
        }
        private void SaveBPE()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "BstuPhotoEditor|*.bpe";
            if (sfd.ShowDialog() == true)
            {
                List<byte> toWrite = new List<byte>();
                toWrite.AddRange(BitConverter.GetBytes((Int32)LayersWidgets.Count));
                foreach (var layer in LayersWidgets)
                {
                    toWrite.AddRange(layer.ThisLayer.ToBytes());
                }
                using (FileStream fstream = new FileStream(sfd.FileName, FileMode.OpenOrCreate))
                {
                    fstream.Write(toWrite.ToArray(), 0, toWrite.Count);
                }
            }
        }

        private void LoadBPE(string fileUri)
        {
            byte[] Read;
            using (FileStream fstream = new FileStream(fileUri, FileMode.OpenOrCreate))
            {
                Read = new byte[fstream.Length];
                fstream.Read(Read, 0, (int)fstream.Length);
            }
            Queue<byte> q = new Queue<byte>(Read);
            LayersWidgets.Clear();
            mainCanvas.Children.Clear();
            Int32 layersCount = Utils.FromBytesInt32(q);
            for (uint i = 0; i < layersCount; i++)
            {
                Layer loadedLayer = Layer.FromBytes(q);
                LayersWidgets.Add(loadedLayer.Widget);
                mainCanvas.Children.Add(loadedLayer);
            }
            GlobalState.CurrentLayerIndex = 0;
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                keyCtrlDown++;
            }
            else if (keyCtrlDown > 0)
            {
                switch (e.Key)
                {
                    case Key.S:
                        SaveBPE();
                        break;
                    case Key.O:
                        OpenPhoto();
                        break;
                }
            }
        }

        private void OnKeyUpHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                keyCtrlDown--;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveBPE();
        }
    }
}