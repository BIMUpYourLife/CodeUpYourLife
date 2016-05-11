using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BUYLTools.Data;
using System.IO;

namespace BUYLRevit
{
    public static class ParameterUtils
    {
        private const string m_Instance = "Instance";
        private const string m_NotAvailable = "NA";

        public static void RevitUpdateSingleFamily(Document doc, ref CadDev.LogManager.LogDLG _logDlg, familiesFamilylistFamily fam)
        {
            if (fam != null)//We have found the ficture in xml structure
            {
                _logDlg.AddLogEntry(String.Format("Family {0} will be processed", GlobalVars.GetDataSet().GetArticleNumberFromFilename(doc.PathName, ref _logDlg)), CadDev.LogManager.LogLevel.Information, null);

                FamilyManager mgr = doc.FamilyManager;
                int n = mgr.Parameters.Size;
                Dictionary<string, FamilyParameter> fps = new Dictionary<string, FamilyParameter>(n);

                RevitRetrieveParameters(mgr, ref fps, ref _logDlg);

                if (fps.Count > 0)
                {
                    //Set all instance parameters
                    UpdaterSetUpInstances(fps, mgr, fam, ref _logDlg);

                    //Set all type parameters
                    UpdaterSetUpTypes(mgr, doc, fps, fam, ref _logDlg);
                }
            }
            else
            {
                _logDlg.AddLogEntry(String.Format("Family {0} can't be casted", GlobalVars.GetDataSet().GetArticleNumberFromFilename(doc.PathName, ref _logDlg)), CadDev.LogManager.LogLevel.Critical, null);
            }
        }

        private static bool CheckForImage(string p, FamilyManager fm, Document doc, out string id, bool load = true)
        {
            bool exists = false;
            id = "";
            FilteredElementCollector families = new FilteredElementCollector(doc).OfClass(typeof(ImageType));

            foreach (ImageType f in families)
            {
                if (Path.GetFileName(p).Equals(Path.GetFileName(f.Path)))
                {
                    id = f.Id.ToString();
                    exists = true;
                    break;
                }
                //str = str + f.Name + "\n";

                // ...
            }

            if (!exists && load)
            {
                //Try to load the image into the current family
                string path = GlobalVars.ConfigManager.MyConfiguration.AppSettings.Settings["imagesourcepath"].Value;
                string imagepath = Path.Combine(path, Path.GetFileName(p));
                if (File.Exists(imagepath))
                {
                    ImageType im = ImageType.Create(doc, imagepath);
                    id = im.Id.ToString();
                    exists = true;
                }
            }

            return exists;
        }

        private static double GetElectrical_Potential(object value)
        {
            double result = 0;
            //Try to find a replacement in app.config
            string source = GlobalVars.ConfigManager.MyConfiguration.AppSettings.Settings["Electrical_Potential_Mapping"].Value;
            string[] sources = source.Split('|');

            foreach (string item in sources)
            {
                string[] subitems = item.Split(';');
                if (subitems.Length == 2)
                {
                    if (subitems[0].Equals(value))
                        if (double.TryParse(subitems[1], out result))
                            break;
                }
            }

            return result;
        }

