using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BUYLTools.CutOut.PfV
{
    public partial class PfVViewDLG : System.Windows.Forms.Form
    {
        Dictionary<string, List<PfVElementData>> m_data;

        public PfVViewDLG()
        {
            InitializeComponent();
            m_data = null;
        }

        public void SetData(Dictionary<string, List<PfVElementData>> data)
        {
            m_data = data;

            SetUpTabControl();
        }

        private void SetUpTabControl()
        {
            _tabControl.TabPages.Clear();
            foreach (string key in m_data.Keys)
            {
                if (m_data[key] == null)
                    continue;

                TabPage tp = new TabPage(Path.GetFileName(key));
                tp.Text = Path.GetFileName(key);
                tp.Tag = key;
                _tabControl.TabPages.Add(tp);

                DataGridView dtView = new DataGridView();
                //dtView.RowHeaderMouseDoubleClick += DtView_RowHeaderMouseDoubleClick;
                dtView.DataSource = m_data[key];
                dtView.Dock = DockStyle.Fill;

                tp.Controls.Add(dtView);
            }
        }
     }
}
