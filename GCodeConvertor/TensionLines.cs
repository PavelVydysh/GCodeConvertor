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
    class TensionLines
    {
        public List<List<Point>> points { get; set; }

        public TensionLines(List<List<Point>> points)
        {
            this.points = points;
        }

        public TensionLines() { }

        public bool loadPreset(string path)
        {
            try
            {
                var serializer = new SharpSerializer();
                TensionLines tensionLines = (TensionLines)serializer.Deserialize(path);
                points = tensionLines.points;
            }
            catch (Exception ex)
            {
                MessageWindow messageWindow = new MessageWindow("Ошибка открытия файла линий напряжения!", "Невозможно открыть файл линий напряжения");
                messageWindow.ShowDialog();
                return false;
            }
            return true;
        }
    }
}
