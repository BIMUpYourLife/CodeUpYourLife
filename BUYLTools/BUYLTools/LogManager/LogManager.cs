using log4net;
using log4net.Config;
using System.IO;
using System;

// Configure log4net using the .log4net file
//[assembly: XmlConfigurator(ConfigFile = "Armacell_Global.dll.log4net", Watch = true)]
// This will cause log4net to look for a configuration file
// called Armacell_Global.dll.log4net in the application base
// directory (i.e. the directory containing TestApp.exe)
// The config file will be watched for changes.
namespace BUYLTools.LogManager
{
    public enum LogState
    {
        Track,
        Info,
        Debug,
        Warn,
        Error
    }

    public class ManagerLog
    {
        //private const string m_logfileConfigName; // = "Armacell_Global.dll.log4net";

        // Define a static logger variable so that it references the
        // Logger instance named "ManagerLog".
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(ManagerLog));
        private static ManagerLog m_manager = null;

        public ManagerLog()
        {
            XmlConfigurator.Configure(new System.IO.FileInfo(Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), GetLogFileLocation())));
        }

        private string GetLogFileLocation()
        {
            //Get setting from appconfig
            Configuration.Manager _confMan = new Configuration.Manager(typeof(ManagerLog).Assembly, true);
            return _confMan.GetValueForAppsetting("logconfig");

        }

        public static ManagerLog Current
        {
            get
            {
                if (m_manager == null)
                { m_manager = new ManagerLog(); }
                return m_manager;
            }
        }

        public void WriteLogMessage(string message, LogState lgState = LogState.Info)
        {
            switch (lgState)
            {
                case LogState.Info:
                case LogState.Track:
                    log.Info(message);
                    break;

                case LogState.Debug:
                    log.Debug(message);
                    break;

                case LogState.Warn:
                    log.Warn(message);
                    break;

                case LogState.Error:
                    log.Error(message);
                    break;

                default:
                    break;
            }
        }
    }
}