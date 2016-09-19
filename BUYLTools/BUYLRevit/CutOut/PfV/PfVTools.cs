#region Header
//
// CmdLinkedFileElements.cs - list elements in linked files
//
// Copyright (C) 2009-2016 by Jeremy Tammik,
// Autodesk Inc. All rights reserved.
//
// Keywords: The Building Coder Revit API C# .NET add-in.
//
#endregion // Header

#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BUYLRevit.Utils;
using System.IO;
#endregion // Namespaces

namespace BUYLRevit.CutOut.PfV
{
    public class Position
    {
        double _x;
        double _y;
        double _z;

        public Position(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X
        {
            get
            {
                return _x;
            }

            set
            {
                _x = value;
            }
        }

        public double Y
        {
            get
            {
                return _y;
            }

            set
            {
                _y = value;
            }
        }

        public double Z
        {
            get
            {
                return _z;
            }

            set
            {
                _z = value;
            }
        }
        public override string ToString()
        {
            return String.Format("x : {0}; y : {1}; z : {2}", X, Y, Z);
        }
    }

    public class PfVElementData
    {
        string _document;
        string _elementName;
        int _idLinked;
        int _idLocal;
        string _uniqueIdLinked;
        string _uniqueIdLocal;
        string _folder;


        double _height;
        double _width;
        double _diameter;
        double _depth;
        string _ifcGuid;
        string _ifcName;
        string _ifcSpatialContainer;
        string _shape;
        string _system;
        Position _pos;

        public PfVElementData(
          string path,
          string elementName,
          int idLinked,
          string uniqueIdLinked)
        {
            int i = path.LastIndexOf("\\");
            _document = Path.GetFileName(path);
            _elementName = elementName;
            _idLinked = idLinked;
            _uniqueIdLinked = uniqueIdLinked;
            _folder = Path.GetDirectoryName(path);

            Pos = new Position(0, 0, 0);
        }


        public string Document
        {
            get { return _document; }
        }
        public string ElementName
        {
            get { return _elementName; }
        }
        public int IdLinked
        {
            get { return _idLinked; }
        }
        public int IdLocal
        {
            get { return _idLocal; }
            set { _idLocal = value; }
        }
        public string UniqueIdLinked
        {
            get { return _uniqueIdLinked; }
        }
        public string UniqueIdLocal
        {
            get { return _uniqueIdLocal; }
            set { _uniqueIdLocal = value; }
        }
        public string Folder
        {
            get { return _folder; }
        }

        public double Height
        {
            get
            {
                return _height;
            }

            set
            {
                _height = value;
            }
        }

        public double Width
        {
            get
            {
                return _width;
            }

            set
            {
                _width = value;
            }
        }

        public double Diameter
        {
            get
            {
                return _diameter;
            }

            set
            {
                _diameter = value;
            }
        }

        public double Depth
        {
            get
            {
                return _depth;
            }

            set
            {
                _depth = value;
            }
        }

        public string IfcGuid
        {
            get
            {
                return _ifcGuid;
            }

            set
            {
                _ifcGuid = value;
            }
        }

        public string IfcName
        {
            get
            {
                return _ifcName;
            }

            set
            {
                _ifcName = value;
            }
        }

        public string IfcSpatialContainer
        {
            get
            {
                return _ifcSpatialContainer;
            }

            set
            {
                _ifcSpatialContainer = value;
            }
        }

        public string Shape
        {
            get
            {
                return _shape;
            }

            set
            {
                _shape = value;
            }
        }

        public string System
        {
            get
            {
                return _system;
            }

            set
            {
                _system = value;
            }
        }

        public Position Pos
        {
            get
            {
                return _pos;
            }

            set
            {
                _pos = value;
            }
        }
    }

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
                dlg.ShowDialog();

                return Result.Succeeded;
            }
            else
                return Result.Failed;

        }

        private static Dictionary<string, List<PfVElementData>> CollectPfVs(
          ExternalCommandData commandData,
          ref string message,
          ElementSet highlightElements)
        {
            /*

            // retrieve all link elements:

            Document doc = app.ActiveUIDocument.Document;
            List<Element> links = GetElements(
              BuiltInCategory.OST_RvtLinks,
              typeof( Instance ), app, doc );

            // determine the link paths:

            DocumentSet docs = app.Documents;
            int n = docs.Size;
            Dictionary<string, string> paths
              = new Dictionary<string, string>( n );

            foreach( Document d in docs )
            {
              string path = d.PathName;
              int i = path.LastIndexOf( "\\" ) + 1;
              string name = path.Substring( i );
              paths.Add( name, path );
            }
            */

            // Retrieve lighting fixture element
            // data from linked documents:

            Dictionary<string, List<PfVElementData>> data = new Dictionary<string, List<PfVElementData>>();
            UIApplication app = commandData.Application;
            DocumentSet docs = app.Application.Documents;

            using (Transaction trans = new Transaction(commandData.Application.ActiveUIDocument.Document))
            {
                trans.Start("PfV transaction");

                foreach (Document doc in docs)
                {
                    if (doc.IsLinked)
                    {
                        ICollection<ElementId> ids = new List<ElementId>();
                        //FilteredElementCollector a = Utils.Util.GetElementsOfType(doc, typeof(DirectShape), BuiltInCategory.OST_GenericModel);
                        FilteredElementCollector a = Utils.Util.GetElementsOfType(doc, typeof(Instance), BuiltInCategory.OST_GenericModel);
                        //foreach (DirectShape dShape in a)

                        if (!data.ContainsKey(doc.PathName))
                            data.Add(doc.PathName, new List<PfVElementData>());

                        ICollection<ElementId> idsLinkedPfV = new List<ElementId>();
                        foreach (Instance inst in a)
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

                                    // Find the location and direction
                                    //Autodesk.Revit.DB.Options opt = new Options();
                                    //Autodesk.Revit.DB.GeometryElement geomElem = inst.get_Geometry(opt);
                                    //foreach (GeometryObject geomObj in geomElem)
                                    //{
                                    //    Solid geomSolid = geomObj as Solid;
                                    //    if (null != geomSolid)
                                    //    {
                                    //        XYZ p= geomSolid.ComputeCentroid();
                                    //        pfv.Pos = new Position(p.X, p.Y, p.Z);
                                    //        break;
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

                                    ids.Add(inst.Id);
                                }
                                catch (Exception ex)
                                {
                                }

                                data[doc.PathName].Add(pfv);
                            }
                        }

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
                }

                commandData.Application.ActiveUIDocument.Document.Regenerate();

                trans.Commit();
            }

            return data;
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
