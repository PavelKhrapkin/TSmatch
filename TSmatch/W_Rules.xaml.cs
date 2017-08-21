/*-----------------------------------------------
 * WPF Window W_Rules 21.8.2017 Pavel Khrapkin
 * ----------------------------------------------
 * --- History ---
 * 2017.05.25 - written
 * 2017.08.9  - nElms column output
 * 2017.08.20 - ListBox<Rules> calculation
 * 2017.08.21 - Contect menu with temporary button "Rule Change"
 * --- Known Issue & ToDos ---
 * - еще нет диалога по допустимости CompSet для выбранного поставщика
 * - не написан метод ChekIfChanges()
 */
using System;
using System.Collections.Generic;
using System.Windows;
using log4net;
using TSmatch.ElmAttSet;
using Msg = TSmatch.Message.Message;

namespace TSmatch
{
    /// <summary>
    /// Interaction logic for W_Rules.xaml
    /// </summary>
    public partial class W_Rules : Window
    {
        public static readonly ILog log = LogManager.GetLogger("W_Rules");

        private bool chkGroups, chkElements;

        public W_Rules()
        {
            InitializeComponent();
            Title = "TSmatch: работа с правилами";
            List<Rl> items = getRuleItems(MainWindow.model, rePrice: false);
            if (!chkGroups || !chkElements)
            {
                var mod = MainWindow.model;
                mod.mh.Pricing(ref MainWindow.model);
                if (!mod.sr.CheckModelIntegrity(mod)) Msg.AskFOK("Model is changed");
                items = getRuleItems(MainWindow.model, rePrice: true);
            }
            WRules.ItemsSource = items;
        }

        //--------- 21/8/17 -----------
        private void Bold_Checked(object sender, RoutedEventArgs e)
        {
            ChangeRule.FontWeight = FontWeights.Bold;
        }

        private void Bold_Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeRule.FontWeight = FontWeights.Normal;
        }

        private void Italic_Checked(object sender, RoutedEventArgs e)
        {
            ChangeRule.FontStyle = FontStyles.Italic;
        }

        private void Italic_Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeRule.FontStyle = FontStyles.Normal;
        }
        //--------- 21/8/17 -----------

        private int nGr, nElms;
        private double price;

        private List<Rl> getRuleItems(Model.Model model, bool rePrice)
        {
            List<Rl> items = new List<Rl>();
            int chkGr = 0, chkElm = 0;
            foreach (var rule in MainWindow.model.Rules)
            {
                nGr = nElms = 0; price = 0;
                if (rePrice)
                    foreach (var match in model.matches)
                        calcGr(match.group, rule, match.rule.text);
                else
                    foreach (Group gr in model.elmGroups) calcGr(gr, rule);
                string gr_price = string.Format("{0}/{1}:{2,12:N2}р", nGr, nElms, price);
                items.Add(new Rl(gr_price, rule.date, rule.sSupl, rule.sCS, rule.text));
                chkGr += nGr; chkElm += nElms;
            }
            chkGroups = model.elmGroups.Count == chkGr;
            chkElements = model.elements.Count == chkElm;
            return items;
        }

        private void calcGr(Group gr, Rule.Rule rule, string mtchRuleTxt = "")
        {
            if (gr.SupplierName != rule.sSupl || gr.CompSetName != rule.sCS) return;
            if (mtchRuleTxt != "" && mtchRuleTxt != rule.text) return;
            nGr++;
            nElms += gr.guids.Count;
            price += gr.totalPrice;
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
                ////WRules.Items.Refresh();
                ////InvalidateArrange();
                //20/8                RePrice.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal
                //20/8    , new NextPrimeDelegate(WrReportPanel));
                break;
            }
            MainWindow.model.isChanged = true;
        }

        private void Chng_Button_Click(object sender, RoutedEventArgs e)
        {
            return;
        }
    }
} // end namespace
