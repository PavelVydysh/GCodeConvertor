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
using System.Windows.Forms.VisualStyles;


namespace GCodeConvertor.WorkspaceInstruments
{
    public class DrawingWorkspaceInstrument : WorkspaceInstrument
    {
        private enum DrawingStates
        {
            START,
            DRAWING,
            CONFLICT,
            END
        }

        private const string END_STATE_MESSAGE = "Слой является законченным";
        private static Color POINT_COLOR = Colors.Red;
        private static Color SELECTED_POINT_COLOR = Colors.BlueViolet;
        private const double ELLIPSE_SIZE = 5;
        private static Color LINE_COLOR = Colors.Red;
        private const double LINE_SIZE = 2;
        private const double SELECTING_RECTANGLE_STROKE_THIKNESS = 2;
        private static Color SELECTING_RECTANGLE_COLOR = Colors.Blue;

        private bool isDraggingEllipse = false;

        private Line fLine;
        private Line sLine;
        private Point offset;
        private Point startDraggingPoint;
        private Point startSelectingPoint;
        private Rectangle selectingRectangle;

        public DrawingWorkspaceInstrument(WorkspaceDrawingControl workspaceDrawingControl) : base(workspaceDrawingControl) { }

        public override void execute(EventType eventType, object sender, EventArgs e)
        {
            if (eventType == EventType.LeftMouseButtonDown && sender is Rectangle)
                lmbDownOnCell((Rectangle) sender, (MouseButtonEventArgs) e);
            if (eventType == EventType.LeftMouseButtonDown && sender is Ellipse)
                lmbDownOnPoint((Ellipse) sender, (MouseButtonEventArgs)e);
            if (eventType == EventType.MouseMove && sender is Ellipse)
                mouseMoveOnPoint((Ellipse)sender, (MouseEventArgs)e);
            if (eventType == EventType.RightMouseButtonDown && sender is Ellipse)
                rmbDownOnPoint((Ellipse)sender, (MouseButtonEventArgs)e);
            if (eventType == EventType.KeyDown && sender is WorkspaceDrawingControl)
                pressOnButton((WorkspaceDrawingControl)sender, (KeyEventArgs)e);
            if (eventType == EventType.RightMouseButtonDown && sender is Canvas)
                rmbDownOnCanvas((Canvas)sender, (MouseButtonEventArgs)e);
            if (eventType == EventType.RightMouseButtonUp && sender is Canvas)
                rmbUpOnCanvas((Canvas)sender, (MouseButtonEventArgs)e);
            if (eventType == EventType.MouseMove && sender is Canvas)
                mouseMoveOnCanvas((Canvas)sender, (MouseEventArgs)e);
            if (eventType == EventType.RightMouseButtonDown && sender is Line)
                rmbDownOnLine((Line)sender, (MouseButtonEventArgs)e);

        }

        private void rmbDownOnLine(Line line, MouseButtonEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftAlt))
            {
                return;
            }
            double x = e.GetPosition(workspaceDrawingControl.workspaceCanvas).X;
            double y = e.GetPosition(workspaceDrawingControl.workspaceCanvas).Y;

