﻿using System;
using System.Collections.Generic;

namespace GCodeConvertor
{
    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(System.Windows.Point))]
    public class Layer
    {
        static int countOfLayers = 1;
        public String name { get; set; }
        public float heightLayer { get; set; }

        public List<System.Windows.Point> layerThread { get; set; }

        public bool enable { get; set; }

        public Layer()
        {
            name = "Слой " + countOfLayers++;
            layerThread = new List<System.Windows.Point>();
            heightLayer = 12;
            enable = true;
        }
    }
}