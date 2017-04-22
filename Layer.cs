using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoEditor
{
    public class Layer : Canvas
    {
        public int LayerIndex { get; set; }
        public bool isCurrentLayer { get; set; }


        public SolidColorBrush layerColorBrush { get; set; }
        public ImageBrush layerImageBrush { get; set; }
        public BitmapFrame bmpFrame {get; set;}
        public LayerWidget widget;

        public Layer(string name, int index, double width, double height, double opacity, int col, int colspan, int row, StackPanel layerCanvas)
        {
            Name = name;
            Height = height;
            Width = width;
            Opacity = opacity;
            if (col != 0) Grid.SetColumn(this, col);
            if (row != 0) Grid.SetRow(this, row);
            if (colspan != 0) Grid.SetColumnSpan(this, colspan);

            LayerIndex = index;
            widget = new LayerWidget(this, name, index);
            layerCanvas.Children.Add(widget);
        }
    }
}