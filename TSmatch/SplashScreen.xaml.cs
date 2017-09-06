/*------------------------------------------------------------
 * SplashWindowsDemo -- https://www.codeproject.com/Articles/116875/WPF-Loading-Splash-Screen
 */
using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Threading;

using Boot = TSmatch.Bootstrap.Bootstrap;

namespace TSmatch
{
    /// <summary>
    /// Interaction logic for splash.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        Thread loadingThread;
        Storyboard Showboard;
        Storyboard Hideboard;
        private delegate void ShowDelegate(string txt);
        private delegate void HideDelegate();
        ShowDelegate showDelegate;
        HideDelegate hideDelegate;

        public SplashScreen()
        {
            InitializeComponent();
            showDelegate = new ShowDelegate(this.showText);
            hideDelegate = new HideDelegate(this.hideText);
            Showboard = this.Resources["showStoryBoard"] as Storyboard;
            Hideboard = this.Resources["HideStoryBoard"] as Storyboard;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadingThread = new Thread(load);
            loadingThread.Start();
        }
        private void load()
        {
            Dispatcher.Invoke(showDelegate, "Loading TSmatch.xlsx");
            MainWindow.boot = new Boot();
 
            //load data 
            this.Dispatcher.Invoke(hideDelegate);

            Thread.Sleep(1000);
            this.Dispatcher.Invoke(showDelegate, "second data loading");
            Thread.Sleep(1000);
            //load data
            this.Dispatcher.Invoke(hideDelegate);

            Thread.Sleep(1000);
            this.Dispatcher.Invoke(showDelegate, "last data loading");
            Thread.Sleep(1000);
            //load data 
            this.Dispatcher.Invoke(hideDelegate);

            //close the window
            Thread.Sleep(1000);
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate () { Close(); });
        }
        private void showText(string txt)
        {
            txtLoading.Text = txt;
            BeginStoryboard(Showboard);
        }
        private void hideText()
        {
            BeginStoryboard(Hideboard);
        }

    }
}