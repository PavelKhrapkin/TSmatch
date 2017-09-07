/*--------------------------------------------------------------------------------------
 * SplashScreen() -- Display TSmatch splash screen and status messages on the data load
 * used idea from https://www.codeproject.com/Articles/116875/WPF-Loading-Splash-Screen
 * 
 * 7.07.2017 Pavel Khrapkin
 * 
 * Note: Hide of the messaes not in use
 */
using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Threading;

using Boot = TSmatch.Bootstrap.Bootstrap;
using Msg = TSmatch.Message.Message;
using Mod = TSmatch.Model.Model;

namespace TSmatch
{
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
            Dispatcher.Invoke(showDelegate, "Loading Bootstrap - TSmatch.xlsx");
            MainWindow.boot = new Boot();

            //load data
            //7/9            Dispatcher.Invoke(hideDelegate);
            Dispatcher.Invoke(showDelegate, Msg.S("Loading", "TSmatchINFO.xlsx","Raw.xml"));
            MainWindow.model = new Mod();
            MainWindow.model = MainWindow.model.sr.SetModel(MainWindow.boot);

            //close the window
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate () { Close(); });
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