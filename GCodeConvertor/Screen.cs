using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeConvertor
{
    internal class Screen
    {
        public ProjectWindow.DrawingStates drawingState { get; set; }

        public bool drawArrow { get; set; }

        public List<System.Windows.Point> layerThread { get; set; }

        public Screen(ProjectWindow.DrawingStates drawingState, bool drawArrow, List<System.Windows.Point> layerThread)
        {
            this.drawingState = drawingState;
            this.layerThread = layerThread;
            this.drawArrow = drawArrow;
        }
    }
}
