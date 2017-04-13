using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using TS = TSmatch.Tekla.Tekla;
using Mod = TSmatch.Model.Model;
using Boot = TSmatch.Bootstrap.Bootstrap;
using Gr = TSmatch.ElmAttSet.Group;
using Supl = TSmatch.Suppliers.Supplier;
using CompSet = TSmatch.CompSet.CompSet;

namespace SELECT
{
    public partial class Form1 : Form
    {
        Boot boot;
        Mod model;
        TS ts = new TS();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            boot = new Boot();
            model = boot.model;
            WrForm(wrForm.modelINFO);
            WrForm(wrForm.modelReport);
            List<Gr> grLst = new List<Gr>();
            foreach (var gr in model.elmGroups)
                if (gr.totalPrice == 0) grLst.Add(gr);
            model.Highlight(grLst);
        }

        enum wrForm { modelINFO, modelReport };
        private void WrForm(wrForm wrf, int indx = -1)
        {
            switch (wrf)
            {
                case wrForm.modelINFO:
                    modINFOhdr.Text = "Информация о модели";
                    label_modINFO.Text =
                        "Название \"" + model.name + "\""
                        + "\nДата сохранения " + model.date.ToLongDateString()
                                         + " " + model.date.ToShortTimeString()
                        + "\nВсего " + model.elementsCount + " элементов"
                        + " в " + model.elmGroups.Count + " группах";
                    label_modINFO.BackColor = Color.LightBlue;
                    break;

                case wrForm.modelReport:
                    List<string> modRep = new List<string>();
                    foreach (var gr in model.elmGroups)
                    {
                        string str = string.Format("{0,12} {1,15} {2,20:N2}", gr.mat, gr.prf, gr.totalPrice);
                        modRep.Add(str);
                    }
                    listBox1.DataSource = modRep;
                    double totalPrice = 0.0;
                    foreach (var gr in model.elmGroups) totalPrice += gr.totalPrice;
                    string st = string.Format("Общая цена проекта {0:N0} руб", totalPrice);
                    label_totalPrice.Text = st;
                    break;
            }
        }

        private void modINFOhdr_Click(object sender, EventArgs e)
        {

        }

        private void OK_Click(object sender, EventArgs e)
        {
            ts.HighlightClear();
            model.docReport.Close();
            boot.docTSmatch.Close();
            Application.Exit();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var gr = model.elmGroups[listBox1.SelectedIndex];
            if (gr.totalPrice == 0)
            {
                label_supl.Text = "";
                label_cs.Text = "";
                label_suplPrice.Text = "";
            }
            else
            {
                string suplName = gr.SupplierName;
                Supl supl = new Supl(suplName);
                label_supl.Text = supl.getSupplierStr();
                label_cs.Text = gr.CompSetName;
                double suplPrice = 0.0;
                foreach(var grp in model.elmGroups)
                {
                    if (grp.SupplierName == suplName) suplPrice += grp.totalPrice;
                }
                label_suplPrice.Text = string.Format("Всего по поставщику {0:N0} руб", suplPrice);
            }
            ts.HighlightClear();
            model.Highlight(model.elmGroups[listBox1.SelectedIndex]);
        }

        private void button_Read_Click(object sender, EventArgs e)
        {
            label_modINFO.BackColor = Color.Yellow;
            ts.Read();
            WrForm(wrForm.modelINFO);
            model.wrModel(Mod.WrMod.ModelINFO);
            model.wrModel(Mod.WrMod.Raw);
            model.getSavedRules();
            model.Handler();
            model.wrModel(Mod.WrMod.Report);
            model.docReport.Close();
        }

        private void label_totalPrice_Click(object sender, EventArgs e)
        {

        }
    }
}
