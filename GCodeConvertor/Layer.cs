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

        public Guid guid { get; }
        public string name { get; set; }
        public float height { get; set; }
        public List<Point> thread { get; set; }
        public bool isEnable { get; set; }

        public Layer(string name, float height)
        {
            guid = Guid.NewGuid();
            this.name = name;
            this.height = height;
            thread = new List<Point>();
            isEnable = true;
        }

        public Layer() : this(DEFAULT_NAME, DEFAULT_HEIGHT) { }
        
        public bool isEnded()
        {
            if (thread.Count < 2)
                return false;
            return thread.First().Equals(thread.Last());
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is not Layer)
            {
                return false;
            }
            return (this.guid.Equals(((Layer)obj).guid));
        }

        public int getThreadPoint(Point point)
        {
            for(int i = 0; i < thread.Count; i++)
            {
                if (thread[i].Equals(point)) return i;
            }
            return -1;
        }

    }
}