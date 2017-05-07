using System;
using System.Collections.Generic;
using System.Linq;
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
        protected bool isDragging;
        private Point clickPosition;

        public string LayerName { get; set; }
        
        public SolidColorBrush layerColorBrush { get; set; }
        public ImageBrush layerImageBrush { get; set; }
        public BitmapFrame layerBmpFrame { get; set; }
        public LayerWidget Widget { get; set; }

        public Layer(string name, double width, double height, double opacity, int col, int colspan, int row)
        {
            this.MouseLeftButtonDown += new MouseButtonEventHandler(Control_MouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(Control_MouseLeftButtonUp);
            this.MouseMove += new MouseEventHandler(Control_MouseMove);

            LayerName = Name = name;
            Height = height;
            Width = width;
            Opacity = opacity;
            if (col != 0) Grid.SetColumn(this, col);
            if (row != 0) Grid.SetRow(this, row);
            if (colspan != 0) Grid.SetColumnSpan(this, colspan);
            SetZIndex(this, GlobalState.LayersCount++);

            Widget = new LayerWidget(this, name);
        }

        public void refreshBrush()
        {
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = layerBmpFrame;
            layerImageBrush = brush;

            Widget.refreshPreviewCanvas();
            Background = brush;
        }

        
        // DRAGGING LAYER


        private void Control_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GlobalState.CurrentTool == GlobalState.Instruments.Arrow)
            {
                isDragging = true;
                var draggableControl = sender as Canvas;
                clickPosition = e.GetPosition(this);
                draggableControl.CaptureMouse();
            }
        }

        private void Control_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GlobalState.CurrentTool == GlobalState.Instruments.Arrow)
            {
                isDragging = false;
                var draggable = sender as Canvas;
                draggable.ReleaseMouseCapture();
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            var draggableControl = sender as Canvas;

            if (isDragging && draggableControl != null)
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
            }
        }
    }
}