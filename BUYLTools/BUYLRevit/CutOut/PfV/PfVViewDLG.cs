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
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BUYLRevit.CutOut.PfV
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
            if(m_data != null)
            {
                if(sender is DataGridView)
                {
                    DataGridView dtView = sender as DataGridView;
                    if(dtView != null)
                    {
                        if(dtView.Parent is TabPage)
                        {
                            if(m_data.ContainsKey(dtView.Parent.Tag.ToString()))
                            {
                                DataGridViewRow row = dtView.Rows[e.RowIndex];
                                int id = 0;

                                PfVElementData pfvData = null;
                                if (Int32.TryParse(row.Cells["IdLinked"].Value.ToString(), out id))
                                {
                                    pfvData = m_data[dtView.Parent.Tag.ToString()].FirstOrDefault<PfVElementData>(item => item.IdLinked == id);
                                    if(pfvData != null)
                                    {
                                        Transaction t = new Transaction(GetUIDocumentFromRevit().Application.ActiveUIDocument.Document);
                                        t.Start("Change to 3D view");
                                        try
                                        {
                                            if(GetUIDocumentFromRevit().ActiveView.ViewType == ViewType.ThreeD)
                                            {
                                                View3D view = (View3D)GetUIDocumentFromRevit().ActiveView;

                                                XYZ up = null;
                                                XYZ target = new XYZ(pfvData.Location.X, pfvData.Location.Y, pfvData.Location.Z);
                                                Transform tr = Transform.CreateRotation(new XYZ(0, 0, 1), Utils.MathUtils.DegreeToRad(90));
                                                XYZ targetN = tr.OfVector(target);

                                                up = target.CrossProduct(targetN); // new XYZ(0, 0, pfvData.Pos.Z)
                                                XYZ targetM = target.Multiply(Utils.MathUtils.MMToFeet(3000) * -1);
                                                view.SetOrientation(new ViewOrientation3D(targetM, up, target));

                                                //view.get_Parameter(BuiltInParameter.VIEW_DETAIL_LEVEL).Set(3);

                                                //view.get_Parameter(BuiltInParameter
                                                //  .MODEL_GRAPHICS_STYLE).Set(6);

                                                t.Commit();

                                                GetUIDocumentFromRevit().Application.ActiveUIDocument.Document.Regenerate();
                                                //GetUIDocumentFromRevit().ActiveViewShowElements(new ElementId(pfvData.IdLinked)); //GetDocumentFromRevit(dtView.Parent.Tag.ToString());
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            t.RollBack();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private Document GetDocumentFromRevit(string docname)
        {
            Document res = null;

            if (m_commandData != null)
            {
                foreach (Document doc in m_commandData.Application.Application.Documents)
                {
                    if (doc.PathName == docname)
                    {
                        res = doc;
                        break;
                    }
                }
            }

            return res;
        }

        private UIDocument GetUIDocumentFromRevit()
        {
            UIDocument res = m_commandData.Application.ActiveUIDocument;

            return res;
        }

        ExternalCommandData m_commandData = null;

        internal void SetCommandData(ExternalCommandData commandData)
        {
            m_commandData = commandData;
        }
    }
}
