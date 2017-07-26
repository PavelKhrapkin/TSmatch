/*-------------------------------------------
 * WPF Main Windows 24.6.2017 Pavel.Khrapkin
 * --- History ---
 * 2017.05.15 - restored as TSmatch 1.0.1 after Source Control excident
 * 2017.05.23 - Menu OnPriceCheck
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
using System.Diagnostics;

using log4net;
using Log = match.Lib.Log;
using FileOp = match.FileOp.FileOp;
using Boot = TSmatch.Bootstrap.Bootstrap;
using Msg = TSmatch.Message.Message;
using Mod = TSmatch.Model.Model;
using Supl = TSmatch.Suppliers.Supplier;
using ElmGr = TSmatch.ElmAttSet.Group;

namespace TSmatch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly ILog log = LogManager.GetLogger("MainWindow");

        const string ABOUT = "TSmatch v1.0.2 27.6.2017";
        public static Boot boot;
        public static string MyCity = "Санкт-Петербург";
        public delegate void NextPrimeDelegate();

        public static Mod model;
        public static ElmGr currentGroup;
        public static string SuplName;
        public static bool ModelIsChanged = false, isRawChanged = false, isRuleChanged = false;
        public static string message;

        public MainWindow()
        {
            Log.START(ABOUT);
            InitializeComponent();
            MainWindowLoad();
        }

        #region --- MainWindow Panels ---
        private void MainWindowLoad()
        {
            Title = "TSmatch - согласование поставщиков в проекте";
            //20/5            message.Text = "..Load MainWindow..";
            boot = new Boot();
            var sr = new SaveReport.SavedReport();
            model = new Mod();
            model.SetModel(boot);
            WrModelInfoPanel();
            WrReportPanel();
            //30/5            model.HighLightElements(Mod.HighLightMODE.NoPrice);
            //25/7 message = "вначале группы без цен...";
            msg.Text = message;
        }

        private void WrModelInfoPanel()
        {
            ModelINFO.Text = "Модель:\t\"" + model.name + "\""
                    + "\nДата сохранения " + model.date.ToLongDateString()
                        + " " + model.date.ToShortTimeString()
                    + "\nДата расценки     " + model.pricingDate.ToLongDateString()
                        + " " + model.pricingDate.ToShortTimeString()
                + "\nВсего " + model.elements.Count + " элементов"
                        + ", " + model.elmGroups.Count + " групп";
        }

        private void WrReportPanel()
        {
            List<gr> items = new List<gr>();
            foreach (var gr in model.elmGroups)
            {
                string sPrice = String.Format("{0, 14:N2}", gr.totalPrice);
                string sWgt = String.Format("{0, 10:N1}", gr.totalWeight);
                string sVol = String.Format("{0, 10:N3}", gr.totalVolume);
                string sLng = String.Format("{0, 10:N1}", gr.totalLength/1000);
                var g = new gr() { mat = gr.Mat, prf = gr.Prf, price = sPrice,
                                   wgt = sWgt,   vol = sVol,   lng = sLng,
                                   supl = gr.SupplierName};
                items.Add(g);
            }
            elm_groups.ItemsSource = items;

            double totalPrice = 0.0;
            foreach (var gr in model.elmGroups) totalPrice += gr.totalPrice;
            string st = string.Format("Общая цена проекта {0:N0} руб", totalPrice);
            ModPriceSummary.Text = st;
        }

        public class gr
        {
            public string mat { get; set; }
            public string prf { get; set; }
            public string price { get; set; }
            public string wgt { get; set; }
            public string vol { get; set; }
            public string lng { get; set; }
            public string supl { get; set; }
        }
        private void elmGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gr g = (gr)elm_groups.SelectedValue;
            if (g == null) return;
            currentGroup = model.elmGroups.Find(x => x.Mat == g.mat && x.Prf == g.prf);
            SuplName = currentGroup.SupplierName;
            Supl supl = new Supl(currentGroup.SupplierName);
            Supl_CS_Mat_Prf.Text = SuplName + "\t" + currentGroup.CompSetName;
            string str = "Адрес: ";
            if (!string.IsNullOrEmpty(supl.index)) str += supl.index + ", ";
            str += supl.City + ", ";
            if (str.Length > 20) str += "\n";
            str += supl.street + "\nтел." + supl.telephone;
            Supl_CS.Text = str;
            //--2017.07.26 не вполне работает Hyperlink- нет вызова сайта при клике. Пока оставил так..
                        Supl_URL.Inlines.Clear();
                        Run myURL = new Run(supl.Url);
                        Hyperlink hyperl = new Hyperlink(myURL);
                        Supl_URL.Inlines.Add(hyperl);           
            //--
            double p = 0, w=0, v=0;
            foreach (var gr in model.elmGroups)
            {
                if (gr.SupplierName != currentGroup.SupplierName) continue;
                w += gr.totalWeight;
                v += gr.totalVolume;
                p += gr.totalPrice;
            }
            TotalSupl_weight_volume.Text = String.Format("Общий вес= {0:N1} кг, объем = {1:N1} м3", w, v);
            string sP = string.Format("{0:N2}", p);
            TotalSupl_price.Text = "Цена по этому поставщику " + sP + " руб";

            message = "выделяю группу..";
            elm_groups.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal
                , new NextPrimeDelegate(HighLighting));
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void HighLighting()
        {
            model.HighLightElements(Mod.HighLightMODE.Guids, currentGroup.guids);
        }
        #endregion --- MainWindow Panels ---

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
            if (string.IsNullOrEmpty(SuplName)) return;
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
            model.Read();
            isRawChanged = true;
        }

        private void RePrice_button_Click(object sender, RoutedEventArgs e)
        {
            Msg.AskFOK("Пересчет стоимости материалов");
            //20/5            if (!Msg.AskYN("Правила годятся?")) { var W_Rules = new W_Rules(); W_Rules.Show(); }
            RePricing();
            RePrice.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal
                , new NextPrimeDelegate(WrReportPanel));
            //15/5            model.mh.Pricing(model);
        }

        internal static void RePricing()
        {
            model.mh.Pricing(ref model);
            ModelIsChanged = true;
        }

        private void OnHelp(object sender, RoutedEventArgs e)
        {
            string helpPath = boot.TOCdir + @"\TSmatchHelp.mht";
            System.Diagnostics.Process.Start(helpPath);
        }

        private void OnAbout(object sender, RoutedEventArgs e)
        {
            Msg.AskFOK(ABOUT);
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            model.Exit();
            Application.Current.Shutdown();
        }
        #endregion --- [Read], [RePrice], and [OK] buttons ---
    }
} //end namespace