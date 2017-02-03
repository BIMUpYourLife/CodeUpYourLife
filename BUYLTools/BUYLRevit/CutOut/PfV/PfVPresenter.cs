#region Namespaces

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using BUYLRevit.Utils;
using BUYLTools.CutOut.PfV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

#endregion Namespaces

namespace BUYLRevit.CutOut.PfV
{
    public class CopyUseDestination : IDuplicateTypeNamesHandler
    {
        public DuplicateTypeAction OnDuplicateTypeNamesFound(
        DuplicateTypeNamesHandlerArgs args)
        {
            return DuplicateTypeAction.UseDestinationTypes;
        }
    }

    public class PfVPresenter : IPresenter
    {
        private const string m_pfvtype = "PfVType";
        private const string m_pfvTypeDirectShape = "DirectShape";
        private const string m_pfvTypeInstance = "Instance";

        //family settings in config file
        private const string m_pfvDummyFamily = "PfVDummyMarker";

        private const string m_pfvWallCutoutRe = "PfVWallCutoutRE";
        private const string m_pfvWallCutoutRd = "PfVWallCutoutRD";
        private const string m_pfvFloorCutoutRe = "PfVFloorCutoutRE";
        private const string m_pfvFloorCutoutRd = "PfVFloorCutoutRD";
        private const string m_pfvOtherCutoutRe = "PfVOtherRE";
        private const string m_pfvOtherCutoutRd = "PfVOtherRD";

        private BUYLTools.Configuration.Manager m_configManager = null;
        internal static Autodesk.Revit.ApplicationServices.Application m_app = null;
        internal static DocumentSet m_docs = null;
        private IPfVView m_view = null;
        private static ExternalCommandData m_cmdData = null;
        private static Document m_hostDoc;
        private static string m_message = null;
        private static IPfVModel m_model;
        private static string m_currentHostDoc;
        private static double m_offsetMinus = -2;
        private static double m_offsetPlus = 2;
        private PfVElementData m_actualPfV = null;
        private string m_actualLink = "";

        private const string m_pfvBoundingAddonSize = "SectionBoxAddonSize";
        private const string m_pfvWidth = "PfVWidth";
        private const string m_pfvHeight = "PfVHeight";
        private const string m_pfvDepth = "PfVDepth";
        private const string m_pfvDiameter = "PfVDiameter";

        public PfVModelData CurrentModel
        {
            get
            {
                return m_model.ActualModel;
            }
        }

        public string CurrentHostDocument
        {
            get { return m_currentHostDoc; }

            set
            {
                if (value != m_currentHostDoc)
                {
                    if (!String.IsNullOrEmpty(m_currentHostDoc))
                        m_model.ModelSave(m_currentHostDoc);

                    m_model.ModelLoad(value);
                    m_currentHostDoc = value;
                }
            }
        }

        #region UICommands

        public Result ProcessPfVs(ExternalCommandData commandData, ref string message, ElementSet highlightElements)
        {
            Result res = Result.Failed;

            try
            {
                m_cmdData = commandData;
                m_message = message;
                m_hostDoc = GetUIDocumentFromRevit().Document;

                if (m_model == null)
                    m_model = new PfVModel();

                m_model.ModelLoad(GetHostModelDocumentPath());

                m_model.UpdateModel(PfVsCollect(), GetHostModelDocumentPath());

                if (m_model.ActualModel.Keys.Count > 0)
                {
                    foreach (string item in m_model.ActualModel.Keys)
                    {
                        m_actualLink = item;
                        break;
                    }
                }

                res = Result.Succeeded;
            }
            catch (Exception ex)
            {
            }

            return res;
        }

        public Result PfVZoomToCurrent(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result res = Result.Failed;

            try
            {
                m_cmdData = commandData;
                m_message = message;
                m_hostDoc = GetUIDocumentFromRevit().Document;

                PfVZoomToDummy();
            }
            catch (Exception ex)
            {
            }

            return res;
        }

        public Result PfVConnectExitingElementToCurrent(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result res = Result.Failed;

            try
            {
                m_cmdData = commandData;
                m_message = message;
                m_hostDoc = GetUIDocumentFromRevit().Document;

                PfVConnectCurrent();
            }
            catch (Exception ex)
            {
            }

            return res;
        }

        public Result PfVPrevious(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result res = Result.Succeeded;

            m_cmdData = commandData;
            m_message = message;
            m_hostDoc = GetUIDocumentFromRevit().Document;

            PfVPrevious();

            return res;
        }

        public Result PfVNext(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result res = Result.Succeeded;

            m_cmdData = commandData;
            m_message = message;
            m_hostDoc = GetUIDocumentFromRevit().Document;

            PfVNext();

            return res;
        }

        public Result PfVPlaceCurrent(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result res = Result.Succeeded;

            m_cmdData = commandData;
            m_message = message;
            m_hostDoc = GetUIDocumentFromRevit().Document;

            PfVPlaceCurrent();

            return res;
        }

