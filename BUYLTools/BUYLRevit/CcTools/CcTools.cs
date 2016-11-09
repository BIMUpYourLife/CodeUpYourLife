using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;

namespace BUYLRevit.CcTools
{
    public static class CcTools
    {
        public static Result StartProductProcess(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result res = Result.Succeeded;

            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            using (Transaction trans = new Transaction(uidoc.Document, "Apply manufacturer"))
            {
                try
                {
                    trans.Start();

                    string selectMessage = "Select an element to apply manufacturer";
                    Reference r = uidoc.Selection.PickObject(ObjectType.Element, selectMessage);

                    if (r != null)
                    {
                        Element el = uidoc.Document.GetElement(r);
                        if (el is FamilyInstance)
                        {
                            FamilyInstance inst = el as FamilyInstance;

                            string code = null;

                            Dictionary<string, Parameter> paramnames = new Dictionary<string, Parameter>();


                            FamilySymbol sym = inst.Symbol;
                            List<Parameter> param = (List<Parameter>)sym.GetOrderedParameters();
                            foreach (Parameter p in param)
                            {
                                paramnames.Add(p.Definition.Name, p);

                                if (p.Definition.Name == "Typ_VDI2552")
                                {
                                    code = p.AsString();
                                    break;
                                }
                            }

                            if (!String.IsNullOrEmpty(code))
                            {
                                CafmConnect.Manufacturer.UI.Presenter pr = new CafmConnect.Manufacturer.UI.Presenter();
                                pr.StartSelection(code);
                                CafmConnect.Manufacturer.Model.CcManufacturerProduct prd = pr.GetSelectedProduct();
                                if (prd != null)
                                {
                                    sym.Name = prd.Name;
                                    foreach (CafmConnect.Manufacturer.Model.CcManufacturerProductDetail detail in prd.Attributes)
                                    {
                                        if (paramnames.ContainsKey(detail.AttributeName))
                                            paramnames[detail.AttributeName].Set(Utils.MathUtils.MToFeet(Double.Parse(detail.AttributeValue)));
                                    }
                                }
                            }
                        }

                        trans.Commit();
                    }
                    else
                    {
                        trans.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    trans.Dispose();
                }
            }

            return res;
        }
    }
}