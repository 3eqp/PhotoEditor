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
using System.Windows.Shapes;

namespace PhotoEditor
{
    /// <summary>
    /// Логика взаимодействия для Turn.xaml
    /// </summary>
    public partial class Turn : Window
    {
        public Turn()
        {
            InitializeComponent();
            Confirm.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Accept_Click), true);
            Cancel.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Cancel_Click), true);
        }

       
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

        }
        public string Turns
        {
            get { return Box.Text; }
        }

        private void Box_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1:
                    e.Handled = false;
                    break;
                case Key.D2:
                    e.Handled = false;
                    break;
                case Key.D3:
                    e.Handled = false;
                    break;
                case Key.D4:
                    e.Handled = false;
                    break;
                case Key.D5:
                    e.Handled = false;
                    break;
                case Key.D6:
                    e.Handled = false;
                    break;
                case Key.D7:
                    e.Handled = false;
                    break;
                case Key.D8:
                    e.Handled = false;
                    break;
                case Key.D9:
                    e.Handled = false;
                    break;
                case Key.D0:
                    e.Handled = false;
                    break;
                case Key.OemMinus:
                    e.Handled = false; 
                    break;

                default:
                    e.Handled = true;
                    break;
            }
        }

        
    }
}