        private static bool IsParameterValueImage(string p)
        {
            bool isImage = false;
            if (!p.Contains(Path.GetInvalidPathChars().ToString()) && !p.Contains(Path.GetInvalidFileNameChars().ToString()))
            {
                try
                {
                    string extension = Path.GetExtension(p);
                    switch (extension)
                    {
                        case ".jpg":
                        case ".jpeg":
                        case ".bmp":
                        case ".png":
                        case ".tif":
                            isImage = true;
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return isImage;
        }

        private static void UpdaterSetGlobalInstanceParameterOverrides(Dictionary<string, FamilyParameter> fps, FamilyManager mgr, familiesFamilylistFamily fixt, ref CadDev.LogManager.LogDLG log)
        {
            foreach (param instparam in GlobalVars.GetDataSet().globalparams)
            {
                foreach (FamilyParameter param in fps.Values)
                {
                    try
                    {
                        if (param.Definition.Name.Equals(instparam.name))
                        {
                            if (param.IsInstance)
                            {
                                if (instparam.fx == "1")
                                {
                                    RevitRawSetFamilyParameterFormula(mgr, m_Instance, param, instparam.GetDecodedValueString(), ref log);
                                    break;
                                }
                                else
                                {
                                    RevitRawSetFamilyParameterValue(mgr, m_Instance, param, instparam.GetDecodedValueString(), ref log);
                                    break;
                                }
                            }
                            //else
                            //{
                            //    log.AddLogEntry(String.Format("Parameter {0} is not an instance parameter! Can't apply value!", instparam.name), CadDev.LogManager.LogLevel.Critical, null);
                            //    paramfound = true;
                            //    break;
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        log.AddLogEntry(String.Format("Error while setting instance parameter {0}:\n{1}", instparam.name, ex.Message), CadDev.LogManager.LogLevel.Critical, null);
                    }
                }
            }
        }

        private static void UpdaterSetGlobalTypeParameterOverrides(FamilyManager fm, Document doc, familiesFamilylistFamilyTypes itemTypes, familiesFamilylistFamilyTypesType item, FamilyType t, ref CadDev.LogManager.LogDLG log)
        {
            foreach (param globalparamoverrides in GlobalVars.GetDataSet().globalparams)
            {
                try
                {
                    FamilyParameter fp = RevitRawFindFamilyParameter(fm, globalparamoverrides.name, ref log);
                    if (fp != null)
                    {
                        if (!fp.IsInstance)
                        {
                            if (IsParameterValueImage(globalparamoverrides.GetDecodedValueString()))
                            {
                                string id = "";
                                if (CheckForImage(globalparamoverrides.GetDecodedValueString(), fm, doc, out id, true))
                                    RevitRawSetFamilyParameterValue(fm, t, fp, id, ref log);
                                else
                                    log.AddLogEntry(String.Format("Error while try loading image {0} for parameter {1}", globalparamoverrides.GetDecodedValueString(), globalparamoverrides.name), CadDev.LogManager.LogLevel.Critical, null);
                            }
                            else
                                RevitRawSetFamilyParameterValue(fm, t, fp, globalparamoverrides.GetDecodedValueString(), ref log);
                        }
                        else
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    log.AddLogEntry(String.Format("Error while updating global type parameter ovverrides {0} : \n{1}", globalparamoverrides.name, ex.Message), CadDev.LogManager.LogLevel.Critical, null);
                }
            }
        }

        private static void UpdaterSetInstanceParameters(Dictionary<string, FamilyParameter> fps, FamilyManager mgr, familiesFamilylistFamily fixt, ref CadDev.LogManager.LogDLG log)
        {
            bool paramfound = false;

            foreach (param instparam in fixt.inst)
            {
                foreach (FamilyParameter param in fps.Values)
                {
                    try
                    {
                        if (param.Definition.Name.Equals(instparam.name))
                        {
                            if (param.IsInstance)
                            {
                                if (instparam.fx == "1")
                                {
                                    RevitRawSetFamilyParameterFormula(mgr, m_Instance, param, instparam.GetDecodedValueString(), ref log);
                                    paramfound = true;
                                    break;
                                }
                                else
                                {
                                    RevitRawSetFamilyParameterValue(mgr, m_Instance, param, instparam.GetDecodedValueString(), ref log);
                                    paramfound = true;
                                    break;
                                }
                            }
                            else
                            {
                                log.AddLogEntry(String.Format("Parameter {0} is not an instance parameter! Can't apply value!", instparam.name), CadDev.LogManager.LogLevel.Critical, null);
                                paramfound = true;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.AddLogEntry(String.Format("Error while setting instance parameter {0}:\n{1}", instparam.name, ex.Message), CadDev.LogManager.LogLevel.Critical, null);
                    }
                }
                if (!paramfound)
                    log.AddLogEntry(String.Format("Parameter {0} not found in family parameters", instparam.name), CadDev.LogManager.LogLevel.Critical, null);
                else
                    break;
            }
        }

        private static void UpdaterSetGlobalTypeParameters(FamilyManager fm, Document doc, familiesFamilylistFamilyTypes itemTypes, familiesFamilylistFamilyTypesType item, FamilyType t, ref CadDev.LogManager.LogDLG log)
        {
            foreach (param globalparam in itemTypes.globaltypeparams)
            {
                try
                {
                    FamilyParameter fp = RevitRawFindFamilyParameter(fm, globalparam.name, ref log);
                    if (fp != null)
                    {
                        if (IsParameterValueImage(globalparam.GetDecodedValueString()))
                        {
                            string id = "";
                            if (CheckForImage(globalparam.GetDecodedValueString(), fm, doc, out id, true))
                                RevitRawSetFamilyParameterValue(fm, t, fp, id, ref log);
                        }
                        else
                            RevitRawSetFamilyParameterValue(fm, t, fp, globalparam.GetDecodedValueString(), ref log);
                    }
                }
                catch (Exception ex)
                {
                    log.AddLogEntry(String.Format("Error while updating global type parameter {0} : \n{1}", globalparam.name, ex.Message), CadDev.LogManager.LogLevel.Critical, null);
                }
            }
        }

        private static void UpdaterSetTypeParameters(FamilyManager fm, ref CadDev.LogManager.LogDLG log, familiesFamilylistFamilyTypesType fixtType, FamilyType t)
        {
            foreach (param paramType in fixtType.param)
            {
                try
                {
                    FamilyParameter fp = RevitRawFindFamilyParameter(fm, paramType.name, ref log);
                    if (fp != null)
                        RevitRawSetFamilyParameterValue(fm, t, fp, paramType.GetDecodedValueString(), ref log);
                }
                catch (Exception ex)
                {
                    log.AddLogEntry(String.Format("Error while updating type parameter {0} : \n{1}", paramType.name, ex.Message), CadDev.LogManager.LogLevel.Critical, null);
                }
            }

            UpdaterSetTypeParametersUpdateDate(fm, fixtType, t, ref log);
        }

        private static void UpdaterSetTypeParametersUpdateDate(FamilyManager fm, familiesFamilylistFamilyTypesType fixtType, FamilyType t, ref CadDev.LogManager.LogDLG log)
        {
            FamilyParameter fp = RevitRawFindFamilyParameter(fm, GlobalVars.GetUpdateValue(), ref log);
            if (fp != null)
                RevitRawSetFamilyParameterValue(fm, t, fp, GlobalVars.GetDataSet().GetUpdataDate(ref log), ref log);
        }

        private static void UpdaterSetUpInstances(Dictionary<string, FamilyParameter> fps, FamilyManager mgr, familiesFamilylistFamily fixt, ref CadDev.LogManager.LogDLG log)
        {
            UpdaterSetInstanceParameters(fps, mgr, fixt, ref log);
            UpdaterSetGlobalInstanceParameterOverrides(fps, mgr, fixt, ref log);
        }

        private static void UpdaterSetUpTypes(FamilyManager fm, Document doc, Dictionary<string, FamilyParameter> fps, familiesFamilylistFamily fixt, ref CadDev.LogManager.LogDLG log)
        {
            if (fm.Types.Size >= 0)
            {
                List<string> typeNames = new List<string>();

                foreach (familiesFamilylistFamilyTypes itemTypes in fixt.types)
                {
                    //Create new types, update existing ones
                    foreach (familiesFamilylistFamilyTypesType item in itemTypes.type)
                    {
                        typeNames.Add(item.name);

                        FamilyType t = GetFamilyType(fm, item.name, ref log);
                        if (t != null)
                        {
                            UpdaterSetTypeParameters(fm, ref log, item, t);
                            UpdaterSetGlobalTypeParameters(fm, doc, itemTypes, item, t, ref log);
                            UpdaterSetGlobalTypeParameterOverrides(fm, doc, itemTypes, item, t, ref log);
                        }
                    }

                }

                //delete types that doesn't exist any more in database export
                DeleteUnusedFamilyTypes(fm, typeNames, ref log);
            }
        }

        private static void DeleteUnusedFamilyTypes(FamilyManager fm, List<string> typeNames, ref CadDev.LogManager.LogDLG log)
        {
            List<string> currentTypeNames = new List<string>();

            //collect current names
            foreach (FamilyType item in fm.Types)
            {
                currentTypeNames.Add(item.Name);
            }

            foreach (string name in currentTypeNames)
            {
                if (!typeNames.Contains(name))
                {
                    FamilyType t = GetFamilyType(fm, name, ref log);
                    if (t != null)
                    {
                        fm.CurrentType = t;
                        fm.DeleteCurrentType();
                    }
                }
            }
        }

        private static FamilyType GetFamilyType(FamilyManager fm, string name, ref CadDev.LogManager.LogDLG log)
        {
            FamilyType t = null;

            foreach (FamilyType tp in fm.Types)
            {
                try
                {
                    if (name == tp.Name)
                    {
                        t = tp;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    log.AddLogEntry(ex.Message, CadDev.LogManager.LogLevel.Critical, null);
                }
            }

            if (t == null)
                t = fm.NewType(name);

            return t;
        }

        public static FamilyParameter RevitRawFindFamilyParameter(FamilyManager fm, string parameterName, ref CadDev.LogManager.LogDLG log)
        {
            //FamilyParameter fp = RawConvertSetToList<FamilyParameter>(fm.Parameters).FirstOrDefault(e => e.Definition.Name.Equals(parameterName, StringComparison.CurrentCultureIgnoreCase));
            FamilyParameter fp = null;
            foreach (FamilyParameter fpTemp in fm.Parameters)
            {
                if (fpTemp.Definition.Name.Equals(parameterName))
                {
                    fp = fpTemp;
                    break;
                }
            }
            if (fp == null)
            {
                log.AddLogEntry(String.Format("Invalid ParameterName Input!: {0}!\nIs this parameter definied in the actual family?", parameterName), CadDev.LogManager.LogLevel.Critical, null);
            }

            return fp;
        }

        public static FamilyType RevitRawFindFamilyType(FamilyManager fm, string familyTypeName, ref CadDev.LogManager.LogDLG log)
        {
            FamilyType famType = null; // RawConvertSetToList<FamilyType>(fm.Types).FirstOrDefault(e => e.Name.Equals(familyTypeName, StringComparison.CurrentCultureIgnoreCase));

            if (familyTypeName.Equals(m_Instance))
                famType = fm.CurrentType;

            foreach (FamilyType famTypeTemp in fm.Types)
            {
                if (famTypeTemp.Name.Equals(familyTypeName))
                {
                    famType = famTypeTemp;
                    break;
                }
            }

            if (famType == null)
            {
                log.AddLogEntry(String.Format("Invalid FamilyTypeName : {0} !", familyTypeName), CadDev.LogManager.LogLevel.Critical, null);
            }

            return famType;
        }

        public static void RevitRawSetFamilyParameterFormula(FamilyManager fm, string familyTypeName, FamilyParameter fp, string value, ref CadDev.LogManager.LogDLG log)
        {
            FamilyType ft = RevitRawFindFamilyType(fm, familyTypeName, ref log);

            FamilyType curFamType = fm.CurrentType;

            if (ft != null)
                fm.CurrentType = ft;

            try
            {
                fm.SetFormula(fp, value);
            }
            catch (System.Exception ex)
            {
                log.AddLogEntry(String.Format("Error while setting formula for parameter {1}, value {2}:\n{3}", fp.Definition.Name, value, ex.Message), CadDev.LogManager.LogLevel.Critical, null);
            }
            finally
            {
                fm.CurrentType = curFamType;
            }
        }

        public static void RevitRawSetFamilyParameterValue(FamilyManager fm, string familyTypeName, string parameterName, object value, ref CadDev.LogManager.LogDLG log)
        {
            FamilyParameter fp = RevitRawFindFamilyParameter(fm, parameterName, ref log);
            if (fp != null)
                RevitRawSetFamilyParameterValue(fm, RevitRawFindFamilyType(fm, familyTypeName, ref log), fp, value, ref log);
        }

        public static void RevitRawSetFamilyParameterValue(FamilyManager fm, FamilyType ft, string parameterName, object value, ref CadDev.LogManager.LogDLG log)
        {
            FamilyParameter fp = RevitRawFindFamilyParameter(fm, parameterName, ref log);
            if (fp != null)
                RevitRawSetFamilyParameterValue(fm, ft, fp, value, ref log);
        }

        public static void RevitRawSetFamilyParameterValue(FamilyManager fm, string familyTypeName, FamilyParameter fp, object value, ref CadDev.LogManager.LogDLG log)
        {
            RevitRawSetFamilyParameterValue(fm, RevitRawFindFamilyType(fm, familyTypeName, ref log), fp, value, ref log);
        }

        public static void RevitRawSetFamilyParameterValue(FamilyManager fm, FamilyType ft, FamilyParameter fp, object value, ref CadDev.LogManager.LogDLG log)
        {
            FamilyType curFamType = fm.CurrentType;

            if (ft != null)
                fm.CurrentType = ft;

            try
            {
                switch (fp.StorageType)
                {
                    case StorageType.None:
                        break;

                    case StorageType.Double:
                        if (value.GetType().Equals(typeof(string)))
                        {
                            if (fp.Definition.UnitType == UnitType.UT_Electrical_Potential)
                            {
                                if (fp.IsDeterminedByFormula)
                                    fm.SetFormula(fp, GetElectrical_Potential(value).ToString());
                                else
                                    fm.Set(fp, GetElectrical_Potential(value));
                            }
                            else
                                if (fp.IsDeterminedByFormula)
                                fm.SetFormula(fp, value as string);
                            else
                                fm.Set(fp, double.Parse(value as string));
                        }
                        else
                        {
                            fm.Set(fp, Convert.ToDouble(value));
                        }
                        break;

                    case StorageType.Integer:
                        if (value.GetType().Equals(typeof(string)))
                        {
                            if (fp.IsDeterminedByFormula)
                                fm.SetFormula(fp, value as string);
                            else
                                fm.Set(fp, int.Parse(value as string));
                        }
                        else
                        {
                            fm.Set(fp, Convert.ToInt32(value));
                        }
                        break;

                    case StorageType.ElementId:
                        if (value.GetType().Equals(typeof(Autodesk.Revit.DB.ElementId)))
                        {
                            fm.Set(fp, value as Autodesk.Revit.DB.ElementId);
                        }
                        else if (value.GetType().Equals(typeof(string)))
                        {
                            fm.Set(fp, new Autodesk.Revit.DB.ElementId(int.Parse(value as string)));
                        }
                        else
                        {
                            fm.Set(fp, new Autodesk.Revit.DB.ElementId(Convert.ToInt32(value)));
                        }
                        break;

                    case StorageType.String:
                        if (fp.IsDeterminedByFormula || param.IsExpression(value.ToString()))
                        {
                            string tmp = value.ToString();

                            if (!param.IsExpression(value.ToString()))
                                tmp = param.EncapsulateStringWithQuotes(value.ToString());

                            fm.SetFormula(fp, tmp);
                        }
                        else
                            fm.Set(fp, value.ToString());
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.AddLogEntry(String.Format("Error while setting family parameter {0} :\n{1}", fp.Definition.Name, ex.Message), CadDev.LogManager.LogLevel.Critical, null);
            }
            finally
            {
                fm.CurrentType = curFamType;
            }
        }

        public static void RevitRetrieveParameters(FamilyManager mgr, ref Dictionary<string, FamilyParameter> fps, ref CadDev.LogManager.LogDLG log)
        {
            foreach (FamilyParameter fp in mgr.Parameters)
            {
                try
                {
                    string name = fp.Definition.Name;
                    fps.Add(name, fp);
                }
                catch (Exception ex)
                {
                    log.AddLogEntry(String.Format("Error while retriving parameter {0}:\n{1}", fp.Definition.Name, ex.Message), CadDev.LogManager.LogLevel.Critical, null);
                }
            }
        }



    }
}
