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
        const string m_extension = ".pfvData";

        public PfVModel()
        {
            m_models = new Dictionary<string, PfVModelData>();
        }

        public PfVModelData ActualModel
        {
            get
            {
                try
                {
                    return m_models[m_currentmodel];
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public void UpdateModel(PfVModelData localdata, string hostmodell)
        {
            SetCurrentModel(hostmodell);
            ModelLoad(hostmodell); // Load Modell from disk
            
            if(!m_models.ContainsKey(m_currentmodel))
            {
                // we have a new modell, nothing on disk stored before
                m_models.Add(m_currentmodel, localdata);
            }
            else
            {
                //TBD: implement merging of existing models
                foreach (string key in localdata.Keys)
                {
                    if (!m_models[m_currentmodel].ContainsKey(key))
                    {
                        m_models[m_currentmodel].Add(key, localdata[key]);
                    }
                    else
                    {
                        foreach (PfVElementData elem in localdata[key])
                        {
                            if (m_models[m_currentmodel].ContainsKey(key))
                            {
                                bool itemfound = false;
                                PfVElementData obj = m_models[m_currentmodel][key].First(test => test.IfcGuid == elem.IfcGuid);
                                if (obj != null)
                                {
                                    //we have the same item, check for update
                                    obj = elem;
                                }
                                else
                                {
                                    m_models[m_currentmodel][key].Add(elem);
                                }
                            }

                        }
                    }

                    m_models[m_currentmodel][key].Sort();
                }
            }

            ModelSave(hostmodell);
        }

        private void SetCurrentModel(string hostmodel)
        {
            m_currentmodel = hostmodel;
        }

        public void ModelLoad(string hostmodel)
        {
            SetCurrentModel(hostmodel);

            if (!m_models.ContainsKey(m_currentmodel))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(PfVModelData));
                PfVModelData temp = null;

                if (File.Exists(GetFilename()))
                {
                    using (XmlTextReader infile = new XmlTextReader(GetFilename()))
                    {
                        try
                        {
                            temp = (PfVModelData)ser.ReadObject(infile);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                if (temp == null)
                    temp = new PfVModelData();

                m_models.Add(m_currentmodel, temp);
            }
        }

        public void ModelSave(string hostmodel)
        {
            SetCurrentModel(hostmodel);

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
