using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUYLTools.CutOut
{
    public class ElementData
    {
        string _document;
        string _elementName;
        int _id;
        double _x;
        double _y;
        double _z;
        string _uniqueId;
        string _folder;

        public ElementData(
          string path,
          string elementName,
          int id,
          double x,
          double y,
          double z,
          string uniqueId)
        {
            int i = path.LastIndexOf("\\");
            _document = path.Substring(i + 1);
            _elementName = elementName;
            _id = id;
            _x = x;
            _y = y;
            _z = z;
            _uniqueId = uniqueId;
            _folder = path.Substring(0, i);
        }

        public string Document
        {
            get { return _document; }
        }
        public string Element
        {
            get { return _elementName; }
        }
        public int Id
        {
            get { return _id; }
        }
        public string X
        {
            get { return _x.ToString(); }
        }
        public string Y
        {
            get { return _y.ToString(); }
        }
        public string Z
        {
            get { return _z.ToString(); }
        }
        public string UniqueId
        {
            get { return _uniqueId; }
        }
        public string Folder
        {
            get { return _folder; }
        }
    }
}
