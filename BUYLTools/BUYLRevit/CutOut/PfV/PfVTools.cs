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

    public class PfVTools : IPresenter
    {
        internal static Autodesk.Revit.ApplicationServices.Application m_app = null;
        internal static DocumentSet m_docs = null;
        private const string m_pfvtype = "PfVType";
        private const string m_pfvTypeDirectShape = "DirectShape";
        private const string m_pfvTypeInstance = "Instance";
        static ExternalCommandData m_cmdData = null;
        static Document m_hostDoc;
        static string m_message = null;
        static PfVModelData m_data;
        static PfVModel m_model;

        public Result ProcessPfVs(
                    ExternalCommandData commandData,
          ref string message,
          ElementSet highlightElements)
        {
            m_cmdData = commandData;
            m_message = message;
            m_hostDoc = m_cmdData.Application.ActiveUIDocument.Document;

            m_model = new PfVModel();
            m_data = m_model.Model(m_hostDoc.PathName);

            m_data = CollectPfVs();

            if (m_data != null)
            {
                //TaskDialog.Show("PfV Manager", String.Format("{0} elements found in links", lst.Count));
                m_view.SetData(m_data);
                //dlg.SetCommandData(commandData);
                if(m_view.ShowPfvDlg() == DialogResult.OK)
                    m_model.ModelSave();

                return Result.Succeeded;
            }
            else
                return Result.Failed;

        }

        //                                        GetUIDocumentFromRevit().Application.ActiveUIDocument.Document.Regenerate();
        //                                        //GetUIDocumentFromRevit().ActiveViewShowElements(new ElementId(pfvData.IdLinked)); //GetDocumentFromRevit(dtView.Parent.Tag.ToString());
        //                                    }
        //                                }
        //                                catch (Exception ex)
        //                                {
        //                                    t.RollBack();
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        private PfVModelData CollectPfVs()
        {
            BUYLTools.Configuration.Manager _confMan = new BUYLTools.Configuration.Manager(typeof(PfVTools).Assembly, true);
            m_app = m_cmdData.Application.Application;
            m_docs = m_app.Documents;


            // Retrieve lighting fixture element
            // data from linked documents:

            PfVModelData data = new PfVModelData();

            using (Transaction trans = new Transaction(m_cmdData.Application.ActiveUIDocument.Document))
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
                                GetPfVDataFromDirectShape(data, docLink, ids, filterCollector, idsLinkedPfV);
                            }
                            else if (sTypeName == m_pfvTypeInstance)
                            {
                                filterCollector = Utils.Util.GetElementsOfType(docLink, typeof(Instance), BuiltInCategory.OST_GenericModel);
                                GetPfVDataFromInstance(data, docLink, ids, filterCollector, idsLinkedPfV);

                                ICollection<ElementId> idClash = null;

                                foreach (Instance inst in filterCollector)
                                {
                                    idClash = FindElementsInterferingWithPfV(inst, m_hostDoc);
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

        private void CopyLinkedElementsToCurrentModel(Dictionary<string, List<PfVElementData>> data, Document doc, ICollection<ElementId> idsLinkedPfV)
        {
            try
            {
                CopyPasteOptions copyOptions = new CopyPasteOptions();
                copyOptions.SetDuplicateTypeNamesHandler(new CopyUseDestination());

                ICollection<ElementId> idsLocalPfV = ElementTransformUtils.CopyElements(doc, idsLinkedPfV, m_cmdData.Application.ActiveUIDocument.Document, null, copyOptions);

                foreach (PfVElementData pfv in data[doc.PathName])
                {
                    foreach (ElementId idnew in idsLocalPfV)
                    {
                        Element el = m_cmdData.Application.ActiveUIDocument.Document.GetElement(idnew);
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

        private ICollection<ElementId> FindElementsInterferingWithPfV(Instance pfvInstance, Document hostdoc)
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

        private void FindPfvHost(Element instPfvLink, PfVElementData pfv)
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

        private void FindPfvLocation(Element instPfvLink, PfVElementData pfv)
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
        private void GetPfVDataFromDirectShape(Dictionary<string, List<PfVElementData>> data, Document docLink, ICollection<ElementId> ids, FilteredElementCollector a, ICollection<ElementId> idsLinkedPfV)
        {
            foreach (DirectShape dShape in a)
            {
                string name = dShape.Name;
                if (IsProvisionForVoid(dShape))
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

                        FindPfvLocation(dShape, pfv);

                        FindPfvHost(dShape, pfv);

                        ids.Add(dShape.Id);
                    }
                    catch (Exception ex)
                    {
                    }

                    data[docLink.PathName].Add(pfv);
                }
            }
        }

        private void GetPfVDataFromInstance(Dictionary<string, List<PfVElementData>> data, Document docLink, ICollection<ElementId> ids, FilteredElementCollector coll, ICollection<ElementId> idsLinkedPfV)
        {
            //foreach (DirectShape dShape in a)
            foreach (Instance inst in coll)
            {
                string name = inst.Name;
                if (IsProvisionForVoid(inst))
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

                        FindPfvLocation(inst, pfv);

                        FindPfvHost(inst, pfv);

                        ids.Add(inst.Id);
                    }
                    catch (Exception ex)
                    {
                    }

                    data[docLink.PathName].Add(pfv);
                }
            }
        }

        private bool IsProvisionForVoid(Element e)
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

        IPfVView m_view = null;
        public void ConnectView(IPfVView view)
        {
            m_view = view;
            m_view.SetPresenter(this);
        }

        void IPresenter.ZoomToPfV(PfVElementData pfvData)
        {
            if (pfvData != null)
            {
                Transaction t = new Transaction(m_hostDoc);
                t.Start("Change to 3D view");
                try
                {
                    if (GetUIDocumentFromRevit().ActiveView.ViewType == ViewType.ThreeD)
                    {
                        PlacePfVDummy(pfvData);
                        //View3D view = (View3D)GetUIDocumentFromRevit().ActiveView;

                        //XYZ up = null;
                        //XYZ target = new XYZ(pfvData.Location.X, pfvData.Location.Y, pfvData.Location.Z);
                        //Transform tr = Transform.CreateRotation(new XYZ(0, 0, 1), Utils.MathUtils.DegreeToRad(90));
                        //XYZ targetN = tr.OfVector(target);

                        //up = target.CrossProduct(targetN); // new XYZ(0, 0, pfvData.Pos.Z)
                        //XYZ targetM = target.Multiply(Utils.MathUtils.MMToFeet(3000) * -1);
                        //view.SetOrientation(new ViewOrientation3D(targetM, up, target));
                    }

                    t.Commit();
                }
                catch (System.Exception ex)
                { t.RollBack(); }
            }
        }

        private static Element FindElementByName(Type targetType, string targetName)
        {
            return new FilteredElementCollector(m_hostDoc)
              .OfClass(targetType)
              .FirstOrDefault<Element>( e => e.Name.Equals(targetName));
        }


        private void PlacePfVDummy(PfVElementData pfvData)
        {
            if (pfvData == null)
                return;

            FamilyInstance inst = null;
            if (pfvData.IdLocal == 0)
            {
                // Retrieve the family if it is already present:
                string familyName = "ALG_Marker_PfV";
                if (String.IsNullOrEmpty(familyName))
                    return;

                Family family = FindElementByName(typeof(Family), familyName) as Family;

                if (family == null)
                {
                    System.Windows.Forms.MessageBox.Show(String.Format("Family {0} couldn't be found in current document", familyName));
                    return;
                }
                // Determine the family symbol

                FamilySymbol symbol = null;

                foreach (ElementId id in family.GetFamilySymbolIds())
                {
                    symbol = m_hostDoc.GetElement(id) as FamilySymbol;

                    // Our family only contains one
                    // symbol, so pick it and leave

                    break;
                }

                // Place the family symbol:

                if (symbol != null)
                {
                    Element el = m_hostDoc.GetElement(new ElementId(pfvData.IdHost));
                    if (el != null)
                    {
                        Level lowerLevel = m_hostDoc.GetElement(el.LevelId) as Level;
                        if (lowerLevel != null)
                            inst = PlaceInstance(pfvData, lowerLevel, symbol, inst);

                        if(inst != null)
                            pfvData.IdLocal = inst.Id.IntegerValue;
                    }
                }
            }
            else
                inst = m_hostDoc.GetElement(new ElementId(pfvData.IdLocal)) as FamilyInstance;

            if (inst != null)
            {
                pfvData.PfVStatus = Status.Dummy;
                m_cmdData.Application.ActiveUIDocument.ShowElements(inst.Id);
                //m_view.HideDlg();
            }
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
    }
}