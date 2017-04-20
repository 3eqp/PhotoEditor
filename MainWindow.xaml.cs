using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            text.Text = "" + mainCanvas.Children.Count;
            GlobalState.LayersCount = 0;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";

            if (op.ShowDialog() == true)
            {
                newLayer();

                BitmapFrame bmpFrame = BitmapFrame.Create(new Uri(op.FileName));
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = bmpFrame;
                int index = GlobalState.currentLayerIndex;
                LayerList.layersList[0].Background = brush;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        public void newLayer()
        {
            string layerName = "NewLayer" + GlobalState.LayersCount;
            GlobalState.LayersCount++;
            GlobalState.currentLayerIndex = GlobalState.LayersCount++;
            Layer layer = new Layer(layerName, GlobalState.LayersCount - 1);
            LayerList.layersList.Add(layer);
            mainCanvas.Children.Add(layer);

            layer.Name = layerName;
            layer.Width = 500;
            layer.Height = 264;
            Grid.SetColumn(layer, 1);
            Grid.SetRow(layer, 1);
            layer.Opacity = 0.5;

            text.Text = "" + mainCanvas.Children.Count;
        }

        private void btnNewLayer_Click(object sender, RoutedEventArgs e)
        {
            newLayer();
        }

        private void btnDeleteLayer_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}