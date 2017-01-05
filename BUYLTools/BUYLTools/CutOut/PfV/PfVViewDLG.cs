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
        //Dictionary<string, List<PfVElementData>> m_data;

        IPresenter m_presenter = null;

        public PfVViewDLG()
        {
            InitializeComponent();
        }

        void IPfVView.SetPresenter(IPresenter presenter)
        {
            m_presenter = presenter;
        }

        void IPfVView.HideDlg()
        {
            this.Hide();
        }

        void IPfVView.ShowDlg()
        {
            this.Show();
        }

        DialogResult IPfVView.ShowPfvDlg()
        {
            SetUpTabControl();
            return this.ShowDialog();
        }

        private void SetUpTabControl()
        {
            _tabControl.TabPages.Clear();

            int irun = 0;
            foreach (string key in m_presenter.CurrentModel.Keys)
            {
                if (m_presenter.CurrentModel[key] == null)
                    continue;

                if (irun == 0)

                    m_presenter.CurrentLinkSet(key);

                TabPage tp = new TabPage(Path.GetFileName(key));
                tp.Text = Path.GetFileName(key);
                tp.Tag = key;
                _tabControl.TabPages.Add(tp);

                DataGridView dtView = new DataGridView();
                dtView.RowHeaderMouseDoubleClick += _DtView_RowHeaderMouseDoubleClick;
                dtView.SelectionChanged += _DtView_SelectionChanged;
                dtView.DataSource = m_presenter.CurrentModel[key];
                dtView.Dock = DockStyle.Fill;

                tp.Controls.Add(dtView);
                _tabControl.SelectedIndexChanged += _tabControl_SelectedIndexChanged;
                _tabControl.Selected += _tabControl_Selected;
                irun++;
            }
        }

        private void SetCurrentSelectionInPresenter(object sender)
        {
            if (m_presenter.CurrentModel != null)
            {
                if (sender is DataGridView)
                {
                    DataGridView dtView = sender as DataGridView;
                    if (dtView != null)
                    {
                        m_currentView = dtView;

                        if (dtView.Parent is TabPage)
                        {
                            string tabname = dtView.Parent.Tag.ToString();
                            m_presenter.CurrentLinkSet(tabname);

                            if (m_presenter.CurrentModel.ContainsKey(tabname))
                            {
                                DataGridViewRow row = null;
                                if (dtView.SelectedRows != null && dtView.SelectedRows.Count > 0)
                                {
                                    row = dtView.SelectedRows[0];//.Rows[e.RowIndex];
                                }
                                else if (dtView.SelectedCells != null && dtView.SelectedCells.Count > 0)
                                {
                                    row = dtView.Rows[dtView.SelectedCells[0].RowIndex];
                                }

                                if (row != null)
                                {
                                    int id = 0;

                                    if (Int32.TryParse(row.Cells[PfVElementData.idLinkedColumn].Value.ToString(), out id))
                                    {
                                        m_presenter.CurrentPfVSet(tabname, id);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void _tabControl_Selected(object sender, TabControlEventArgs e)
        {
            if(sender is TabPage)
            {
                TabPage tp = sender as TabPage;
                m_presenter.CurrentLinkSet(tp.Tag.ToString());
            }
        }

        DataGridView m_currentView = null;

        private void _DtView_SelectionChanged(object sender, EventArgs e)
        {
            SetCurrentSelectionInPresenter(sender);

            PfVElementData pfvData = null;
            pfvData = m_presenter.CurrentPfVGet();
            if (pfvData != null)
            {
                _propertyGridPfV.SelectedObject = pfvData;
                _buttonPlacePfV.Enabled = true;
            }
            else
            {
                _propertyGridPfV.SelectedObject = null;
                _buttonPlacePfV.Enabled = false;
            }
        }
        private void _DtView_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            SetCurrentSelectionInPresenter(sender);

            if (sender is DataGridView)
            {
                DataGridView dtView = sender as DataGridView;
                if (dtView != null)
                {
                    PfVElementData pfvData = null;
                    pfvData = m_presenter.CurrentPfVGet();
                    if (pfvData != null)
                    {
                        m_presenter.PfVZoomToDummy();
                        dtView.Refresh();
                    }
                }
            }
        }

        private void _tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            _propertyGridPfV.SelectedObject = null;
        }

        private void _buttonPlacePfV_Click(object sender, EventArgs e)
        {
            //SetCurrentSelectionInPresenter(sender);
            m_presenter.PfVPlaceCurrent();

            if(m_currentView != null)
                m_currentView.Refresh();
        }
    }
}
