using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUYLRevit.Utils
{
    public class ParameterExtensions
    {
        public static object GetParameterValue(string name, Element e)
        {
            object result = null;
            foreach (Parameter p in GetParameters(e))
            {
                if (p.Definition.Name == name)
                {
                    switch (p.StorageType)
                    {
                        case StorageType.None:
                            result = null;
                            break;
                        case StorageType.Integer:
                            int ival = 0;
                            if (Int32.TryParse(p.AsString(), out ival))
                                ival = ival != 0 ? p.AsInteger() : 0;
                            result = ival;
                            break;
                        case StorageType.Double:
                            double dval = p.AsDouble();
                            result = dval;
                            break;
                        case StorageType.String:
                        case StorageType.ElementId:
                            result = p.AsString();
                            break;
                        default:
                            result = null;
                            break;
                    }
                    break;
                }
            }

            return result;
        }

        public static List<Parameter> GetParameters(Element e)
        {
            List<Parameter> lst = new List<Parameter>();
            if (e is FamilyInstance)
            {
                FamilyInstance famInst = e as FamilyInstance;
                if (famInst != null)
                {
                    lst.AddRange(famInst.GetOrderedParameters());

                    FamilySymbol famSym = famInst.Symbol;
                    if (famSym != null)
                        lst.AddRange(famSym.GetOrderedParameters());

                    Family fam = famSym.Family;
                    if (fam != null)
                        lst.AddRange(fam.GetOrderedParameters());
                }
            }

            return lst;
        }

        public static bool SetFamilyParameterValue(FamilyManager famman, FamilyParameter par, object val)
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

    }
}
