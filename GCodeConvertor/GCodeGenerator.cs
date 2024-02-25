using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
                if (layer.heightLayer != 0)
                {
                    gcode += "G1 Z" + layer.heightLayer + "\n";
                }

                foreach (System.Windows.Point point in layer.layerThread)
                {
                    string x = point.X.ToString();
                    x = x.Replace(",", ".");
                    string y = point.Y.ToString();
                    y = y.Replace(",", ".");

                    gcode += "G1 X" + x + " Y" + y + "\n";
                }
            }

            saveAsync(gcode);
        }

        private static async Task saveAsync(String gcode)
        {
            using (StreamWriter writer = new StreamWriter(ProjectSettings.preset.topology.path + "\\" +
                                                            ProjectSettings.preset.topology.name+".txt", false))
            {
                await writer.WriteLineAsync(gcode);
            }

            MessageBox.Show("GCode сгенерирован и помещён в файл...",
                                "Создание GCode`а",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
        }
    }
}
