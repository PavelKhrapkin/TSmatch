/*-------------------------------------------
 * WPF Main Windows 3.11.2017 Pavel.Khrapkin
 * --- History ---
 * 2017.05.15 - restored as TSmatch 1.0.1 after Source Control excident
 * 2017.05.23 - Menu OnPriceCheck
 * 2017.08.07 - modified SetModel initialization
 * 2017.09.07 - Splash screen add
 * 2017.09.13 - Messages from TSmatchMsg.resx
 * 2017.09.19 - Iso Read button
 * 2017.10.01 - DataGrid support
 * 2017.11.03 - Property.Settings parameters, menu multi-language
 * --- Known Issue & ToDos ---
 * - ToDo some kind of progress bar moving on the MainWindow, when Tekla re-draw HighLight.
 */
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using Boot = TSmatch.Bootstrap.Bootstrap;
using ElmGr = TSmatch.Group.Group;
using Ifc = TSmatch.IFC.IFC;
using Log = match.Lib.Log;
using Mod = TSmatch.Model.Model;
using Msg = TSmatch.Message.Message;
using Supl = TSmatch.Suppliers.Supplier;

namespace TSmatch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly ILog log = LogManager.GetLogger("MainWindow");

        private readonly string version = Properties.Settings.Default.VERSION;
        public string sLanguage = Properties.Settings.Default.sLanguage;
        public string MyCity = Properties.Settings.Default.MyCity;

        public static readonly Boot boot = new Boot();
        public delegate void NextPrimeDelegate();

        public static Mod model;
        public static ElmGr currentGroup;
        public static string SuplName;
        public static string message;

        public MainWindow()
        {
            Log.START(version);
            InitializeComponent();
            new SplashScreen(version).ShowDialog();
            MainWindowLoad();
        }

        #region --- MainWindow Panels ---
        private void MainWindowLoad()
        {
 //12/11           Title = Msg.S("MainWindow__Title");
            SetMenuHdrs();
            WrModelInfoPanel();
            WrReportPanel();
            MWmsg("No Price Groups highlighted");
            if (!boot.isTeklaActive) TeklaRead.IsEnabled = false;
        }

        private void WrModelInfoPanel()
        {
            ModelName.Text = model.name;
            City.Text = model.adrCity;
            if (model.adrStreet != string.Empty)
                City.Text = model.adrCity + ", " + model.adrStreet;
            DateCAD.Text = model.date.ToLongDateString()
                + " " + model.date.ToShortTimeString();
            DatePricing.Text = model.pricingDate.ToLongDateString()
                + " " + model.pricingDate.ToShortTimeString();
            elm_count.Text = model.elements.Count.ToString();
            gr_count.Text = model.elmGroups.Count.ToString();
        }

        private bool adrIsChanged = false;
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                string[] str = City.Text.Split(',');
                string _city = str[0].Trim();
                string _street = City.Text.Substring(str[0].Length + 1).Trim();
                if (_city != model.adrCity || _street != model.adrStreet)
                {
                    //27/7                    ModelIsChanged = true;
                    model.isChanged = true;
                    DataContext = this;
                    adrIsChanged = true;
                    model.adrCity = _city;
                    model.adrStreet = _street;
                }
            }
        }

        private void WrReportPanel()
        {
            //////////////grMat.Header = Msg.S("WPF_MainWindow_grMat");
            //////////////grPrf.Header = Msg.S("WPF_MainWindow_grPrf");
            //////////////grPrice.Header = Msg.S("WPF_MainWindow_grPrice");
            //12/11 //////grVol.Header = Msg.S("WPF_MainWindow_grVolume");
            //////////////grWgt.Header = Msg.S("WPF_MainWindow_grWeight");
            //////////////grLng.Header = Msg.S("WPF_MainWindow_grLength");
            //////////////grSupl.Header = Msg.S("WPF_MainWindow_grSupplier");
            List<gr> items = new List<gr>();
            foreach (var gr in model.elmGroups)
            {
                string sPrice = String.Format("{0, 14:N2}", gr.totalPrice);
                string sWgt = String.Format("{0, 10:N1}", gr.totalWeight);
                string sVol = String.Format("{0, 10:N3}", gr.totalVolume);
                string sLng = String.Format("{0, 10:N1}", gr.totalLength / 1000);
                string sSupl = string.IsNullOrEmpty(gr.SupplierName) ? "---" : gr.SupplierName;
                var g = new gr()
                {
                    mat = gr.Mat,
                    prf = gr.Prf,
                    price = sPrice,
                    wgt = sWgt,
                    vol = sVol,
                    lng = sLng,
                    //27/7                                   supl = gr.SupplierName};
                    supl = sSupl
                };
                items.Add(g);
            }
            elm_groups.ItemsSource = items;

            double totalPrice = 0.0;
            foreach (var gr in model.elmGroups) totalPrice += gr.totalPrice;
            string st = string.Format("Общая цена проекта {0:N0} руб", totalPrice);
            ModPriceSummary.Text = st;

            //--TMP!!
            List<string> suppliers = new List<string>() { "СтальХолдинг", "ЛенСпецСталь", "База СЕВЗАПМЕТАЛЛ", "ГК Монолит СПб" };
            grSupl.ItemsSource = suppliers;
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
            if (!string.IsNullOrEmpty(supl.Index)) str += supl.Index + ", ";
            str += supl.City + ", ";
            if (str.Length > 20) str += "\n";
            str += supl.Street + "\nтел." + supl.Telephone;
            Supl_CS.Text = str;
            //--2017.07.26 не вполне работает Hyperlink- нет вызова сайта при клике. Пока оставил так..
            Supl_URL.Inlines.Clear();
            Run myURL = new Run(supl.Url);
            Hyperlink hyperl = new Hyperlink(myURL);
            Supl_URL.Inlines.Add(hyperl);
            //--
            double p = 0, w = 0, v = 0;
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

            MWmsg("выделяю группу..");
            elm_groups.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal
                , new NextPrimeDelegate(HighLighting));
        }

        private void SupplierChanged(object sender, SelectionChangedEventArgs e)
        {
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
        private void SetMenuHdrs()
        {
            ////////////mFile.Header = Msg.S("MainWindow mFile");
            ////////////mSaveAs.Header = Msg.S("MainWindow mSaveAs");
            //12/11/////mSetting.Header = Msg.S("MainWindow mSetting");
            ////////////mLanguage.Header = Msg.S("MainWindow_mLanguage");
            ////////////mSelect.Header = Msg.S("MainWindow mSelect");
            ////////////mExit.Header = Msg.S("MainWindow mExit");

            //3/11            MyLng.Header = setHeader("Language", "Язык");           

            //3/11            mView.Header = setHeader("View", "Вид");

        }

        private void OnLanguage(object sender, RoutedEventArgs e)
        {
//3/11/17            mLanguage.Icon = new System.Windows.Controls.Image(Source = new BitmapCacheOption());
//12/11            Msg.AskFOK("Not ready yet");
        }

        private void OnSaveAs(object sender, RoutedEventArgs e)
        {
 //12//11           Msg.AskFOK("Not ready yet");
        }

        private void OnFontSize(object sender, RoutedEventArgs e)
        {
 //12//11           Msg.AskFOK("Not ready yet");
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
//12/11            Msg.AskFOK("Not ready yeat");
            //22/5            var wChoice = new WindowSupplierChain();
            //22/5            wChoice.Show();
        }

        private void OnProfile(object sender, RoutedEventArgs e)
        {
 //12/11           Msg.AskFOK("Not ready yeat");
            //22/5            var wChoice = new WindowSupplierChain();
            //22/5            wChoice.Show();
        }

        private void OnRules(object sender, RoutedEventArgs e)
        {
            var wRules = new W_Rules();
            wRules.Show();
        }
        #endregion --- [Read], [RePrice], and [OK] buttons ---

        #region --- [Read], [iso], [RePrice], and [OK] buttons ---
        private void OnTeklaRead_button_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Читать?", "TSmatch", MessageBoxButton.OK);
            model.Read();
            model.isChanged = true;
        }

        private void OnIsoRead_button_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Читать ISO?", "TSmatch", MessageBoxButton.OK);
            model.ifcPath = @"C:\Users\khrapkin\Desktop\Сибур IFC\18.09.17 (3).ifc";
            model.elements = Ifc.Read(model.ifcPath);
            model.isChanged = true;
        }

        private void RePrice_button_Click(object sender, RoutedEventArgs e)
        {
  //12/11   Msg.AskFOK("MainWindow__RePrice");  //works only as literal, not as a Name(?) - "Пересчет стоимости материалов"
            model.mh.Pricing(ref model);
            model.isChanged = true;
            RePrice.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal
                , new NextPrimeDelegate(WrReportPanel));
        }

        private void OnHelp(object sender, RoutedEventArgs e)
        {
            string helpPath = boot.TOCdir + @"\TSmatchHelp.mht";
            System.Diagnostics.Process.Start(helpPath);
        }

        private void OnAbout(object sender, RoutedEventArgs e)
        {
 //12/11   Msg.AskFOK(version);
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            model.Exit();
            Application.Current.Shutdown();
        }
        #endregion --- [Read], [RePrice], and [OK] buttons ---

        private void MWmsg(string str, params object[] p)
        {
   //12/11         string message = Msg.S(str, p);
            Dispatcher.Invoke(new Action(() => { StatusMsg.Text = message; }));
        }
    }
} //end namespace