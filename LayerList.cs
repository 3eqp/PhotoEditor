using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PhotoEditor
{
    class LayerList
    {
        static public List<Canvas> layersList = new List<Canvas>();
        public static int currentLayerIndex { get; set; }
        public static int LayersIndexes { get; set; }
    }
}