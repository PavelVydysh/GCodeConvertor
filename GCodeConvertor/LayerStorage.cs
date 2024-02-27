using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeConvertor
{
    internal class LayerStorage
    {
        public List<Layer> layers;

        public LayerStorage()
        {
            layers = new List<Layer>();
        }

        public void addLayer(Layer layer)
        {
            layers.Add(layer);
        }

        public Layer getLayerByName(string name)
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

        public void removeLayerByName(string name)
        {
            Layer layerToRemove = getLayerByName(name);
            layers.Remove(layerToRemove);
        }
    }
}
