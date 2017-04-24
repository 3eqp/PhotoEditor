using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PhotoEditor.Controls
{
    public partial class LayerWidget : UserControl
    {
        public Layer ThisLayer;
        public LayerWidget(Layer layer, string name, int index)
        {
            Name = name;
            Height = 50;
            //Background = new SolidColorBrush(Colors.Gray);
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
            }
        }
    }
}
