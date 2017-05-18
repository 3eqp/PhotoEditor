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
    /// Логика взаимодействия для Start.xaml
    /// </summary>
    public partial class Start : Window
    {
        public Start()
        {
            InitializeComponent();
        }

        private void NewFile(object sender, RoutedEventArgs e)
        {
           
          
           
            SizeCanvas BoxWindow = new  SizeCanvas();
            if (BoxWindow.ShowDialog() == true)
            {
                MainWindow.indexi = 1; 
              double Width = double.Parse(BoxWindow.SizeWs);
               double Height = double.Parse(BoxWindow.SizeHs);
               MainWindow.WidthCanvas = (int)Width;
               MainWindow.HeightCanvas = (int)Height;
             
          
            }
            this.Close(); 
        }

      
        private void btnOpenFile(object sender, RoutedEventArgs e)
        {
            //MainWindow ClassOpenFile = new MainWindow();
            //ClassOpenFile.functionflag(true);
            MainWindow.indexi = 2;
            this.Close(); 
        }

        private void OpenEx(object sender, RoutedEventArgs e)
        {
            MainWindow.indexi = 3;
        }
    }
    
}
