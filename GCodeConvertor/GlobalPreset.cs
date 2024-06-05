using Polenter.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
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

        public GlobalPreset(TopologyModel model) 
        {
            topology = new Topology(model);
            layers = new List<Layer>();
        }
        public GlobalPreset(TopologyByLineModel model)
        {
            var result = Topology.fillNozzlesAndLayer(model);
            topology = result.topology;
            layers = new List<Layer>();
            layers.Add(result.layer);
        }

        public GlobalPreset(List<Layer> layers, Topology topology) 
        {
            this.layers = layers;
            this.topology = topology;
        }
        public void savePreset() 
        {
            try
            {
                var serializer = new SharpSerializer();
                serializer.Serialize(this, $"{topology.path}\\{topology.name}.gcd");
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

        public void loadPreset(string path)
        {
            try
            {
                var serializer = new SharpSerializer();
                GlobalPreset tempPreset = (GlobalPreset)serializer.Deserialize(path);
                layers = tempPreset.layers;
                topology = tempPreset.topology;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
    }
}
