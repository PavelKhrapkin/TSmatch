/*-------------------------------------------
 * WPF Main Windows 15.5.2017 Pavel.Khrapkin
 * ------------------------------------------
 * --- History ---
 * 2017.05.15 - restored as Tsmatch 1.0.1 after Source Control excident
 * --- Known Issue ---
 * - It is good re-design XAML idea to have two column on MainWindow with the Width = "*".
 * Than with Window size changed, Group<Mat,Prf,Price> part would become wider.
 * - ToDo some kind of progress bar moving on the MainWindow, when Tekla re-draw HighLight.
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

using log4net;
using Log = match.Lib.Log;
using FileOp = match.FileOp.FileOp;
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

        public static Mod model;
        public static ElmGr currentGroup;

        public MainWindow()
        {
            Log.START("TSmatch v1.0.2 16.5.2017");
            InitializeComponent();
            MainWindowLoad();
        }

        private void MainWindowLoad()
        {
            Title = "TSmatch - согласование поставщиков в проекте";
            var boot = new TSmatch.Bootstrap.Bootstrap();
            var sr = new SaveReport.SavedReport();
            model = sr;
            model.SetModel(boot);
            WrForm(wrForm.modelINFO);
            WrForm(wrForm.modelReport);
            model.HighLightElements(Mod.HighLightMODE.NoPrice);
        }

        public class gr
        {
            public string mat { get; set; }
            public string prf { get; set; }
            //3/5           public double price { get; set; }
            public string price { get; set; }
        }

        enum wrForm { modelINFO, modelReport };
        private void WrForm(wrForm wrf, int indx = -1)
        {
            switch (wrf)
            {
                case wrForm.modelINFO:
                    ModelINFO.Text = "Модель:\t\"" + model.name + "\""
                        + "\nДата сохранения " + model.date.ToLongDateString()
                            + " " + model.date.ToShortTimeString()
                        + "\nДата расценки     " + model.pricingDate.ToLongDateString()
                            + " " + model.pricingDate.ToShortTimeString()
                    + "\nВсего " + model.elementsCount + " элементов"
                            + ", " + model.elmGroups.Count + " групп";
                    break;

                case wrForm.modelReport:
                    List<gr> items = new List<gr>();
                    foreach (var gr in model.elmGroups)
                    {
                        string sPrice = String.Format("{0, 20:N2}", gr.totalPrice);
                        var g = new gr() { mat = gr.Mat, prf = gr.Prf, price = sPrice };
                        items.Add(g);
                    }
                    elmGroups.ItemsSource = items;

                    double totalPrice = 0.0;
                    foreach (var gr in model.elmGroups) totalPrice += gr.totalPrice;
                    string st = string.Format("Общая цена проекта {0:N0} руб", totalPrice);
                    //14/5                   string prDate = "\nДата расценки " + model.pricingDate.ToString("d.MM.yyyy H:mm");
                    ModPriceSummary.Text = st; //14/5 + prDate;
                    break;
            }
        }

        private void elmGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gr v = (gr)elmGroups.SelectedValue;
            if (v == null) return;
            currentGroup = model.elmGroups.Find(x => x.Mat == v.mat && x.Prf == v.prf);
            string cs_name = string.Empty;
            try { cs_name = currentGroup.match.rule.CompSet.name; }
            catch { }
            string suplName = currentGroup.SupplierName;
            Supl supl = new Supl(currentGroup.SupplierName);
            Supplier.Content = suplName + "\t" + cs_name; ;
            Supl_CS.Text = supl.getSupplierStr();

            model.HighLightElements(Mod.HighLightMODE.Guids, currentGroup.guids);

            double p = 0;
            foreach (var gr in model.elmGroups)
            {
                if (gr.SupplierName != currentGroup.SupplierName) continue;
                p += gr.totalPrice;
            }
            string sP = string.Format("{0:N2}", p);
            TotalSupl_price.Text = "Всего по этому поставщику " + sP + " руб";
        }

        private void OnSuplClick(object sender, RoutedEventArgs e)
        {
            var SuplChoiceWindow = new WindowSuplCSChoice();
            SuplChoiceWindow.Show();
        }

        private void OnTeklaRead_button_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Читать?", "TSmatch", MessageBoxButton.OK);
            //8/5            model.Read();
        }

        private void RePrice_button_Click(object sender, RoutedEventArgs e)
        {
//15/5            var SuplChoiceWindow = new WindowSuplCSChoice();
//15/5            SuplChoiceWindow.Show();
            MessageBox.Show("Пересчет стоимости материалов", "TSmatch");
//15/5            model.mh.Pricing(model);
//15/5            WrForm(wrForm.modelReport);
        }
        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            model.HighLightClear();
            FileOp.AppQuit();
            Application.Current.Shutdown();
        }
    }
}
