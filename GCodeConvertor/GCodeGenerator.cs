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
            float lastHeight = 0;
            String gcode = "";
            foreach (Layer layer in layers)
            {
                if (layer.heightLayer != 0)
                {
                    lastHeight += layer.heightLayer;
                    gcode += "G1 Z" + lastHeight + "\n";

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

            string currentDatetime = "_" + DateTime.Now.ToString().Replace(".", "_").Replace(":", "-").Replace(" ", "_");

            string pathToFile = ProjectSettings.preset.topology.path + "\\" +
                                                            ProjectSettings.preset.topology.name + currentDatetime + ".txt";

            using (StreamWriter writer = new StreamWriter(pathToFile, false))
            {
                await writer.WriteLineAsync(gcode);
            }

            MessageBox.Show($"GCode сгенерирован и помещён в файл. \n {pathToFile}",
                                "Создание GCode`а",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
        }
    }
}
