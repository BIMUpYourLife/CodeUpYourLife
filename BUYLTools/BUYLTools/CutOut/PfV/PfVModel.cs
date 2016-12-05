using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BUYLTools.CutOut.PfV
{
    public class PfVModel : IPfVModel
    {
        string m_currentmodel = null;
        static Dictionary<string, PfVModelData> m_models = null;

        public PfVModel()
        {
            m_models = new Dictionary<string, PfVModelData>();
        }

        public PfVModelData ModelLoad()
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(PfVModelData));
            PfVModelData tempmodel = null;

            if (File.Exists(GetFilename()))
            {
                using (XmlTextReader infile = new XmlTextReader(GetFilename()))
                {
                    try
                    {
                        tempmodel = (PfVModelData)ser.ReadObject(infile);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            if (tempmodel == null)
                tempmodel = new PfVModelData();

            m_models.Add(m_currentmodel, tempmodel);

            return tempmodel;
        }

        public PfVModelData Model(string hostmodel)
        {
            m_currentmodel = hostmodel;
            PfVModelData temp = null;

            if (m_models.ContainsKey(m_currentmodel))
                temp = m_models[m_currentmodel];
            else
                temp = ModelLoad();

            return temp;
        }

        public void ModelSave()
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(PfVModelData));

            if (m_models.ContainsKey(m_currentmodel))
            {
                using (XmlTextWriter outfile = new XmlTextWriter(GetFilename(), Encoding.UTF8))
                {
                    try
                    {
                        ser.WriteObject(outfile, m_models[m_currentmodel]);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        const string m_extension = ".pfvData";

        private string GetFilename()
        {
            string fname = Path.Combine(Path.GetDirectoryName(m_currentmodel), Path.GetFileNameWithoutExtension(m_currentmodel) + m_extension);
            return fname;
        }
    }

    [CollectionDataContract()]
    public class PfVModelData : Dictionary<string, List<PfVElementData>>
    {
        public PfVModelData() : base()
        { }
    }
}
