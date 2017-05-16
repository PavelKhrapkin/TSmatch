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

using Decl = TSmatch.Declaration.Declaration;
using Msg = TSmatch.Message.Message;
using Supl = TSmatch.Suppliers.Supplier;
using Docs = TSmatch.Document.Document;

namespace TSmatch
{
    /// <summary>
    /// Interaction logic for WindowSupplierChoice.xaml
    /// </summary>
    public partial class WindowSupplierChoice : Window
    {
        public WindowSupplierChoice()
        {
            InitializeComponent();
            Title = "TSmatch: выбор поставщика";
            List<Supl> items = new List<Supl>();
            Docs doc = Docs.getDoc(Decl.SUPPLIERS);
            for (int i = doc.i0; i <= doc.il; i++)
            {
                string Name = doc.Body.Strng(i, Decl.SUPL_NAME);
                string city = doc.Body.Strng(i, Decl.SUPL_CITY);
                int Indx = doc.Body.Int(i, Decl.SUPL_INDEX);
                string Strt = doc.Body.Strng(i, Decl.SUPL_STREET);
                string Url = doc.Body.Strng(i, Decl.SUPL_URL);
                if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(city)) continue;
                Supl s = new Supl(Name, Indx, city, Strt, Url);
                items.Add(s);
            }
            Suppliers.ItemsSource = items;
        }
        public class Supl
        {
            public string Name { get; set; }
            public int Index { get; set; }
            public string City { get; set; }
            public string Street { get; set; }
            public string Url { get; set; }

            public Supl(string name, int index, string city, string street, string url)
            {
                Name = name;
                Index = index;
                City = city;
                Street = street;
                Url = url;
            }
        }

        private void OnSupplier_changed(object sender, SelectionChangedEventArgs e)
        {
            string curSN = MainWindow.SuplName;
            Supl selectedSupl = (Supl) Suppliers.SelectedItem;
            string selSN = selectedSupl.Name;
            if (!Msg.AskYN("Вы действительно хотите заменить \"{0}\" на \"{1}\"?", curSN, selSN)) return;
            MainWindow.SuplName = selSN;
            MainWindow.RePricing();
            Close();
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
