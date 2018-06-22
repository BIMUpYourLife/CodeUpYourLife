using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Forms;

namespace BUYLRevitAddin
{
    #region Application
    public class BUYLRevitAddin : IExternalApplication
    {
        BaseCommand baseCmd = new BaseCommand();

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            CreateRibbonPanel(application);
            return Result.Succeeded;
        }

        private RibbonPanel CreateRibbonPanel(UIControlledApplication _cachedUiCtrApp)
        {
            RibbonPanel panel = _cachedUiCtrApp.CreateRibbonPanel("BUYL");
            AddRibbonButtonsAndTexts(panel);

            return panel;
        }

        private void AddRibbonButtonsAndTexts(RibbonPanel panel)
        {
            try
            {
                PushButtonData downloadButtonData = new PushButtonData("Download", "Download", baseCmd.AssemblyFullName, "BUYLRevitAddin.BUYLLoadGithubRepo");
                PushButton downloadButton = panel.AddItem(downloadButtonData) as PushButton;
                downloadButton.ToolTip = "BUYL - Download BUYL content";
                //Later will set an icon
                //lastEditBtn.Image = new BitmapImage(new Uri(Path.Combine(GetContenttDirectory(), "LastEdit.png"), UriKind.Absolute));
                //lastEditBtn.LargeImage = new BitmapImage(new Uri(Path.Combine(GetContenttDirectory(), "LastEdit.png"), UriKind.Absolute));

                PushButtonData dechButtonData = new PushButtonData("Start DECH", "Start DECH", baseCmd.AssemblyFullName, "BUYLRevitAddin.BUYLRevitAddinStartDECHProject");
                PushButton dechButton = panel.AddItem(dechButtonData) as PushButton;
                dechButton.ToolTip = "BUYL - Start DECH project";
                //Later will set an icon
                //lastEditBtn.Image = new BitmapImage(new Uri(Path.Combine(GetContenttDirectory(), "LastEdit.png"), UriKind.Absolute));
                //lastEditBtn.LargeImage = new BitmapImage(new Uri(Path.Combine(GetContenttDirectory(), "LastEdit.png"), UriKind.Absolute));

                PushButtonData dedeButtonData = new PushButtonData("Start DEDE", "Start DEDE", baseCmd.AssemblyFullName, "BUYLRevitAddin.BUYLRevitAddinStartDEDEProject");
                PushButton dedeButton = panel.AddItem(dedeButtonData) as PushButton;
                dedeButton.ToolTip = "BUYL - Start DEDE project";
                //Later will set an icon
                //lastEditBtn.Image = new BitmapImage(new Uri(Path.Combine(GetContenttDirectory(), "LastEdit.png"), UriKind.Absolute));
                //lastEditBtn.LargeImage = new BitmapImage(new Uri(Path.Combine(GetContenttDirectory(), "LastEdit.png"), UriKind.Absolute));

                PushButtonData manufacturerButtonData = new PushButtonData("Apply manufacturer", "Apply manufacturer", baseCmd.AssemblyFullName, "BUYLRevitAddin.BUYLRevitAddinApplyManufacturer");
                PushButton manufacturerButton = panel.AddItem(manufacturerButtonData) as PushButton;
                manufacturerButton.ToolTip = "BUYL - Apply Manufacturer";
                //Later will set an icon
                //lastEditBtn.Image = new BitmapImage(new Uri(Path.Combine(GetContenttDirectory(), "LastEdit.png"), UriKind.Absolute));
                //lastEditBtn.LargeImage = new BitmapImage(new Uri(Path.Combine(GetContenttDirectory(), "LastEdit.png"), UriKind.Absolute));
                
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Failure", ex.InnerException.ToString());
            }
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    public class BUYLLoadGithubRepo : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application.Run(new WindowsFormsApp1.Form1());
            return Result.Succeeded;
            
        }
    }

    #endregion

    #region Content and template commands
    /*
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
    */
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
    /*
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class BUYLRevitAddinPfVApp : BaseCommand, IExternalApplication
    {
        #region IExternalApplication Member
        public Result OnShutdown(UIControlledApplication application)
        {
            Result res = Result.Succeeded;

            application.ViewActivated -= Application_ViewActivated;
            application.ViewActivating -= Application_ViewActivating;

            return res;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            AddRibbonButtonsAndTexts(application);

            application.ViewActivated += Application_ViewActivated;
            application.ViewActivating += Application_ViewActivating;

            application.ControlledApplication.DocumentOpened += ControlledApplication_DocumentOpened;
            //application.ControlledApplication.DocumentChanged += ControlledApplication_DocumentChanged;
            application.ControlledApplication.DocumentClosing += ControlledApplication_DocumentClosing;
            application.ControlledApplication.DocumentSaved += ControlledApplication_DocumentSaved;
            application.ControlledApplication.DocumentSavedAs += ControlledApplication_DocumentSavedAs;

            return Result.Succeeded;
        }

        private void ControlledApplication_DocumentSavedAs(object sender, Autodesk.Revit.DB.Events.DocumentSavedAsEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ControlledApplication_DocumentSaved(object sender, Autodesk.Revit.DB.Events.DocumentSavedEventArgs e)
        {
            SaveCurrentModel(e.Document.PathName);
        }

        private void ControlledApplication_DocumentClosing(object sender, Autodesk.Revit.DB.Events.DocumentClosingEventArgs e)
        {
            //SaveCurrentModel(e.Document.PathName);
        }

        private void ControlledApplication_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {
            CheckCurrentPfVModel(e.Document.PathName);
        }

        private void ControlledApplication_DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs e)
        {
            //CheckCurrentPfVModel(e.GetDocument().PathName);
        }

        private void Application_ViewActivating(object sender, Autodesk.Revit.UI.Events.ViewActivatingEventArgs e)
        {
            //CheckCurrentPfVModel(e.Document.PathName);
        }

        private void Application_ViewActivated(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs e)
        {
            //CheckCurrentPfVModel(e.Document.PathName);
        }

        void CheckCurrentPfVModel(string pathname)
        {
            if (pathname != BUYLRevitAddinGlobalPfV.Presenter.CurrentHostDocument)
                BUYLRevitAddinGlobalPfV.Presenter.CurrentHostDocument = pathname;
        }

        void SaveCurrentModel(string pathname)
        {
            if (pathname == BUYLRevitAddinGlobalPfV.Presenter.CurrentHostDocument)
                BUYLRevitAddinGlobalPfV.Presenter.SaveCurrentModel();
        }
        #endregion

        private void AddRibbonButtonsAndTexts(UIControlledApplication application)
        {
            try
            {
                RibbonPanel panel = application.CreateRibbonPanel("PfV Manager");

                // Process button
                PushButtonData itemDataProcess = new PushButtonData("Process", "Update", AssemblyFullName, "BUYLRevitAddin.BUYLRevitAddinProcessPfV");
                PushButton itemProcess = panel.AddItem(itemDataProcess) as PushButton;
                itemProcess.ToolTip = "Processes the PfV elements";
                itemProcess.Image = new BitmapImage(new Uri(Path.Combine( AssemblyPath, "Resources\\PfVProcess.bmp"), UriKind.Absolute));
                itemProcess.LargeImage = new BitmapImage(new Uri(Path.Combine( AssemblyPath, "Resources\\PfVProcess.bmp"), UriKind.Absolute));

                // locate button
                PushButtonData itemDataLocate = new PushButtonData("Locate", "Locate", AssemblyFullName, "BUYLRevitAddin.BUYLRevitAddinLocatePfV");
                PushButton itemLocate = panel.AddItem(itemDataLocate) as PushButton;
                itemLocate.ToolTip = "Locate pfv";
                itemLocate.Image = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVLocate.bmp"), UriKind.Absolute));
                itemLocate.LargeImage = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVLocate.bmp"), UriKind.Absolute));

                // connect button
                PushButtonData itemDataConnect = new PushButtonData("Connect", "Connect", AssemblyFullName, "BUYLRevitAddin.BUYLRevitAddinConnectPfV");
                PushButton itemConnect = panel.AddItem(itemDataConnect) as PushButton;
                itemConnect.ToolTip = "Connect pfv";
                itemConnect.Image = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVConnect.bmp"), UriKind.Absolute));
                itemConnect.LargeImage = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVConnect.bmp"), UriKind.Absolute));

                // previous button
                PushButtonData itemDataPrev = new PushButtonData("Previous", "Previous", AssemblyFullName, "BUYLRevitAddin.BUYLRevitAddinPreviousPfV");
                //itemData1.Text = ;
                PushButton itemPrev = panel.AddItem(itemDataPrev) as PushButton;
                itemPrev.ToolTip = "Previous pfv";
                itemPrev.Image = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVPrevious.bmp"), UriKind.Absolute));
                itemPrev.LargeImage = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVPrevious.bmp"), UriKind.Absolute));

                // next button
                PushButtonData itemDataNext = new PushButtonData("Next", "Next", AssemblyFullName, "BUYLRevitAddin.BUYLRevitAddinNextPfV");
                //itemData1.Text = ;
                PushButton itemNext = panel.AddItem(itemDataNext) as PushButton;
                itemNext.ToolTip = "Next pfv";
                itemNext.Image = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVNext.bmp"), UriKind.Absolute));
                itemNext.LargeImage = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVNext.bmp"), UriKind.Absolute));

                // place button
                PushButtonData itemDataPlace = new PushButtonData("Place", "Place", AssemblyFullName, "BUYLRevitAddin.BUYLRevitAddinPlacePfV");
                //itemData1.Text = ;
                PushButton itemPlace = panel.AddItem(itemDataPlace) as PushButton;
                itemPlace.ToolTip = "Place pfv";
                itemPlace.Image = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVPlace.bmp"), UriKind.Absolute));
                itemPlace.LargeImage = new BitmapImage(new Uri(Path.Combine(AssemblyPath, "Resources\\PfVPlace.bmp"), UriKind.Absolute));

                // manager button
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
    public class BUYLRevitAddinLocatePfV : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return BUYLRevitAddinGlobalPfV.Presenter.PfVZoomToCurrent(commandData, ref message, elements);
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class BUYLRevitAddinConnectPfV : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return BUYLRevitAddinGlobalPfV.Presenter.PfVConnectExitingElementToCurrent(commandData, ref message, elements);
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
    public class BUYLRevitAddinPlacePfV : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return BUYLRevitAddinGlobalPfV.Presenter.PfVPlaceCurrent(commandData, ref message, elements);
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
    */
    #endregion

    #region manufacturer
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class BUYLRevitAddinApplyManufacturer : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Result.Succeeded; //BUYLRevit.CcTools.CcTools.StartApplyManufacturerProcess(commandData, ref message, elements);
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
