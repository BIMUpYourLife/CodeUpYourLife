using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BUYLTools.Data
{
    public partial class families
    {
        public bool ContainsFamilyFile(string pathname, ref LogManager.LogDLG log)
        {
            bool contains = false;

            try
            {
                if (this != null)
                {
                    string id = GetArticleNumberFromFilename(pathname, ref log);
                    familiesFamilylistFamily fam = GetFamily(pathname, ref log);
                    if (fam != null && fam.id == id)
                        contains = true;
                }
            }
            catch (Exception ex)
            {
                log.AddLogEntry(ex.Message, LogManager.LogLevel.Critical, null);
            }
            return contains;
        }

        private List<string> GetSupportedLanguages()
        {
            List<string> lang = new List<string>();
            return lang;
        }

        public string GetFileNamesFromArticleNumberAndLanguage(string prefix, string arcticleNr, string language, ref LogManager.LogDLG log)
        {
            string result = null;

            try
            {
                result = String.Format("{0}_{1}_{2}", prefix, arcticleNr, language);
            }
            catch (Exception ex)
            {
                log.AddLogEntry(ex.Message, LogManager.LogLevel.Critical, null);
            }
            return result;
        }

        public string GetArticleNumberFromFilename(string path, ref LogManager.LogDLG log)
        {
            string result = "";
            try
            {
                string temp = Path.GetFileNameWithoutExtension(path);
                string[] items = temp.Split('_');
                if (items.Count<string>() >= 3)
                {
                    result = items[1];
                }
            }
            catch (Exception ex)
            {
                log.AddLogEntry(ex.Message, LogManager.LogLevel.Critical, null);
            }
            return result;
        }

        public familiesFamilylistFamily GetFamily(string path, ref LogManager.LogDLG log)
        {
            familiesFamilylistFamily fixt = null;

            try
            {
                if (this != null)
                {
                    string id = GetArticleNumberFromFilename(path, ref log);

                    foreach (familiesFamilylistFamily itemFamily in this.familylist)
                    {
                        if (id == itemFamily.id)
                        {
                            fixt = itemFamily;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.AddLogEntry(ex.Message, LogManager.LogLevel.Critical, null);
            }
            return fixt;
        }

        public string GetUpdataDate(ref LogManager.LogDLG log)
        {
            string date = "";
            try
            {
                if (this != null)
                {
                    date = this.timestamp;
                }
            }
            catch (Exception ex)
            {
                log.AddLogEntry(ex.Message, LogManager.LogLevel.Critical, null);
            }

            return date;
        }

    }


    public partial class param
    {
        const string m_Quotes = "\"";

        public string GetDecodedValueString()
        {
            string result = "";

            if (val.Contains("http"))
                result = System.Web.HttpUtility.UrlDecode(val);
            else
                result = val;


            if(fx == "1")
            {
                result = ReplaceHTMLQuotes(result);
                if(!IsExpression(result))
                    result = EncapsulateStringWithQuotes(result);
            }
            else
            {

            }

            return result;
        }

        public static bool IsExpression(string value)
        {
            bool result = false;
            if (value.StartsWith("IF(") || value.StartsWith("if("))
                result = true;

            return result;
        }

        const string m_HtmlQuote = "&quot;";
        public string ReplaceHTMLQuotes(string result)
        {
            result = result.Replace(m_HtmlQuote, m_Quotes);
            return result;
        }

        public static string EncapsulateStringWithQuotes(string _val)
        {
            if (!_val.StartsWith(m_Quotes))
                _val = m_Quotes + _val;
            if (!_val.EndsWith(m_Quotes))
                _val = _val + m_Quotes;
            return _val;
        }
    }
}