        public Result ShowManager(ExternalCommandData commandData, ref string message, ElementSet highlightElements)
        {
            Result res = Result.Failed;

            m_cmdData = commandData;
            m_message = message;
            m_hostDoc = GetUIDocumentFromRevit().Document;

            try
            {
                if (m_model.ActualModel != null)
                {
                    if (m_view.ShowPfvDlg() == DialogResult.OK)
                    {
                    }
                }
                res = Result.Succeeded;
            }
            catch (Exception ex)
            {
            }

            return res;
        }

        #endregion UICommands

        public void SaveCurrentModel()
        {
            m_model.ModelSave(m_currentHostDoc);
        }

        public void PfVPrevious()
        {
            PfVElementData temp = CurrentPfVGet();
            int index = 0;
            if (temp != null)
            {
                index = m_model.ActualModel[m_actualLink].IndexOf(CurrentPfVGet());

                if (index > 0)
                {
                    PfVElementData dt = m_model.ActualModel[m_actualLink][index - 1];
                    CurrentPfVSet(m_actualLink, dt.UniqueIdLinked);
                }
                else
                {
                    int counter = m_model.ActualModel[m_actualLink].Count;
                    PfVElementData dt = m_model.ActualModel[m_actualLink][counter - 1];
                    CurrentPfVSet(m_actualLink, dt.UniqueIdLinked);
                }

                PfVZoomToDummy();
            }
        }

        public void PfVNext()
        {
            PfVElementData temp = CurrentPfVGet();
            int index = 0;
            if (temp != null)
            {
                index = m_model.ActualModel[m_actualLink].IndexOf(CurrentPfVGet());

                if (index < m_model.ActualModel[m_actualLink].Count - 1)
                {
                    PfVElementData dt = m_model.ActualModel[m_actualLink][index + 1];
                    CurrentPfVSet(m_actualLink, dt.UniqueIdLinked);
                }
                else
                {
                    PfVElementData dt = m_model.ActualModel[m_actualLink][0];
                    CurrentPfVSet(m_actualLink, dt.UniqueIdLinked);
                }

                PfVZoomToDummy();
            }
        }

        public void PfVPlaceCurrent()
        {
            PfVElementData pfvData = CurrentPfVGet();
            if (pfvData != null)
            {
                Transaction t = new Transaction(m_hostDoc);
                t.Start("Change to 3D view");
                try
                {
                    if (GetUIDocumentFromRevit().ActiveView.ViewType == ViewType.ThreeD)
                    {
                        if (pfvData.IsWallPfV(GetHostCategoryName(pfvData)) ||
                            pfvData.IsFloorPfV(GetHostCategoryName(pfvData)))
                        {
                            PfVPlaceOnHost(pfvData);
                        }
                        else
                        {
                            MessageBox.Show(String.Format("Placing of cutout for current host {0} not implemented!", GetHostCategoryName(pfvData)));
                        }

                        //ZoomToPfV(pfvData);
                    }
                    else
                    {
                        MessageBox.Show("Please use a 3D view to place the cutouts!");
                    }

                    //m_model.ModelSave(m_currentHostDoc);
                    t.Commit();
                }
                catch (System.Exception ex)
                {
                    t.RollBack();
                }
            }
        }

        private string GetHostCategoryName(PfVElementData pfvData)
        {
            string result = "";
            Element el = m_hostDoc.GetElement(pfvData.UniqueIdHost);
            if (el != null)
            {
                if (!String.IsNullOrEmpty(el.Category.Name))
                    result = el.Category.Name;
            }

            return result;
        }

        private void PfVConnectCurrent()
        {
            PfVElementData pfvData = CurrentPfVGet();
            if (pfvData != null)
            {
                Transaction t = new Transaction(m_hostDoc);
                t.Start("Connect to existing pfv element  in model");
                try
                {
                    Reference reference = GetUIDocumentFromRevit().Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Select the already placed PFV familyinstance!");
                    if (reference != null)
                    {
                        Element el = GetUIDocumentFromRevit().Document.GetElement(reference.ElementId);
                        if (el != null)
                        {
                            pfvData.UniqueIdLocal = el.UniqueId;
                            pfvData.PfVStatus = Status.Ok;
                        }
                    }

                    t.Commit();
                }
                catch (System.Exception ex)
                {
                    t.RollBack();
                }
            }
        }

        public void PfVZoomToDummy()
        {
            PfVElementData pfvData = CurrentPfVGet();
            if (pfvData != null)
            {
                //using (Transaction t = new Transaction(m_hostDoc))
                //{
                //    t.Start("Remove Section Box");
                //    BoundingBox3DDelete();
                //    t.Commit();
                //}

                using (Transaction t = new Transaction(m_hostDoc))
                {
                    t.Start("Change to 3D view");
                    try
                    {
                        if (GetUIDocumentFromRevit().ActiveView.ViewType == ViewType.ThreeD ||
                            GetUIDocumentFromRevit().ActiveView.ViewType == ViewType.FloorPlan)
                        {
                            PfVPlaceDummyAndZoom(pfvData);
                        }
                        else
                        {
                        }

                        t.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        t.RollBack();
                    }
                }
            }
        }

