using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUYLTools.Data
{
    public static class DataStructure_Handler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static families DataStructureLoad(string filename)
        {
            families ds = null;
            if (File.Exists(filename))
            {
                try
                {
                    ds = BUYLTools.Utils.ObjectXmlSerializer.ObjectXMLSerializer<families>.Load(filename);
                }
                catch (Exception ex)
                {
                    log.Error("Error while loading datastructure", ex);
                }
            }
            return ds;
        }

    }
}
