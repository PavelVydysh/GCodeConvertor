using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using GCodeConvertor.WorkspaceInstruments;
using static System.Windows.Forms.LinkLabel;
using GCodeConvertor.AutoConflicts;
using System.Diagnostics;
using System.ComponentModel;
using System.Xml.Linq;

namespace GCodeConvertor
{
    /// <summary>
    /// Логика взаимодействия для WorkspaceDrawingControl.xaml
    /// </summary>
    public partial class WorkspaceDrawingControl : UserControl, INotifyPropertyChanged
    {
        private static SolidColorBrush WORKSPACE_START_POINT_BRUSH = (SolidColorBrush)Application.Current.Resources["WorkspaceStartPointBrush"];
        private static SolidColorBrush WORKSPACE_BACKGROUND_BRUSH = (SolidColorBrush)Application.Current.Resources["WorkspaceBackgroundBrush"];
        private static SolidColorBrush WORKSPACE_NEEDLE_BRUSH = (SolidColorBrush)Application.Current.Resources["WorkspaceNeedleBrush"];
        private static SolidColorBrush WORKSPACE_PLATFORM_BRUSH = (SolidColorBrush)Application.Current.Resources["WorkspacePlatformBrush"];

        public static Color POINT_COLOR = Colors.Red;
        public static double ELLIPSE_SIZE = 5;
        public static Color LINE_COLOR = Colors.Red;
        public static double LINE_SIZE = 2;
        private static Color SELECTED_POINT_COLOR = Colors.BlueViolet;
        private static Color CONFLICT_LINE_COLOR = Colors.Gray;

        private static Color PREDICTED_ELLIPSE_COLOR = Colors.Black;
        private static double PREDICTED_ELLIPSE_SIZE = 5;
        private static Color PREDICTED_LINE_COLOR = Colors.Black;
        private static double PREDICTED_LINE_SIZE = 2;
        //??????????????????????????

        // Константы имён элементов
        private const string WORKSPACE_CANVAS_NAME = "WorkspaceCanvas";
        private const string CUSTOM_ELEMENT_TAG = "custom";

        public event PropertyChangedEventHandler? PropertyChanged;

        // Элементы UI элементов рабочей области
        public Canvas workspaceCanvas { get; }
        
        // Списки иголок кружочков квадратов
        public List<Ellipse> ellipses { get; }
        public List<Rectangle> needles { get; } // список игл 
        private HashSet<Rectangle> rectangles { get; } // крутой список игол
        public List<Line> conflictLines { get; }

        public CustomLineStorage customLineStorage { get; set; }
        public double cellSize { get; set; }

        // Элементы логики рабочей области
        public Topology topology { get; }
        public Layer activeLayer { get; set; }
        public WorkspaceInstrument workspaceIntrument { get; set; }
        public bool areAnyConflictsHere { get; set; }

        private string currentThreadXPosition;
        public string CurrentThreadXPosition {
            get { return currentThreadXPosition; }
            set
            {
                if (currentThreadXPosition != value)
                {
                    currentThreadXPosition = value;
                    OnPropertyChanged("CurrentThreadXPosition");
                }
            }
        }
        private string currentThreadYPosition;

        public string CurrentThreadYPosition
        {
            get { return currentThreadYPosition; }
            set
            {
                if (currentThreadYPosition != value)
                {
                    currentThreadYPosition = value;
                    OnPropertyChanged("CurrentThreadYPosition");
                }
            } 
        }

