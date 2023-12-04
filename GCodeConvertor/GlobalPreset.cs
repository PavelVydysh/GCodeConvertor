using Polenter.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace GCodeConvertor
{
    [Serializable]
    [XmlInclude(typeof(Topology))]
    [XmlInclude(typeof(Layer))]
    public class GlobalPreset
    {
        public List<Layer> layers {get; set;}

        public Topology topology { get; set; }

        public GlobalPreset() { }

        public GlobalPreset(List<Layer> layers, Topology topology) 
        {
            this.layers = layers;
            this.topology = topology;
        }
        public void saveGlobalPreset() 
        {
            try
            {
                var serializer = new SharpSerializer();
                serializer.Serialize(this, "C:\\gcode-projects\\global_preset.xml");
                MessageBox.Show("Пресет успешно сохранён",
                                "Создание пресета",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
