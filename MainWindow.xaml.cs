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

namespace WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainWindowLoad();
        }

        public class gr
        {
            public string mat { get; set; }
            public string prf { get; set; }
            public double price { get; set; }
        }

        private void MainWindowLoad()
        {
            Title = "TSmatch - согласование поставщиков в проекте";
            var boot = new TSmatch.Bootstrap.Bootstrap();
            var sr = new TSmatch.SaveReport.SavedReport();
            ////////////model = sr;
            ////////////sr.dir = boot.ModelDir;
            ////////////model.SetModel();
            sr.getSavedReport();
            //////////////24/4            sr.CloseReport();
            WrForm(wrForm.modelINFO);
            WrForm(wrForm.modelReport);
            ////////////////16/4            model.HighLightElements(Mod.HighLightMODE.NoPrice);
            //////////////List<Gr> grLst = new List<Gr>();
            //////////////foreach (var gr in model.elmGroups)
            //////////////    if (gr.totalPrice == 0) grLst.Add(gr);
            //////////////model.Highlight(grLst);
            //////////////rePrice.Text = "Пересчитать\nстоимость";
        }
        enum wrForm { modelINFO, modelReport };
        private void WrForm(wrForm wrf, int indx = -1)
        {
            switch (wrf)
            {
                case wrForm.modelINFO:
                    ModelINFO.Text = "Модель:\tЗС2"
                        + "\nКаталог:\t" + @"C:\TeklaStructuresModels\2016\ЗС2"
                        + "\nЗапись:\t05.03.2016 22:07:15"
                        + "\nВсего 1136 элементова, 38 групп";
                    break;

                case wrForm.modelReport:
                    List<gr> items = new List<gr>();
                    items.Add(new gr() { mat = "B12,5", prf = "1900x1700", price = 42 });
                    items.Add(new gr() { mat = "C255", prf = "PL30", price = 39 });
                    items.Add(new gr() { mat = "C255", prf = "Швеллер 2П", price = 7 });
                    elmGroups.ItemsSource = items;
                    break;
            }
        }

        private void elmGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gr v = (gr)elmGroups.SelectedValue;
            TotalSupl_price.Text = "по поставщику " + v.price + " руб";
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            // 28/4/17            MessageBox.Show("Кнопка ОК");
            Application.Current.Shutdown();
        }
    }
}
