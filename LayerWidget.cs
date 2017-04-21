using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PhotoEditor
{
    public class LayerWidget : Canvas
    {
        public Layer ThisLayer;
        public LayerWidget(Layer layer, string name, int index)
        {
            Name = name;
            Height = 50;
            Background = new SolidColorBrush(Colors.Red);

            ThisLayer = layer;
        }
    }
}