using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApplication1
{
    class Layer : Canvas
    {
        private string _layerName;
        public string LayerName
        {
            get
            {
                return _layerName;
            }
            set
            {
                _layerName = value;
            }
        }

        public int LayerIndex { get; set; }

        public SolidColorBrush layerColorBrush { get; set; }
        public ImageBrush layerImageBrush { get; set; }

        public Layer(string name, int index)
        {
            LayerName = name;
            LayerIndex = index;
        }
    }
}