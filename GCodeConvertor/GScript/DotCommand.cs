using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GCodeConvertor.GScript
{
    public class DotCommand : AbstractCommand
    {
        public DotCommand(string type, double size) : base(type, size)
        {

        }

        public override Point execute(Point prevPoint, int steps)
        {
            return prevPoint;
        }
    }
}
