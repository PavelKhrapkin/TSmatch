/*--------------------------------------------------------------------------------------
 * SplashScreen() -- Display TSmatch splash screen and status messages on the data load
 * used idea from https://www.codeproject.com/Articles/116875/WPF-Loading-Splash-Screen
 * 
 * 1.10.2017 Pavel Khrapkin
 * 
 *--- History ---
 *  7.09.2017 - created
 * 14.09.2017 - Multilanguage support from Message module
 *  1.10.2017 - Version TextBlock
 * 
 * Note: Hide of the messages not in use
 */
using System;
using System.Windows;
using System.Resources;
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

        public SplashScreen(string version)
        {
            InitializeComponent();
            AboutStr.Text = version;
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
            Dispatcher.Invoke(showDelegate, Msg.S("Splash__Loading_Bootstrap"));
            MainWindow.boot = new Boot();

            //load data
//7/9            Dispatcher.Invoke(hideDelegate);
            string msg = Msg.S("Splash__Loading_TSmatchINFO", "TSmatchINFO.xlsx", "Raw.xml");
            Dispatcher.Invoke(showDelegate, msg);
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