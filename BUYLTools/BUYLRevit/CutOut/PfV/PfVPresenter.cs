#region Namespaces

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BUYLRevit.Utils;
using BUYLTools.CutOut.PfV;
using System;
using System.Collections.Generic;
using System.IO;
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
        internal static Autodesk.Revit.ApplicationServices.Application m_app = null;
        internal static DocumentSet m_docs = null;
        IPfVView m_view = null;
        static ExternalCommandData m_cmdData = null;
        static Document m_hostDoc;
        static string m_message = null;
        static IPfVModel m_model;
        static string m_currentHostDoc;

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
                if( value != m_currentHostDoc)
                {
                    if(!String.IsNullOrEmpty(m_currentHostDoc))
                        m_model.ModelSave(m_currentHostDoc);

                    m_model.ModelLoad(value);
                    m_currentHostDoc = value;
                }
            }
        }

        public Result ProcessPfVs(ExternalCommandData commandData, ref string message, ElementSet highlightElements)
        {
            Result res = Result.Failed;

            try
            {
                m_cmdData = commandData;
                m_message = message;
                m_hostDoc = GetUIDocumentFromRevit().Document;

                if(m_model == null)
                    m_model = new PfVModel();
                //m_model.ModelLoad(m_hostDoc.PathName);

                m_model.UpdateModel(PfVsCollect(), m_hostDoc.PathName);

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

        public Result PfVPrevious(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result res = Result.Failed;

            m_cmdData = commandData;
            m_message = message;
            m_hostDoc = GetUIDocumentFromRevit().Document;

            PfVPrevious();

            return res;
        }

        public Result PfVNext(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result res = Result.Failed;

            m_cmdData = commandData;
            m_message = message;
            m_hostDoc = GetUIDocumentFromRevit().Document;

            PfVNext();

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

        public void PfVPrevious()
        {
            PfVElementData temp = CurrentPfVGet();
            int index = 0;
            if (temp != null)
                index = m_model.ActualModel[m_actualLink].IndexOf(CurrentPfVGet());

            if (index > 0)
            {
                PfVElementData dt = m_model.ActualModel[m_actualLink][index - 1];
                CurrentPfVSet(m_actualLink, dt.IdLinked);
            }
            else
            {
                int counter = m_model.ActualModel[m_actualLink].Count;
                PfVElementData dt = m_model.ActualModel[m_actualLink][counter - 1];
                CurrentPfVSet(m_actualLink, dt.IdLinked);
            }

            PfVZoomToDummy();
        }

        public void PfVNext()
        {
            PfVElementData temp = CurrentPfVGet();
            int index = 0;
            if(temp != null)
                index = m_model.ActualModel[m_actualLink].IndexOf(CurrentPfVGet());

            if(index < m_model.ActualModel[m_actualLink].Count)
            {
                PfVElementData dt = m_model.ActualModel[m_actualLink][index + 1];
                CurrentPfVSet(m_actualLink, dt.IdLinked);
            }
            else
            {
                PfVElementData dt = m_model.ActualModel[m_actualLink][0];
                CurrentPfVSet(m_actualLink, dt.IdLinked);
            }

            PfVZoomToDummy();
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
                        if(pfvData.IsWallPfV())
                        {                            
                            PfVPlaceOnWall(pfvData);
                        }
                        else
                        {

                        }

                        View3D view3d = GetUIDocumentFromRevit().ActiveView as View3D;
                        if(view3d != null)
                        {
                            Create3DBoundingBox(pfvData, view3d);
                        }
                    }

                    t.Commit();
                }
                catch (System.Exception ex)
                { t.RollBack(); }
            }
        }

        public void PfVZoomToDummy()
        {
            PfVElementData pfvData = CurrentPfVGet();
            if (pfvData != null)
            {
                using (Transaction t = new Transaction(m_hostDoc))
                {
                    t.Start("Change to 3D view");
                    try
                    {
                        if (GetUIDocumentFromRevit().ActiveView.ViewType == ViewType.ThreeD)
                        {
                            PfVPlaceDummyAndZoom(pfvData);

                            View3D view3d = GetUIDocumentFromRevit().ActiveView as View3D;
                            if (view3d != null)
                            {
                                Create3DBoundingBox(pfvData, view3d);
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
        }
        static double m_offsetMinus = -2;
        static double m_offsetPlus = 2;
        private static double ApplyNegativeOffset(double basevalue)
        {
            double temp = 0;

            temp = basevalue + m_offsetMinus;
               
            return temp;
        }

        private static double ApplyPositiveOffset(double basevalue)
        {
            double temp = 0;

            temp = basevalue + (m_offsetPlus);            
                
            return temp;
        }
        private static void Create3DBoundingBox(PfVElementData pfvData, View3D view3d)
        {
            XYZ min = new XYZ(ApplyNegativeOffset(pfvData.Location.X), ApplyNegativeOffset(pfvData.Location.Y), ApplyNegativeOffset(pfvData.Location.Z));
            XYZ max = new XYZ(ApplyPositiveOffset(pfvData.Location.X), ApplyPositiveOffset(pfvData.Location.Y), ApplyPositiveOffset(pfvData.Location.Z));
            view3d.SetSectionBox(new BoundingBoxXYZ() { Min = min, Max = max });
        }

        public void ConnectView(IPfVView view)
        {
            m_view = view;
            m_view.SetPresenter(this);
        }

        PfVElementData m_actualPfV = null;
        string m_actualLink = "";

        public void CurrentPfVSet(string linkedFile, int idLinked)
        {
            m_actualLink = linkedFile;
            m_actualPfV = m_model.ActualModel[m_actualLink].First(pfvdata => pfvdata.IdLinked == idLinked);
        }

        public PfVElementData CurrentPfVGet()
        {
            if(m_actualPfV == null)
            {
                if(!String.IsNullOrEmpty(m_actualLink))
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
        private PfVModelData PfVsCollect()
        {
            BUYLTools.Configuration.Manager _confMan = new BUYLTools.Configuration.Manager(typeof(PfVPresenter).Assembly, true);
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
                    string sTypeName = _confMan.GetValueForAppsetting(m_pfvtype);
                    foreach (Document docLink in m_docs)
                    {
                        if (docLink.PathName == m_hostDoc.PathName)
                            continue;

                        if (docLink.IsLinked)
                        {
                            ICollection<ElementId> ids = new List<ElementId>();
                            FilteredElementCollector filterCollector = null;

                            ICollection<ElementId> idsLinkedPfV = new List<ElementId>();

                            if (!data.ContainsKey(docLink.PathName))
                                data.Add(docLink.PathName, new List<PfVElementData>());

                            if (sTypeName == m_pfvTypeDirectShape)
                            {
                                filterCollector = Utils.Util.GetElementsOfType(docLink, typeof(DirectShape), BuiltInCategory.OST_GenericModel);
                                PfVGetDataFromDirectShape(data, docLink, ids, filterCollector, idsLinkedPfV);
                            }
                            else if (sTypeName == m_pfvTypeInstance)
                            {
                                filterCollector = Utils.Util.GetElementsOfType(docLink, typeof(Instance), BuiltInCategory.OST_GenericModel);
                                PfVGetDataFromInstance(data, docLink, ids, filterCollector, idsLinkedPfV);

                                ICollection<ElementId> idClash = null;

                                foreach (Instance inst in filterCollector)
                                {
                                    idClash = PfVFindElementsInterferingWith(inst, m_hostDoc);
                                }
                            }

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

        private ICollection<ElementId> PfVFindElementsInterferingWith(Instance pfvInstance, Document hostdoc)
        {
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
            Solid geomSolid;
            foreach (GeometryObject geomObj in geomElem)
            {
                geomSolid = geomObj as Solid;
                if (null != geomSolid)
                {
                    // Set up a filter which matches elements whose solid geometry intersects
                    // with the accessibility region
                    ElementIntersectsSolidFilter intersectionFilter = new ElementIntersectsSolidFilter(geomSolid);
                    interferingCollector.WherePasses(intersectionFilter);
                }
            }

            // Return all elements passing the collector
            return interferingCollector.ToElementIds();
        }

        private void PfvFindHost(Element instPfvLink, PfVElementData pfv)
        {
            //Find the location and direction
            Autodesk.Revit.DB.Options opt = new Options();
            Autodesk.Revit.DB.GeometryElement geomElem = instPfvLink.get_Geometry(opt);
            foreach (GeometryInstance instGeo in geomElem)
            {
                foreach (var geomSub in instGeo.SymbolGeometry)
                {
                    if (geomSub is Solid)
                    {
                        Solid geomSolid = geomSub as Solid;
                        if (null != geomSolid)
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
                                    pfv.IdHost = elem.Id.IntegerValue;

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
                    }
                }
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

                    PfVElementData pfv = new PfVElementData(docLink.PathName, dShape.Name, dShape.Id.IntegerValue, dShape.UniqueId);
                    try
                    {
                        pfv.Depth = ParameterExtensions.GetParameterValue("Depth(Pset_ProvisionForVoid)", dShape) != null ? (double)ParameterExtensions.GetParameterValue("Depth(Pset_ProvisionForVoid)", dShape) : 0;
                        pfv.Diameter = ParameterExtensions.GetParameterValue("Diameter(Pset_ProvisionForVoid)", dShape) != null ? (double)ParameterExtensions.GetParameterValue("Diameter(Pset_ProvisionForVoid)", dShape) : 0;
                        pfv.Height = ParameterExtensions.GetParameterValue("Height(Pset_ProvisionForVoid)", dShape) != null ? (double)ParameterExtensions.GetParameterValue("Height(Pset_ProvisionForVoid)", dShape) : 0;
                        pfv.Width = ParameterExtensions.GetParameterValue("Width(Pset_ProvisionForVoid)", dShape) != null ? (double)ParameterExtensions.GetParameterValue("Width(Pset_ProvisionForVoid)", dShape) : 0;
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

                    PfVElementData pfv = new PfVElementData(docLink.PathName, inst.Name, inst.Id.IntegerValue, inst.UniqueId);
                    try
                    {
                        pfv.Depth = ParameterExtensions.GetParameterValue("Depth", inst) != null ? (double)ParameterExtensions.GetParameterValue("Depth", inst) : 0;
                        pfv.Diameter = ParameterExtensions.GetParameterValue("Diameter", inst) != null ? (double)ParameterExtensions.GetParameterValue("Diameter", inst) : 0;
                        pfv.Height = ParameterExtensions.GetParameterValue("Height", inst) != null ? (double)ParameterExtensions.GetParameterValue("Height", inst) : 0;
                        pfv.Width = ParameterExtensions.GetParameterValue("Width", inst) != null ? (double)ParameterExtensions.GetParameterValue("Width", inst) : 0;
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
            if (pfvData.IdLocal != 0 && pfvData.PfVStatus == Status.Dummy)
            {
                //delete dummy
                Element el = m_hostDoc.GetElement(new ElementId(pfvData.IdLocal));
                if (el != null)
                {
                    m_hostDoc.Delete2(el);
                    pfvData.IdLocal = 0;
                }
            }

            if (pfvData.IdLocal == 0)
            {
                // Retrieve the family if it is already present:
                string familyName = "ALG_CutOut_re_Void";
                FamilySymbol symbol = GetFirstSymbolFromFamily(familyName);

                // Place the family symbol
                if (symbol != null)
                {
                    XYZ insertion = new XYZ(pfvData.Location.X, pfvData.Location.Y, pfvData.Location.Z);
                    inst = ElementPlacement.AddFaceBasedFamilyToWall(m_hostDoc, new ElementId(pfvData.IdHost), symbol.Id, insertion);
                    pfvData.IdLocal = inst.Id.IntegerValue;
                }
            }
            else
                inst = m_hostDoc.GetElement(new ElementId(pfvData.IdLocal)) as FamilyInstance;

            if (inst != null)
            {
                pfvData.PfVStatus = Status.Ok;
                GetUIDocumentFromRevit().ShowElements(inst.Id);
            }
        }

        private void PfVPlaceDummyAndZoom(PfVElementData pfvData)
        {
            if (pfvData == null)
                return;

            FamilyInstance inst = null;
            if (pfvData.IdDummy == 0)
            {
                // Retrieve the family if it is already present:
                string familyName = "ALG_Marker_PfV";
                FamilySymbol symbol = GetFirstSymbolFromFamily(familyName);

                // Place the family symbol:

                if (symbol != null)
                {
                    Element el = m_hostDoc.GetElement(new ElementId(pfvData.IdHost));
                    if (el != null)
                    {
                        Level lowerLevel = m_hostDoc.GetElement(el.LevelId) as Level;
                        if (lowerLevel != null)
                            inst = PlaceInstance(pfvData, lowerLevel, symbol, inst);

                        if (inst != null)
                            pfvData.IdDummy = inst.Id.IntegerValue;
                    }
                }
            }
            else
                inst = m_hostDoc.GetElement(new ElementId(pfvData.IdDummy)) as FamilyInstance;

            if (inst != null)
            {
                pfvData.PfVStatus = Status.Dummy;
                GetUIDocumentFromRevit().ShowElements(inst.Id);

#if RELEASE
                m_hostDoc.Delete2(inst);
                pfvData.IdLocal = 0;
#endif
                //m_view.HideDlg();
            }
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
              .FirstOrDefault<Element>( e => e.Name.Equals(targetName));
        }

        FamilySymbol GetFirstSymbolFromFamily(string familyName)
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
        private FamilyInstance PlaceInstance(PfVElementData pfvData, Level lowerLevel, FamilySymbol symbol, FamilyInstance inst)
        {
            using (SubTransaction trans = new SubTransaction(m_hostDoc))
            {
                trans.Start();

                try
                {
                    XYZ insertion = new XYZ(pfvData.Location.X, pfvData.Location.Y, pfvData.Location.Z);

                    if (!symbol.IsActive)
                        symbol.Activate();

                    inst = m_hostDoc.Create.NewFamilyInstance(insertion, symbol, lowerLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural); //, view);

                    trans.Commit();
                    m_hostDoc.Regenerate();
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
                        pfv.IdLocal = el.Id.IntegerValue;
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