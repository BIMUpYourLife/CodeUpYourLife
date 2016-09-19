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
        public static void AddFaceBasedFamilyToLinks(Document doc, ElementId idEle)
        {
            ElementId alignedLinkId = idEle;

            // Get symbol

            ElementId symbolId = new ElementId(126580);

            FamilySymbol fs = doc.GetElement(symbolId)
              as FamilySymbol;

            // Aligned

            RevitLinkInstance linkInstance = doc.GetElement(
              alignedLinkId) as RevitLinkInstance;

            Document linkDocument = linkInstance
              .GetLinkDocument();

            FilteredElementCollector wallCollector
              = new FilteredElementCollector(linkDocument);

            wallCollector.OfClass(typeof(Wall));

            Wall targetWall = wallCollector.FirstElement()
              as Wall;

            Reference exteriorFaceRef
              = HostObjectUtils.GetSideFaces(
                targetWall, ShellLayerType.Exterior)
                  .First<Reference>();

            Reference linkToExteriorFaceRef
              = exteriorFaceRef.CreateLinkReference(
                linkInstance);

            Line wallLine = (targetWall.Location
              as LocationCurve).Curve as Line;

            XYZ wallVector = (wallLine.GetEndPoint(1)
              - wallLine.GetEndPoint(0)).Normalize();

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Add to face");

                doc.Create.NewFamilyInstance(
                  linkToExteriorFaceRef, XYZ.Zero,
                  wallVector, fs);

                t.Commit();
            }
        }
        #endregion // AddFaceBasedFamilyToLinks

    }
}
