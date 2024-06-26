using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GCodeConvertor.AutoConflicts
{
    internal class LeftConflictResolver : ConflictResolver
    {
        public override Point resolve(double x, double y, double size)
        {
            return new Point(x - size, y);
        }
    }
}
