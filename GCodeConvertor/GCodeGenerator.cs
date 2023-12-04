using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GCodeConvertor
{
    public class GCodeGenerator
    {
        public static void generate(List<Layer> layers) 
        {
            String gcode = "";
            foreach (Layer layer in layers) 
            {
               foreach(System.Drawing.Point point in layer.layerThread) 
                {
                    gcode += "G1 X" + point.X + " Y" + point.Y + "\n";
                }
            }

            MessageBox.Show(gcode);
        }
    }
}
