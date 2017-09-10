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

namespace TSmatch
{
    /// <summary>
    /// Interaction logic for WindowMaterialChoice.xaml
    /// </summary>
    public partial class WindowMaterialChoice : Page
    {
        public WindowMaterialChoice()
        {
            InitializeComponent();
            Title = "TSmatch: выбор материала";
//18/5            List<Supl> items = new List<Supl>();
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            //Close();
        }
    }
}
