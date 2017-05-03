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
    /// Логика взаимодействия для sizeimage.xaml
    /// </summary>
    public partial class sizeimage : Window
    {
        public sizeimage()
        {
            InitializeComponent();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

        }
        public string SizeWs
        {
            get { return SizeW.Text; }
        }
        public string SizeHs
        {
            get { return SizeH.Text; }
        }
    }
}