        private void PfVPlaceDummyAndZoom(PfVElementData pfvData)
        {
            if (pfvData == null)
                return;

            FamilyInstance inst = null;
            if (String.IsNullOrEmpty(pfvData.UniqueIdLocal) && pfvData.PfVStatus == Status.New)
            {
                // Retrieve the family if it is already present:
                string familyName = MyConfigManager.GetValueForAppsetting(m_pfvDummyFamily);

                FamilySymbol symbol = GetFirstSymbolFromFamily(familyName);

                // Place the family symbol:

                if (symbol != null)
                {
                    Element el = m_hostDoc.GetElement(pfvData.UniqueIdHost);
                    if (el != null)
                    {
                        inst = PlaceInstance(pfvData, el.LevelId, symbol);

                        if (inst != null)
                        {
                            pfvData.UniqueIdLocal = inst.UniqueId;
                            pfvData.PfVStatus = Status.Dummy;
                        }
                    }
                }
            }
            else
                inst = m_hostDoc.GetElement(pfvData.UniqueIdLocal) as FamilyInstance;

            if (inst != null)
            {
                if (pfvData.PfVStatus == Status.Ok || pfvData.PfVStatus == Status.Dummy)
                {
                    if (GetUIDocumentFromRevit().ActiveView.ViewType == ViewType.ThreeD)
                    {
                        View3D view3d = GetUIDocumentFromRevit().ActiveView as View3D;
                        if (view3d != null)
                        {
                            GetUIDocumentFromRevit().Application.DialogBoxShowing += Application_DialogBoxShowing;

                            BoundingBox3DDelete();

                            GetUIDocumentFromRevit().ShowElements(inst.Id);

                            BoundingBox3DCreate(pfvData);

                            GetUIDocumentFromRevit().Application.DialogBoxShowing -= Application_DialogBoxShowing;
                        }
                    }
                    else if (GetUIDocumentFromRevit().ActiveView.ViewType == ViewType.FloorPlan)
                    {
                        GetUIDocumentFromRevit().ShowElements(inst.Id);
                    }

                    if (pfvData.PfVStatus == Status.Dummy)
                    {
                        m_hostDoc.Delete2(inst);
                        pfvData.UniqueIdLocal = "";
                        pfvData.PfVStatus = Status.New;
                    }
                }
            }
        }
        private void BoundingBox3DCreate(PfVElementData pfvData)
        {
            View3D view3d = GetUIDocumentFromRevit().ActiveView as View3D;
            if (view3d != null)
            {
                XYZ min = new XYZ(ApplyNegativeOffset(pfvData.Location.X, pfvData.GetHorizontalExtend()), ApplyNegativeOffset(pfvData.Location.Y, pfvData.GetHorizontalExtend()), ApplyNegativeOffset(pfvData.Location.Z, pfvData.GetVerticalExtend()));
                XYZ max = new XYZ(ApplyPositiveOffset(pfvData.Location.X, pfvData.GetHorizontalExtend()), ApplyPositiveOffset(pfvData.Location.Y, pfvData.GetHorizontalExtend()), ApplyPositiveOffset(pfvData.Location.Z, pfvData.GetVerticalExtend()));
                BoundingBoxXYZ bound = new BoundingBoxXYZ() { Min = min, Max = max };
                view3d.SetSectionBox(bound);

                foreach(UIView uiview in GetUIDocumentFromRevit().GetOpenUIViews())
                {
                    if (uiview.ViewId == view3d.Id)
                    {
                        uiview.ZoomAndCenterRectangle(min, max); //.ZoomToFit();
                        uiview.Zoom(0.5);
                        break;
                    }
                }
            }
        }

        private void BoundingBox3DDelete()
        {

            View3D view3d = GetUIDocumentFromRevit().ActiveView as View3D;
            if (view3d != null)
            {
                using (SubTransaction subt = new SubTransaction(GetUIDocumentFromRevit().Document))
                {
                    subt.Start();

                    if (view3d.IsSectionBoxActive)
                        view3d.IsSectionBoxActive = false;

                    GetUIDocumentFromRevit().RefreshActiveView();

                    subt.Commit();
                }
            }
        }

        private void Application_DialogBoxShowing(object sender, Autodesk.Revit.UI.Events.DialogBoxShowingEventArgs e)
        {
            // DialogBoxShowingEventArgs has two subclasses - TaskDialogShowingEventArgs & MessageBoxShowingEventArgs
            // In this case we are interested in this event if it is TaskDialog being shown. 
            TaskDialogShowingEventArgs t = e as TaskDialogShowingEventArgs;
            if (t != null)
            {
                // Call OverrideResult to cause the dialog to be dismissed with the specified return value
                // (int) is used to convert the enum TaskDialogResult.No to its integer value which is the data type required by OverrideResult
                e.OverrideResult((int)TaskDialogResult.No);
            }
        }

        private double GetOffsetValueFromConfig()
        {
            string bsize = MyConfigManager.GetValueForAppsetting(m_pfvBoundingAddonSize);
            double offset = 0;

            if (Double.TryParse(bsize, out offset))
            {
                offset = BUYLRevit.Utils.MathUtils.MeterToFeet(offset);
            }

            return offset;
        }

