using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace GCodeConvertor.WorkspaceInstruments
{
    class MoveWorkspaceInstrument : WorkspaceInstrument
    {

        private const double TRANSLATION_FACTOR = 1.0;

        private Point startPoint;
        private bool isDragging;

        public MoveWorkspaceInstrument(WorkspaceDrawingControl workspaceDrawingControl) : base(workspaceDrawingControl) {}

        public override void execute(EventType eventType, object sender, EventArgs e)
        {
            if(sender is Grid)
            {
                switch (eventType)
                {
                    case EventType.LeftMouseButtonDown:
                        lmbDown((Grid) sender, (MouseButtonEventArgs) e);
                        break;
                    case EventType.MouseMove:
                        mouseMove((Grid)sender, (MouseEventArgs) e);
                        break;
                    case EventType.LeftMouseButtonUp:
                        lmbUp((Grid)sender, (MouseButtonEventArgs)e);
                        break;
                }
            }
            
        }

        private void lmbDown(Grid grid, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(grid);
            isDragging = true;
            grid.CaptureMouse();

        }

        private void mouseMove(Grid grid, MouseEventArgs e)
        {
            if (isDragging)
            {
                var position = e.GetPosition(grid);

                double dx = (position.X - startPoint.X) * TRANSLATION_FACTOR;
                double dy = (position.Y - startPoint.Y) * TRANSLATION_FACTOR;

                Matrix matrix = ((MatrixTransform)grid.RenderTransform).Matrix;

                matrix.Translate(dx, dy);

                grid.RenderTransform = new MatrixTransform(matrix);

                startPoint = position;
            }
        }

        private void lmbUp(Grid grid, MouseButtonEventArgs e)
        {
            isDragging = false;
            grid.ReleaseMouseCapture();
        }
    }
}
