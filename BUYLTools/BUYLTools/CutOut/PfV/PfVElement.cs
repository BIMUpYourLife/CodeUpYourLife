using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

    public class PfVElementData
    {
        Status _status = Status.New;
        const string m_pfvRectangularKey = "pfvrectangular";
        const string m_pfvRoundConfigKey = "pfvround";
        const string m_pfvWall = "wallcutout";
        const string m_pfvFloor = "floorcutout";
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

        [Category("Info")]
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

        [Category("Dimensions")]
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

        [Category("Dimensions")]
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

        [Category("Dimensions")]
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

        [Category("Dimensions")]
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


        [Category("Document")]
        public string Document
        {
            get { return _document; }
        }

        [Category("Info")]
        public string ElementName
        {
            get { return _elementName; }
        }

        [Category("Document")]
        public string Folder
        {
            get { return _folder; }
        }


        [Category("Info")]
        public int IdLinked
        {
            get { return _idLinked; }
        }

        [Category("Info")]
        public int IdLocal
        {
            get { return _idLocal; }
            set { _idLocal = value; }
        }

        [Category("Info")]
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

        [Category("Info")]
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

        [Category("Info")]
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

        [Category("Info")]
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

        [Category("Geometry")]
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

        [Category("Geometry")]
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

        [Category("Info")]
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

        [Category("Info")]
        public string UniqueIdLinked
        {
            get { return _uniqueIdLinked; }
        }

        [Category("Info")]
        public string UniqueIdLocal
        {
            get { return _uniqueIdLocal; }
            set { _uniqueIdLocal = value; }
        }

        [Category("Info")]
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
                if (items.Contains(this.IfcDescription))
                    result = true;
                else if (items.Contains(this.ElementName))
                    result = true;
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
