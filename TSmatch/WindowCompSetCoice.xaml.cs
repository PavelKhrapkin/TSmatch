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

namespace TSmatch
{
    /// <summary>
    /// Interaction logic for WindowCompSetCoice.xaml
    /// </summary>
    public partial class WindowCompSetCoice : Window
    {
        public WindowCompSetCoice()
        {
            InitializeComponent();
            Title = "TSmatch: выбор профиля";
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
