using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BUYLRevit.CcTools
{
    public static class CcTools
    {
        const string m_ClassificationCode = "Typ_VDI2552";
        public static Result StartApplyManufacturerProcess(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result res = Result.Succeeded;

            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            try
            {
                string selectMessage = "Select an element for applying manufacturer data";
                Reference r = uidoc.Selection.PickObject(ObjectType.Element, selectMessage);

                if (r != null)
                {
                    Element el = uidoc.Document.GetElement(r);
                    if (el is FamilyInstance)
                    {
                        FamilyInstance inst = el as FamilyInstance;

                        string code = null;

                        FamilySymbol sym = inst.Symbol;
                        List<Parameter> param = (List<Parameter>)sym.GetOrderedParameters();
                        foreach (Parameter p in param)
                        {
                            if (p.Definition.Name == m_ClassificationCode)
                            {
                                code = p.AsString();
                                break;
                            }
                        }

                        if (!String.IsNullOrEmpty(code))
                        {
                            CafmConnect.Manufacturer.UI.ViewDLG view = new CafmConnect.Manufacturer.UI.ViewDLG();
                            CafmConnect.Manufacturer.UI.Presenter pr = new CafmConnect.Manufacturer.UI.Presenter(view);

                            pr.StartSelection(code);

                            CafmConnect.Manufacturer.Model.CcManufacturerProduct prd = pr.GetSelectedProduct();
                            if (prd != null)
                                EditFamilyTypes(uidoc.Document, inst, prd);
                        }
                    }
                    else
                        System.Windows.Forms.MessageBox.Show("Current implementation works for family instances only!");
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
            }

            return res;
        }

        private static void EditFamilyTypes(Document maindocument, FamilyInstance familyInstance, CafmConnect.Manufacturer.Model.CcManufacturerProduct prd)
        {
            if ((null == maindocument) || (null == familyInstance.Symbol))
                return;   // invalid arguments

            // Get family associated with this
            Family family = familyInstance.Symbol.Family;
            if (null == family)
                return;    // could not get the family

            // Get Family document for family
            Document familyDoc = maindocument.EditFamily(family);
            if (null == familyDoc)
                return;    // could not open a family for edit

            FamilyManager familyManager = familyDoc.FamilyManager;
            if (null == familyManager)
                return;  // could not get a family manager

            // Start transaction for the family document
            using (Transaction newFamilyTypeTransaction = new Transaction(familyDoc, "Add Type to Family"))
            {
                bool changesMade = false;
                newFamilyTypeTransaction.Start();

                // add a new type and edit its parameters
                FamilyType newFamilyType = familyManager.NewType(prd.Name);

                if (newFamilyType != null)
                {
                    familyManager.CurrentType = newFamilyType;
                    List<FamilyParameter> paras = familyManager.GetParameters() as List<FamilyParameter>;

                    PrintParameterNames(paras);

                    foreach (CafmConnect.Manufacturer.Model.CcManufacturerProductDetail detail in prd.Attributes)
                    {
                        if (detail.AttributeValue != null)
                        {
                            try
                            {
                                FamilyParameter familyParam = null;

                                foreach (FamilyParameter p in paras)
                                {
                                    if (p.Definition.Name == detail.AttributeName)
                                    {
                                        familyParam = p;
                                        break;
                                    }
                                }

                                if (null != familyParam)
                                {
                                    changesMade = SetParameterValue(familyManager, familyParam, detail.AttributeValue);
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }

                if (changesMade)
                {
                    newFamilyTypeTransaction.Commit();
                }
                else
                {
                    newFamilyTypeTransaction.RollBack();
                }

                // if could not make the change or could not commit it, we return
                if (newFamilyTypeTransaction.GetStatus() != TransactionStatus.Committed)
                {
                    return;
                }
            }

            // now update the Revit project with Family which has a new type
            LoadOpts loadOptions = new LoadOpts();

            //familyDoc.SaveAs(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), Path.GetFileName(familyDoc.PathName)));

            // This overload is necessary for reloading an edited family
            // back into the source document from which it was extracted
            family = familyDoc.LoadFamily(maindocument, loadOptions);

            if (null != family)
            {
                FamilySymbol familySymbol = null;
                bool assignToMany = false;

                // find the new type and assign it to FamilyInstance
                ISet<ElementId> familySymbolIds = family.GetFamilySymbolIds();
                foreach (ElementId id in familySymbolIds)
                {
                    familySymbol = family.Document.GetElement(id) as FamilySymbol;
                    if ((null != familySymbol) && familySymbol.Name == prd.Name)
                    {
                        using (Transaction changeSymbol = new Transaction(maindocument, "Change Symbol Assignment"))
                        {
                            changeSymbol.Start();

                            familySymbol.Activate();
                            familyInstance.Symbol = familySymbol;

                            changeSymbol.Commit();
                        }

                        assignToMany = true;
                        break;
                    }
                }


                if (assignToMany)
                {
                    //familyInstance.Symbol = familySymbol;

                    ElementClassFilter filter = new ElementClassFilter(typeof(FamilyInstance));
                    FilteredElementCollector collector = new FilteredElementCollector(maindocument);
                    collector.WherePasses(filter);

                    //var query = from element in collector
                    //            where (element as FamilyInstance).Symbol == familyInstance.Symbol
                    //            select element;
                    //List<FamilyInstance> instances = query.Cast<FamilyInstance>().ToList<FamilyInstance>();

                    //using (Transaction changeSymbol = new Transaction(maindocument, "Change Symbol Assignment"))
                    //{
                    //    changeSymbol.Start();

                    //    familySymbol.Activate();
                    //    foreach (FamilyInstance inst in instances)
                    //    {
                    //        inst.Symbol = familySymbol;
                    //    }

                    //    changeSymbol.Commit();
                    //}
                }
            }
        }

        private static void PrintParameterNames(List<FamilyParameter> paras)
        {
#if DEBUG
            foreach (FamilyParameter p in paras)
            {
                Debug.WriteLine(p.Definition.Name);
            }
#endif
        }

        private static bool SetParameterValue(FamilyManager famman, FamilyParameter par, object val)
        {
            bool result = false;
            try
            {
                if (val != null)
                {
                    switch (par.StorageType)
                    {
                        case StorageType.None:
                            break;

                        case StorageType.Double:
                            if (val.GetType().Equals(typeof(string)))
                            {
                                double d = Utils.MathUtils.MToFeet(double.Parse(val as string));
                                famman.SetValueString(par, val.ToString() + " m");
                                result = true;
                            }
                            else
                            {
                                double d = Utils.MathUtils.MToFeet(Convert.ToDouble(val));
                                famman.Set(par, d);
                                result = true;
                            }
                            break;

                        case StorageType.Integer:
                            if (val.GetType().Equals(typeof(string)))
                            {
                                famman.Set(par, int.Parse(val as string));
                                result = true;
                            }
                            else
                            {
                                famman.Set(par, Convert.ToInt32(val));
                                result = true;
                            }
                            break;

                        case StorageType.ElementId:
                            if (val.GetType().Equals(typeof(ElementId)))
                            {
                                famman.Set(par, val as ElementId);
                                result = true;
                            }
                            else if (val.GetType().Equals(typeof(string)))
                            {
                                famman.Set(par, new ElementId(int.Parse(val as string)));
                                result = true;
                            }
                            else
                            {
                                famman.Set(par, new ElementId(Convert.ToInt32(val)));
                                result = true;
                            }
                            break;

                        case StorageType.String:
                            famman.Set(par, val.ToString());
                            result = true;

                            break;
                    }
                }
            }
            catch
            {
                throw new Exception("Invalid Value Input!");
            }

            return result;
        }

        private class LoadOpts : IFamilyLoadOptions
        {
            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                overwriteParameterValues = true;
                return true;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                source = FamilySource.Family;
                overwriteParameterValues = true;
                return true;
            }
        }
    }
}