        private double ApplyNegativeOffset(double basevalue, double dimension)
        {
            double temp = 0;

            double offset = GetOffsetValueFromConfig();

            temp = basevalue + (offset * -1);

            return temp;
        }

        private double ApplyPositiveOffset(double basevalue, double dimension)
        {
            double temp = 0;

            double offset = GetOffsetValueFromConfig();

            temp = basevalue + offset;

            return temp;
        }

        public void ConnectView(IPfVView view)
        {
            m_view = view;
            m_view.SetPresenter(this);
        }

        public void CurrentPfVSet(string linkedFile, string idLinked)
        {
            m_actualLink = linkedFile;
            m_actualPfV = m_model.ActualModel[m_actualLink].First(pfvdata => pfvdata.UniqueIdLinked == idLinked);
        }

        public PfVElementData CurrentPfVGet()
        {
            if (m_actualPfV == null)
            {
                if (!String.IsNullOrEmpty(m_actualLink))
                {
                    if (CurrentModel[m_actualLink].Count > 0)
                        m_actualPfV = CurrentModel[m_actualLink][0];
                }
            }

            return m_actualPfV;
        }

        public void CurrentLinkSet(string linkedFile)
        {
            m_actualLink = linkedFile;
        }

        private BUYLTools.Configuration.Manager MyConfigManager
        {
            get
            {
                if (m_configManager == null)
                    m_configManager = new BUYLTools.Configuration.Manager(typeof(PfVPresenter).Assembly, true);

                return m_configManager;
            }
        }

        private string GetHostModelDocumentPath()
        {
            string result = "";

            if (m_hostDoc.IsWorkshared)
            {
                ModelPath mp = m_hostDoc.GetWorksharingCentralModelPath();

                if (!mp.Empty)
                    result = ModelPathUtils.ConvertModelPathToUserVisiblePath(mp);
            }
            else
                result = m_hostDoc.PathName;
            return result;
        }

        private PfVModelData PfVsCollect()
        {
            m_app = m_cmdData.Application.Application;
            m_docs = m_app.Documents;

            // Retrieve lighting fixture element
            // data from linked documents:

            PfVModelData data = new PfVModelData();

            using (Transaction trans = new Transaction(GetUIDocumentFromRevit().Document))
            {
                trans.Start("PfV transaction");
                try
                {
                    string sTypeName = MyConfigManager.GetValueForAppsetting(m_pfvtype);
                    foreach (Document docLink in m_docs)
                    {
                        if (docLink.PathName == GetHostModelDocumentPath())
                            continue;

                        if (docLink.IsLinked)
                        {
                            m_actualLink = docLink.PathName;

                            ICollection<ElementId> ids = new List<ElementId>();
                            FilteredElementCollector filterCollector = null;

                            ICollection<ElementId> idsLinkedPfV = new List<ElementId>();

                            if (!data.ContainsKey(m_actualLink))
                                data.Add(m_actualLink, new List<PfVElementData>());

                            if (sTypeName == m_pfvTypeDirectShape)
                            {
                                filterCollector = Utils.Util.GetElementsOfType(docLink, typeof(DirectShape), BuiltInCategory.OST_GenericModel);
                                PfVGetDataFromDirectShape(data, docLink, ids, filterCollector, idsLinkedPfV);
                            }
                            else if (sTypeName == m_pfvTypeInstance)
                            {
                                filterCollector = Utils.Util.GetElementsOfType(docLink, typeof(Instance), BuiltInCategory.OST_GenericModel);
                                PfVGetDataFromInstance(data, docLink, ids, filterCollector, idsLinkedPfV);
                            }

                            data[m_actualLink].Sort();

                            //if (filterCollector != null)
                            //{
                            //    ICollection<ElementId> idClash = null;
                            //    foreach (Element elem in filterCollector)
                            //    {
                            //        idClash = PfVFindElementsInterferingWith(elem, m_hostDoc);
                            //    }
                            //}
                            //CopyLinkedElementsToCurrentModel(commandData, data, docLink, idsLinkedPfV);
                        }
                    }

                    m_hostDoc.Regenerate();

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                }
                finally
                {
                }
            }

            return data;
        }

