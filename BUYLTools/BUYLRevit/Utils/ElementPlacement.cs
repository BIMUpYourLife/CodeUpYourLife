using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUYLRevit.Utils
{
    public class ElementPlacement
    {
        #region AddFaceBasedFamilyToLinks
        //public static void AddFaceBasedFamilyToLinkedWall(Document doc, ElementId linkDocId, ElementId symbolId)
        //{
        //    // Get symbol
        //    FamilySymbol fs = doc.GetElement(symbolId) as FamilySymbol;

        //    if (fs == null)
        //        return;

        //    // Aligned

        //    RevitLinkInstance linkInstance = doc.GetElement(linkDocId) as RevitLinkInstance;

        //    Document linkDocument = linkInstance.GetLinkDocument();

        //    FilteredElementCollector wallCollector = new FilteredElementCollector(linkDocument);

        //    wallCollector.OfClass(typeof(Wall));

        //    Wall targetWall = wallCollector.FirstElement() as Wall;

        //    Reference exteriorFaceRef = HostObjectUtils.GetSideFaces( targetWall, ShellLayerType.Exterior).First<Reference>();

        //    Reference linkToExteriorFaceRef = exteriorFaceRef.CreateLinkReference( linkInstance);

        //    Line wallLine = (targetWall.Location as LocationCurve).Curve as Line;

        //    XYZ wallVector = (wallLine.GetEndPoint(1) - wallLine.GetEndPoint(0)).Normalize();

        //    using (Transaction t = new Transaction(doc))
        //    {
        //        t.Start("Add to face");

        //        doc.Create.NewFamilyInstance( linkToExteriorFaceRef, XYZ.Zero, wallVector, fs);

        //        t.Commit();
        //    }
        //}
        #endregion // AddFaceBasedFamilyToLinks

        //public static FamilyInstance AddFaceBasedFamilyToWall(Document doc, ElementId wallId, ElementId symbolId, XYZ location)
        //{
        //    // Get symbol
        //    FamilySymbol fs = doc.GetElement(symbolId) as FamilySymbol;

        //    FamilyInstance inst = null;

        //    if (fs == null)
        //        return null;

        //    Element el = doc.GetElement(wallId);

        //    Wall targetWall = null;
        //    WallFoundation targetfoundation = null;

        //    if (el is HostObject)
        //    {
        //        Reference exteriorFaceRef = null;
        //        XYZ wallVector = null;

        //        if (el is Wall)
        //        {
        //            targetWall = el as Wall;

        //            if (targetWall != null)
        //            {
        //                exteriorFaceRef = HostObjectUtils.GetSideFaces(targetWall, ShellLayerType.Exterior).First<Reference>();
        //                wallVector = GetWallVector(targetWall);
        //            }
        //        }
        //        else if (el is WallFoundation)
        //        {
        //            targetfoundation = el as WallFoundation;
        //            if (targetfoundation != null)
        //            {
        //                exteriorFaceRef = HostObjectUtils.GetSideFaces(targetWall, ShellLayerType.Interior).First<Reference>();
        //                wallVector = GetWallFoundationVector(targetfoundation);
        //            }
        //        }

        //        if (exteriorFaceRef != null && wallVector != null)
        //        {
        //            using (SubTransaction t = new SubTransaction(doc))
        //            {
        //                t.Start();

        //                inst = doc.Create.NewFamilyInstance(exteriorFaceRef, location, wallVector, fs);

        //                try
        //                {
        //                    //SolidSolidCutUtils.AddCutBetweenSolids(doc, el, inst);
        //                }
        //                catch (Exception ex)
        //                {
        //                }

        //                t.Commit();
        //            }
        //        }
        //    }

        //    return inst;
        //}

        internal static FamilyInstance AddFaceBasedFamilyToHost(Document doc, string hostId, ElementId symbolId, XYZ location)
        {
            // Get symbol
            FamilySymbol fs = doc.GetElement(symbolId) as FamilySymbol;

            FamilyInstance inst = null;

            if (fs == null)
                return null;

            Element el = doc.GetElement(hostId);

            if (el == null)
                return inst;

            if (el is HostObject)
            {
                Reference faceRef = null;
                XYZ placementVector = null;

                try
                {
                    if (el is Floor)
                    {
                        Floor targetFloor = el as Floor;

                        if (targetFloor != null)
                        {
                            faceRef = HostObjectUtils.GetBottomFaces(targetFloor).First<Reference>();
                            placementVector = GetFloorVector();
                        }
                    }
                    else if(el is RoofBase)
                    {
                        RoofBase targetroof = el as RoofBase;
                        if(targetroof != null)
                        {
                            faceRef = HostObjectUtils.GetBottomFaces(targetroof).First<Reference>();
                            placementVector = GetFloorVector();
                        }
                    }
                    else if (el is Wall)
                    {
                        Wall targetWall = el as Wall;

                        if (targetWall != null)
                        {
                            faceRef = HostObjectUtils.GetSideFaces(targetWall, ShellLayerType.Interior).First<Reference>();
                            placementVector = GetWallVector(targetWall);
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(String.Format("Object type {0} currently not supported as host", el.GetType().ToString()));
                    }
                    //else if (el is WallFoundation)
                    //{
                    //    WallFoundation targetfoundation = el as WallFoundation;
                    //    if (targetfoundation != null)
                    //    {
                    //        faceRef = HostObjectUtils.GetSideFaces(targetfoundation, ShellLayerType.Exterior).First<Reference>();
                    //        placementVector = GetWallFoundationVector(targetfoundation);
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(String.Format("Error while retriving faces and vector of host : {0}", ex.Message));
                }

                if (faceRef != null && placementVector != null)
                {
                    using (SubTransaction t = new SubTransaction(doc))
                    {
                        try
                        {
                            t.Start();

                            inst = doc.Create.NewFamilyInstance(faceRef, location, placementVector, fs);

                            //SolidSolidCutUtils.AddCutBetweenSolids(doc, el, inst);

                            t.Commit();
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show(ex.Message);
                            t.RollBack();
                        }
                        finally
                        {
                        }
                    }
                }
            }
            else
            { System.Windows.Forms.MessageBox.Show("No suitable host object found!");}

            return inst;
        }

        //static PlanarFace GetTopFace(Document doc, Floor floor)
        //{
        //    Options opt = app.Create.NewGeometryOptions();
        //    GeoElement geo = floor.get_Geometry(opt);
        //    GeometryObjectArray objects = geo.Objects;
        //    foreach (GeometryObject obj in objects)
        //    {
        //        Solid solid = obj as Solid;
        //        if (solid != null)
        //        {
        //            PlanarFace f = GetTopFace(solid);
        //            if (null == f)
        //            {
        //                Debug.WriteLine(
        //                  Util.ElementDescription(floor)
        //                  + " has no top face.");
        //                ++nNullFaces;
        //            }
        //            topFaces.Add(f);
        //        }
        //    }
        //    PlanarFace topFace = null;
        //    FaceArray faces = solid.Faces;
        //    foreach (Face f in faces)
        //    {
        //        PlanarFace pf = f as PlanarFace;
        //        if (null != pf && Util.IsHorizontal(pf))
        //        {
        //            if ((null == topFace)|| (topFace.Origin.Z < pf.Origin.Z))
        //            {
        //                topFace = pf;
        //            }
        //        }
        //    }
        //    return topFace;
        //}

        private static XYZ GetWallVector(Wall targetWall)
        {
            Line wallLine = (targetWall.Location as LocationCurve).Curve as Line;

            XYZ wallVector = (wallLine.GetEndPoint(1) - wallLine.GetEndPoint(0)).Normalize();
            return wallVector;
        }

        private static XYZ GetFloorVector()
        {
            XYZ floorVector = new XYZ(0, -1, -1);//(floorLine.GetEndPoint(1) - floorLine.GetEndPoint(0)).Normalize();
            return floorVector;
        }

        private static XYZ GetWallFoundationVector(WallFoundation targetWall)
        {
            Line wallLine = (targetWall.Location as LocationCurve).Curve as Line;

            XYZ wallVector = (wallLine.GetEndPoint(1) - wallLine.GetEndPoint(0)).Normalize();
            return wallVector;
        }
    }
}