        public WorkspaceDrawingControl(Topology topology)
        {
            InitializeComponent();

            WORKSPACE_START_POINT_BRUSH = (SolidColorBrush)Application.Current.Resources["WorkspaceStartPointBrush"];
            WORKSPACE_BACKGROUND_BRUSH = (SolidColorBrush)Application.Current.Resources["WorkspaceBackgroundBrush"];
            WORKSPACE_NEEDLE_BRUSH = (SolidColorBrush)Application.Current.Resources["WorkspaceNeedleBrush"];
            WORKSPACE_PLATFORM_BRUSH = (SolidColorBrush)Application.Current.Resources["WorkspacePlatformBrush"];

            this.topology = topology;

            //Инициализация Canvas (рабочей области)
            workspaceCanvas = (Canvas)FindName(WORKSPACE_CANVAS_NAME);
            setupWorkspaceCanvasEvents();
            needles = new List<Rectangle>();
            rectangles = new HashSet<Rectangle>();
            ellipses = new List<Ellipse>();
            conflictLines = new List<Line>();
            customLineStorage = new CustomLineStorage();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void setupWorkspaceCanvasEvents()
        {
            workspaceCanvas.Loaded += Canvas_Loaded;

            // Назначаем обработку событий, за которые должны отвечать инструменты на реализацию WorkspaceInstrument
            workspaceCanvas.MouseLeftButtonDown += element_MouseLeftButtonDown;
            workspaceCanvas.MouseRightButtonDown += element_MouseRightButtonDown;
            workspaceCanvas.MouseRightButtonUp += element_MouseRightButtonUp;
            workspaceCanvas.MouseLeftButtonUp += element_MouseLeftButtonUp;
            workspaceCanvas.MouseMove += element_MouseMove;
            workspaceCanvas.MouseWheel += element_MouseWheel;
            this.KeyDown += element_KeyDown;
        }

        private void executeInstrument(EventType eventType, object sender, EventArgs e)
        {
            if (workspaceIntrument is not null)
            {
                workspaceIntrument.execute(eventType, sender, e);
            }
        }

        /// <summary>
        /// Интерфейс для выбора активного слоя "рисовалки"
        /// </summary>
        /// <param name="activeLayer">Слой, с которым должна работать "рисовалка"</param>
        public void setActiveLayer(Layer activeLayer)
        {
            this.activeLayer = activeLayer;
            repaint();
        }

        public void repaint()
        {
            customLineStorage.clear();
            ellipses.Clear();
            conflictLines.Clear();
            deleteCustomItems();
            if (activeLayer.isStarted() && checkRubberBandStatus())
            {
                initSpringLines();
            }
            initLayer();
        }

        private bool checkRubberBandStatus()
        {
            if(Settings.Default.RubberBand == "On")
            {
                return true;
            }
            return false;
        }

        private bool checkAutoConflictsStatus()
        {
            if(Settings.Default.ConflictResolving == "Auto")
            {
                return true;
            }
            return false;
        }

        private void initSpringLines()
        {
            List<Point> drawingPoints = new List<Point>();

            foreach (Point point in activeLayer.thread)
            {
                drawingPoints.Add(new Point(getDrawingValueByThreadValue(point.X), getDrawingValueByThreadValue(point.Y)));
            }

            List<Point> bandPoints = getRubberBandPath(drawingPoints.ToArray());

            int pointLastIndex = bandPoints.Count - 1;

            for (int pointIndex = 0; pointIndex <= pointLastIndex; pointIndex++)
            {
                //drawPredictedPoint(bandPoints[pointIndex].X, bandPoints[pointIndex].Y);

                if (pointIndex != pointLastIndex)
                {
                    drawPredictedLine(bandPoints[pointIndex].X, bandPoints[pointIndex].Y, bandPoints[pointIndex + 1].X, bandPoints[pointIndex + 1].Y);
                }
            }
        }

        //private void drawPredictedPoint(double  x, double y)
        //{
        //    Ellipse drawingPoint = setupPredictEllipse();
        //    ellipses.Add(drawingPoint);
        //    Canvas.SetLeft(drawingPoint, x - ELLIPSE_SIZE / 2);
        //    Canvas.SetTop(drawingPoint, y - ELLIPSE_SIZE / 2);
        //    workspaceCanvas.Children.Add(drawingPoint);
        //}

        //private Ellipse setupPredictEllipse()
        //{
        //    Ellipse ellipse = new Ellipse();
        //    ellipse.Tag = getCustomElementTag();
        //    ellipse.Height = PREDICTED_ELLIPSE_SIZE;
        //    ellipse.Width = PREDICTED_ELLIPSE_SIZE;
        //    ellipse.Fill = new SolidColorBrush(PREDICTED_ELLIPSE_COLOR);
        //    return ellipse;
        //}

        private void drawPredictedLine(double startX, double startY, double endX, double endY)
        {
            Line drawingLine = setupPredictedLine();
            drawingLine.X1 = startX;
            drawingLine.Y1 = startY;
            drawingLine.X2 = endX;
            drawingLine.Y2 = endY;
            workspaceCanvas.Children.Add(drawingLine);
        }

        private Line setupPredictedLine()
        {
            Line line = new Line();
            line.Tag = getCustomElementTag();
            line.Fill = new SolidColorBrush(PREDICTED_LINE_COLOR);
            line.Visibility = Visibility.Visible;
            line.StrokeThickness = PREDICTED_LINE_SIZE;
            line.Stroke = new SolidColorBrush(PREDICTED_LINE_COLOR);
            return line;
        }


        public void addDrawingPointsToActiveLayer(List<Point> points)
        {
            activeLayer.addAllThreadPoints(points);
            repaint();
        }

        private void deleteCustomItems()
        {
            var canvasChildrens = workspaceCanvas.Children.Cast<FrameworkElement>().ToList();

            foreach (var child in canvasChildrens)
            {
                if(child.Tag is not null && child.Tag.Equals(CUSTOM_ELEMENT_TAG))
                    workspaceCanvas.Children.Remove(child);
            }
        }

        /// <summary>
        /// Интерфейс для выбора активного инструмента рисовалки
        /// </summary>
        /// <param name="workspaceInstrument">Объект, реализующий интерфейс WorkspaceInstrument</param>
        public void setActiveWorkspaceInstrument(WorkspaceInstrument workspaceInstrument)
        {
            this.workspaceIntrument = workspaceInstrument;
        }

        public string getCustomElementTag()
        {
            return CUSTOM_ELEMENT_TAG;
        }
        
        // Инициализации сетки поля и ячеек
        private void initWorkspace()
        {
            cellSize = (double)(workspaceCanvas.Height / (topology.map.GetLength(1) > topology.map.GetLength(0) ? topology.map.GetLength(1) : topology.map.GetLength(0)));

            ELLIPSE_SIZE = PREDICTED_LINE_SIZE = cellSize / 2;
            LINE_SIZE = PREDICTED_LINE_SIZE = cellSize / 4;

            for (int topologyX = 0; topologyX < topology.map.GetLength(0); topologyX++)
            {
                for (int topologyY = 0; topologyY < topology.map.GetLength(1); topologyY++)
                {
                    Rectangle cell = setupCell(topology.map[topologyX, topologyY], topologyX, topologyY);

                    Canvas.SetLeft(cell, cellSize * topologyX);
                    Canvas.SetTop(cell, cellSize * topologyY);

                    if (topology.map[topologyX, topologyY] == 3)
                        setupRectangle(topologyX, topologyY);

                    workspaceCanvas.Children.Add(cell);
                }
            }

            for (int i = 0; i < topology.map.GetLength(0); i++)
            {
                for (int j = 0; j < topology.map.GetLength(1); j++)
                {
                    if (topology.map[i, j] == 4) topology.map[i, j] = 3;
                }
            }
        }

        private Rectangle setupCell(int cellType, int topologyX, int topologyY)
        {
            Rectangle cell = new Rectangle();

            cell.Height = cellSize;
            cell.Width = cellSize;
            cell.StrokeThickness = 1;

            if (cellType == 1)
            {
                cell.Fill = WORKSPACE_PLATFORM_BRUSH;
            }
            else if (cellType == 2)
            {
                cell.Fill = WORKSPACE_START_POINT_BRUSH;
            }
            else if (cellType == 3 || cellType == 4)
            {
                cell.Fill = WORKSPACE_NEEDLE_BRUSH;
                //if (cellType == 3)
                //{
                //    setupRectangle(topologyX, topologyY);
                //}
                needles.Add(cell);
            }
            else
            {
                cell.Fill = WORKSPACE_BACKGROUND_BRUSH;
            }

            cell.MouseLeftButtonDown += element_MouseLeftButtonDown;

            return cell;
        }

        private void setupRectangle(int i, int j)
        {
            int downHeight = 0;
            int rightWidth = 0;

            while (ProjectSettings.preset.topology.map[i, j + rightWidth] == 3)
            {
                ProjectSettings.preset.topology.map[i, j + downHeight] = 4;

                rightWidth = 0;
                while (ProjectSettings.preset.topology.map[i + rightWidth + 1, j + downHeight] == 3)
                {
                    ProjectSettings.preset.topology.map[i + rightWidth + 1, j + downHeight] = 4;
                    rightWidth++;   
                }
                downHeight++;
            }

            Rectangle rectangleToAdd = new Rectangle();
            rectangleToAdd.Height = (downHeight) * cellSize;
            rectangleToAdd.Width = (rightWidth + 1) * cellSize;
            rectangleToAdd.StrokeThickness = 1;
            Canvas.SetTop(rectangleToAdd, cellSize * j);
            Canvas.SetLeft(rectangleToAdd, cellSize * i);
            rectangles.Add(rectangleToAdd);
        }

        private void initLayer()
        {
            int pointLastIndex = activeLayer.thread.Count - 1;

            Ellipse oldEllipse = null;
            Ellipse currentEllipse = null;
            Line currentLine = null;

            areAnyConflictsHere = false;

            for (int pointIndex = 0; pointIndex <= pointLastIndex; pointIndex++)
            {
                int previousPointIndex = pointIndex - 1;

                bool isConflict = false;

                if (previousPointIndex >= 0)
                {
                    isConflict |= isLineCrossTheNeedles(
                        new Point(getDrawingValueByThreadValue(activeLayer.thread[previousPointIndex].X),
                                  getDrawingValueByThreadValue(activeLayer.thread[previousPointIndex].Y)),
                        new Point(getDrawingValueByThreadValue(activeLayer.thread[pointIndex].X),
                                  getDrawingValueByThreadValue(activeLayer.thread[pointIndex].Y))
                        );
                }

                if (isConflict)
                {
                    currentLine.Stroke = new SolidColorBrush(CONFLICT_LINE_COLOR);
                    conflictLines.Add(currentLine);
                }

                Line tempLine = null;

                if (pointIndex != pointLastIndex)
                {
                    tempLine = drawLine(
                        getDrawingValueByThreadValue(activeLayer.thread[pointIndex].X),
                        getDrawingValueByThreadValue(activeLayer.thread[pointIndex].Y),
                        getDrawingValueByThreadValue(activeLayer.thread[pointIndex + 1].X),
                        getDrawingValueByThreadValue(activeLayer.thread[pointIndex + 1].Y));
                }

                oldEllipse = currentEllipse;
                currentEllipse = drawPoint(
                    getDrawingValueByThreadValue(activeLayer.thread[pointIndex].X),
                    getDrawingValueByThreadValue(activeLayer.thread[pointIndex].Y),
                    activeLayer.isDotSelected(activeLayer.thread[pointIndex]));

                if (ellipses.Count >= 2)
                {
                    customLineStorage.addLine(new CustomLine(currentLine, oldEllipse, currentEllipse));
                }

                currentLine = tempLine;
            }
            if(checkAutoConflictsStatus())
            {
                if(conflictLines.Count > 0)
                {
                    resolveConflict(conflictLines[conflictLines.Count - 1]);
                    repaint();
                }
            }

        }

        private void resolveConflict(Line conflictLine)
        {
            CustomLine custLine = customLineStorage.getLineByInnerLine(conflictLine);

            Line line = custLine.line;
            double midX = (line.X1 + line.X2) / 2;
            double midY = (line.Y1 + line.Y2) / 2;
            ConflictResolver resolver = null;

            EllipseGeometry ellipseGeometry = new EllipseGeometry(new Point(midX, midY), ELLIPSE_SIZE, ELLIPSE_SIZE);
            foreach (Rectangle rect in rectangles)
            {
                RectangleGeometry rectGeom = new RectangleGeometry(new Rect(Canvas.GetLeft(rect), Canvas.GetTop(rect), rect.Width, rect.Height));
                IntersectionDetail intersectionDetail = ellipseGeometry.FillContainsWithDetail(rectGeom);
                if (intersectionDetail != IntersectionDetail.Empty)
                {
                    resolver = findConflictResolver(Canvas.GetLeft(custLine.firstEllipse),
                                                        Canvas.GetTop(custLine.firstEllipse),
                                                        Canvas.GetLeft(custLine.secondEllipse),
                                                        Canvas.GetTop(custLine.secondEllipse));
                }

                while (intersectionDetail != IntersectionDetail.Empty)
                {
                    Debug.WriteLine($"Intersection detected at ({midX}, {midY})");
                    Point point = resolver.resolve(midX, midY, cellSize);
                    midX = point.X;
                    midY = point.Y;

                    int indexPoint = activeLayer.getThreadPoint(new Point(getThreadValueByTopologyValue((int)Math.Floor(point.X / cellSize)),
                                               getThreadValueByTopologyValue((int)Math.Floor(point.Y / cellSize))));
                    if (indexPoint != -1) 
                    {
                        point = resolver.resolve(midX, midY, cellSize);
                        midX = point.X;
                        midY = point.Y;
                    }

                    ellipseGeometry = new EllipseGeometry(new Point(midX, midY), ELLIPSE_SIZE, ELLIPSE_SIZE);
                    intersectionDetail = ellipseGeometry.FillContainsWithDetail(rectGeom);
                    foreach (Rectangle rectIn in rectangles)
                    {
                        rectGeom = new RectangleGeometry(new Rect(Canvas.GetLeft(rectIn), Canvas.GetTop(rectIn), rectIn.Width, rectIn.Height));
                        intersectionDetail = ellipseGeometry.FillContainsWithDetail(rectGeom);
                        if (intersectionDetail != IntersectionDetail.Empty)
                        {
                            break;
                        }
                    }

                    Debug.WriteLine($"Moved to ({midX}, {midY}), new intersection detail: {intersectionDetail}");
                }
            }
            addLine(line, midX, midY);
            Debug.WriteLine($"Ellipse placed at ({midX}, {midY})");
        }

        private void addLine(Line line, double x, double y)
        {
            Line pressedLine = line;
            CustomLine customLine = customLineStorage.getLineByInnerLine(pressedLine);

            double threadX = getThreadValueByTopologyValue((int)Math.Floor(x / cellSize));
            double threadY = getThreadValueByTopologyValue((int)Math.Floor(y / cellSize));

            Point point = new Point(threadX, threadY);

            Point threadEndPoint = new Point(getThreadValueByTopologyValue((int)Math.Floor(Canvas.GetLeft(customLine.secondEllipse) / cellSize)),
                                               getThreadValueByTopologyValue((int)Math.Floor(Canvas.GetTop(customLine.secondEllipse) / cellSize)));

            int indexOfEndPoint = activeLayer.getThreadPoint(threadEndPoint);

            activeLayer.insertBeforePositionThreadPoint(point, indexOfEndPoint);
            MessageBox.Show("");
        }

        private double getThreadValueByTopologyValue(double topologyValue)
        {
            return topologyValue * ProjectSettings.preset.topology.accuracy + ProjectSettings.preset.topology.accuracy / 2;
        }

        private ConflictResolver findConflictResolver(double fX, double fY, double sX, double sY)
        {
            double midPosX = (fX + sX) / 2;
            double midPosY = (fY + sY) / 2;
            int fIterations = 0;
            int sIterations = 0;
            if ((fX > sX && fY > sY) || (fX < sX && fY < sY))
            {
                fIterations = checkDirection(midPosX, midPosY, -1, 1);
                sIterations = checkDirection(midPosX, midPosY, 1, -1);

                if (fIterations > sIterations)
                {
                    return new FrontUpConflictResolver();
                }
                else
                {
                    return new BackBottomConflictResolver();
                }
            }
            else if ((fX > sX && fY < sY) || (fX < sX && fY > sY))
            {
                fIterations = checkDirection(midPosX, midPosY, 1, 1);
                sIterations = checkDirection(midPosX, midPosY, -1, -1);

                if (fIterations < sIterations)
                {
                    return new FrontBottomConflictResolver();
                }
                else
                {
                    return new BackUpConflictResolver();
                }
            }
            else if ((fX > sX && fY == sY) || (fX < sX && fY == sY))
            {
                fIterations = checkDirection(midPosX, midPosY, 0, 1);
                sIterations = checkDirection(midPosX, midPosY, 0, -1);

                if (fIterations <= sIterations)
                {
                    return new DownConflictResolver();
                }
                else
                {
                    return new UpConflictResolver();
                }
            }
            else if ((fX == sX && fY > sY) || (fX == sX && fY < sY))
            {
                fIterations = checkDirection(midPosX, midPosY, -1, 0);
                sIterations = checkDirection(midPosX, midPosY, 1, 0);

                if (fIterations < sIterations)
                {
                    return new LeftConflictResolver();
                }
                else
                {
                    return new RightConflictResolver();
                }
            }
            return new FrontUpConflictResolver();
        }

        private int checkDirection(double tempMidPosX, double tempMidPosY, int xPlus, int yPlus)
        {
            int iterations = 0;
            IntersectionDetail intersectionDetail;
            foreach (Rectangle rectIn in needles)
            {
                EllipseGeometry elGeom = new EllipseGeometry(new Point(tempMidPosX, tempMidPosY), ELLIPSE_SIZE, ELLIPSE_SIZE);
                RectangleGeometry rectGeo = new RectangleGeometry(new Rect(Canvas.GetLeft(rectIn), Canvas.GetTop(rectIn), rectIn.Width, rectIn.Height));
                intersectionDetail = elGeom.FillContainsWithDetail(rectGeo);
                while (intersectionDetail != IntersectionDetail.Empty)
                {
                    if (xPlus == 1)
                    {
                        tempMidPosX += cellSize;
                    }
                    else if (xPlus == -1)
                    {
                        tempMidPosX -= cellSize;
                    }

                    if (yPlus == 1)
                    {
                        tempMidPosY += cellSize;
                    }
                    else if (yPlus == -1)
                    {
                        tempMidPosY -= cellSize;
                    }
                    elGeom = new EllipseGeometry(new Point(tempMidPosX, tempMidPosY), ELLIPSE_SIZE, ELLIPSE_SIZE);
                    intersectionDetail = elGeom.FillContainsWithDetail(rectGeo);
                    iterations++;
                }
            }

            return iterations;
        }

        private double FindDistanceToSegment(Point pt, Point p1, Point p2)
        {
            Point closest;
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // Это точка не отрезка.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Вычислим t, который минимизирует расстояние.
            double t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            // Посмотрим, представляет ли это один из сегментов
            // конечные точки или точка в середине.
            if (t < 0)
            {
                closest = new Point(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new Point(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new Point(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return Math.Sqrt(dx * dx + dy * dy);
        }

        public List<Point> getRubberBandPath(Point[] route)
        {
            route = RemoveExtraPoints(route.ToList(), 1).ToArray();
            route = RoutePreprocessing(route, cellSize);
            var result = new List<Point>();
            var sigmentStart = 0;
            result.Add(route[0]);
            Point point;
            Rectangle block = null;
            bool isInsert;
            for (int i = 1; i < route.GetLength(0); i++)
            {
                point = route[i];
                isInsert = false;

                for (int j = sigmentStart; j < i; j++)
                {
                    foreach (Rectangle rectangle in rectangles)
                    {
                        Point rectangleCenter = new Point(Canvas.GetLeft(rectangle) + rectangle.Width / 2, Canvas.GetTop(rectangle) + rectangle.Height / 2);
                        double distance = FindDistanceToSegment(rectangleCenter, route[j], point);

                        Boolean isBlock = false;
                        if (distance < rectangle.Width / 2) isBlock = true;
                        if (isBlock)
                        {
                            double angle = -Math.Atan2(route[j].Y - point.Y, route[j].X - point.X);
                            if (angle > Math.PI / 4 || angle < -3 * Math.PI / 4)
                            {
                                isInsert = true;
                                block = rectangle;
                                sigmentStart = j;
                            }
                            else
                            {
                                isInsert = true;
                                block = rectangle;
                                sigmentStart = j;
                                break;
                            }

                        }
                    }
                }

                if (isInsert)
                {
                    Point leftTop = new Point(Canvas.GetLeft(block) - cellSize / 2, Canvas.GetTop(block) - cellSize / 2);
                    Point rightTop = new Point(Canvas.GetLeft(block) + block.Width + cellSize / 2, Canvas.GetTop(block) - cellSize / 2);
                    Point rightDown = new Point(Canvas.GetLeft(block) + block.Width + cellSize / 2, Canvas.GetTop(block) + block.Height + cellSize / 2);
                    Point leftDown = new Point(Canvas.GetLeft(block) - cellSize / 2, Canvas.GetTop(block) + block.Height + cellSize / 2);

                    Point[] segment = new Point[i - sigmentStart];
                    Array.Copy(route, sigmentStart, segment, 0, i - sigmentStart);

                    double leftTopMinDistance = GetAvgDistance(leftTop, segment);
                    double rightTopMinDistance = GetAvgDistance(rightTop, segment);
                    double rightDownMinDistance = GetAvgDistance(rightDown, segment);
                    double leftDownMinDistance = GetAvgDistance(leftDown, segment);


                    double minDistance = double.MaxValue;
                    double angle = -Math.Atan2(route[i].Y - route[sigmentStart].Y, route[i].X - route[sigmentStart].X);
                    Point nearestPoint;

                    // левый верх
                    if ((-Math.PI < angle && angle <= -Math.PI / 2 || 0 <= angle && angle < Math.PI / 2) && leftTopMinDistance < minDistance)
                    {
                        minDistance = leftTopMinDistance;
                        nearestPoint = leftTop;
                    }
                    // правый верх
                    if ((-Math.PI / 2 <= angle && angle < 0 || Math.PI / 2 < angle && angle <= Math.PI) && rightTopMinDistance < minDistance)
                    {
                        minDistance = rightTopMinDistance;
                        nearestPoint = rightTop;
                    }
                    // правый низ
                    if ((0 < angle && angle <= Math.PI / 2 || -Math.PI <= angle && angle < -Math.PI / 2) && rightDownMinDistance < minDistance)
                    {
                        minDistance = rightDownMinDistance;
                        nearestPoint = rightDown;
                    }
                    // левый низ
                    if ((Math.PI / 2 <= angle && angle < Math.PI || -Math.PI / 2 < angle && angle <= 0) && leftDownMinDistance < minDistance)
                    {
                        minDistance = leftDownMinDistance;
                        nearestPoint = leftDown;
                    }
                    result.Add(nearestPoint);
                    sigmentStart = i - 1;
                    route[sigmentStart] = nearestPoint;
                }
            }
            if (result.Count > 1) result.Add(route.Last());
            return RemoveExtraPoints(result, 1);
            //return result;
        }

        public static Point[] RoutePreprocessing(Point[] route, double segmentLength)
        {
            List<Point> subdividedRoute = new List<Point>();

            for (int i = 0; i < route.Length - 1; i++)
            {
                Point start = route[i];
                Point end = route[i + 1];

                double distance = GetDistance(start, end);
                int segmentsCount = (int)Math.Ceiling(distance / segmentLength);

                double deltaX = (end.X - start.X) / distance * segmentLength;
                double deltaY = (end.Y - start.Y) / distance * segmentLength;

                for (int j = 0; j < segmentsCount; j++)
                {
                    double newX = start.X + deltaX * j;
                    double newY = start.Y + deltaY * j;
                    subdividedRoute.Add(new Point(newX, newY));
                }
            }

            subdividedRoute.Add(route[route.Length - 1]);

            return subdividedRoute.ToArray();
        }

        public List<Point> RemoveExtraPoints(List<Point> subdividedRoute, int count)
        {
            List<Point> start;
            do
            {
                start = new List<Point>(subdividedRoute);

                for (int i = 0; i < subdividedRoute.Count - 2;)
                {

                    Boolean isBlock = false;
                    foreach (Rectangle rectangle in rectangles)
                    {
                        Point rectangleCenter = new Point(Canvas.GetLeft(rectangle) + rectangle.Width / 2, Canvas.GetTop(rectangle) + rectangle.Height / 2);
                        double distance = FindDistanceToSegment(rectangleCenter, subdividedRoute[i], subdividedRoute[i + 2]);

                        if (distance < rectangle.Width / 2 || IsPointInsideTriangle(subdividedRoute[i], subdividedRoute[i + 1], subdividedRoute[i + 2], rectangleCenter))
                        {
                            isBlock = true;
                            break;
                        }
                    }
                    if (!isBlock)
                    {
                        subdividedRoute.RemoveAt(i + 1);
                    }
                    else i += count;
                }
            }
            while (subdividedRoute.Count != start.Count);
            return subdividedRoute;
        }

        static bool IsPointInsideTriangle(Point A, Point B, Point C, Point P)
        {
            // Вычисляем площади треугольника ABC и трех подтреугольников с использованием формулы Герона
            double ABC = Math.Abs((A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y)) / 2.0);
            double ABP = Math.Abs((A.X * (B.Y - P.Y) + B.X * (P.Y - A.Y) + P.X * (A.Y - B.Y)) / 2.0);
            double APC = Math.Abs((A.X * (P.Y - C.Y) + P.X * (C.Y - A.Y) + C.X * (A.Y - P.Y)) / 2.0);
            double PBC = Math.Abs((P.X * (B.Y - C.Y) + B.X * (C.Y - P.Y) + C.X * (P.Y - B.Y)) / 2.0);

            // Если сумма площадей подтреугольников равна площади треугольника ABC, то точка P внутри треугольника
            return Math.Abs(ABC - (ABP + APC + PBC)) < 5;
        }

        private static double GetAvgDistance(Point point, Point[] route)
        {
            double avgDistance = 0;
            double distance = 0;
            int count = 0;
            foreach (Point p in route)
            {
                distance = GetDistance(p, point);
                if (distance < 100)
                {
                    avgDistance += distance;
                    count++;
                }
            }
            return avgDistance / count;
        }

        private static double GetDistance(Point p1, Point p2)
        {
            double deltaX = p2.X - p1.X;
            double deltaY = p2.Y - p1.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        private Line drawLine(double previousDrawingX, double previousDrawingY, double currentDrawingX, double currentDrawingY)
        {
            Line drawingLine = setupLine();
            drawingLine.X1 = previousDrawingX;
            drawingLine.Y1 = previousDrawingY;
            drawingLine.X2 = currentDrawingX;
            drawingLine.Y2 = currentDrawingY;
            workspaceCanvas.Children.Add(drawingLine);
            return drawingLine;
        }

        private Ellipse drawPoint(double currentDrawingX, double currentDrawingY, bool isSelected)
        {
            Ellipse drawingPoint = setupEllipse(isSelected);
            ellipses.Add(drawingPoint);
            //ellipse.MouseRightButtonDown += Ellipse_MouseRightDown;
            Canvas.SetLeft(drawingPoint, currentDrawingX - ELLIPSE_SIZE / 2);
            Canvas.SetTop(drawingPoint, currentDrawingY - ELLIPSE_SIZE / 2);
            //layerEllipses.Add(ellipse);
            drawingPoint.MouseLeftButtonDown += element_MouseLeftButtonDown;
            drawingPoint.MouseRightButtonDown += element_MouseRightButtonDown;
            drawingPoint.MouseMove += element_MouseMove;
            workspaceCanvas.Children.Add(drawingPoint);
            return drawingPoint;
        }

        private Line setupLine()
        {
            Line line = new Line();
            line.Tag = getCustomElementTag();
            line.Fill = new SolidColorBrush(LINE_COLOR);
            line.Visibility = Visibility.Visible;
            line.StrokeThickness = LINE_SIZE;
            line.Stroke = new SolidColorBrush(LINE_COLOR);
            line.MouseRightButtonDown += element_MouseRightButtonDown;

            return line;
        }

        private Ellipse setupEllipse(bool isSelected)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Tag = getCustomElementTag();
            ellipse.Height = ELLIPSE_SIZE;
            ellipse.Width = ELLIPSE_SIZE;
            ellipse.Fill = new SolidColorBrush(POINT_COLOR);
            if(isSelected)
            {
                ellipse.Fill = new SolidColorBrush(SELECTED_POINT_COLOR);

            }
            return ellipse;
        }

        private bool isLineCrossTheNeedles(Point previousPoint, Point currentPoint)
        {
            LineGeometry lineGeometry = new LineGeometry(previousPoint, currentPoint);
            foreach (Rectangle needle in needles)
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

        private double getDrawingValueByThreadValue(double threadValue)
        {
            return threadValue / ProjectSettings.preset.topology.accuracy * cellSize;
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            initWorkspace();
        }

        // Отлавливание Events и перенаправление на вызов выбранного инструмента 

        // При наличии нескольких ссылок 2+, то рекомендуется выставлять e.Hadlded = false, 
        // т.к. вышестоящий элемент в иерархии

        public void element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            executeInstrument(EventType.LeftMouseButtonUp, sender, e);
        }
        public void element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            executeInstrument(EventType.LeftMouseButtonDown, sender, e);
            e.Handled = false;
        }
        public void element_MouseWheel(object sender, MouseEventArgs e)
        {
            executeInstrument(EventType.MouseWheel, sender, e);
        }
        public void element_MouseMove(object sender, MouseEventArgs e)
        {
            executeInstrument(EventType.MouseMove, sender, e);
        }
        public void element_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            executeInstrument(EventType.RightMouseButtonDown, sender, e);
        }
        public void element_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            executeInstrument(EventType.RightMouseButtonUp, sender, e);
        }
        public void element_KeyDown(object sender, KeyEventArgs e)
        {
            executeInstrument(EventType.KeyDown, sender, e);
        }

    }
}
