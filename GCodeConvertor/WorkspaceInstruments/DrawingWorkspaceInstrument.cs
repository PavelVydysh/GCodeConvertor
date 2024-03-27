using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Rectangle = System.Windows.Shapes.Rectangle;
using Ellipse = System.Windows.Shapes.Ellipse;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;


namespace GCodeConvertor.WorkspaceInstruments
{
    public class DrawingWorkspaceInstrument : WorkspaceInstrument
    {
        private enum DrawingStates
        {
            START,
            DRAWING,
            END
        }

        private const string END_STATE_MESSAGE = "Слой является законченным";

        private static Color POINT_COLOR = Colors.Red;
        private const double ELLIPSE_SIZE = 5;

        private static Color LINE_COLOR = Colors.Red;
        private const double LINE_SIZE = 2;

        // Текущее состояние рисовалки
        private DrawingStates drawingState;
        private int startTopologyX, startTopologyY;
        private double previousDrawingX, previousDrawingY;


        public DrawingWorkspaceInstrument(WorkspaceDrawingControl workspaceDrawingControl) : base(workspaceDrawingControl) 
        {
            drawingState = DrawingStates.START;
        }


        public override void execute(EventType eventType, object sender, EventArgs e)
        {
            if (eventType == EventType.LeftMouseButtonDown && sender is Rectangle)
                lmbDownOnCell((Rectangle) sender, (MouseButtonEventArgs) e );
        }

        private void lmbDownOnCell(Rectangle cell, MouseButtonEventArgs e)
        {
            int currentTopologyX = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.WorkspaceCanvas).X / cell.ActualWidth);
            int currentTopologyY = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.WorkspaceCanvas).Y / cell.ActualHeight);

            double currentDrawingX = Canvas.GetLeft(cell) + cell.ActualWidth / 2;
            double currentDrawingY = Canvas.GetTop(cell) + cell.ActualHeight / 2;

            int type = workspaceDrawingControl.topology.map[currentTopologyX, currentTopologyY];

            switch (drawingState)
            {
                case DrawingStates.START:
                    if (type == 2)
                    {
                        startTopologyX = currentTopologyX;
                        startTopologyY = currentTopologyY;

                        drawPoint(currentDrawingX, currentDrawingY);

                        drawingState = DrawingStates.DRAWING;

                        
                    }
                    break;
                case DrawingStates.DRAWING:
                    if (type != 3)
                    {
                        drawLine(currentDrawingX, currentDrawingY);

                        if (currentTopologyX == startTopologyX && currentTopologyY == startTopologyY)
                        {
                            drawingState = DrawingStates.END;
                        }
                    }
                    break;
                case DrawingStates.END:
                    {
                        MessageBox.Show(END_STATE_MESSAGE);
                        return;
                    }

            }
        }

        private void drawLine(double currentX, double currentY)
        {
            if(isLineCrossTheNeedles(new Point(previousDrawingX, previousDrawingY), new Point(currentX, currentY)))
            {
                return;
            }

            Line line = setupLine();

            line.X1 = previousDrawingX;
            line.Y1 = previousDrawingY;
            line.X2 = currentX;
            line.Y2 = currentY;

            workspaceDrawingControl.WorkspaceCanvas.Children.Add(line);

            drawPoint(currentX, currentY);
            // layerPoints.Add(new System.Windows.Point((double)((int)Math.Floor((e.GetPosition(CanvasMain).X / size)) + 0.5),
            //                                              (double)((int)Math.Floor(e.GetPosition(CanvasMain).Y / size) + 0.5)));

            //activeLayer.thread.Add(new System.Windows.Point(currentDotX - ELLIPSE_SIZE / 2, currentDotY - ELLIPSE_SIZE / 2));
        }

        private void drawPoint(double X, double Y)
        {
            Ellipse ellipse = setupEllipse();
            //ellipse.MouseRightButtonDown += Ellipse_MouseRightDown;
            Canvas.SetLeft(ellipse, X - ELLIPSE_SIZE / 2);
            Canvas.SetTop(ellipse, Y - ELLIPSE_SIZE / 2);
            //layerEllipses.Add(ellipse);
            workspaceDrawingControl.WorkspaceCanvas.Children.Add(ellipse);

            previousDrawingX = X;
            previousDrawingY = Y;
        }

        private Line setupLine()
        {
            Line line = new Line();
            line.Fill = new SolidColorBrush(LINE_COLOR);
            line.Visibility = Visibility.Visible;
            line.StrokeThickness = LINE_SIZE;
            line.Stroke = new SolidColorBrush(LINE_COLOR);

            return line;
        }

        private Ellipse setupEllipse()
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Height = ELLIPSE_SIZE;
            ellipse.Width = ELLIPSE_SIZE;
            ellipse.Fill = new SolidColorBrush(POINT_COLOR);

            return ellipse;
        }

        private bool isLineCrossTheNeedles(Point previousPoint, Point currentPoint)
        {
            LineGeometry lineGeometry = new LineGeometry(previousPoint, currentPoint);

            foreach (Rectangle needle in workspaceDrawingControl.needles)
            {
                RectangleGeometry rectangleGeometry = new RectangleGeometry(
                    new Rect(Canvas.GetLeft(needle), Canvas.GetTop(needle), needle.Width, needle.Height));
                if (lineGeometry.FillContainsWithDetail(rectangleGeometry) != IntersectionDetail.Empty)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
