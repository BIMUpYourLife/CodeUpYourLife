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
        public static void AddFaceBasedFamilyToLinkedWall(Document doc, ElementId linkDocId, ElementId symbolId)
        {
            // Get symbol
            FamilySymbol fs = doc.GetElement(symbolId) as FamilySymbol;

            if (fs == null)
                return;

            // Aligned

            RevitLinkInstance linkInstance = doc.GetElement(linkDocId) as RevitLinkInstance;

            Document linkDocument = linkInstance.GetLinkDocument();

            FilteredElementCollector wallCollector = new FilteredElementCollector(linkDocument);

            wallCollector.OfClass(typeof(Wall));

            Wall targetWall = wallCollector.FirstElement() as Wall;

            Reference exteriorFaceRef = HostObjectUtils.GetSideFaces( targetWall, ShellLayerType.Exterior).First<Reference>();

            Reference linkToExteriorFaceRef = exteriorFaceRef.CreateLinkReference( linkInstance);

            Line wallLine = (targetWall.Location as LocationCurve).Curve as Line;

            XYZ wallVector = (wallLine.GetEndPoint(1) - wallLine.GetEndPoint(0)).Normalize();

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Add to face");

                doc.Create.NewFamilyInstance( linkToExteriorFaceRef, XYZ.Zero, wallVector, fs);

                t.Commit();
            }
        }
        #endregion // AddFaceBasedFamilyToLinks

        public static FamilyInstance AddFaceBasedFamilyToWall(Document doc, ElementId wallId, ElementId symbolId, XYZ location)
        {
            // Get symbol
            FamilySymbol fs = doc.GetElement(symbolId) as FamilySymbol;

            FamilyInstance inst = null;

            if (fs == null)
                return null;

            // Aligned

            Element el = doc.GetElement(wallId);

            Wall targetWall = null;
            if(el is Wall)
               targetWall  = el as Wall;

            if (targetWall != null)
            {
                Reference exteriorFaceRef = HostObjectUtils.GetSideFaces(targetWall, ShellLayerType.Exterior).First<Reference>();

                Line wallLine = (targetWall.Location as LocationCurve).Curve as Line;

                XYZ wallVector = (wallLine.GetEndPoint(1) - wallLine.GetEndPoint(0)).Normalize();

                using (SubTransaction t = new SubTransaction(doc))
                {
                    t.Start();

                    inst = doc.Create.NewFamilyInstance(exteriorFaceRef, location, wallVector, fs);

                    try
                    {
                        SolidSolidCutUtils.AddCutBetweenSolids(doc, targetWall, inst);
                    }
                    catch (Exception ex)
                    {
                    }

                    t.Commit();
                }
            }

            return inst;
        }
    }
}