        private ICollection<ElementId> PfVFindElementsInterferingWith(Element pfvInstance, Document hostdoc)
        {
            ICollection<ElementId> result = null;

            // Setup the filtered element collector for all document elements.
            FilteredElementCollector interferingCollector = new FilteredElementCollector(hostdoc);

            // Only accept element instances
            interferingCollector.WhereElementIsNotElementType();

            // Exclude intersections with the door itself or the host wall for the door.
            List<ElementId> excludedElements = new List<ElementId>();
            excludedElements.Add(pfvInstance.Id);
            ExclusionFilter exclusionFilter = new ExclusionFilter(excludedElements);
            interferingCollector.WherePasses(exclusionFilter);

            Autodesk.Revit.DB.Options opt = new Options();
            Autodesk.Revit.DB.GeometryElement geomElem = pfvInstance.get_Geometry(opt);

            if (geomElem == null)
                return null;

            foreach (GeometryInstance geomInst in geomElem)
            {
                foreach (Solid geomSolid in geomInst.SymbolGeometry)
                {
                    if (null != geomSolid)
                    {
                        // Set up a filter which matches elements whose solid geometry intersects
                        // with the accessibility region
                        ElementIntersectsSolidFilter intersectionFilter = new ElementIntersectsSolidFilter(geomSolid);
                        interferingCollector.WherePasses(intersectionFilter);

                        result = interferingCollector.ToElementIds();
                        if (result.Count > 0)
                            return result;
                    }
                }
            }

            // Return all element ids passing the collector
            return null;
        }

