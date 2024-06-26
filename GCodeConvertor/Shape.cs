using GCodeConvertor.UI;
using Polenter.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

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

        public bool loadPreset(string path)
        {
            try
            {
                var serializer = new SharpSerializer();
                Shape shape = (Shape)serializer.Deserialize(path);
                points = shape.points;
            }
            catch (Exception ex)
            {
                MessageWindow messageWindow = new MessageWindow("Ошибка открытия файла фигуры!", "Невозможно открыть файл фигуры");
                messageWindow.ShowDialog();
                return false;
            }
            return true;
        }
    }
}
