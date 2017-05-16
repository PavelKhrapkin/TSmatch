/*--------------------------------------------------
 * WPF SuplCSChoice Window 14.5.2017 Pavel.Khrapkin
 * -------------------------------------------------
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
using System.Windows.Shapes;

using Mod = TSmatch.Model.Model;
using Supl = TSmatch.Suppliers.Supplier;
using ElmGr = TSmatch.ElmAttSet.Group;
using TSmatch.ElmAttSet;

namespace TSmatch
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class WindowSuplCSChoice : Window
    {
        public WindowSuplCSChoice()
        {
            InitializeComponent();
            Title = "TSmatch - окно выбора";
            Supl_CS_Panel();
        }

        private void Supl_CS_Panel()
        {
            ElmGr group = MainWindow.currentGroup;
            string suplName = group.SupplierName;
            Supplier.Content = suplName;
            string csName = group.CompSetName;
            CompSet.Content = csName;
            Supl supl = new Supl(suplName);
            Supl_CS.Text = supl.getSupplierStr();

            List<ElmGr> elmGroups = MainWindow.model.elmGroups;

            MainWindow.model.HighLightElements(Mod.HighLightMODE.Guids, group.guids);

            double p = 0;
            foreach (var gr in elmGroups)
            {
                if (gr.SupplierName != suplName) continue;
                p += gr.totalPrice;
            }
            string sP = string.Format("{0:N2}", p);
            TotalSupl_price.Text = "Всего по этому поставщику " + sP + " руб";

            GrMat.Content = group.Mat;
            GrPrf.Content = group.Prf;
            GrVol.Text=string.Format("{0:N1} м3", group.totalVolume);
            GrWgt.Text = string.Format("{0:N1} кг", group.totalWeight);
            GrLng.Text = string.Format("{0:N1} м", group.totalLength);
            GrPrice.Text = string.Format("{0:N2} руб", group.totalPrice);
        }

        private void Supplier_Click(object sender, RoutedEventArgs e)
        {
            var SupplierChoiceWindow = new WindowSupplierChoice();
            SupplierChoiceWindow.Show();
        }

        private void CompSet_Click(object sender, RoutedEventArgs e)
        {
            var CompSetChoiceWindow = new WindowCompSetCoice();
            CompSetChoiceWindow.Show();
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
             Close();
        }
     }
}
