using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoEditor
{
    class GlobalState
    {
        internal enum Instruments
        {
            Arrow, Brush, Resize, Eraser
        }
        public static Instruments CurrentTool;

        public static bool MousePressed { get; set; }
        public static bool isResizing { get; set; }
        public static bool isErasing { get; set; }

        public static int currentLayerIndex { get; set; }
        public static int LayersCount { get; set; }
    }
}