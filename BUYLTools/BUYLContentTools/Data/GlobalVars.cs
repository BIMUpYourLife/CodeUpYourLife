using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUYLTools.Data
{
    public enum LanguageEnum
    {
        de_DE,
        en_GB
    }

    public static class GlobalVars
    {
        static families _dsDEDE;
        static families _dsENGB;

        private const string m_UpdateParameterNameDEDE = "Aktualisierung";
        private const string m_UpdateParameterNameENGB = "Update";

        public const string m_Quotes = "\"";

        static LanguageEnum _lang = LanguageEnum.de_DE;
        static BackgroundWorker _worker;

        public static void PreLoadDatabases()
        {
            _worker = new BackgroundWorker();
            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            _worker.RunWorkerAsync();
        }

        private static void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
#if DEBUG
            System.Windows.Forms.MessageBox.Show("BEGA Databases pre-loaded in background");
#endif
        }

        private static void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            SetDataSetLanguage(LanguageEnum.de_DE);
            GetDataSet();
            SetDataSetLanguage(LanguageEnum.en_GB);
            GetDataSet();
        }

        public static void SetDataSetLanguage(string filename)
        {
            if (filename.EndsWith(families.langDE))
                _lang = LanguageEnum.de_DE;
            else if (filename.EndsWith(families.langEN))
                _lang = LanguageEnum.en_GB;
        }

        public static void SetDataSetLanguage(LanguageEnum lang)
        {
            _lang = lang;
        }

        public static families GetDataSet()
        {
            families temp = null;

            if (_lang == LanguageEnum.de_DE)
                temp = DataSetDEDE;
            else if (_lang == LanguageEnum.en_GB)
                temp = DataSetENGB;

            return temp;
        }

        public static string GetUpdateValue()
        {
            string result = m_UpdateParameterNameDEDE;

            if (_lang == LanguageEnum.de_DE)
                result = m_UpdateParameterNameDEDE;
            else if (_lang == LanguageEnum.en_GB)
                result = m_UpdateParameterNameENGB;

            return result;
        }

        private static families DataSetDEDE
        {
            get 
            { 
                if(_dsDEDE == null)
                {
                    CadDev.LogManager.LogDLG _log = new CadDev.LogManager.LogDLG();
                    _dsDEDE = families.DataStructureLoad(ref _log, families.GetFileNameDEDatabase());

                    if (_log.HasLogEntrys())
                        _log.ShowDialog();
                }
                return _dsDEDE; 
            }
            set { _dsDEDE = value; }
        }

        private static families DataSetENGB
        {
            get
            {
                if (_dsENGB == null)
                {
                    CadDev.LogManager.LogDLG _log = new CadDev.LogManager.LogDLG();
                    _dsENGB = families.DataStructureLoad(ref _log, families.GetFileNameENDatabase());

                    if (_log.HasLogEntrys())
                        _log.ShowDialog();
                }
                return _dsENGB;
            }
            set { _dsENGB = value; }
        }

        static CadDev.ConfigManager.Manager _man = new CadDev.ConfigManager.Manager(typeof(GlobalVars).Assembly, true);

        public static CadDev.ConfigManager.Manager ConfigManager
        {
            get { return _man; }
        }
    }
}
