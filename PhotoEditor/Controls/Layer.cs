using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoEditor.Controls
{
    public class Layer : Canvas
    {
        protected bool IsDragging { get; set; }
        public bool IsLayerVisible { get; set; }
        public Point clickPosition;

        public string LayerName { get; set; }
        public Point LayerPosition;

        public SolidColorBrush LayerColorBrush { get; set; }
        public ImageBrush LayerImageBrush { get; set; }
        public BitmapFrame LayerBmpFrame { get; set; }
        public double LayerScale { get; set; }
        public LayerWidget Widget { get; set; }

        public Layer(string name, double width, double height, double opacity, double scale, int col, int colspan, int row)
        {
            this.MouseLeftButtonDown += new MouseButtonEventHandler(Control_MouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(Control_MouseLeftButtonUp);
            this.MouseMove += new MouseEventHandler(Control_MouseMove);

            LayerName = Name = name;
            Height = height;
            Width = width;
            MinHeight = 100;
            MinWidth = 100;
            Opacity = opacity;
            LayerScale = scale;
            IsLayerVisible = true;
            Visibility = Visibility.Visible;

            if (col != 0) Grid.SetColumn(this, col);
            if (row != 0) Grid.SetRow(this, row);
            if (colspan != 0) Grid.SetColumnSpan(this, colspan);
            SetZIndex(this, GlobalState.LayersCount++);

            Widget = new LayerWidget(this, name);
        }

        public void RefreshBrush()
        {
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = LayerBmpFrame;
            LayerImageBrush = brush;

            Widget.RefreshPreviewCanvas();
            Background = brush;
        }

        public void VisibleChange()
        {
            if (IsLayerVisible)
            {
                Visibility = Visibility.Hidden;
                Widget.VisibilityON.Visibility = Visibility.Hidden;
                Widget.VisibilityOFF.Visibility = Visibility.Visible;
                IsLayerVisible = false;
            }
            else if (!IsLayerVisible)
            {
                Visibility = Visibility.Visible;
                Widget.VisibilityON.Visibility = Visibility.Visible;
                Widget.VisibilityOFF.Visibility = Visibility.Hidden;
                IsLayerVisible = true;
            }
        }


        // DRAGGING LAYER


        private void Control_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Arrow
            if (GlobalState.CurrentTool == GlobalState.Instruments.Arrow)
            {
                IsDragging = true;
                var draggableControl = sender as Canvas;
                clickPosition = e.GetPosition(this);
                draggableControl.CaptureMouse();
                var widgetIndex = MainWindow.LayersWidgets.IndexOf(this.Widget);
                GlobalState.CurrentLayerIndex = widgetIndex;
                ((MainWindow)Application.Current.MainWindow).widgetsCanvas.SelectedIndex = widgetIndex;
            }

            // Fill
            if (GlobalState.CurrentTool == GlobalState.Instruments.Fill)
            {
                Background = new SolidColorBrush(VisualHost.BrushColor.Color);
                Widget.previewCanvas.Fill = new SolidColorBrush(VisualHost.BrushColor.Color);
            }
        }

        private void Control_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GlobalState.CurrentTool == GlobalState.Instruments.Arrow)
            {
                IsDragging = false;
                var draggable = sender as Canvas;
                draggable.ReleaseMouseCapture();
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            var draggableControl = sender as Canvas;

            if (IsDragging && draggableControl != null)
            {
                Point currentPosition = e.GetPosition(this.Parent as UIElement);

                var transform = draggableControl.RenderTransform as TranslateTransform;
                if (transform == null)
                {
                    transform = new TranslateTransform();
                    draggableControl.RenderTransform = transform;
                }

                transform.X = currentPosition.X - clickPosition.X;
                transform.Y = currentPosition.Y - clickPosition.Y;
                LayerPosition.X = transform.X;
                LayerPosition.Y = transform.Y;
                Console.WriteLine("layer " + LayerPosition.X + " " + LayerPosition.Y);
            }
        }

        public List<byte> ToBytes()
        {
            List<byte> result = new List<byte>();
            result.AddRange(LayerName.ToBytes());
            result.AddRange(LayerPosition.ToBytes());
            if (LayerBmpFrame == null)
            {
                result.AddRange(BitConverter.GetBytes((Int32)0));
            }
            else
            {
                result.AddRange(BitConverter.GetBytes((Int32)LayerBmpFrame.PixelWidth));
                result.AddRange(BitConverter.GetBytes((Int32)LayerBmpFrame.PixelHeight));

                int stride = LayerBmpFrame.PixelWidth * (LayerBmpFrame.Format.BitsPerPixel / 8);
                byte[] data = new byte[LayerBmpFrame.PixelHeight * stride];

                result.AddRange(BitConverter.GetBytes((Int32)data.Length));

                LayerBmpFrame.CopyPixels(data, stride, 0);
                result.AddRange(data);
            }
            return result;
        }

        public static Layer FromBytes(Queue<byte> q)
        {
            string layerName = Utils.FromBytesString(q);
            Point layerPosition = Utils.FromBytesPoint(q);
            int width = Utils.FromBytesInt32(q);
            if (width != 0)
            {
                int height = Utils.FromBytesInt32(q);
                //Layer result = new Layer(layerName, width, height, 1, 1, 2, 1);
                Layer result = new Layer(layerName, 350, 350, 1, 1, 1, 2, 1);
                int bytesPerImage = Utils.FromBytesInt32(q);
                byte[] imageData = new byte[bytesPerImage];
                for (uint i = 0; i < bytesPerImage; i++)
                {
                    imageData[i] = q.Dequeue();
                }
                WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                bitmap.WritePixels(new Int32Rect(0, 0, width, height), imageData, width * (bitmap.Format.BitsPerPixel / 8), 0);
                result.LayerBmpFrame = BitmapFrame.Create(bitmap);
                // TODO: позиция записывается верная, но рендерится картинка всё-равно в начале координат
                result.LayerPosition = layerPosition;
                result.RefreshBrush();
                return result;
            }
            return null;
        }

    }
}