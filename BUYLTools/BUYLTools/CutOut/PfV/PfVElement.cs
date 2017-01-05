using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BUYLTools.CutOut.PfV
{
    public enum Status
    {
        Dummy,
        New,
        Ok,
        Modified,
        Deleted
    }

    [DataContract()]
    public class PfVElementData
    {
        Status _status = Status.New;
        const string m_pfvRectangularKey = "pfvrectangular";
        const string m_pfvRoundConfigKey = "pfvround";
        const string m_pfvWall = "wallcutout";
        const string m_pfvFloor = "floorcutout";
        public const string idLinkedColumn = "IdLinked";
        private double _depth;
        private double _diameter;
        private string _document;
        private string _elementName;
        private string _folder;
        private double _height;
        private int _idLinked;
        private int _idHost;
        private int _idLocal;
        private string _ifcDescription;
        private string _ifcGuid;
        private string _ifcName;
        private string _ifcSpatialContainer;
        private Position _pos;
        private string _shape;
        private string _system;
        private string _uniqueIdLinked;
        private string _uniqueIdLocal;
        private double _width;
        public PfVElementData(
          string path,
          string elementName,
          int idLinked,
          string uniqueIdLinked)
        {
            //int i = path.LastIndexOf("\\");
            _document = Path.GetFileName(path);
            _elementName = elementName;
            _idLinked = idLinked;
            _uniqueIdLinked = uniqueIdLinked;
            _folder = Path.GetDirectoryName(path);


            Location = new Position(0, 0, 0);
        }

        [ReadOnly(true), Category("Info"), DataMember()]
        public Status PfVStatus
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
            }
        }

        [ReadOnly(true), Category("Info"), DataMember()]
        public string ElementName
        {
            get { return _elementName; }
            set { _elementName = value; }
        }

        [ReadOnly(true), Category("Dimensions"), DataMember()]
        public double Width
        {
            get
            {
                return _width;
            }

            set
            {
                _width = value;
            }
        }

        [ReadOnly(true), Category("Dimensions"), DataMember()]
        public double Height
        {
            get
            {
                return _height;
            }

            set
            {
                _height = value;
            }
        }

        [ReadOnly(true), Category("Dimensions"), DataMember()]
        public double Diameter
        {
            get
            {
                return _diameter;
            }

            set
            {
                _diameter = value;
            }
        }

        [ReadOnly(true), Category("Dimensions"), DataMember()]
        public double Depth
        {
            get
            {
                return _depth;
            }

            set
            {
                _depth = value;
            }
        }


        [ReadOnly(true), Category("Document"), DataMember()]
        public string Document
        {
            get { return _document; }
            set { _document = value; }
        }
        [ReadOnly(true), Category("Document"), DataMember()]
        public string Folder
        {
            get { return _folder; }
            set { _folder = value; }
        }
        [ReadOnly(true), Category("Info"), DataMember()]
        public int IdLinked
        {
            get { return _idLinked; }
            set { _idLinked = value; }
        }

        [ReadOnly(true), Category("Info"), DataMember()]
        public int IdLocal
        {
            get { return _idLocal; }
            set { _idLocal = value; }
        }

        [ReadOnly(true), Category("Info"), DataMember()]
        public string IfcDescription
        {
            get
            {
                return _ifcDescription;
            }

            set
            {
                _ifcDescription = value;
            }
        }

        [ReadOnly(true), Category("Info"), DataMember()]
        public string IfcGuid
        {
            get
            {
                return _ifcGuid;
            }

            set
            {
                _ifcGuid = value;
            }
        }

        [ReadOnly(true), Category("Info"), DataMember()]
        public string IfcName
        {
            get
            {
                return _ifcName;
            }

            set
            {
                _ifcName = value;
            }
        }

        [ReadOnly(true), Category("Info"), DataMember()]
        public string IfcSpatialContainer
        {
            get
            {
                return _ifcSpatialContainer;
            }

            set
            {
                _ifcSpatialContainer = value;
            }
        }

        [ReadOnly(true), Category("Geometry"), DataMember()]
        public Position Location
        {
            get
            {
                return _pos;
            }

            set
            {
                _pos = value;
            }
        }

        [ReadOnly(true), Category("Geometry"), DataMember()]
        public string Shape
        {
            get
            {
                return _shape;
            }

            set
            {
                _shape = value;
            }
        }

        [ReadOnly(true), Category("Info"), DataMember()]
        public string System
        {
            get
            {
                return _system;
            }

            set
            {
                _system = value;
            }
        }

        [ReadOnly(true), Category("Info"), DataMember()]
        public string UniqueIdLinked
        {
            get { return _uniqueIdLinked; }
            set { _uniqueIdLinked = value; }
        }

        [ReadOnly(true), Category("Info"), DataMember()]
        public string UniqueIdLocal
        {
            get { return _uniqueIdLocal; }
            set { _uniqueIdLocal = value; }
        }

        [ReadOnly(true), Category("Info"), DataMember()]
        public int IdHost
        {
            get
            {
                return _idHost;
            }

            set
            {
                _idHost = value;
            }
        }

        public bool IsRectangular()
        {
            bool result = false;
            BUYLTools.Configuration.Manager _confMan = new BUYLTools.Configuration.Manager(typeof(PfVElementData).Assembly, true);
            string sTypeName = _confMan.GetValueForAppsetting(m_pfvRectangularKey);

            if (!String.IsNullOrEmpty(sTypeName))
            {
                List<string> items = sTypeName.Split(',').ToList();
                if (items.Contains(this.Shape))
                    result = true;
            }

            return result;
        }

        public bool IsRound()
        {
            bool result = false;
            BUYLTools.Configuration.Manager _confMan = new BUYLTools.Configuration.Manager(typeof(PfVElementData).Assembly, true);
            string sTypeName = _confMan.GetValueForAppsetting(m_pfvRoundConfigKey);

            if (!String.IsNullOrEmpty(sTypeName))
            {
                List<string> items = sTypeName.Split(',').ToList();
                if (items.Contains(this.Shape))
                    result = true;
            }

            return result;
        }

        public bool IsWallPfV()
        {
            bool result = false;
            BUYLTools.Configuration.Manager _confMan = new BUYLTools.Configuration.Manager(typeof(PfVElementData).Assembly, true);
            string sTypeName = _confMan.GetValueForAppsetting(m_pfvWall);

            if (!String.IsNullOrEmpty(sTypeName))
            {
                List<string> items = sTypeName.Split(',').ToList();
                if (StringContains(this.IfcDescription, items))
                    result = true;
                else if (StringContains(this.ElementName, items))
                    result = true;
            }

            return result;
        }

        bool StringContains(string source, List<string> items)
        {
            bool result = false;

            foreach (string  it in items)
            {
                if(source.Contains(it))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public bool IsFloorPfV()
        {
            bool result = false;
            BUYLTools.Configuration.Manager _confMan = new BUYLTools.Configuration.Manager(typeof(PfVElementData).Assembly, true);
            string sTypeName = _confMan.GetValueForAppsetting(m_pfvFloor);

            if (!String.IsNullOrEmpty(sTypeName))
            {
                List<string> items = sTypeName.Split(',').ToList();
                if (items.Contains(this.IfcDescription))
                    result = true;
                else if (items.Contains(this.ElementName))
                    result = true;
            }

            return result;
        }
    }

    [DataContract()]
    public class Position
    {
        private double _x;
        private double _y;
        private double _z;

        public Position(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [DataMember()]
        public double X
        {
            get
            {
                return _x;
            }

            set
            {
                _x = value;
            }
        }

        [DataMember()]
        public double Y
        {
            get
            {
                return _y;
            }

            set
            {
                _y = value;
            }
        }

        [DataMember()]
        public double Z
        {
            get
            {
                return _z;
            }

            set
            {
                _z = value;
            }
        }

        public override string ToString()
        {
            return String.Format("x : {0}; y : {1}; z : {2}", X, Y, Z);
        }
    }
}
