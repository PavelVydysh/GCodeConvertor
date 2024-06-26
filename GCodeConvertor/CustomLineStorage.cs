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

        public CustomLine getLineByInnerLine(Line line)
        {
            foreach (CustomLine customLine in lines)
            {
                if (customLine.line.Equals(line))
                {
                    return customLine;
                }
            }

            return null;
        }

        public int indexOf(CustomLine line)
        {
            return lines.IndexOf(line);
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

        public void addLine(CustomLine line, int index)
        {
            lines.Insert(index, line);
        }

        public CustomLine getByIndex(int index)
        {
            return lines[index];
        }

        public int size()
        {
            return lines.Count;
        }
    }
}
