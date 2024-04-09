using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GCodeConvertor.GScript
{
    public abstract class AbstractCommand
    {
        public string type { get; }

        public double size { get; }

        public AbstractCommand(string type, double size) 
        {
            this.type = type;
            this.size = size;
        }

        public abstract List<Point> execute(Point prevPoint, int steps, List<Point> points = null);
    }
}
