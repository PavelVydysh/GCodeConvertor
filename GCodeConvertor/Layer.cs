using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeConvertor
{
    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(Point))]
    public class Layer
    {
        public float heightLayer { get; set; }

        public List<Point> layerThread { get; set; }

        public Layer() { }
    }
}