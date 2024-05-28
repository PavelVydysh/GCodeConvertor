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
        private bool isDraggingEllipse = false;
        private Line fLine;
        private Line sLine;
        private Point offset;
        private Point startDraggingPoint;

        public DrawingWorkspaceInstrument(WorkspaceDrawingControl workspaceDrawingControl) : base(workspaceDrawingControl) { }

        public override void execute(EventType eventType, object sender, EventArgs e)
        {
            if (eventType == EventType.LeftMouseButtonDown && sender is Rectangle)
                lmbDownOnCell((Rectangle) sender, (MouseButtonEventArgs) e);
            if (eventType == EventType.LeftMouseButtonDown && sender is Ellipse)
                lmbDownOnPoint((Ellipse) sender, (MouseButtonEventArgs)e);
            if (eventType == EventType.MouseMove && sender is Ellipse)
                mouseMoveOnPoint((Ellipse)sender, (MouseEventArgs)e);
        }

        private void mouseMoveOnPoint(Ellipse point, MouseEventArgs e)
        {
            if (isDraggingEllipse)
            {
                var position = e.GetPosition(workspaceDrawingControl.WorkspaceCanvas);
                Canvas.SetLeft(point, position.X - offset.X);
                Canvas.SetTop(point, position.Y - offset.Y);

                if (fLine != null)
                {
                    UpdateLinePosition(fLine, point, false);
                }

                if (sLine != null)
                {
                    UpdateLinePosition(sLine, point, true);
                }
            }
        }

        private void lmbDownOnPoint(Ellipse point, MouseButtonEventArgs e)
        {
            if (isDraggingEllipse)
            {
                isDraggingEllipse = false;
                point.ReleaseMouseCapture();
                int newTopologyX = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.WorkspaceCanvas).X / workspaceDrawingControl.cellSize);
                int newTopologyY = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.WorkspaceCanvas).Y / workspaceDrawingControl.cellSize);
                int index = workspaceDrawingControl.activeLayer.getThreadPoint(startDraggingPoint);
                workspaceDrawingControl.activeLayer.thread[index] = new Point(getThreadValueByTopologyValue(newTopologyX), getThreadValueByTopologyValue(newTopologyY));
                workspaceDrawingControl.repaint();
                //clearTable();
                //repaintTable();
                return;
            }

            if (point.Equals(workspaceDrawingControl.ellipses[0]))
            {
                return;
            }

            int currentTopologyX = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.WorkspaceCanvas).X / workspaceDrawingControl.cellSize);
            int currentTopologyY = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.WorkspaceCanvas).Y / workspaceDrawingControl.cellSize);
            startDraggingPoint = new Point(getThreadValueByTopologyValue(currentTopologyX), getThreadValueByTopologyValue(currentTopologyY));

            CustomLine[] cuLines = workspaceDrawingControl.customLineStorage.getLinesByEllipse(point);
            if (cuLines != null)
            {
                if (cuLines[0] != null)
                {
                    fLine = cuLines[0].line;
                }

                if (cuLines[1] != null)
                {
                    sLine = cuLines[1].line;
                }
            }
            else
            {
                MessageBox.Show("Внутренняя ошибка");
            }

            isDraggingEllipse = true;
            offset = e.GetPosition(point);
            point.CaptureMouse();
        }

        private void UpdateLinePosition(Line lineL, Ellipse el, bool isSec)
        {
            var ellipseCenterX = Canvas.GetLeft(el) + el.Width / 2;
            var ellipseCenterY = Canvas.GetTop(el) + el.Height / 2;

            if (isSec)
            {
                lineL.X2 = ellipseCenterX;
                lineL.Y2 = ellipseCenterY;
                return;
            }
            lineL.X1 = ellipseCenterX;
            lineL.Y1 = ellipseCenterY;
        }

        private DrawingStates getCurrentDrawingState()
        {
            if(workspaceDrawingControl.activeLayer.thread.Count == 0)
                return DrawingStates.START;
            if (workspaceDrawingControl.activeLayer.isEnded())
                return DrawingStates.END;
            return DrawingStates.DRAWING;
        }

        private void lmbDownOnCell(Rectangle cell, MouseButtonEventArgs e)
        {
            DrawingStates currentDrawingState = getCurrentDrawingState();

            int currentTopologyX = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.WorkspaceCanvas).X / workspaceDrawingControl.cellSize);
            int currentTopologyY = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.WorkspaceCanvas).Y / workspaceDrawingControl.cellSize);

            int cellType = workspaceDrawingControl.topology.map[currentTopologyX, currentTopologyY];

            switch (currentDrawingState)
            {
                case DrawingStates.START:
                    if (cellType == 2) { 
                        drawPoint(getDrawingValueByTopologyValue(currentTopologyX), getDrawingValueByTopologyValue(currentTopologyY));
                        workspaceDrawingControl.activeLayer.thread.Add(new Point(getThreadValueByTopologyValue(currentTopologyX), getThreadValueByTopologyValue(currentTopologyY)));
                    }
                    break; 
                case DrawingStates.DRAWING:
                    if (cellType != 3) { 
                        drawLine(currentTopologyX, currentTopologyY); 
                    }
                    break;
                case DrawingStates.END:
                    MessageBox.Show(END_STATE_MESSAGE);
                    break;
            }
        }

        private double getDrawingValueByTopologyValue(int topologyValue)
        {
            return topologyValue * workspaceDrawingControl.cellSize + workspaceDrawingControl.cellSize / 2.0;
        }

        private double getDrawingValueByThreadValue(double threadValue)
        {
            return threadValue * workspaceDrawingControl.cellSize;
        }

        private double getThreadValueByTopologyValue(double topologyValue)
        {
            return topologyValue + 0.5;
        }

        private void drawLine(int currentTopologyX, int currentTopologyY)
        {
            double previousDrawingX = getDrawingValueByThreadValue(workspaceDrawingControl.activeLayer.thread.Last().X);
            double previousDrawingY = getDrawingValueByThreadValue(workspaceDrawingControl.activeLayer.thread.Last().Y);

            double currentDrawingX = getDrawingValueByTopologyValue(currentTopologyX);
            double currentDrawingY = getDrawingValueByTopologyValue(currentTopologyY);

            if (isLineCrossTheNeedles(new Point(previousDrawingX, previousDrawingY), new Point(currentDrawingX, currentDrawingY)))
            {
                return;
            }

            Line line = setupLine();
            line.X1 = previousDrawingX;
            line.Y1 = previousDrawingY;
            line.X2 = currentDrawingX;
            line.Y2 = currentDrawingY;

            workspaceDrawingControl.WorkspaceCanvas.Children.Add(line);

            Ellipse ellipse = drawPoint(currentDrawingX, currentDrawingY);

            if (workspaceDrawingControl.ellipses.Count >= 2)
            {
                workspaceDrawingControl.customLineStorage.addLine(new CustomLine(line, workspaceDrawingControl.ellipses[workspaceDrawingControl.ellipses.Count - 2], ellipse));
            }                

            workspaceDrawingControl.activeLayer.thread.Add(new Point(getThreadValueByTopologyValue(currentTopologyX), getThreadValueByTopologyValue(currentTopologyY)));

            // layerPoints.Add(new System.Windows.Point((double)((int)Math.Floor((e.GetPosition(CanvasMain).X / size)) + 0.5),
            //                                              (double)((int)Math.Floor(e.GetPosition(CanvasMain).Y / size) + 0.5)));

        }

        private Ellipse drawPoint(double currentDrawingX, double currentDrawingY)
        {
            Ellipse drawingPoint = setupEllipse();
            workspaceDrawingControl.ellipses.Add(drawingPoint);
            //ellipse.MouseRightButtonDown += Ellipse_MouseRightDown;
            Canvas.SetLeft(drawingPoint, currentDrawingX - ELLIPSE_SIZE / 2);
            Canvas.SetTop(drawingPoint, currentDrawingY - ELLIPSE_SIZE / 2);
            //layerEllipses.Add(ellipse);
            workspaceDrawingControl.WorkspaceCanvas.Children.Add(drawingPoint);
            return drawingPoint;
        }

        private Line setupLine()
        {
            Line line = new Line();
            line.Tag = workspaceDrawingControl.getCustomElementTag();
            line.Fill = new SolidColorBrush(LINE_COLOR);
            line.Visibility = Visibility.Visible;
            line.StrokeThickness = LINE_SIZE;
            line.Stroke = new SolidColorBrush(LINE_COLOR);

            return line;
        }

        private Ellipse setupEllipse()
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Tag = workspaceDrawingControl.getCustomElementTag();
            ellipse.Height = ELLIPSE_SIZE;
            ellipse.Width = ELLIPSE_SIZE;
            ellipse.Fill = new SolidColorBrush(POINT_COLOR);
            ellipse.MouseLeftButtonDown += workspaceDrawingControl.element_MouseLeftButtonDown;
            ellipse.MouseMove += workspaceDrawingControl.element_MouseMove;

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
