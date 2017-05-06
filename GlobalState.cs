using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor
{
    class GlobalState
    {
        internal enum Instruments
        {
            Arrow, Brush
        }
        public static Instruments CurrentTool;

        public static bool MousePressed { get; set; }

        public static double layerWidth { get; set; }
        public static double layerHeight { get; set; }

        public static int currentLayerIndex { get; set; }
        
        public static int LayersCount { get; set; }

        public static void setLayerSize(double width, double height)
        {
            layerWidth = width;
            layerHeight = height;
        }
    }
}