using GCodeConvertor.UI;
using Polenter.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace GCodeConvertor
{
    [Serializable]
    [XmlInclude(typeof(Topology))]
    [XmlInclude(typeof(Layer))]
    public class GlobalPreset
    {
        public List<Layer> layers {get; set;}

        private Topology _topology;
        public Topology topology {
            get { return _topology; } 
            set {
                _topology = value;
            } 
        }

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

        public GlobalPreset(TopologyByLineModel model)
        {
            var result = Topology.fillNozzlesAndLayer(model);
            topology = result.topology;
            layers = new List<Layer>() { result.layer };
        }

        public bool isPointTopologyCorrect(Point topologyPoint)
        {
            if(topologyPoint.X >= topology.map.GetLength(0) || topologyPoint.X < 0)
            {
                return false;
            }
            if (topologyPoint.Y >= topology.map.GetLength(1) || topologyPoint.Y < 0)
            {
                return false;
            }
            if (topology.map[(int)topologyPoint.X, (int)topologyPoint.Y] == 3 || topology.map[(int)topologyPoint.X, (int)topologyPoint.Y] == 4)
            {
                return false;
            }
            return true;
        }

        public void addLayer(Layer layer)
        {
            layers.Add(layer);
        }

        public void insertLayer(int position, Layer layer)
        {
            layers.Insert(position, layer);
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

        public bool checkIfPointCanBeFirst(Point firstLayerPoint)
        {
            bool canBeFirst = true;
            foreach(Layer layer in layers)
            {
                if(layer.thread.Count > 0)
                {
                    canBeFirst &= layer.thread[0].Equals(firstLayerPoint); 
                }
            }
            return canBeFirst;
        }

        public void savePreset() 
        {
            try
            {
                var serializer = new SharpSerializer();
                serializer.Serialize(this, $"{topology.path}\\{topology.name}.gcd");
                //MessageBox.Show("Пресет успешно сохранён",
                //                "Создание пресета",
                //                MessageBoxButton.OK,
                //                MessageBoxImage.Information);
                MessageWindow messageWindow = new MessageWindow("Проект успешно сохранен!", $"Проект сохранен в файл: \n {topology.path}\\{topology.name}.gcd");
                messageWindow.ShowDialog();
            }
            catch (Exception ex) 
            {
                MessageWindow messageWindow = new MessageWindow("Ошибка сохранения проекта!", "Не удалось сохранить проект.");
                messageWindow.ShowDialog();
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
                MessageWindow messageWindow = new MessageWindow("Ошибка загрузки проекта!", "Не удалось загрузить проект.");
                messageWindow.ShowDialog();
            }
            
        }

        public bool checkIsActualSaved()
        {
            try
            {
                var serializer = new SharpSerializer();
                GlobalPreset tempPreset = (GlobalPreset)serializer.Deserialize(topology.getFullPath());
                if(layers.SequenceEqual(tempPreset.layers))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return false;
        }
    }
}
