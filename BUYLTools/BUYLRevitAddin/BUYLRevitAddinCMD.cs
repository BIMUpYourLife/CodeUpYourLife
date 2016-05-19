using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;

namespace BUYLRevitAddin
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class BUYLRevitAddinContentRepLoader : IExternalCommand
    {
        BackgroundWorker m_bw = null;
        ExternalCommandData m_commData = null;
        const string m_BUYL = "BIMUpYourLife Content";

        public BUYLRevitAddinContentRepLoader()
        {
            m_bw = new BackgroundWorker();
            m_bw.DoWork += Bw_DoWork;
            m_bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                System.Windows.Forms.MessageBox.Show("Repository update completed", m_BUYL, System.Windows.Forms.MessageBoxButtons.OK);
            }
            else
            {
                
            }
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("The repository will be downloaded in background. You'll get a message, once it is completed!", m_BUYL, System.Windows.Forms.MessageBoxButtons.OK);

            BUYLTools.ContentLoader.GitContentLoader.BUYLCloneOrUpdateRepository();
            Loader.ContentLoader.BUYLSetContentLibraryPath(m_commData.Application.Application);
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_commData = commandData;

            Result res = Result.Failed;

            try
            {
                // Start the download operation in the background.
                this.m_bw.RunWorkerAsync();
                res = Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                TaskDialog.Show("BIMUpYourLife Content Loader error", message);
                res = Result.Failed;
            }

            return res;
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class BUYLRevitAddinStartDECHProject : IExternalCommand
    {
        ExternalCommandData m_commData = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_commData = commandData;

            Result res = Result.Failed;

            try
            {
                Loader.ContentLoader.BUYLStartNewDECHProject(m_commData.Application);
                res = Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("BIMUpYourLife project start error", ex.Message);
                res = Result.Failed;
            }

            return res;
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class BUYLRevitAddinStartDEDEProject : IExternalCommand
    {
        ExternalCommandData m_commData = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_commData = commandData;

            Result res = Result.Failed;

            try
            {
                Loader.ContentLoader.BUYLStartNewDEDEProject(m_commData.Application);
                res = Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("BIMUpYourLife project start error", ex.Message);
                res = Result.Failed;
            }

            return res;
        }
    }
    //// Let's control with this utility class when our command can 
    //// be executed. This requires this option to be added to the addin
    //// manifest file
    ////  <AvailabilityClassName>
    ////    ADNPlugin.Revit.RoomRenumbering.CommandAvailability
    ////  </AvailabilityClassName>
    //// In the addin manifest file we are also checking that our command 
    //// cannot be used in a family document and when no document is 
    //// active using the two below options:
    ////  <VisibilityMode>NotVisibleInFamily</VisibilityMode>
    ////  <VisibilityMode>NotVisibleWhenNoActiveDocument</VisibilityMode>
    //public class CommandAvailability : IExternalCommandAvailability
    //{
    //    public bool IsCommandAvailable(
    //      UIApplication applicationData,
    //      CategorySet selectedCategories
    //    )
    //    {
    //        UIDocument uiDoc = applicationData.ActiveUIDocument;

    //        switch (uiDoc.Document.ActiveView.ViewType)
    //        {
    //            case ViewType.AreaPlan:
    //            case ViewType.CeilingPlan:
    //            case ViewType.EngineeringPlan:
    //            case ViewType.FloorPlan:
    //                return true;
    //        }

    //        return false;
    //    }
    //}
}
