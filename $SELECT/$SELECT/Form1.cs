using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using TS = TSmatch.Tekla.Tekla;
using Mod = TSmatch.Model.Model;
using Boot = TSmatch.Bootstrap.Bootstrap;
using Gr = TSmatch.ElmAttSet.Group;

namespace TSmatch_SELECT
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

        private void OK_button_Click(object sender, EventArgs e)
        {
            ts.HighlightClear();
            model.docReport.Close();
            boot.docTSmatch.Close();
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            boot = new Boot();
            model = boot.model;
            WrForm(wrForm.modelINFO);
            WrForm(wrForm.modelReport);
            List<Gr> grLst = new List<Gr>();
            foreach(var gr in model.elmGroups)            
                if(gr.totalPrice == 0) grLst.Add(gr);
            model.Highlight(grLst);
        }


        enum wrForm { modelINFO, modelReport };
        private void WrForm(wrForm wrf)
        {
            switch (wrf)
            {
                case wrForm.modelINFO:
                    modINFOhdr.Text = "Общая информация о модели";
                    label_modINFO.Text = 
                        "Название \"" + model.name + "\""
                        + "\nДата сохранения " + model.date.ToLongDateString()
                                         + " " + model.date.ToShortTimeString()
                        + "\nВсего " + model.elementsCount + " элементов"
                        + " в " + model.elmGroups.Count + " группах";
                    label_modINFO.BackColor = Color.LightBlue;
                    break;

                case wrForm.modelReport:
                    //                 ListBox rep = new ListBox();
                    //8/4                    rep.Size = new Size(300, 300);
                    List<string> modRep = new List<string>();
                    foreach (var gr in model.elmGroups)
                    {
                        string str = string.Format("{0,12} {1,15} {2,20:N2}",gr.mat, gr.prf, gr.totalPrice);
                        modRep.Add(str);
                    }
                    listBox1.DataSource = modRep;
                    break;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ts.HighlightClear();
            model.Highlight(model.elmGroups[listBox1.SelectedIndex]);
        }
    }
}
