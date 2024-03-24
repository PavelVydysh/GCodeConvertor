using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GCodeConvertor
{
    public class CustomLine
    {
        public Line line { get; set; }
        public Ellipse firstEllipse { get; set; }
        public Ellipse secondEllipse { get; set; }

        public CustomLine(Line line, Ellipse firstEllipse, Ellipse secondEllipse)
        {
            this.line = line;
            this.firstEllipse = firstEllipse;
            this.secondEllipse = secondEllipse;
        }
    }
}
