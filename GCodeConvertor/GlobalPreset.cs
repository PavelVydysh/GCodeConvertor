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

        public GlobalPreset(List<Layer> layers, Topology topology) 
        {
            this.topology = topology;
            this.layers = layers;
        }

        public void addLayer(Layer layer)
        {
            layers.Add(layer);
        }

        public Layer getLayerByGuid(string name)
        {
            foreach (Layer layer in layers)
            {
                if (layer.name == name)
                {
                    return layer;
                }
            }

            return null;
        }

        public void removeLayer(Layer layer)
        {
            layers.Remove(layer);
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
