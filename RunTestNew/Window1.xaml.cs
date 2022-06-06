using LottieSharp.WpfSurface;

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

namespace RunTestNew
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

        }

        [DllImport("user32.dll", SetLastError =true)]
        internal extern static bool PostMessage(IntPtr hWnd, uint Msg, IntPtr WParam, IntPtr LParam);
        const uint WM_SYSCOMMAND = 0x0112;
        const uint DOMOVE = 0xF012;
        const uint DOSIZE = 0xF008;
        [DllImport("user32", CharSet = CharSet.Auto)]
        internal extern static bool ReleaseCapture();
        public new void DragMove()
        {

            var r2 = new WindowInteropHelper(this);
            
           // ReleaseCapture();
           bool r =  PostMessage(r2.Handle, WM_SYSCOMMAND, (IntPtr)0xF008, (IntPtr)0);

            // PostMessage(r2.Handle, WM_SYSCOMMAND, (IntPtr)0xf012, IntPtr.Zero);
            int error = Marshal.GetLastPInvokeError();
          //  PostMessage(r2.Handle, 0x0202, IntPtr.Zero, IntPtr.Zero);
          //  int error2 = Marshal.GetLastPInvokeError();

        }
        //private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        //{
        //    if (e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        this.DragMove();
        //    }
        //}

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                var w = this;
                this.LottieAnimationView.Scale += 0.105;

            }
            else
            {

                this.LottieAnimationView.Scale -= 0.105;
            }
        }

        private void Window_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
