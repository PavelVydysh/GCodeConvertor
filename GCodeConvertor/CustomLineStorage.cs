using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GCodeConvertor
{
    public class CustomLineStorage
    {
        List<CustomLine> lines = new List<CustomLine>();

        public CustomLine[] getLinesByEllipse(Ellipse el) 
        {

            CustomLine[] remLines = new CustomLine[2];

            foreach (CustomLine line in lines)
            {
                if (line.firstEllipse.Equals(el))
                {
                    remLines[0] = line;
                }
                else if (line.secondEllipse.Equals(el)) 
                {
                    remLines[1] = line;
                }
            }

            return remLines;
        }

        public void clear() 
        {
            lines.Clear();
        }

        public void remove(CustomLine line)
        {
            lines.Remove(line);
        }

        public void addLine(CustomLine line) 
        {
            lines.Add(line);
        }
    }
}
