/*-----------------------------------------------
 * W_Supplier WPF Window 21.5.2017 Pavel Khrapkin
 * ----------------------------------------------
 * --- History ---
 * 2017.05.21 - written
 * --- Known Issue & ToDos ---
 * - еще нет диалога по допустимости CompSet для выбранного поставщика
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

using log4net;
using Lib = match.Lib.MatchLib;
using Decl = TSmatch.Declaration.Declaration;
using Msg = TSmatch.Message.Message;
using Docs = TSmatch.Document.Document;
using Supl = TSmatch.Suppliers.Supplier;

namespace TSmatch
{
    /// <summary>
    /// Interaction logic for W_Supplier.xaml
    /// </summary>
    public partial class W_Supplier : Window
    {
        public static readonly ILog log = LogManager.GetLogger("W_Supplier");

        private string selectedSupplier;
        List<Supl> suppliers = new List<Supl>();

        public W_Supplier()
        {
            InitializeComponent();
            Title = "TSmatch: выбор поставщика";
            List<Spl> items = new List<Spl>();
            Docs doc = Docs.getDoc(Decl.SUPPLIERS);
            for (int i = doc.i0; i <= doc.il; i++)
            {
                Supl s = new Supl(i);
                if (s == null || string.IsNullOrWhiteSpace(s.Name) || string.IsNullOrWhiteSpace(s.City)) continue;
                items.Add(new Spl(s.Name, Lib.ToInt(s.Index), s.City, s.Street, s.Url));
                suppliers.Add(s);
            }
            Spl oldSupl = items.Find(x => x.SuplName == MainWindow.SuplName);
            if(oldSupl == null) Msg.F("W_Supplier: No Selected SuplName");
            int ind = items.IndexOf(oldSupl);
            items[ind].Flag = true;
            log.Info("Supplier.Count=" + suppliers.Count + "\tInitial Supplier =\"" + oldSupl.SuplName + "\"");
            items.Sort();
            Suppliers.ItemsSource = items;
        }

        public class Spl : IComparable<Spl>
        {
            public string SuplName { get; set; }
            public int Index { get; set; }
            public string City { get; set; }
            public string Street { get; set; }
            public string Url { get; set; }
            public bool Flag { get; set; }

            public Spl(string name, int index, string city, string street, string url)
            {
                SuplName = name;
                Index = index;
                City = city;
                Street = street;
                Url = url;
                Flag = false;
            }

            int IComparable<Spl>.CompareTo(Spl other)
            {
                if (Flag && !other.Flag) return -1;
                if (!Flag && other.Flag) return 1;
                int result = - City.CompareTo(other.City);
                if (result == 0) result = SuplName.CompareTo(other.SuplName);
                return result;
            }
        }

        private void OnSupplier_changed(object sender, SelectionChangedEventArgs e)
        {
            string oldSupplier = MainWindow.SuplName;
            Spl selectedSupl = (Spl)Suppliers.SelectedItem;
            selectedSupplier = selectedSupl.SuplName;
            log.Info("new Supplier selected = \"" + selectedSupl.SuplName + "\"");

            var selSupl = suppliers.Find(x => x.Name == selectedSupl.SuplName);
            if (selSupl == null) Msg.F("Inconsystent W_Supplier");
            var grOLD = MainWindow.currentGroup;
            var grNEW = selSupl.getNEWcs(selSupl, grOLD);
            if(grNEW == null)
            {
                 Msg.AskOK("\"{0}\" не поставляет [{1}, {2}], но Вы можете изменить Правила" +
                    " и согласовать изменения с проектировщиком."
                    , selSupl.Name, grOLD.Mat, grOLD.Prf);
            }
            else
            {
                //////                // пересчитывать вес, объем, стоимость группы; диалог по "тому же" сортаменту
                ////////22/5                selSupl = selSupl.getTotals();
            }
#if notReady
            Supl supl = 
            bool csOK = false;
            do {
                if (!Msg.AskYN("Вы действительно хотите заменить \"{0}\" на \"{1}\"?"
                    , oldSupplier, selectedSupplier)) return;
                var cs = 
                csOK = 
            } while (csOK);
//21/5            MainWindow.SuplName = selSN;
//21/5            MainWindow.RePricing();

            Close();
#endif // notReady
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            CheckIfChanges();
            Close();
        }

        private void CheckIfChanges()
        {
//22/5            throw new NotImplementedException();
        }
    }
} // end namespace W_Supplier
