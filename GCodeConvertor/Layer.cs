using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Point = System.Windows.Point;

namespace GCodeConvertor
{
    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(Point))]
    public class Layer
    {
        private const string DEFAULT_NAME = "Без имени";
        private const float DEFAULT_HEIGHT = 12;

        public string name { get; set; }
        
        public float height { get; set; }
        
        public List<Point> thread { get; set; }
        
        public bool enable { get; set; }
        
        public Layer(string name, float height)
        {
            this.name = name;
            this.height = height;
            thread = new List<Point>();
            enable = true;
        }

        public Layer() : this(DEFAULT_NAME, DEFAULT_HEIGHT) { }
        
    }
}