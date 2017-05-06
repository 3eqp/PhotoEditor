using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace PhotoEditor
{
    /// <summary>
    /// Класс, отвечающий за рисование
    /// </summary>
    class VisualHost : FrameworkElement
    {
        public Brush FillBrush { get; set; }
        public static Brush Color = Brushes.Black;
        public static Size BrushSize { get; set; }


    }
}
