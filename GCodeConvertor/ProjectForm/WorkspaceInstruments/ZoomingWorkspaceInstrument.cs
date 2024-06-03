using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GCodeConvertor.WorkspaceInstruments
{
    public class ZoomingWorkspaceInstrument : WorkspaceInstrument
    {

        private const double MIN_SCALE = 1;

        public ZoomingWorkspaceInstrument(WorkspaceDrawingControl workspaceDrawingControl) : base(workspaceDrawingControl){ }

        public override void execute(EventType eventType, object sender, EventArgs e)
        {
            if(eventType == EventType.MouseWheel && sender is Grid)
            {
                mouseWheel((Grid) sender, (MouseWheelEventArgs) e);
            }
        }

        private void mouseWheel(Grid grid, MouseWheelEventArgs e)
        {
            var mousePosition = e.GetPosition(grid);

            var previousScale = ((MatrixTransform)grid.RenderTransform).Matrix.M11;
            double scale = (e.Delta > 0 ? 1.1 : 0.9) * previousScale;

            if (scale >= MIN_SCALE)
            {
                Matrix matrix = new Matrix();
                matrix.ScaleAtPrepend(scale, scale, mousePosition.X, mousePosition.Y);
                grid.RenderTransform = new MatrixTransform(matrix);
            }
        }
    }
}
