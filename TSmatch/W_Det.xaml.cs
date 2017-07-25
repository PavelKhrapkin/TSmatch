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

using Msg = TSmatch.Message.Message;
using Mod = TSmatch.Model.Model;

namespace TSmatch
{
    /// <summary>
    /// Interaction logic for W_Det.xaml
    /// </summary>
    public partial class W_Det : Window
    {
        Mod mod;

        public W_Det()
        {
            InitializeComponent();
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            mod.Exit();
            Application.Current.Shutdown();
        }
    }
}
