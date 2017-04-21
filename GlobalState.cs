using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor
{
    class GlobalState
    {
        public static double layerWidth { get; set; }
        public static double layerHeight { get; set; }

        public static void setLayerSize(double width, double height)
        {
            layerWidth = width;
            layerHeight = height;
        }
    }
}