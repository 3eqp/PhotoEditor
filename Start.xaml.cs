using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PhotoEditor
{
    public partial class Start : Window
    {
        // For Window blur
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        // Window Global Parameters
        public static double WindowTop { get; set; }
        public static double WindowLeft { get; set; }
        public static double WindowWidth { get; set; }
        public static double WindowHeight { get; set; }

        public Start()
        {
            InitializeComponent();

            SizeToContent = SizeToContent.Manual;
            MaxHeight = SystemParameters.WorkArea.Height;

            // Button Events
            MinimizeButton.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(MinimizeButtonUp), true);
            CloseButton.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(CloseButtonUp), true);
            CreateNewFile.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(CreateNewFileButtonUp), true);
            OpenPhoto.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OpenPhotoButtonUp), true);
            OpenFile.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OpenFileButtonUp), true);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.EnableBlur(this);

            // Set Window Global Paremeters
            WindowTop = myWindow.Top;
            WindowLeft = myWindow.Left;
            WindowHeight = myWindow.Height;
            WindowWidth = myWindow.Width;
        }

        private void CreateNewFileButtonUp(object sender, RoutedEventArgs e)
        {
            SizeCanvas CanvasSizeWindow = new  SizeCanvas();

            if (CanvasSizeWindow.ShowDialog() == true)
            {
                double LayerWidth, LayerHeight;
                if (CanvasSizeWindow.SizeWs != "" && CanvasSizeWindow.SizeHs != "")
                {
                    LayerWidth = double.Parse(CanvasSizeWindow.SizeWs);
                    LayerHeight = double.Parse(CanvasSizeWindow.SizeHs);
                }
                else
                {
                    LayerWidth = 0;
                    LayerHeight = 0;
                }
                GlobalState.NewLayerHeight = LayerHeight;
                GlobalState.NewLayerWidth = LayerWidth;

                MainWindow.WindowTrigger = 1;
                Close();
                MainWindow.ShowMainWindow();
            }
        }

        private void OpenPhotoButtonUp(object sender, RoutedEventArgs e)
        {
            MainWindow.WindowTrigger = 3;
            Close();
            MainWindow.ShowMainWindow();
        }

        private void OpenFileButtonUp(object sender, RoutedEventArgs e)
        {
            MainWindow.WindowTrigger = 2;
            Close();
            MainWindow.ShowMainWindow();
        }


        // SYSTEM BUTTONS


        private void MinimizeButtonUp(object sender, EventArgs e)
        {
            myWindow.WindowState = WindowState.Minimized;
        }

        private void MaximizeButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (myWindow.Top != 0 && myWindow.Left != 0)
            {
                myWindow.WindowState = WindowState.Normal;
                myWindow.Left = 0;
                myWindow.Top = 0;
                myWindow.Width = SystemParameters.PrimaryScreenWidth;
                myWindow.Height = SystemParameters.PrimaryScreenHeight;
            }
            else
            {
                myWindow.WindowState = WindowState.Normal;
                myWindow.Left = WindowLeft;
                myWindow.Top = WindowTop;
                myWindow.Width = WindowWidth;
                myWindow.Height = WindowHeight;
            }
        }

        private void CloseButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.CloseMainWindow();
            Close();
        }


        // WINDOW MOVE


        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            WindowTop = myWindow.Top;
            WindowLeft = myWindow.Left;
        }
    }
}
