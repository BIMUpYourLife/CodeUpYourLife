using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUYLTools.LogManager
{
    internal interface ILogService
    {
        void Fatal(string message, Type logClass);
        void Error(string message, Type logClass);
        void Warn(string message, Type logClass);
        void Info(string message, Type logClass);
        void Debug(string message, Type logClass);
    }
}
