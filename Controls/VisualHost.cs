using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace PhotoEditor
{
    /// <summary>
    /// Класс, отвечающий за рисование
    /// </summary>
    class VisualHost : FrameworkElement
    {
        public static SolidColorBrush BrushColor = Brushes.Black;
        public static double BrushSize { get; set; }
    }
}
