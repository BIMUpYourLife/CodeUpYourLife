﻿using System;
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
using BUYLTools.CutOut.PfV;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Media;

namespace BUYLRevitAddin
{
    #region Content and templates
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
            BUYLRevit.Loader.ContentLoader.BUYLSetContentLibraryPath(m_commData.Application.Application);
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
                BUYLRevit.Loader.ContentLoader.BUYLStartNewProject(m_commData.Application, BUYLTools.Utils.Countries.DECH);
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
                BUYLRevit.Loader.ContentLoader.BUYLStartNewProject(m_commData.Application, BUYLTools.Utils.Countries.DEDE);
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

    #endregion

    #region pfv
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class BUYLRevitAddinPfVApp : BaseCommand, IExternalApplication
    {
        #region IExternalApplication Member
        public Result OnShutdown(UIControlledApplication application)
        {
            Result res = Result.Succeeded;

            return res;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            AddRibbonButtonsAndTexts(application);

            application.ViewActivated += Application_ViewActivated;
            return Result.Succeeded;
        }

        private void Application_ViewActivated(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs e)
        {
            if (e.Document.PathName != BUYLRevitAddinGlobalPfV.Presenter.CurrentHostDocument)
                BUYLRevitAddinGlobalPfV.Presenter.CurrentHostDocument = e.Document.PathName;
        }

        #endregion

        private void AddRibbonButtonsAndTexts(UIControlledApplication application)
        {
            try
            {
                RibbonPanel panel = application.CreateRibbonPanel("PfV Manager");

                PushButtonData itemDataProcess = new PushButtonData("Process", "Update", AssemblyFullName, "BUYLRevitAddin.BUYLRevitAddinProcessPfV");
                //itemData1.Text = ;
                PushButton itemProcess = panel.AddItem(itemDataProcess) as PushButton;
                itemProcess.ToolTip = "Processes the PfV elements";
                itemProcess.Image = new BitmapImage(new Uri(Path.Combine( AssemblyPath, "Resources\\PfVProcess.bmp"), UriKind.Absolute));
                itemProcess.LargeImage = new BitmapImage(new Uri(Path.Combine( AssemblyPath, "Resources\\PfVProcess.bmp"), UriKind.Absolute));

                PushButtonData itemDataPrev = new PushButtonData("Previous", "Previous", AssemblyFullName, "BUYLRevitAddin.BUYLRevitAddinPreviousPfV");
                //itemData1.Text = ;
                PushButton itemPrev = panel.AddItem(itemDataPrev) as PushButton;
                itemPrev.ToolTip = "Previous pfv";
                itemPrev.Image = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVPrevious.bmp"), UriKind.Absolute));
                itemPrev.LargeImage = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVPrevious.bmp"), UriKind.Absolute));

                PushButtonData itemDataNext = new PushButtonData("Next", "Next", AssemblyFullName, "BUYLRevitAddin.BUYLRevitAddinNextPfV");
                //itemData1.Text = ;
                PushButton itemNext = panel.AddItem(itemDataNext) as PushButton;
                itemNext.ToolTip = "Next pfv";
                itemNext.Image = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVNext.bmp"), UriKind.Absolute));
                itemNext.LargeImage = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVNext.bmp"), UriKind.Absolute));

                PushButtonData itemDataDlg = new PushButtonData("Manager", "Manager", AssemblyFullName, "BUYLRevitAddin.BUYLRevitAddinManagePfV");
                //itemData1.Text = ;
                PushButton itemDlg = panel.AddItem(itemDataDlg) as PushButton;
                itemDlg.ToolTip = "Shows the manager for pfv elements";
                itemDlg.Image = new BitmapImage(new Uri(Path.Combine( AssemblyPath, "Resources\\PfVManager.bmp"), UriKind.Absolute));
                itemDlg.LargeImage = new BitmapImage(new Uri(Path.Combine( AssemblyPath, "Resources\\PfVManager.bmp"), UriKind.Absolute));
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class BUYLRevitAddinProcessPfV : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return BUYLRevitAddinGlobalPfV.Presenter.ProcessPfVs(commandData, ref message, elements);
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class BUYLRevitAddinPreviousPfV : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return BUYLRevitAddinGlobalPfV.Presenter.PfVPrevious(commandData, ref message, elements);
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class BUYLRevitAddinNextPfV : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return BUYLRevitAddinGlobalPfV.Presenter.PfVNext(commandData, ref message, elements);
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class BUYLRevitAddinManagePfV : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return BUYLRevitAddinGlobalPfV.Presenter.ShowManager(commandData, ref message, elements);
        }
    }

    internal static class BUYLRevitAddinGlobalPfV
    {
        private static string m_currentDocPath;
        private static BUYLRevit.CutOut.PfV.PfVPresenter m_pfvpresenter = null;
        private static IPfVView m_dlg = null;

        internal static BUYLRevit.CutOut.PfV.PfVPresenter Presenter
        {
            get
            {
                if (m_pfvpresenter == null)
                {
                    m_pfvpresenter = new BUYLRevit.CutOut.PfV.PfVPresenter();
                    SetupView();
                }
                return m_pfvpresenter;
            }
        }

        private static void SetupView()
        {
            if(m_dlg == null)
                m_dlg = new PfVViewDLG();

            Presenter.ConnectView(m_dlg);
        }
    }
    #endregion

    #region manufacturer
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class BUYLRevitAddinApplyManufacturer : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return BUYLRevit.CcTools.CcTools.StartApplyManufacturerProcess(commandData, ref message, elements);
        }
    }

    #endregion
    public class BaseCommand
    {
        public string GetRootPackagePath(string assemblyPath)
        {
            return new DirectoryInfo(assemblyPath).Parent.FullName;
        }

        public string AssemblyPath
        {
            get
            {
                return Path.GetDirectoryName(AssemblyFullName);
            }
        }

        public string AssemblyFullName
        {
            get
            {
                return this.GetType().Assembly.Location; //.FullName; //Assembly.GetExecutingAssembly().Location;
            }
        }

        public System.Windows.Media.ImageSource BmpImageSource(string embeddedPath)
        {
            Stream stream = this.GetType().Assembly.GetManifestResourceStream(embeddedPath);
            var decoder = new System.Windows.Media.Imaging.BmpBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            return decoder.Frames[0];
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
