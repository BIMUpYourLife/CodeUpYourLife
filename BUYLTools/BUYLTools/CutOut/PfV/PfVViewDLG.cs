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
    public partial class PfVViewDLG : System.Windows.Forms.Form, IPfVView
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
                dtView.RowHeaderMouseDoubleClick += DtView_RowHeaderMouseDoubleClick;
                dtView.DataSource = m_data[key];
                dtView.Dock = DockStyle.Fill;

                tp.Controls.Add(dtView);
            }
        }

        private void DtView_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (m_data != null)
            {
                if (sender is DataGridView)
                {
                    DataGridView dtView = sender as DataGridView;
                    if (dtView != null)
                    {
                        if (dtView.Parent is TabPage)
                        {
                            string tabname = dtView.Parent.Tag.ToString();
                            if (m_data.ContainsKey(tabname))
                            {
                                DataGridViewRow row = dtView.Rows[e.RowIndex];
                                int id = 0;

                                PfVElementData pfvData = null;
                                if (Int32.TryParse(row.Cells["IdLinked"].Value.ToString(), out id))
                                {
                                    pfvData = m_data[tabname].FirstOrDefault<PfVElementData>(item => item.IdLinked == id);
                                    if (pfvData != null)
                                    {
                                        m_presenter.ZoomToPfV(pfvData);
                                        dtView.Refresh();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        IPresenter m_presenter = null;

        public void SetPresenter(IPresenter presenter)
        {
            m_presenter = presenter;
        }

        public DialogResult ShowPfvDlg()
        {
            return this.ShowDialog();
        }

        public void HideDlg()
        {
            this.Hide();
        }

        public void ShowDlg()
        {
            this.Show();
        }
    }
}