            addLine(line, x, y);
            workspaceDrawingControl.repaint();
        }

        private void addLine(Line line, double x, double y)
        {
            Line pressedLine = line;
            CustomLine customLine = workspaceDrawingControl.customLineStorage.getLineByInnerLine(pressedLine);

            double threadX = getThreadValueByTopologyValue((int)Math.Floor(x / workspaceDrawingControl.cellSize));
            double threadY = getThreadValueByTopologyValue((int)Math.Floor(y / workspaceDrawingControl.cellSize));

            Point point = new Point(threadX, threadY);

            Point threadEndPoint = new Point(getThreadValueByTopologyValue((int)Math.Floor(Canvas.GetLeft(customLine.secondEllipse) / workspaceDrawingControl.cellSize)),
                                               getThreadValueByTopologyValue((int)Math.Floor(Canvas.GetTop(customLine.secondEllipse) / workspaceDrawingControl.cellSize)));

            int indexOfEndPoint = workspaceDrawingControl.activeLayer.getThreadPoint(threadEndPoint);

            workspaceDrawingControl.activeLayer.insertBeforePositionThreadPoint(point, indexOfEndPoint);
        }

        private void mouseMoveOnCanvas(Canvas canvas, MouseEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.RightButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(workspaceDrawingControl.workspaceCanvas);

                double left = Math.Min(startSelectingPoint.X, currentPoint.X);
                double top = Math.Min(startSelectingPoint.Y, currentPoint.Y);

                double width = Math.Abs(startSelectingPoint.X - currentPoint.X);
                double height = Math.Abs(startSelectingPoint.Y - currentPoint.Y);

                selectingRectangle.Width = width;
                selectingRectangle.Height = height;

                Canvas.SetLeft(selectingRectangle, left);
                Canvas.SetTop(selectingRectangle, top);
                selectEllipsesInsideSelectionRectangle(left, top, width, height);
            }
        }

        private void selectEllipsesInsideSelectionRectangle(double left, double top, double width, double height)
        {
            foreach (Point point in workspaceDrawingControl.activeLayer.thread)
            {
                double pointX = getDrawingValueByThreadValue(point.X);
                double pointY = getDrawingValueByThreadValue(point.Y);

                if (pointX >= left && pointX <= left + width &&
                        pointY >= top && pointY <= top + height)
                {
                    if (!workspaceDrawingControl.activeLayer.selectedThread.Contains(point) && workspaceDrawingControl.activeLayer.thread.IndexOf(point) != 0)
                    {
                        workspaceDrawingControl.activeLayer.selectedThread.Add(point);
                    }
                }
                else
                {
                    workspaceDrawingControl.activeLayer.selectedThread.Remove(point);
                }
            }
            workspaceDrawingControl.repaint();
        }

        private void rmbUpOnCanvas(Canvas canvas, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Released)
            {
                workspaceDrawingControl.workspaceCanvas.ReleaseMouseCapture();
                workspaceDrawingControl.workspaceCanvas.Children.Remove(selectingRectangle);
            }
        }

        private void rmbDownOnCanvas(Canvas canvas, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.ButtonState == MouseButtonState.Pressed)
            {
                workspaceDrawingControl.activeLayer.selectedThread.Clear();

                startSelectingPoint = e.GetPosition(workspaceDrawingControl.workspaceCanvas);
                selectingRectangle = new Rectangle
                {
                    StrokeThickness = 2,
                    Fill = new SolidColorBrush(SELECTING_RECTANGLE_COLOR) { Opacity = 0.3 },
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(selectingRectangle, startSelectingPoint.X);
                Canvas.SetTop(selectingRectangle, startSelectingPoint.Y);
                workspaceDrawingControl.workspaceCanvas.Children.Add(selectingRectangle);
                workspaceDrawingControl.workspaceCanvas.CaptureMouse();
                workspaceDrawingControl.repaint();
            }
        }

        private void pressOnButton(WorkspaceDrawingControl window, KeyEventArgs e)
        {
            //TODO: перенести, чтобы и в кнопках можно было делать
            if(Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Z))
            {
                workspaceDrawingControl.activeLayer.backHistory();
                workspaceDrawingControl.repaint();
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Y))
            {
                workspaceDrawingControl.activeLayer.forwardHistory();
                workspaceDrawingControl.repaint();
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Delete))
            {
                hardDelete();
                return;
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.A))
            { 
                selectAll();
                return;
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.I))
            {
                selectInterval();
                return;
            }

            switch (e.Key)
            {
                case Key.Delete:
                    {
                        deleteAll();
                        break;
                    }
            }
        }

        private void selectInterval()
        {
            if (workspaceDrawingControl.activeLayer.selectedThread.Count >= 2)
            {
                List<int> positions = new List<int>();
                foreach (Point point in workspaceDrawingControl.activeLayer.selectedThread)
                {
                    positions.Add(workspaceDrawingControl.activeLayer.thread.IndexOf(point));
                }

                int minPosition = positions.Min();
                int maxPosition = positions.Max();

                for (int i = minPosition + 1; i < maxPosition; i++)
                {
                    Point point = workspaceDrawingControl.activeLayer.thread[i];
                    if (!workspaceDrawingControl.activeLayer.selectedThread.Contains(point))
                    {
                        workspaceDrawingControl.activeLayer.selectedThread.Add(point);
                    }
                }
                workspaceDrawingControl.repaint();
            }
        }

        private void selectAll()
        {
            if (workspaceDrawingControl.activeLayer.thread.Skip(1).Except(workspaceDrawingControl.activeLayer.selectedThread).Count() == 0)
            {
                workspaceDrawingControl.activeLayer.selectedThread.Clear();
            }
            else
            {
                workspaceDrawingControl.activeLayer.selectedThread.Clear();
                workspaceDrawingControl.activeLayer.selectedThread.AddRange(workspaceDrawingControl.activeLayer.thread.Skip(1));
            }
            workspaceDrawingControl.repaint();
        }

        private void hardDelete()
        {
            int minPosition = int.MaxValue;
            foreach (Point selectedPoint in workspaceDrawingControl.activeLayer.selectedThread)
            {
                int position = workspaceDrawingControl.activeLayer.getThreadPoint(selectedPoint);
                if (position < minPosition)                 
                    minPosition = position;
            }
            if(minPosition != -1)
            {
                workspaceDrawingControl.activeLayer.removeRangeThreadPoint(minPosition, workspaceDrawingControl.activeLayer.thread.Count - minPosition);
                workspaceDrawingControl.activeLayer.selectedThread.Clear();
                workspaceDrawingControl.repaint();
            }
        }

        private void deleteAll()
        {
            workspaceDrawingControl.activeLayer.removeAllSelectedThreadPoint();
            workspaceDrawingControl.repaint();
        }

        private void rmbDownOnPoint(Ellipse point, MouseButtonEventArgs e) {
            double threadX = getThreadValueByTopologyValue((int)Math.Floor(e.GetPosition(workspaceDrawingControl.workspaceCanvas).X / workspaceDrawingControl.cellSize));
            double threadY = getThreadValueByTopologyValue((int)Math.Floor(e.GetPosition(workspaceDrawingControl.workspaceCanvas).Y / workspaceDrawingControl.cellSize));

            Point selectedPoint = new Point(threadX, threadY);

            if (workspaceDrawingControl.activeLayer.thread[0].Equals(selectedPoint))
            {
                return;
            }
            if (workspaceDrawingControl.activeLayer.isDotSelected(selectedPoint))
            {
                workspaceDrawingControl.activeLayer.selectedThread.Remove(selectedPoint);
                point.Fill = new SolidColorBrush(POINT_COLOR);
            }
            else
            {
                workspaceDrawingControl.activeLayer.selectedThread.Add(selectedPoint);
                point.Fill = new SolidColorBrush(SELECTED_POINT_COLOR);
            }
        }

        private void mouseMoveOnPoint(Ellipse point, MouseEventArgs e)
        {
            if (isDraggingEllipse)
            {
                var position = e.GetPosition(workspaceDrawingControl.workspaceCanvas);
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
                int newTopologyX = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.workspaceCanvas).X / workspaceDrawingControl.cellSize);
                int newTopologyY = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.workspaceCanvas).Y / workspaceDrawingControl.cellSize);
                int index = workspaceDrawingControl.activeLayer.getThreadPoint(startDraggingPoint);
                workspaceDrawingControl.activeLayer.changeThreadPoint(new Point(getThreadValueByTopologyValue(newTopologyX), getThreadValueByTopologyValue(newTopologyY)), index);
                int selectedIndex = workspaceDrawingControl.activeLayer.getSelectedThreadPoint(startDraggingPoint);
                if (selectedIndex != -1)
                {
                    workspaceDrawingControl.activeLayer.selectedThread[selectedIndex] = new Point(getThreadValueByTopologyValue(newTopologyX), getThreadValueByTopologyValue(newTopologyY));
                }
                workspaceDrawingControl.repaint();
                //clearTable();
                //repaintTable();
                return;
            }

            if (point.Equals(workspaceDrawingControl.ellipses[0]))
            {
                return;
            }

            int currentTopologyX = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.workspaceCanvas).X / workspaceDrawingControl.cellSize);
            int currentTopologyY = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.workspaceCanvas).Y / workspaceDrawingControl.cellSize);
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
            if (workspaceDrawingControl.conflictLines.Count > 0)
                return DrawingStates.CONFLICT;
            return DrawingStates.DRAWING;
        }

        private void lmbDownOnCell(Rectangle cell, MouseButtonEventArgs e)
        {
            DrawingStates currentDrawingState = getCurrentDrawingState();

            int currentTopologyX = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.workspaceCanvas).X / workspaceDrawingControl.cellSize);
            int currentTopologyY = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.workspaceCanvas).Y / workspaceDrawingControl.cellSize);

            int cellType = workspaceDrawingControl.topology.map[currentTopologyX, currentTopologyY];

            switch (currentDrawingState)
            {
                case DrawingStates.CONFLICT:
                    MessageBoxResult result = MessageBox.Show("На указанном маршруте обнаружен конфликт: траектория сопла проходит через иглу.\n Нажмите \"Отмена\", чтобы вернуть траекторию в исходное состояние.\n" +
                                                     "Нажмите \"Ок\" для решения конфликта вручную.", "Конфликт", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    break;
                case DrawingStates.START:
                    if (cellType == 2) {
                        if (ProjectSettings.preset.checkIfPointCanBeFirst(
                            new Point(getThreadValueByTopologyValue(currentTopologyX),
                            getThreadValueByTopologyValue(currentTopologyY)))) 
                            {
                            drawPoint(getDrawingValueByTopologyValue(currentTopologyX), getDrawingValueByTopologyValue(currentTopologyY));
                            workspaceDrawingControl.activeLayer.addThreadPoint(new Point(getThreadValueByTopologyValue(currentTopologyX), getThreadValueByTopologyValue(currentTopologyY)));
                        }
                        else
                        {
                            MessageBox.Show("Стартовая точка текущего слоя не совпадает со стартовыми точками других слоев.", "Недопустимая стартовая точка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
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
            //double previousDrawingX = getDrawingValueByThreadValue(workspaceDrawingControl.activeLayer.thread.Last().X);
            //double previousDrawingY = getDrawingValueByThreadValue(workspaceDrawingControl.activeLayer.thread.Last().Y);

            //double currentDrawingX = getDrawingValueByTopologyValue(currentTopologyX);
            //double currentDrawingY = getDrawingValueByTopologyValue(currentTopologyY);

            //if (isLineCrossTheNeedles(new Point(previousDrawingX, previousDrawingY), new Point(currentDrawingX, currentDrawingY)))
            //{
            //    return;
            //}

            //Line line = setupLine();
            //line.X1 = previousDrawingX;
            //line.Y1 = previousDrawingY;
            //line.X2 = currentDrawingX;
            //line.Y2 = currentDrawingY;

            //workspaceDrawingControl.workspaceCanvas.Children.Add(line);

            //Ellipse ellipse = drawPoint(currentDrawingX, currentDrawingY);

            //if (workspaceDrawingControl.ellipses.Count >= 2)
            //{
            //    workspaceDrawingControl.customLineStorage.addLine(new CustomLine(line, workspaceDrawingControl.ellipses[workspaceDrawingControl.ellipses.Count - 2], ellipse));
            //}                

            workspaceDrawingControl.activeLayer.addThreadPoint(new Point(getThreadValueByTopologyValue(currentTopologyX), getThreadValueByTopologyValue(currentTopologyY)));
            workspaceDrawingControl.repaint();
        }

        private Ellipse drawPoint(double currentDrawingX, double currentDrawingY)
        {
            Ellipse drawingPoint = setupEllipse();
            workspaceDrawingControl.ellipses.Add(drawingPoint);
            Canvas.SetLeft(drawingPoint, currentDrawingX - ELLIPSE_SIZE / 2);
            Canvas.SetTop(drawingPoint, currentDrawingY - ELLIPSE_SIZE / 2);
            workspaceDrawingControl.workspaceCanvas.Children.Add(drawingPoint);
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
            line.MouseRightButtonDown += workspaceDrawingControl.element_MouseRightButtonDown;

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
            ellipse.MouseRightButtonDown += workspaceDrawingControl.element_MouseRightButtonDown;

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
