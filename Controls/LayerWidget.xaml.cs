using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PhotoEditor.Controls
{
    public partial class LayerWidget : UserControl
    {
        public Layer ThisLayer;

        public LayerWidget(Layer layer, string name)
        {
            Height = 50;
            ThisLayer = layer;
            DataContext = ThisLayer;

            InitializeComponent();
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditBox.Text = WidgetText.Text;
            WidgetText.Visibility = Visibility.Hidden;
            EditBox.Visibility = Visibility.Visible;
            EditBox.Focus();
            EditBox.SelectAll();
        }

        private void editBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                WidgetText.Text = EditBox.Text;
                EditBox.Visibility = Visibility.Hidden;
                WidgetText.Visibility = Visibility.Visible;
                ThisLayer.LayerName = WidgetText.Text;
            }
        }

        public void refreshPreviewCanvas()
        {
            previewCanvas.Background = ThisLayer.layerImageBrush;
        }

        public void UserContol_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var widgetIndex = MainWindow.LayersWidgets.IndexOf(this);
            GlobalState.currentLayerIndex = widgetIndex;
            ((MainWindow)Application.Current.MainWindow).sliderOpacity.Value = ThisLayer.Opacity * 100;


            MainWindow.Text_2(ThisLayer);
        }
    }
}
