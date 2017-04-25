﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PhotoEditor.Controls;

namespace PhotoEditor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LayersWidgets = new List<LayerWidget>();
        }

        // Layer -> Widget
        public static List<LayerWidget> LayersWidgets { get; set; }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalState.setLayerSize(mainCanvas.ActualWidth, mainCanvas.ActualHeight);
            text_2.Text = "" + mainCanvas.ActualHeight + " " + mainCanvas.ActualWidth;

            newLayer(1);

            text.Text = "" + mainCanvas.Children.Count + layerCanvas.Children.Count + GlobalState.currentLayerIndex;
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
                newLayer(1);

                int index = GlobalState.currentLayerIndex;
                var layer = (Layer)mainCanvas.Children[index];

                BitmapFrame bmpFrame = BitmapFrame.Create(new Uri(op.FileName));
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = bmpFrame;
                layer.layerImageBrush = brush;
                layer.bmpFrame = bmpFrame;

                layer.Background = brush;
            }
        }

        private void SaveCanvas(Canvas canvas, int dpi, string filename)
        {
            var width = canvas.ActualWidth;
            var height = canvas.ActualHeight;

            var size = new Size(width, height);
            canvas.Measure(size);

            var rtb = new RenderTargetBitmap(
                (int)width,
                (int)height,
                dpi, //dpi x 
                dpi, //dpi y 
                PixelFormats.Pbgra32 // pixelformat 
                );
            rtb.Render(canvas);

            SaveAsPng(rtb, filename);
        }

        private static void SaveAsPng(RenderTargetBitmap bmp, string filename)
        {
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmp));

            using (FileStream stm = File.Create(filename))
            {
                enc.Save(stm);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var saveDlg = new SaveFileDialog
            {
                FileName = "Masterpiece",
                DefaultExt = ".png",
                Filter = "PNG (.png)|*.png"
            };

            if (saveDlg.ShowDialog() == true)
            {
                SaveCanvas(mainCanvas, 96, saveDlg.FileName);
            }
        }

        public static void RefreshLayersWidgets()
        {
            int count = 0;
            foreach (LayerWidget widget in LayersWidgets)
            {
                if (GlobalState.currentLayerIndex != count)
                    widget.Background = new SolidColorBrush(Colors.Transparent);
                else widget.Background = new SolidColorBrush(Colors.Red);
                count += 1;
            }
        }

        public void newLayer(double opacity)
        {
            double Width = GlobalState.layerWidth;
            double Height = GlobalState.layerHeight;
            string layerName = "NewLayer" + LayersWidgets.Count;
            var layer = new Layer(layerName, LayersWidgets.Count, Width, Height, opacity, 1, 2, 1, layerCanvas);
            mainCanvas.Children.Add(layer);
            LayersWidgets.Add(layer.Widget);
            GlobalState.currentLayerIndex = LayersWidgets.Count - 1;
            layer.Background = new SolidColorBrush(Colors.White);


            RefreshLayersWidgets();
            text.Text = "" + mainCanvas.Children.Count + layerCanvas.Children.Count + GlobalState.currentLayerIndex;
        }

        private void btnNewLayer_Click(object sender, RoutedEventArgs e)
        {
            newLayer(1);
        }

        private void btnDeleteLayer_Click(object sender, RoutedEventArgs e)
        {
            int index = GlobalState.currentLayerIndex;
            if (LayersWidgets.Count > 0)
            {
                var layer = (Layer)mainCanvas.Children[index];
                LayerWidget widget = layer.Widget;
                mainCanvas.Children.Remove(layer);
                LayersWidgets.Remove(widget);
                GlobalState.currentLayerIndex = LayersWidgets.Count - 1;
                layerCanvas.Children.Remove(widget);
            }


            RefreshLayersWidgets();
            text.Text = "" + mainCanvas.Children.Count + layerCanvas.Children.Count + GlobalState.currentLayerIndex;
        }

        private void btnEffect_Click(object sender, RoutedEventArgs e)
        {
            var layer = (Layer)mainCanvas.Children[GlobalState.currentLayerIndex];
            Effects.Negative(layer);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = layer.bmpFrame;
            layer.Background = brush;
        }
        // TEST OUTPUT
        static public void Text_2(Layer layer)
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).text_2.Text = "" + layer.LayerName + " " + GlobalState.currentLayerIndex;

        }
        private void Grayscale(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)LayerList.layersList[LayerList.currentLayerIndex];
            Effects.Grayscale(layer);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = layer.bmpFrame;
            layer.Background = brush;
        }

        private void GaussianBlur(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)LayerList.layersList[LayerList.currentLayerIndex];
            Effects.GaussianBlur(layer, 4);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = layer.bmpFrame;
            layer.Background = brush;
        }

        private void SobelFilter(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)LayerList.layersList[LayerList.currentLayerIndex];
            Effects.SobelFilter(layer);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = layer.bmpFrame;
            layer.Background = brush;
        }

        private void SobelFilterGrayscale(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)LayerList.layersList[LayerList.currentLayerIndex];
            Effects.SobelFilter(layer, true);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = layer.bmpFrame;
            layer.Background = brush;
        }

        private void Rotate90(object sender, RoutedEventArgs e)
        {
            Layer layer = (Layer)LayerList.layersList[LayerList.currentLayerIndex];
            Effects.Rotate(layer, 90);
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = layer.bmpFrame;
            layer.Background = brush;
        }
    }
}