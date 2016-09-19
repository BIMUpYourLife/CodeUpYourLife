using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BUYLRevit.CutOut.PfV
{
    public partial class PfVViewDLG : Form
    {
        public PfVViewDLG()
        {
            InitializeComponent();
        }

        public void SetData(Dictionary<string, List<PfVElementData>> data)
        {
            _tabControl.TabPages.Clear();

            foreach (string key in data.Keys)
            {
                _tabControl.TabPages.Add(key, key);
                DataGridView dtView = new DataGridView();
                dtView.DataSource = data[key];
                dtView.Dock = DockStyle.Fill;
                _tabControl.TabPages[key].Controls.Add(dtView);
            }
        }
    }
}
