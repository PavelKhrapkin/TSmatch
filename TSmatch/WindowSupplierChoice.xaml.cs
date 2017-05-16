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
            List<supl> items = new List<supl>();
            Docs doc = Docs.getDoc(Decl.SUPPLIERS);
            for (int i = doc.i0; i <= doc.il; i++)
            {
                string sName = doc.Body.Strng(i, Decl.SUPL_NAME);
                string sCity = doc.Body.Strng(i, Decl.SUPL_CITY);
                string sIndx = doc.Body.Strng(i, Decl.SUPL_STREET);
                string sStrt = doc.Body.Strng(i, Decl.SUPL_STREET);
                string sUrl = doc.Body.Strng(i, Decl.SUPL_URL);
                var s = new supl(sName, sCity, sIndx, sStrt, sUrl);
                items.Add(s);
                Suppliers.ItemsSource = items;
            }
        }

        public class supl
        {
            public string SuplName;
            public string City;
            public string Index;
            public string StreetAdr;
            public string Url;

            public supl(string _SupName, string _City, string _Index, string _Street, string _Url)
            {
                SuplName = _SupName;
                City = _City;
                Index = _Index;
                StreetAdr = _Street;
                Url = _Url;
            }
        }

        private void OnSupplier_changed(object sender, SelectionChangedEventArgs e)
        {
            Msg.AskFOK("ыыыыs");
        }

            private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
