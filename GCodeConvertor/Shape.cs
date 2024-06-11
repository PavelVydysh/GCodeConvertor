using Polenter.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

using Point = System.Windows.Point;

namespace GCodeConvertor
{
    [Serializable]
    [XmlInclude(typeof(Point))]
    class Shape
    {
        public List<Point> points { get; set; }

        public Shape(List<Point> points)
        {
            this.points = points;
        }

        public Shape() { }

        public void saveShape()
        {
            try
            {
                var serializer = new SharpSerializer();
                serializer.Serialize(this, "C:\\Users\\vicst\\Documents\\shape.xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void loadPreset(string path)
        {
            try
            {
                var serializer = new SharpSerializer();
                Shape shape = (Shape)serializer.Deserialize(path);
                points = shape.points;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }

    [Serializable]
    [XmlInclude(typeof(Point))]
    class TensionLines
    {
        public List<List<Point>> points { get; set; }

        public TensionLines(List<List<Point>> points)
        {
            this.points = points;
        }

        public TensionLines() { }

        public void saveTensionLines()
        {
            try
            {
                var serializer = new SharpSerializer();
                serializer.Serialize(this, "C:\\Users\\vicst\\Documents\\TensionLines.xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void loadPreset(string path)
        {
            try
            {
                var serializer = new SharpSerializer();
                TensionLines tensionLines = (TensionLines)serializer.Deserialize(path);
                points = tensionLines.points;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
