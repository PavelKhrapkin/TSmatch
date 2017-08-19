/*-----------------------------------------------
 * WPF Window W_Rules 11.8.2017 Pavel Khrapkin
 * ----------------------------------------------
 * --- History ---
 * 2017.05.25 - written
 * 2017.08.9  - nElms column output
 * --- Known Issue & ToDos ---
 * - еще нет диалога по допустимости CompSet для выбранного поставщика
 * - не написан метод ChekIfChanges()
 */
using System;
using System.Collections.Generic;
using System.Windows;
using log4net;

using Lib = match.Lib.MatchLib;
using Msg = TSmatch.Message.Message;
using Decl = TSmatch.Declaration.Declaration;
using Docs = TSmatch.Document.Document;
using Mod = TSmatch.Model.Model;
using System.Linq;

namespace TSmatch
{
    /// <summary>
    /// Interaction logic for W_Rules.xaml
    /// </summary>
    public partial class W_Rules : Window
    {
        public static readonly ILog log = LogManager.GetLogger("W_Rules");

        List<Rule.Rule> rules = new List<Rule.Rule>();

        public W_Rules()
        {
            InitializeComponent();
            Title = "TSmatch: работа с правилами";
            List<Rl> items = new List<Rl>();

            if (MainWindow.model.Rules.Count == 0)
            {
                var mod = MainWindow.model;
                mod.mh.Pricing(ref MainWindow.model);
                if (!mod.sr.CheckModelIntegrity(mod)) Msg.AskFOK("Model is changed");
            }

            foreach (var rule in MainWindow.model.Rules)
            {
                int nGr = 0, nElms = 0;
                double price = 0;
                foreach (var match in MainWindow.model.matches)
                {
                    if (match.rule.sSupl != rule.sSupl || match.rule.sCS != rule.sCS) continue;
                    nGr++;
                    nElms += match.group.guids.Count;
                    price += match.group.totalPrice;
                }
                string gr_price = string.Format("{0}/{1}:{2,12:N2}р", nGr, nElms, price);
                items.Add(new Rl(gr_price, rule.date, rule.sSupl, rule.sCS, rule.text));
            }
            WRules.ItemsSource = items;
        }

        //private void OnRule_changed(object sender, SelectionChangedEventHandled y) //, SelectionChangedEventArgs e)
        //{
        //}

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            CheckIfChanges();
            Close();
        }

        private void CheckIfChanges()
        {
            //25/5            throw new NotImplementedException();
        }

        private class Rl : IComparable<Rl>
        {
            public string gr_price { get; set; }
            public string Date { get; set; }
            public string Supplier { get; set; }
            public string CompSet { get; set; }
            public string RuleText { get; set; }
            public string keyRule { get; set; }

            public Rl(string _gr_price, DateTime date, string supl, string cs, string ruletxt)
            {
                gr_price = _gr_price;
                Date = date.ToString("d.MM.yy H:mm");
                Supplier = supl;
                CompSet = cs;
                RuleText = ruletxt;
            }

            int IComparable<Rl>.CompareTo(Rl other)
            {
                //11/8                if (Flag && !other.Flag) return -1;
                //11/8                if (!Flag && other.Flag) return 1;
                int result = -CompSet.CompareTo(other.CompSet);
                if (result == 0) result = Supplier.CompareTo(other.Supplier);
                if (result == 0) result = RuleText.CompareTo(other.RuleText);
                return result;
            }
        }

        private void OnRule_changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!Msg.AskYN("Delete this Rule?")) return;
            Rl sel = (Rl)WRules.SelectedValue;
            foreach (var r in MainWindow.model.Rules)
            {
                if (r.sSupl != sel.Supplier || r.sCS != sel.CompSet || r.text != sel.RuleText) continue;
                MainWindow.model.Rules.Remove(r);
                WRules.Items.Refresh();
                InvalidateArrange();

                break;
            }
            MainWindow.model.isChanged = true;
        }
    }
} // end namespace