        private void PfvFindHost(Element instPfvLink, PfVElementData pfv)
        {
            //Find the location and direction
            Autodesk.Revit.DB.Options opt = new Options();
            Autodesk.Revit.DB.GeometryElement geomElem = instPfvLink.get_Geometry(opt);
            try
            {
                foreach (GeometryObject geo in geomElem)
                {
                    if (geo is Solid)
                    {
                        Solid geomSolid = geo as Solid;
                        if (null != geomSolid)
                        {
                            GetClashesForSolid(instPfvLink, pfv, geomSolid);
                        }
                    }
                    else if (geo is GeometryInstance)
                    {
                        GeometryInstance instGeo = geo as GeometryInstance;
                        foreach (var geomSub in instGeo.SymbolGeometry)
                        {
                            if (geomSub is Solid)
                            {
                                Solid geomSolid = geomSub as Solid;
                                if (null != geomSolid)
                                {
                                    GetClashesForSolid(instPfvLink, pfv, geomSolid);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static void GetClashesForSolid(Element instPfvLink, PfVElementData pfv, Solid geomSolid)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_hostDoc);
                ElementIntersectsSolidFilter elementFilter = new ElementIntersectsSolidFilter(geomSolid, false);
                collector.WherePasses(elementFilter);

                List<ElementId> excludes = new List<ElementId>();
                excludes.Add(instPfvLink.Id);
                collector.Excluding(excludes);

                foreach (Element elem in collector)
                {
                    pfv.UniqueIdHost = elem.UniqueId;

                    if (collector.GetElementCount() > 1)
                    {
                        //TODO: handle the correct element, if more than one clash detected
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void PfvFindLocation(Element instPfvLink, PfVElementData pfv)
        {
            //Find the location and direction
            Autodesk.Revit.DB.Options opt = new Options();
            Utils.GetCentroid.Centroid.CentroidVolume vol = Utils.GetCentroid.Centroid.GetCentroid(instPfvLink, opt);
            if (vol != null)
                pfv.Location = new Position(vol.Centroid.X, vol.Centroid.Y, vol.Centroid.Z);

            //foreach (Face fa in geomSolid.Faces)
            //{
            //    if (fa is PlanarFace)
            //    {
            //        PlanarFace pFa = fa as PlanarFace;
            //        if (pFa.FaceNormal.Z == 1 || pFa.FaceNormal.Z == -1)
            //        {
            //            pfv.Location = new Position(pFa.Origin.X, pFa.Origin.Y, pFa.Origin.Z);

            //            //foreach (EdgeArray edArray in fa.EdgeLoops)
            //            //{
            //            //    foreach (Edge ed in edArray)
            //            //    {
            //            //        if (fa.ComputeNormal(ed.EvaluateOnFace(0, fa)).Equals(new XYZ(0, 0, 1)))
            //            //        {
            //            //            //fa.or
            //            //            break;
            //            //        }
            //            //    }
            //            //    break;
            //            //}
            //            break;
            //        }
            //        else
            //        {
            //        }
            //    }
            //}

            //GeometryElement gElem = inst.get_Geometry(new Options() /*{ View = doc.ActiveView }*/);
            //GeometryElement gElemTransformed = gElem.GetTransformed(Transform.Identity);

            //foreach (GeometryObject gObj in gElemTransformed)
            //{
            //    Solid gSolid = gObj as Solid;
            //    if (null != gSolid)
            //    {
            //        XYZ p = gSolid.ComputeCentroid();
            //        pfv.Pos = new Position(p.X, p.Y, p.Z);
            //        break;
            //    }
            //}
            //LocationPoint lp = inst.Location as LocationPoint;
            //if (null != lp)
            //{
            //    XYZ p = lp.Point;
            //    pfv.Pos = new Position(p.X, p.Y, p.Z);
            //}
        }

        private void PfVGetDataFromDirectShape(Dictionary<string, List<PfVElementData>> data, Document docLink, ICollection<ElementId> ids, FilteredElementCollector a, ICollection<ElementId> idsLinkedPfV)
        {
            foreach (DirectShape dShape in a)
            {
                string name = dShape.Name;
                if (IsPfV(dShape))
                {
                    idsLinkedPfV.Add(dShape.Id);

                    PfVElementData pfv = new PfVElementData(docLink.PathName, dShape.Name, dShape.UniqueId, dShape.UniqueId);
                    try
                    {
                        pfv.Depth = ParameterExtensions.GetParameterValue("Depth(Pset_ProvisionForVoid)", dShape) != null ? ConvertDBLengthToMeter((double)ParameterExtensions.GetParameterValue("Depth(Pset_ProvisionForVoid)", dShape)) : 0;
                        pfv.Diameter = ParameterExtensions.GetParameterValue("Diameter(Pset_ProvisionForVoid)", dShape) != null ? ConvertDBLengthToMeter((double)ParameterExtensions.GetParameterValue("Diameter(Pset_ProvisionForVoid)", dShape)) : 0;
                        pfv.Height = ParameterExtensions.GetParameterValue("Height(Pset_ProvisionForVoid)", dShape) != null ? ConvertDBLengthToMeter((double)ParameterExtensions.GetParameterValue("Height(Pset_ProvisionForVoid)", dShape)) : 0;
                        pfv.Width = ParameterExtensions.GetParameterValue("Width(Pset_ProvisionForVoid)", dShape) != null ? ConvertDBLengthToMeter((double)ParameterExtensions.GetParameterValue("Width(Pset_ProvisionForVoid)", dShape)) : 0;
                        pfv.IfcGuid = ParameterExtensions.GetParameterValue("IfcGUID", dShape) != null ? (string)ParameterExtensions.GetParameterValue("IfcGUID", dShape) : "";
                        pfv.IfcName = ParameterExtensions.GetParameterValue("IfcName", dShape) != null ? (string)ParameterExtensions.GetParameterValue("IfcName", dShape) : "";
                        pfv.IfcSpatialContainer = ParameterExtensions.GetParameterValue("IfcSpatialContainer", dShape) != null ? (string)ParameterExtensions.GetParameterValue("IfcSpatialContainer", dShape) : "";
                        pfv.Shape = ParameterExtensions.GetParameterValue("Shape(Pset_ProvisionForVoid)", dShape) != null ? (string)ParameterExtensions.GetParameterValue("Shape(Pset_ProvisionForVoid)", dShape) : "";
                        pfv.System = ParameterExtensions.GetParameterValue("System(Pset_ProvisionForVoid)", dShape) != null ? (string)ParameterExtensions.GetParameterValue("System(Pset_ProvisionForVoid)", dShape) : "";
                        // TODO : Add ifcDescription parameter

                        PfvFindLocation(dShape, pfv);

                        PfvFindHost(dShape, pfv);

                        ids.Add(dShape.Id);
                    }
                    catch (Exception ex)
                    {
                    }

                    data[docLink.PathName].Add(pfv);
                }
            }
        }

        private void PfVGetDataFromInstance(Dictionary<string, List<PfVElementData>> data, Document docLink, ICollection<ElementId> ids, FilteredElementCollector coll, ICollection<ElementId> idsLinkedPfV)
        {
            //foreach (DirectShape dShape in a)
            foreach (Instance inst in coll)
            {
                string name = inst.Name;
                if (IsPfV(inst))
                {
                    idsLinkedPfV.Add(inst.Id);

                    PfVElementData pfv = new PfVElementData(docLink.PathName, inst.Name, inst.UniqueId, inst.UniqueId);
                    try
                    {
                        pfv.Depth = ParameterExtensions.GetParameterValue("Depth", inst) != null ? ConvertDBLengthToMeter((double)ParameterExtensions.GetParameterValue("Depth", inst)) : 0;
                        pfv.Diameter = ParameterExtensions.GetParameterValue("Diameter", inst) != null ? ConvertDBLengthToMeter((double)ParameterExtensions.GetParameterValue("Diameter", inst)) : 0;
                        pfv.Height = ParameterExtensions.GetParameterValue("Height", inst) != null ? ConvertDBLengthToMeter((double)ParameterExtensions.GetParameterValue("Height", inst)) : 0;
                        pfv.Width = ParameterExtensions.GetParameterValue("Width", inst) != null ? ConvertDBLengthToMeter((double)ParameterExtensions.GetParameterValue("Width", inst)) : 0;
                        pfv.IfcGuid = ParameterExtensions.GetParameterValue("IfcGUID", inst) != null ? (string)ParameterExtensions.GetParameterValue("IfcGUID", inst) : "";
                        pfv.IfcName = ParameterExtensions.GetParameterValue("IfcName", inst) != null ? (string)ParameterExtensions.GetParameterValue("IfcName", inst) : "";
                        pfv.IfcSpatialContainer = ParameterExtensions.GetParameterValue("IfcSpatialContainer", inst) != null ? (string)ParameterExtensions.GetParameterValue("IfcSpatialContainer", inst) : "";
                        pfv.Shape = ParameterExtensions.GetParameterValue("Shape", inst) != null ? (string)ParameterExtensions.GetParameterValue("Shape", inst) : "";
                        pfv.System = ParameterExtensions.GetParameterValue("System", inst) != null ? (string)ParameterExtensions.GetParameterValue("System", inst) : "";
                        pfv.IfcDescription = ParameterExtensions.GetParameterValue("IfcDescription", inst) != null ? (string)ParameterExtensions.GetParameterValue("IfcDescription", inst) : "";

                        PfvFindLocation(inst, pfv);

                        PfvFindHost(inst, pfv);

                        ids.Add(inst.Id);
                    }
                    catch (Exception ex)
                    {
                    }

                    data[docLink.PathName].Add(pfv);
                }
            }
        }

        private bool IsPfV(Element e)
        {
            bool result = false;

            foreach (Parameter p in ParameterExtensions.GetParameters(e))
            {
                if (p.Definition.Name == "ObjectTypeOverride")
                {
                    string pVal = p.AsString();
                    if (pVal == "ProvisionForVoid")
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        private void PfVPlaceOnWall(PfVElementData pfvData)
        {
            if (pfvData == null)
                return;

            FamilyInstance inst = null;
            if (!String.IsNullOrEmpty(pfvData.UniqueIdLocal) && pfvData.PfVStatus == Status.Dummy)
            {
                //delete dummy
                Element el = m_hostDoc.GetElement(pfvData.UniqueIdLocal);
                if (el != null)
                {
                    m_hostDoc.Delete2(el);
                    pfvData.UniqueIdLocal = "";
                }
            }

            if (String.IsNullOrEmpty(pfvData.UniqueIdLocal))
            {
                // Retrieve the family if it is already present:
                string familyName = "";
                if (pfvData.IsRectangular())
                    familyName = MyConfigManager.GetValueForAppsetting(m_pfvWallCutoutRe);
                else if (pfvData.IsRound())
                    familyName = MyConfigManager.GetValueForAppsetting(m_pfvWallCutoutRd);

                FamilySymbol symbol = GetFirstSymbolFromFamily(familyName);

                // Place the family symbol
                if (symbol != null)
                {
                    if (!symbol.IsActive)
                        symbol.Activate();

                    XYZ insertion = new XYZ(pfvData.Location.X, pfvData.Location.Y, pfvData.Location.Z);

                    inst = ElementPlacement.AddFaceBasedFamilyToHost(m_hostDoc, pfvData.UniqueIdHost, symbol.Id, insertion);

                    if (inst != null)
                        pfvData.UniqueIdLocal = inst.UniqueId;
                }
            }
            else
                inst = m_hostDoc.GetElement(pfvData.UniqueIdLocal) as FamilyInstance;

            if (inst != null)
            {
                pfvData.PfVStatus = Status.Ok;

                ApplyDimensions(pfvData, inst);
            }
        }

        private void PfVPlaceOnHost(PfVElementData pfvData)
        {
            if (pfvData == null)
                return;

            FamilyInstance inst = null;
            if (!String.IsNullOrEmpty(pfvData.UniqueIdLocal) && pfvData.PfVStatus == Status.Dummy)
            {
                //delete dummy
                Element el = m_hostDoc.GetElement(pfvData.UniqueIdLocal);
                if (el != null)
                {
                    m_hostDoc.Delete2(el);
                    pfvData.UniqueIdLocal = "";
                }
            }

            if (String.IsNullOrEmpty(pfvData.UniqueIdLocal))
            {
                // Retrieve the family if it is already present:
                string familyName = null;

                if (pfvData.IsWallPfV(GetHostCategoryName(pfvData)))
                {
                    if (pfvData.IsRectangular())
                        familyName = MyConfigManager.GetValueForAppsetting(m_pfvWallCutoutRe);
                    else if (pfvData.IsRound())
                        familyName = MyConfigManager.GetValueForAppsetting(m_pfvWallCutoutRd);
                }
                else if (pfvData.IsFloorPfV(GetHostCategoryName(pfvData)))
                {
                    if (pfvData.IsRectangular())
                        familyName = MyConfigManager.GetValueForAppsetting(m_pfvFloorCutoutRe);
                    else if (pfvData.IsRound())
                        familyName = MyConfigManager.GetValueForAppsetting(m_pfvFloorCutoutRd);
                }
                else
                {
                    if (pfvData.IsRectangular())
                        familyName = MyConfigManager.GetValueForAppsetting(m_pfvOtherCutoutRe);
                    else if (pfvData.IsRound())
                        familyName = MyConfigManager.GetValueForAppsetting(m_pfvOtherCutoutRd);
                }

                FamilySymbol symbol = GetFirstSymbolFromFamily(familyName);

                // Place the family symbol
                if (symbol != null)
                {
                    if (!symbol.IsActive)
                        symbol.Activate();

                    XYZ insertion = new XYZ(pfvData.Location.X, pfvData.Location.Y, pfvData.Location.Z);

                    inst = ElementPlacement.AddFaceBasedFamilyToHost(m_hostDoc, pfvData.UniqueIdHost, symbol.Id, insertion);
                }
            }
            else
                inst = m_hostDoc.GetElement(pfvData.UniqueIdLocal) as FamilyInstance;

            if (inst != null)
            {
                ApplyDimensions(pfvData, inst);
                pfvData.UniqueIdLocal = inst.UniqueId;
                pfvData.PfVStatus = Status.Ok;

                List<ElementId> lst = new List<ElementId>();
                lst.Add(inst.Id);
                GetUIDocumentFromRevit().Selection.SetElementIds(lst);
            }
        }
        private void ApplyDimensions(PfVElementData pfvData, FamilyInstance inst)
        {
            foreach (Parameter p in inst.GetOrderedParameters())
            {
                if (pfvData.IsRectangular())
                {
                    if (p.Definition.Name == m_configManager.GetValueForAppsetting(m_pfvWidth))
                        ParameterExtensions.SetParameterValue(p, ConvertMeterToDBLengthUnit(pfvData.Width));
                    else if (p.Definition.Name == m_configManager.GetValueForAppsetting(m_pfvHeight))
                        ParameterExtensions.SetParameterValue(p, ConvertMeterToDBLengthUnit(pfvData.Height));
                }
                else if (pfvData.IsRound())
                {
                    if (p.Definition.Name == m_configManager.GetValueForAppsetting(m_pfvDiameter))
                        ParameterExtensions.SetParameterValue(p, ConvertMeterToDBLengthUnit(pfvData.Diameter));
                }

                if (p.Definition.Name == m_configManager.GetValueForAppsetting(m_pfvDepth))
                    ParameterExtensions.SetParameterValue(p, ConvertMeterToDBLengthUnit(pfvData.Depth));
            }
        }

        private double ConvertMeterToDBLengthUnit(double input)
        {
            return Utils.MathUtils.MeterToFeet(input);
        }

        private double ConvertDBLengthToMeter(double input)
        {
            double resu = 0;

            resu = Utils.MathUtils.FeetToMeter(input);
            return resu;
        }

        private Document GetDocumentFromRevit(string docname)
        {
            Document res = null;

            if (m_cmdData != null)
            {
                foreach (Document doc in m_cmdData.Application.Application.Documents)
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
            UIDocument res = m_cmdData.Application.ActiveUIDocument;

            return res;
        }

        private static Element FindElementByName(Type targetType, string targetName)
        {
            return new FilteredElementCollector(m_hostDoc)
              .OfClass(targetType)
              .FirstOrDefault<Element>(e => e.Name.Equals(targetName));
        }

        private FamilySymbol GetFirstSymbolFromFamily(string familyName)
        {
            FamilySymbol sym = null;
            if (String.IsNullOrEmpty(familyName))
                return null;

            Family family = FindElementByName(typeof(Family), familyName) as Family;

            if (family == null)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("Family {0} couldn't be found in current document", familyName));
                return null;
            }

            // Determine the family symbol
            foreach (ElementId id in family.GetFamilySymbolIds())
            {
                sym = m_hostDoc.GetElement(id) as FamilySymbol;

                // Our family only contains one
                // symbol, so pick it and leave

                break;
            }

            return sym;
        }

        private FamilyInstance PlaceInstance(PfVElementData pfvData, ElementId lowerLevelid, FamilySymbol symbol)
        {
            FamilyInstance inst = null;

            using (SubTransaction trans = new SubTransaction(m_hostDoc))
            {
                trans.Start();

                try
                {
                    XYZ insertion = new XYZ(pfvData.Location.X, pfvData.Location.Y, pfvData.Location.Z);

                    if (!symbol.IsActive)
                        symbol.Activate();
                    Level lowerLevel = null;

                    if (lowerLevelid != ElementId.InvalidElementId)
                    {
                        lowerLevel = m_hostDoc.GetElement(lowerLevelid) as Level;
                    }
                    else
                    {
                        //lowerLevel = m_hostDoc.Create.NewFamilyInstance(insertion, symbol, )
                    }

                    inst = m_hostDoc.Create.NewFamilyInstance(insertion, symbol, lowerLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural); //, view);
                    trans.Commit();
                    //m_hostDoc.Regenerate();
                }
                catch (Exception ex)
                {
                    trans.Dispose();
                }
            }

            return inst;
        }

        private void CopyLinkedElementsToCurrentModel(Dictionary<string, List<PfVElementData>> data, Document doc, ICollection<ElementId> idsLinkedPfV)
        {
            try
            {
                CopyPasteOptions copyOptions = new CopyPasteOptions();
                copyOptions.SetDuplicateTypeNamesHandler(new CopyUseDestination());

                ICollection<ElementId> idsLocalPfV = ElementTransformUtils.CopyElements(doc, idsLinkedPfV, GetUIDocumentFromRevit().Document, null, copyOptions);

                foreach (PfVElementData pfv in data[doc.PathName])
                {
                    foreach (ElementId idnew in idsLocalPfV)
                    {
                        Element el = GetUIDocumentFromRevit().Document.GetElement(idnew);
                        pfv.UniqueIdLocal = el.UniqueId;

                        LocationPoint lp = el.Location as LocationPoint;
                        XYZ p;
                        if (lp != null)
                        {
                            p = lp.Point;
                        }

                        //commandData.Application.ActiveUIDocument.Document.Delete(idnew);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
    }
}