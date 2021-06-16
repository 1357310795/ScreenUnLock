using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random rd = new Random();
        int[] a = new int[5];
        int n,err;
        public MainWindow()
        {
           
        }

        private void ScreenUnlock_AfterDraw(object sender, RoutedEventArgs e)
        {
            if (check()) ok(); else { err++; ScreenUnlock1.SelectedColor =new SolidColorBrush( Colors.Red); }
            if (err == 3) Init();
        }

        private void ok()
        {
            ScreenUnlock1.SelectedColor = new SolidColorBrush(Colors.Green);
            System.Diagnostics.Process.Start(@"C:\Program Files (x86)\datronicsoft\spacedeskWindowsVIEWER\spacedeskWindowsVIEWER .exe");
            Application.Current.Shutdown();
        }

        private bool check()
        {
            string s1 = "", s = "";
            for (int i = 1; i <= n; i++)
                s1 = s1 + (a[i] % 3).ToString();
            Console.WriteLine(s1);
            foreach (int i in ScreenUnlock1.Points)
                s += ((i - 1) % 3).ToString();
            Console.WriteLine(s);
            return s == s1;
        }

        private int Ran(int l, int r)
        {
            return rd.Next(l, r + 1);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Init()
        {
            err = 0;
            string s = "";
            n = Ran(2, 4);

            for (int i = 1; i <= n; i++)
            {
                a[i] = Ran(0, 9);
                if ((i == 4) && (a[4] % 3 == a[2] % 3))
                    i--;
            }
            int[] b = new int[15];

            for (int i = 1; i <= 10; i++)
            {
                if (i % 2 == 1 && (i + 1) / 2 <= n)
                    b[i] = a[(i + 1) / 2];
                else
                    if (i == 10)
                    b[i] = n;
                else
                    b[i] = Ran(0, 9);
                s = s + b[i].ToString();
            }
            text1.Text = s;
        }

    }
}
