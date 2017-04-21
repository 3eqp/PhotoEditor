using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    class GlobalState
    {
        public static double layerWidth { get; set; }
        public static double layerHeight { get; set; }
        public static int LayersCount { get; private set; }
        public static int currentLayerIndex { get; set; }
        public static int LayersIndexes { get; set; }

        public static void refreshGlobal()
        {
            LayersCount = LayerList.layersList.Count;
        }

        public static void setLayerSize(double width, double height)
        {
            layerWidth = width;
            layerHeight = height;
        }
    }
}