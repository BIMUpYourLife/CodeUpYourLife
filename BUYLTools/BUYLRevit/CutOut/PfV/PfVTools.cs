#region Header

//
// CmdLinkedFileElements.cs - list elements in linked files
//
// Copyright (C) 2009-2016 by Jeremy Tammik,
// Autodesk Inc. All rights reserved.
//
// Keywords: The Building Coder Revit API C# .NET add-in.
//

#endregion Header

#region Namespaces

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BUYLRevit.Utils;
using BUYLTools.CutOut.PfV;
using System;
using System.Collections.Generic;
using System.IO;

#endregion Namespaces

namespace BUYLRevit.CutOut.PfV
{
    public class PfVTools
    {
        public static Result ProcessPfVs(
            ExternalCommandData commandData,
          ref string message,
          ElementSet highlightElements)
        {
            Dictionary<string, List<PfVElementData>> lst = CollectPfVs(commandData, ref message, highlightElements);

            if (lst != null)
            {
                //TaskDialog.Show("PfV Manager", String.Format("{0} elements found in links", lst.Count));
                PfVViewDLG dlg = new PfVViewDLG();
                dlg.SetData(lst);
                //dlg.SetCommandData(commandData);
                dlg.ShowDialog();

                return Result.Succeeded;
            }
            else
                return Result.Failed;
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

        //private void DtView_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        //{
        //    if (m_data != null)
        //    {
        //        if (sender is DataGridView)
        //        {
        //            DataGridView dtView = sender as DataGridView;
        //            if (dtView != null)
        //            {
        //                if (dtView.Parent is TabPage)
        //                {
        //                    if (m_data.ContainsKey(dtView.Parent.Tag.ToString()))
        //                    {
        //                        DataGridViewRow row = dtView.Rows[e.RowIndex];
        //                        int id = 0;

        //                        PfVElementData pfvData = null;
        //                        if (Int32.TryParse(row.Cells["IdLinked"].Value.ToString(), out id))
        //                        {
        //                            pfvData = m_data[dtView.Parent.Tag.ToString()].FirstOrDefault<PfVElementData>(item => item.IdLinked == id);
        //                            if (pfvData != null)
        //                            {
        //                                Transaction t = new Transaction(GetUIDocumentFromRevit().Application.ActiveUIDocument.Document);
        //                                t.Start("Change to 3D view");
        //                                try
        //                                {
        //                                    if (GetUIDocumentFromRevit().ActiveView.ViewType == ViewType.ThreeD)
        //                                    {
        //                                        View3D view = (View3D)GetUIDocumentFromRevit().ActiveView;

        //                                        XYZ up = null;
        //                                        XYZ target = new XYZ(pfvData._location.X, pfvData._location.Y, pfvData._location.Z);
        //                                        Transform tr = Transform.CreateRotation(new XYZ(0, 0, 1), Utils.MathUtils.DegreeToRad(90));
        //                                        XYZ targetN = tr.OfVector(target);

        //                                        up = target.CrossProduct(targetN); // new XYZ(0, 0, pfvData.Pos.Z)
        //                                        XYZ targetM = target.Multiply(Utils.MathUtils.MMToFeet(3000) * -1);
        //                                        view.SetOrientation(new ViewOrientation3D(targetM, up, target));

        //                                        //view.get_Parameter(BuiltInParameter.VIEW_DETAIL_LEVEL).Set(3);

        //                                        //view.get_Parameter(BuiltInParameter
        //                                        //  .MODEL_GRAPHICS_STYLE).Set(6);

        //                                        t.Commit();

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

        ExternalCommandData m_commandData = null;

        internal static Application m_app = null;
        internal static DocumentSet m_docs = null;
        private const string m_pfvtype = "PfVType";
        private const string m_pfvTypeDirectShape = "DirectShape";
        private const string m_pfvTypeInstance = "Instance";

        private static Dictionary<string, List<PfVElementData>> CollectPfVs(
          ExternalCommandData commandData,
          ref string message,
          ElementSet highlightElements)
        {
            BUYLTools.Configuration.Manager _confMan = new BUYLTools.Configuration.Manager(typeof(PfVTools).Assembly, true);
            m_app = commandData.Application.Application;
            m_docs = m_app.Documents;

            Document myDoc = commandData.Application.ActiveUIDocument.Document;

            // Retrieve lighting fixture element
            // data from linked documents:

            Dictionary<string, List<PfVElementData>> data = new Dictionary<string, List<PfVElementData>>();

            using (Transaction trans = new Transaction(commandData.Application.ActiveUIDocument.Document))
            {
                trans.Start("PfV transaction");
                try
                {
                    string sTypeName = _confMan.GetValueForAppsetting(m_pfvtype);
                    foreach (Document docLink in m_docs)
                    {
                        if (docLink.PathName == myDoc.PathName)
                            continue;

                        if (docLink.IsLinked)
                        {
                            ICollection<ElementId> ids = new List<ElementId>();
                            FilteredElementCollector filCollector = null;

                            ICollection<ElementId> idsLinkedPfV = new List<ElementId>();

                            if (!data.ContainsKey(docLink.PathName))
                                data.Add(docLink.PathName, new List<PfVElementData>());

                            if (sTypeName == m_pfvTypeDirectShape)
                            {
                                filCollector = Utils.Util.GetElementsOfType(docLink, typeof(DirectShape), BuiltInCategory.OST_GenericModel);
                                GetPfVDataFromDirectShape(data, docLink, ids, filCollector, idsLinkedPfV);
                            }
                            else if (sTypeName == m_pfvTypeInstance)
                            {
                                filCollector = Utils.Util.GetElementsOfType(docLink, typeof(Instance), BuiltInCategory.OST_GenericModel);
                                GetPfVDataFromInstance(data, docLink, ids, filCollector, idsLinkedPfV);

                                ICollection<ElementId> idClash = null;

                                foreach (Instance inst in filCollector)
                                {
                                    idClash = FindElementsInterferingWithPfV(inst, myDoc);
                                }
                            }

                            //CopyLinkedElementsToCurrentModel(commandData, data, docLink, idsLinkedPfV);
                        }
                    }

                    myDoc.Regenerate();

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

        private static ICollection<ElementId> FindElementsInterferingWithPfV(Instance pfvInstance, Document hostdoc)
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

        private static void CopyLinkedElementsToCurrentModel(ExternalCommandData commandData, Dictionary<string, List<PfVElementData>> data, Document doc, ICollection<ElementId> idsLinkedPfV)
        {
            try
            {
                CopyPasteOptions copyOptions = new CopyPasteOptions();
                copyOptions.SetDuplicateTypeNamesHandler(new CopyUseDestination());

                ICollection<ElementId> idsLocalPfV = ElementTransformUtils.CopyElements(doc, idsLinkedPfV, commandData.Application.ActiveUIDocument.Document, null, copyOptions);

                foreach (PfVElementData pfv in data[doc.PathName])
                {
                    foreach (ElementId idnew in idsLocalPfV)
                    {
                        Element el = commandData.Application.ActiveUIDocument.Document.GetElement(idnew);
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

        private static void GetPfVDataFromInstance(Dictionary<string, List<PfVElementData>> data, Document doc, ICollection<ElementId> ids, FilteredElementCollector coll, ICollection<ElementId> idsLinkedPfV)
        {
            //foreach (DirectShape dShape in a)
            foreach (Instance inst in coll)
            {
                string name = inst.Name;
                if (IsProvisionForVoid(inst))
                {
                    idsLinkedPfV.Add(inst.Id);

                    PfVElementData pfv = new PfVElementData(doc.PathName, inst.Name, inst.Id.IntegerValue, inst.UniqueId);
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

                        ids.Add(inst.Id);
                    }
                    catch (Exception ex)
                    {
                    }

                    data[doc.PathName].Add(pfv);
                }
            }
        }

        private static void GetPfVDataFromDirectShape(Dictionary<string, List<PfVElementData>> data, Document doc, ICollection<ElementId> ids, FilteredElementCollector a, ICollection<ElementId> idsLinkedPfV)
        {
            foreach (DirectShape dShape in a)
            {
                string name = dShape.Name;
                if (IsProvisionForVoid(dShape))
                {
                    idsLinkedPfV.Add(dShape.Id);

                    PfVElementData pfv = new PfVElementData(doc.PathName, dShape.Name, dShape.Id.IntegerValue, dShape.UniqueId);
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

                        ids.Add(dShape.Id);
                    }
                    catch (Exception ex)
                    {
                    }

                    data[doc.PathName].Add(pfv);
                }
            }
        }

        private static void FindPfvLocation(Element instPfvLink, PfVElementData pfv)
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
                            Utils.GetCentroid.Centroid.CentroidVolume vol = Utils.GetCentroid.Centroid.GetCentroid(geomSolid);
                            if(vol != null)
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
                        }
                    }
                }
            }

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

        private static bool IsProvisionForVoid(Element e)
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
    }

    public class CopyUseDestination : IDuplicateTypeNamesHandler
    {
        public DuplicateTypeAction OnDuplicateTypeNamesFound(
        DuplicateTypeNamesHandlerArgs args)
        {
            return DuplicateTypeAction.UseDestinationTypes;
        }
    }
}