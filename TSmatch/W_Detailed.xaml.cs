/*-------------------------------------------
 * W_Detailed 25.6.2017 Pavel.Khrapkin
 * --- History ---
 * 2017.07.25 - W_etailed maid from MainWindow module
 * --- Known Issue & ToDos ---
 * - It is good re-design XAML idea to have two column on MainWindow with the Width = "*".
 * Than with Window size changed, Group<Mat,Prf,Price> part would become wider.
 * - ToDo some kind of progress bar moving on the MainWindow, when Tekla re-draw HighLight.
 * - Implement [RePricing] button
 */
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Msg = TSmatch.Message.Message;
using Mod = TSmatch.Model.Model;

namespace TSmatch
{
    /// <summary>
    /// Interaction logic for W_Detailed.xaml
    /// </summary>
    public partial class W_Detailed : Page
    {
        Mod mod;

        public W_Detailed()
        {
            InitializeComponent();
        }


        #region --- Menu Items ---
        private void OnSaveAs(object sender, RoutedEventArgs e)
        {
            Msg.AskFOK("Not ready yet");
        }

        private void OnFontSize(object sender, RoutedEventArgs e)
        {
            Msg.AskFOK("Not ready yet");
        }

        private void OnPriceCheck(object sender, RoutedEventArgs e)
        {
            var p = new PriceList.PriceList();
            p.CheckAll();
        }

        private void OnSupllier(object sender, RoutedEventArgs e)
        {
            ////////if (string.IsNullOrEmpty(SuplName)) return;
            var wSupplier = new W_Supplier();
            wSupplier.Show();
            // AskYN() если изменился - новый выбор Supplier, else return
            // RePricing();
        }

        private void OnCompSet(object sender, RoutedEventArgs e)
        {
            //22/5            var wChoice = new WindowSupplierChain();
            //22/5            wChoice.Show();
        }

        private void OnMaterial(object sender, RoutedEventArgs e)
        {
            Msg.AskFOK("Not ready yeat");
            //22/5            var wChoice = new WindowSupplierChain();
            //22/5            wChoice.Show();
        }

        private void OnProfile(object sender, RoutedEventArgs e)
        {
            Msg.AskFOK("Not ready yeat");
            //22/5            var wChoice = new WindowSupplierChain();
            //22/5            wChoice.Show();
        }

        private void OnRules(object sender, RoutedEventArgs e)
        {
            var wRules = new W_Rules();
            wRules.Show();
        }
        #endregion --- [Read], [RePrice], and [OK] buttons ---

        #region --- [Read], [RePrice], and [OK] buttons ---
        private void OnTeklaRead_button_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Читать?", "TSmatch", MessageBoxButton.OK);
            //////////model.Read();
            //////////isRawChanged = true;
        }

        private void RePrice_button_Click(object sender, RoutedEventArgs e)
        {
            Msg.AskFOK("Пересчет стоимости материалов");
            //20/5            if (!Msg.AskYN("Правила годятся?")) { var W_Rules = new W_Rules(); W_Rules.Show(); }
            RePricing();
            ////////////////RePrice.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal
            ////////////////    , new NextPrimeDelegate(WrReportPanel));
            //15/5            model.mh.Pricing(model);
        }

        internal static void RePricing()
        {
            throw new NotImplementedException();
            ////model.mh.Pricing(ref model);
            ////ModelIsChanged = true;
        }

        private void OnHelp(object sender, RoutedEventArgs e)
        {
//25/7            string helpPath = boot.TOCdir + @"\TSmatchHelp.mht";
//25/7            System.Diagnostics.Process.Start(helpPath);
        }
        private void OnAbout(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException(); 
//            Msg.AskFOK(ABOUT);
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            mod.Exit();
            Application.Current.Shutdown();
        }
        #endregion --- [Read], [RePrice], and [OK] buttons ---
    } //end class W_Detailed
} //end namespace
