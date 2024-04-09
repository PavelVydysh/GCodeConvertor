using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GCodeConvertor.GScript
{
    public class RightCommand : AbstractCommand
    {
        public RightCommand(string type, double size) : base(type, size)
        {

        }

        public override Point execute(Point prevPoint, int steps)
        {
            return new Point(prevPoint.X + steps * size, prevPoint.Y);
        }
    }
}
