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
            Arrow, Brush, Resize, Eraser, Fill
        }
        public static Instruments CurrentTool;

        public static bool MousePressed { get; set; }
        public static bool IsResizing { get; set; }
        public static bool IsErasing { get; set; }

        public static int CurrentLayerIndex { get; set; }
        public static int LayersCount { get; set; }

        public static double DefaultLayerWidth = 500;
        public static double DefaultLayerHeight = 500;
        public static double NewLayerWidth { get; set; }
        public static double NewLayerHeight { get; set; }

    }

    class MainWindowState
    {
        public static bool IsOpen { get; set; }
        public static bool IsMaximized { get; set; }
    }
}