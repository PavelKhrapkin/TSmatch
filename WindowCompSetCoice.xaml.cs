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
            List<User> items = new List<User>();
            items.Add(new User() { Name = "John Doe", Age = 42, Mail = "john@doe-family.com" });
            items.Add(new User() { Name = "Jane Doe", Age = 39, Mail = "jane@doe-family.com" });
            items.Add(new User() { Name = "Sammy Doe", Age = 7, Mail = "sammy.doe@gmail.com" });
            lvUsers.ItemsSource = items;
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
    public class User
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string Mail { get; set; }
    }
}
