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
            if(sender is Canvas)
            {
                switch (eventType)
                {
                    case EventType.LeftMouseButtonDown:
                        lmbDown((Canvas) sender, (MouseButtonEventArgs) e);
                        break;
                    case EventType.MouseMove:
                        mouseMove((Canvas)sender, (MouseEventArgs) e);
                        break;
                    case EventType.LeftMouseButtonUp:
                        lmbUp((Canvas)sender, (MouseButtonEventArgs)e);
                        break;
                }
            }
            
        }

        private void lmbDown(Canvas canvas, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(canvas);
            isDragging = true;
            canvas.CaptureMouse();

        }

        private void mouseMove(Canvas canvas, MouseEventArgs e)
        {
            if (isDragging)
            {
                var position = e.GetPosition(canvas);

                double dx = (position.X - startPoint.X) * TRANSLATION_FACTOR;
                double dy = (position.Y - startPoint.Y) * TRANSLATION_FACTOR;

                Matrix matrix = ((MatrixTransform)canvas.RenderTransform).Matrix;

                matrix.Translate(dx, dy);

                canvas.RenderTransform = new MatrixTransform(matrix);

                startPoint = position;
            }
        }

        private void lmbUp(Canvas canvas, MouseButtonEventArgs e)
        {
            isDragging = false;
            canvas.ReleaseMouseCapture();
        }
    }
}
