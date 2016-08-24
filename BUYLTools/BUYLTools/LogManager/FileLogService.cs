//This source code was generated using Visual Studio Design Patterns add-in
//You can find the source code and binaries at http://VSDesignPatterns.codeplex.com

//Singleton: //Ensure a class only has one instance, and provide a global point of access to it.
using System;
using System.Threading;
using log4net;
using System.IO;

namespace BUYLTools.LogManager
{
    public sealed class FileLogService : ILogService
    {
        private static readonly Lazy<FileLogService> _instance = new Lazy<FileLogService>(() => new FileLogService(), LazyThreadSafetyMode.PublicationOnly);
        static ILog _logger;

        public static FileLogService Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private FileLogService()
        {
            // Gets directory path of the calling application
            // RelativeSearchPath is null if the executing assembly i.e. calling assembly is a
            // stand alone exe file (Console, WinForm, etc). 
            // RelativeSearchPath is not null if the calling assembly is a web hosted application i.e. a web site
            var log4NetConfigDirectory = Path.GetDirectoryName(this.GetType().Assembly.Location);

            var log4NetConfigFilePath = Path.Combine(log4NetConfigDirectory, "log4net.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(log4NetConfigFilePath));
        }

        public void Fatal(string message, Type logClass)
        {
            GetLogger(logClass);
            Instance.Fatal(message);
        }

        private void Fatal(string message)
        {
            if (_logger != null && _logger.IsFatalEnabled)
                _logger.Fatal(message);
        }

        public void Error(string message, Type logClass)
        {
            GetLogger(logClass);
            Instance.Error(message);
        }

        private void Error(string message)
        {
            if (_logger != null && _logger.IsErrorEnabled)
                _logger.Error(message);
        }

        public void Warn(string message, Type logClass)
        {
            GetLogger(logClass);
            Instance.Warn(message);
        }

        private void Warn(string message)
        {
            if (_logger != null && _logger.IsWarnEnabled)
                _logger.Warn(message);
        }

        public void Info(string message, Type logClass)
        {
            GetLogger(logClass);
            Instance.Info(message);
        }

        private void Info(string message)
        {
            if (_logger != null && _logger.IsInfoEnabled)
                _logger.Info(message);
        }

        public void Debug(string message, Type logClass)
        {
            GetLogger(logClass);
            Instance.Debug(message);
        }

        private void Debug(string message)
        {
            if (_logger != null && _logger.IsDebugEnabled)
                _logger.Debug(message);
        }

        private static void GetLogger(Type logClass)
        {
            if(_logger == null)
                _logger = log4net.LogManager.GetLogger(logClass);
        }
    }
}
