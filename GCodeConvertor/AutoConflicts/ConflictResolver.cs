using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GCodeConvertor.AutoConflicts
{
    public abstract class ConflictResolver
    {
        public abstract Point resolve(double x, double y, double size);
    }
}
