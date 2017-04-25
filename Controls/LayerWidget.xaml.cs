using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PhotoEditor.Controls
{
    public partial class LayerWidget : UserControl
    {
        public Layer ThisLayer;
        public int widgetIndex { get; set; }

        public LayerWidget(Layer layer, string name)
        {
            Height = 50;
            ThisLayer = layer;
            DataContext = ThisLayer;
            widgetIndex = MainWindow.LayersWidgets.IndexOf(this);

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

        public void UserContol_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GlobalState.currentLayerIndex = widgetIndex;

            MainWindow.RefreshLayersWidgets();
            MainWindow.Text_2(ThisLayer);
        }

        public void refreshPreviewCanvas()
        {
            previewCanvas.Background = ThisLayer.layerImageBrush;
        }
    }
}
