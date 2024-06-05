﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Drawing;
using Polenter.Serialization;
using System.Security.Policy;
using static System.Windows.Forms.LinkLabel;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using static System.Windows.Forms.AxHost;
using Point = System.Windows.Point;

namespace GCodeConvertor
{
    /// <summary>
    /// Логика взаимодействия для ProjectWindow.xaml
    /// </summary>

    public partial class ProjectWindow : Window
    {
        private enum DrawingStates{
            SET_START_POINT,
            DRAWING,
            SET_END_POINT
        }
        private double ELLIPSE_SIZE = 5;


        private HashSet<System.Windows.Shapes.Rectangle> rectangles = new HashSet<System.Windows.Shapes.Rectangle>();

        private DrawingStates drawingState;

        private int startPointX;
        private int startPointY;

        private double currentDotX;
        private double currentDotY;
        double size;
        private bool drawArrow = false;
        Line line;

        List<System.Windows.Point> layerPoints;
        List<Line> layerPredictedLines;
        Layer activeLayer;

        LayerStorage storage;

        ObservableCollection<CustomItem> ItemsList { get; set; }
        public ProjectWindow()
        {
            InitializeComponent();
            DataContext = ProjectSettings.preset;
            line = new Line();
            drawingState = DrawingStates.SET_START_POINT;
            layerPoints = new List<System.Windows.Point>();
            layerPredictedLines = new List<Line>();
            storage = new LayerStorage();

            ItemsList = new ObservableCollection<CustomItem>();
            layerListBox.ItemsSource = ItemsList;
        }


        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int docY = (int)Math.Floor(e.GetPosition(CanvasMain).Y / size);
            int docX = (int)Math.Floor(e.GetPosition(CanvasMain).X / size);

            int dotType = ProjectSettings.preset.topology.map[docY, docX];

            switch (drawingState)
            {
                case DrawingStates.SET_START_POINT:
                    if(dotType == 2)
                    {
                        startPointX = docX;
                        startPointY = docY;
                        draw(sender, e, dotType);
                        drawingState = DrawingStates.DRAWING;
                    }
                    break;
                case DrawingStates.DRAWING:
                    if (dotType != 3)
                    {
                        draw(sender, e, dotType);

                        if (docY == startPointY && docX == startPointX)
                        {
                            drawingState = DrawingStates.SET_END_POINT;
                        }
                    }
                    break;
                case DrawingStates.SET_END_POINT:
                    MessageBox.Show("Слой является законченным");
                    break;

            }

            //startPointFlag
        }

        private void draw(object sender, MouseButtonEventArgs e, int dotType)
        {
            if (drawArrow)
            {
                double startX = currentDotX;
                double startY = currentDotY;

                double endX = Canvas.GetLeft(sender as System.Windows.Shapes.Rectangle) + size / 2;
                double endY = Canvas.GetTop(sender as System.Windows.Shapes.Rectangle) + size / 2;

                bool isInsert = false; 

                LineGeometry lineGeometry = new LineGeometry(new System.Windows.Point(startX, startY), new System.Windows.Point(endX , endY));

                foreach(System.Windows.Shapes.Rectangle rectangle in rectangles)
                {
                    RectangleGeometry rectangleGeometry = new RectangleGeometry(new Rect(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle), rectangle.Width, rectangle.Height));
                    isInsert = lineGeometry.FillContainsWithDetail(rectangleGeometry) != IntersectionDetail.Empty;
                    if (isInsert)
                    {
                        break;
                    }
                }

                if (!isInsert)
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Height = ELLIPSE_SIZE;
                    ellipse.Width = ELLIPSE_SIZE;
                    ellipse.Fill = new SolidColorBrush(Colors.Red);
                    Canvas.SetLeft(ellipse, endX - ELLIPSE_SIZE / 2);
                    Canvas.SetTop(ellipse, endY - ELLIPSE_SIZE / 2);
                    ((sender as System.Windows.Shapes.Rectangle).Parent as Canvas).Children.Add(ellipse);

                    line.Fill = new SolidColorBrush(Colors.Red);
                    line.Visibility = System.Windows.Visibility.Visible;
                    line.StrokeThickness = 2;
                    line.Stroke = System.Windows.Media.Brushes.Red;
                    line.X1 = currentDotX;
                    line.Y1 = currentDotY;
                    line.X2 = endX;
                    line.Y2 = endY;
                    CanvasMain.Children.Add(line);
                    line = new Line();

                    currentDotX = endX;
                    currentDotY = endY;

                    layerPoints.Add(new System.Windows.Point((double)((int)Math.Floor((e.GetPosition(CanvasMain).X / size)) + 0.5),
                                                                    (double)((int)Math.Floor(e.GetPosition(CanvasMain).Y / size) + 0.5)));

                    activeLayer.layerThread.Add(new System.Windows.Point(currentDotX - ELLIPSE_SIZE / 2, currentDotY - ELLIPSE_SIZE / 2));

                }
            }
            else
            {
                drawArrow = true;

                currentDotX = Canvas.GetLeft(sender as System.Windows.Shapes.Rectangle) + size / 2 ;
                currentDotY = Canvas.GetTop(sender as System.Windows.Shapes.Rectangle) + size / 2;

                Ellipse ellipse = new Ellipse();
                ellipse.Height = 5;
                ellipse.Width = 5;
                ellipse.Fill = new SolidColorBrush(Colors.Red);
                Canvas.SetLeft(ellipse, currentDotX - ELLIPSE_SIZE / 2);
                Canvas.SetTop(ellipse, currentDotY - ELLIPSE_SIZE / 2);
                ((sender as System.Windows.Shapes.Rectangle).Parent as Canvas).Children.Add(ellipse);

                layerPoints.Add(new System.Windows.Point((double)((int)Math.Floor((e.GetPosition(CanvasMain).X / size)) + 0.5),
                                                                (double)((int)Math.Floor(e.GetPosition(CanvasMain).Y / size) + 0.5)));

                activeLayer.layerThread.Add(new System.Windows.Point(currentDotX - ELLIPSE_SIZE / 2, currentDotY - ELLIPSE_SIZE / 2));
            }
            foreach (var line in layerPredictedLines)
            {
                CanvasMain.Children.Remove(line);
            }
           
            activeLayer.layerPredictThread = getRubberBandPath(activeLayer.layerThread.ToArray());
            //activeLayer.layerPredictThread = getRubberBandPath(getRubberBandPath(activeLayer.layerThread.ToArray()).ToArray());
            System.Windows.Point startPoint;
            System.Windows.Point endPoint;
            for (int i = 0; i < activeLayer.layerPredictThread.Count; i++)
            {
                System.Windows.Point point = activeLayer.layerPredictThread[i];
                Ellipse ellipse = new Ellipse();
                ellipse.Height = ELLIPSE_SIZE;
                ellipse.Width = ELLIPSE_SIZE;
                ellipse.Fill = new SolidColorBrush(Colors.Blue);
                Canvas.SetLeft(ellipse, point.X);
                Canvas.SetTop(ellipse, point.Y);
                CanvasMain.Children.Add(ellipse);

                if (i == 0)
                {
                    startPoint = point;
                }
                else
                {
                    endPoint = point;
                    Line lineToAdd = new Line();
                    lineToAdd.Fill = new SolidColorBrush(Colors.Red);
                    lineToAdd.Visibility = System.Windows.Visibility.Visible;
                    lineToAdd.StrokeThickness = 2;
                    lineToAdd.Stroke = System.Windows.Media.Brushes.Blue;
                    lineToAdd.X1 = startPoint.X + ELLIPSE_SIZE / 2;
                    lineToAdd.Y1 = startPoint.Y + ELLIPSE_SIZE / 2;
                    lineToAdd.X2 = endPoint.X + ELLIPSE_SIZE / 2;
                    lineToAdd.Y2 = endPoint.Y + ELLIPSE_SIZE / 2;
                    CanvasMain.Children.Add(lineToAdd);
                    layerPredictedLines.Add(lineToAdd);
                    startPoint = endPoint;
                }
            }
        }

        private List<Point> getRubberBandPath(Point[] route)
        {
            route = RemoveExtraPoints(route.ToList(),1).ToArray();
            route = RoutePreprocessing(route, size);
            var result = new List<Point>();
            var sigmentStart = 0;
            result.Add(route[0]);
            Point point;
            System.Windows.Shapes.Rectangle block = null;
            bool isInsert;
            for (int i = 1; i < route.GetLength(0); i++)
            {
                point = route[i];
                isInsert = false;

                for( int j = sigmentStart; j<i; j++)
                {
                    foreach (System.Windows.Shapes.Rectangle rectangle in rectangles)
                    {
                        Point rectangleCenter = new Point(Canvas.GetLeft(rectangle) + rectangle.Width/2, Canvas.GetTop(rectangle) + rectangle.Height/2);
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
                            else {
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
                    Point leftTop = new Point(Canvas.GetLeft(block) - size / 2, Canvas.GetTop(block) - size / 2);
                    Point rightTop = new Point(Canvas.GetLeft(block) + block.Width, Canvas.GetTop(block) - size / 2);
                    Point rightDown = new Point(Canvas.GetLeft(block) + block.Width, Canvas.GetTop(block) + block.Height);
                    Point leftDown = new Point(Canvas.GetLeft(block) - size / 2, Canvas.GetTop(block) + block.Height);

                    Point[] segment = new Point[i-sigmentStart];
                    Array.Copy(route, sigmentStart, segment, 0, i - sigmentStart);

                    double leftTopMinDistance = GetAvgDistance(leftTop, segment);
                    double rightTopMinDistance = GetAvgDistance(rightTop, segment);
                    double rightDownMinDistance = GetAvgDistance(rightDown, segment);
                    double leftDownMinDistance = GetAvgDistance(leftDown, segment);


                    double minDistance = double.MaxValue;
                    double angle =- Math.Atan2(route[i].Y - route[sigmentStart].Y, route[i].X - route[sigmentStart].X);
                    Point nearestPoint;

                    // левый верх
                    if ((-Math.PI < angle && angle <= -Math.PI / 2 || 0 <= angle && angle < Math.PI / 2) && leftTopMinDistance < minDistance)
                    {
                        minDistance = leftTopMinDistance;
                        nearestPoint = leftTop;
                    }
                    // правый верх
                    if ((-Math.PI/2 <= angle && angle < 0 || Math.PI/2 < angle && angle <= Math.PI) && rightTopMinDistance < minDistance)
                    {
                        minDistance = rightTopMinDistance;
                        nearestPoint = rightTop;
                    }
                    // правый низ
                    if ((0 < angle && angle <= Math.PI/2 || -Math.PI <= angle && angle < - Math.PI/2) && rightDownMinDistance < minDistance)
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
            if(result.Count>1) result.Add(route.Last());
            return RemoveExtraPoints(result,1);
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
                    foreach (System.Windows.Shapes.Rectangle rectangle in rectangles)
                    {
                        Point rectangleCenter = new Point(Canvas.GetLeft(rectangle) + rectangle.Width / 2, Canvas.GetTop(rectangle) + rectangle.Height / 2);
                        double distance = FindDistanceToSegment(rectangleCenter, subdividedRoute[i], subdividedRoute[i + 2]);

                        //double angleToPoint = Math.Atan2(subdividedRoute[i + 1].Y - rectangleCenter.Y, subdividedRoute[i + 1].X - rectangleCenter.X);
                        //double angleToPoint = CalculateAngleBetweenPoints(subdividedRoute[i + 1], rectangleCenter);
                        //double angleToLine = Math.PI / 2 - Math.Atan2(subdividedRoute[i + 2].Y - subdividedRoute[i].Y, subdividedRoute[i + 2].X - subdividedRoute[i].X);
                        //double angleToLine = -Math.Atan2((subdividedRoute[i].Y+ subdividedRoute[i + 2].Y)/2 - rectangleCenter.Y, (subdividedRoute[i].X+ subdividedRoute[i + 2].X) /2 - rectangleCenter.X);
                        //double angleToLine = CalculatePerpendicularAngle(rectangleCenter, subdividedRoute[i + 2], subdividedRoute[i]);
                        //if (distance < rectangle.Width / 2 || CalculateAngleBetween( angleToLine, angleToPoint) > Math.PI / 2)
                        if (distance < rectangle.Width / 2 || IsPointInsideTriangle(subdividedRoute[i], subdividedRoute[i+1], subdividedRoute[i+2], rectangleCenter))
                        {
                            isBlock = true;
                            break;
                        }
                    }
                    if (!isBlock)
                    {
                        subdividedRoute.RemoveAt(i + 1);
                    }
                    else i+=count;
                }
            }
            while (subdividedRoute.Count!=start.Count);
            return subdividedRoute;
        }

        static double CalculatePerpendicularAngle(Point point, Point linePoint1, Point linePoint2)
        {
            // Вычисляем векторы для точек на линии
            double lineVectorX = linePoint2.X - linePoint1.X;
            double lineVectorY = linePoint2.Y - linePoint1.Y;
            double pointVectorX = point.X - linePoint1.X;
            double pointVectorY = point.Y - linePoint1.Y;

            // Вычисляем скалярное произведение векторов
            double dotProduct = lineVectorX * pointVectorX + lineVectorY * pointVectorY;

            // Вычисляем длину вектора линии и длину вектора, проведенного до точки
            double lineLength = Math.Sqrt(lineVectorX * lineVectorX + lineVectorY * lineVectorY);
            double pointLength = Math.Sqrt(pointVectorX * pointVectorX + pointVectorY * pointVectorY);

            // Вычисляем угол через скалярное произведение и длины векторов
            double angle = Math.Acos(dotProduct / (lineLength * pointLength));

            return angle;
        }

        static bool IsPointInsideTriangle(Point A, Point B, Point C, Point P)
        {
            // Вычисляем площади треугольника ABC и трех подтреугольников с использованием формулы Герона
            double ABC = Math.Abs((A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y)) / 2.0);
            double ABP = Math.Abs((A.X * (B.Y - P.Y) + B.X * (P.Y - A.Y) + P.X * (A.Y - B.Y)) / 2.0);
            double APC = Math.Abs((A.X * (P.Y - C.Y) + P.X * (C.Y - A.Y) + C.X * (A.Y - P.Y)) / 2.0);
            double PBC = Math.Abs((P.X * (B.Y - C.Y) + B.X * (C.Y - P.Y) + C.X * (P.Y - B.Y)) / 2.0);

            // Если сумма площадей подтреугольников равна площади треугольника ABC, то точка P внутри треугольника
            return Math.Abs(ABC - (ABP + APC + PBC))<5;
        }

        static double CalculateAngleBetweenPoints(Point point1, Point point2)
        {
            // Вычисляем векторы для точек
            double vectorX1 = point1.X;
            double vectorY1 = point1.Y;
            double vectorX2 = point2.X;
            double vectorY2 = point2.Y;

            // Вычисляем скалярное произведение векторов
            double dotProduct = vectorX1 * vectorX2 + vectorY1 * vectorY2;

            // Вычисляем длины векторов
            double length1 = Math.Sqrt(vectorX1 * vectorX1 + vectorY1 * vectorY1);
            double length2 = Math.Sqrt(vectorX2 * vectorX2 + vectorY2 * vectorY2);

            // Вычисляем угол через скалярное произведение и длины векторов
            double angle = Math.Acos(dotProduct / (length1 * length2));

            return angle;
        }

        static double CalculateAngleBetween(double angle1, double angle2)
        {
            // Вычисляем разницу между углами
            double angleDifference = Math.Abs(angle1 - angle2);

            // Если разница больше половины оборота (2*pi), то берем обратную разницу
            if (angleDifference > Math.PI)
            {
                angleDifference = 2 * Math.PI - angleDifference;
            }

            return angleDifference;
        }

        private static double GetAvgDistance(Point point, Point[] route) 
        {
            double avgDistance = 0;
            double distance = 0;
            int count = 0;
            foreach(Point p in route)
            {
                distance = GetDistance(p, point);
                if(distance < 100)
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

        public static double DistanceFromPointToLine(Point point, Point l1, Point l2)
        {
            return Math.Abs((l2.X - l1.X) * (l1.Y - point.Y) - (l1.X - point.X) * (l2.Y - l1.Y)) /
                    Math.Sqrt(Math.Pow(l2.X - l1.X, 2) + Math.Pow(l2.Y - l1.Y, 2));
        }
        private double FindDistanceToSegment(Point pt, Point p1, Point p2 )
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

        private void openMenu(object sender, RoutedEventArgs e)
        {

            DoubleAnimation anim = new DoubleAnimation();
            if (RightMenu.Width > 0)
            {
                anim.From = 250;
                anim.To = 0;
                anim.Duration = TimeSpan.FromSeconds(0.2);
                RightMenu.BeginAnimation(WidthProperty, anim);
                return;
            }

            anim.From = 0;
            anim.To = 250;
            anim.Duration = TimeSpan.FromSeconds(0.2);
            RightMenu.BeginAnimation(WidthProperty, anim);
        }

        private void Canvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (drawArrow)
            {
                line.Fill = new SolidColorBrush(Colors.Red);
                line.Visibility = System.Windows.Visibility.Visible;
                line.StrokeThickness = 4;
                line.Stroke = System.Windows.Media.Brushes.Red;
                line.X1 = currentDotX;
                line.Y1 = currentDotY;
                line.X2 = e.GetPosition(CanvasMain).X;
                line.Y2 = e.GetPosition(CanvasMain).Y;
            }
        }

        private void Canvas_(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            ProjectSettings.preset.savePreset();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(drawingState == DrawingStates.SET_END_POINT)
            {
                List<Layer> layersToGenerate = new List<Layer>();
                foreach (Layer layer in storage.layers)
                {
                    if (layer.enable)
                    {
                        layersToGenerate.Add(layer);
                    }
                }

                if ((bool)manyCheck.IsChecked)
                {
                    List<Layer> tempList = new List<Layer>();
                    for (int i = 0; i < int.Parse(layerZ_Count.Text); i++)
                    {
                        tempList.AddRange(layersToGenerate);
                    }
                    layersToGenerate = tempList;
                }
                GCodeGenerator.generate(layersToGenerate);
            }
            else
            {
                MessageBox.Show("Закончите слой для генерации g-code'а!", "Невозможно сгенерировать g-code!");
            }
        }

        private void manyCheck_Checked(object sender, RoutedEventArgs e)
        {
            layerZ_Count.IsEnabled = true;
        }
        private void manyCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            layerZ_Count.IsEnabled = false;
        }
        private void CanvasMain_Loaded(object sender, RoutedEventArgs e)
        {
            size = (double)(CanvasMain.ActualWidth / ProjectSettings.preset.topology.map.GetLength(0));
            MessageBox.Show(ProjectSettings.preset.topology.map.Length.ToString());

            for (int i = 0; i < ProjectSettings.preset.topology.map.GetUpperBound(1) + 1; i++)
            {
                for (int j = 0; j < ProjectSettings.preset.topology.map.GetUpperBound(0) + 1; j++)
                {
                    System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                    if (ProjectSettings.preset.topology.map[j, i] == 1)
                    {
                        rectangle.Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFBE98"));
                    }
                    else if (ProjectSettings.preset.topology.map[j, i] == 2)
                    {
                        rectangle.Fill = new SolidColorBrush(Colors.Green);
                    }
                    else if (ProjectSettings.preset.topology.map[j, i] == 3 || ProjectSettings.preset.topology.map[j, i] == 4)
                    {
                        rectangle.Fill = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        rectangle.Fill = new SolidColorBrush(Colors.White);
                    }
                    rectangle.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
                    rectangle.Height = size;
                    rectangle.Width = size;
                    rectangle.StrokeThickness = 1;

                    if(ProjectSettings.preset.topology.map[j, i] == 3)
                    {
                        //int upHeight = 0;
                        int downHeight = 0;
                        //int leftWidth = 0;
                        int rightWidth = 0;
                        while (ProjectSettings.preset.topology.map[j + downHeight, i] == 3)
                        {
                            ProjectSettings.preset.topology.map[j + downHeight, i] = 4;
                            rightWidth = 0;
                            while (ProjectSettings.preset.topology.map[j + downHeight, i + rightWidth+1] == 3)
                            {
                                ProjectSettings.preset.topology.map[j + downHeight, i + rightWidth+1] = 4;
                                rightWidth++;
                            }
                            downHeight++;
                        }
                        System.Windows.Shapes.Rectangle rectangleToAdd = new System.Windows.Shapes.Rectangle();
                        rectangleToAdd.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
                        rectangleToAdd.Height = (downHeight) * size;
                        rectangleToAdd.Width = (rightWidth +1) * size;
                        rectangleToAdd.StrokeThickness = 1;
                        Canvas.SetTop(rectangleToAdd, size * j);
                        Canvas.SetLeft(rectangleToAdd, size * i);
                        rectangles.Add(rectangleToAdd);
                    }

                    Canvas.SetTop(rectangle, size * j);
                    Canvas.SetLeft(rectangle, size * i);
                    CanvasMain.Children.Add(rectangle);
                }
            }
            for (int i = 0; i < ProjectSettings.preset.topology.map.GetUpperBound(1) + 1; i++)
            {
                for (int j = 0; j < ProjectSettings.preset.topology.map.GetUpperBound(0) + 1; j++)
                {
                    if (ProjectSettings.preset.topology.map[j, i] == 4) ProjectSettings.preset.topology.map[j, i] = 3;
                }
            }
                    activeLayer = new Layer();
            storage.addLayer(activeLayer);
            ItemsList.Add(new CustomItem(activeLayer.name, "12"));
            layerListBox.SelectedIndex = 0;
        }

        private void createLayer(object sender, RoutedEventArgs e)
        {
            if (drawingState != DrawingStates.SET_END_POINT)
            {
                MessageBox.Show("Текущий слой не закончен");
                return;
            }

            Layer layer = new Layer();
            storage.addLayer(layer);
            //layerListBox.Items.Add(layer.name);
            CustomItem currentItem = new CustomItem(layer.name, "12");
            ItemsList.Add(currentItem);
            layerListBox.SelectedItem = currentItem;
            activeLayer = layer;
            drawingState = DrawingStates.SET_START_POINT;
        }

        private void layerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (drawingState != DrawingStates.SET_END_POINT && layerListBox.Items.Count != 1 && !((CustomItem)layerListBox.SelectedItem).Equals(getCustomItemEqualsLayer(activeLayer.name)))
            {
                layerListBox.SelectedItem = getCustomItemEqualsLayer(activeLayer.name);
                MessageBox.Show("Текущий слой не закончен");
                return;
            }

            List<object> toRemove = new List<object>();
            foreach (object o in CanvasMain.Children)
            {
                if (o is Ellipse || o is Line || o is LineGeometry)
                {
                    toRemove.Add(o);
                }
            }

            foreach (object o in toRemove)
            {
                CanvasMain.Children.Remove(o as UIElement);
            }

            CustomItem selectedItemListBox = (CustomItem)layerListBox.SelectedItem;
            activeLayer = storage.getLayerByName(selectedItemListBox.LabelContent);
            loadActiveLayer(sender);
        }

        private CustomItem getCustomItemEqualsLayer(string name)
        {
            foreach (CustomItem item in ItemsList)
            {
                if (item.LabelContent.Equals(name))
                {
                    return item;
                }
            }
            return null;
        }

        private void loadActiveLayer(object sender)
        {
            if (activeLayer.layerThread != null)
            {
                System.Windows.Point startPoint;
                System.Windows.Point endPoint;
                for (int i = 0; i < activeLayer.layerThread.Count; i++)
                {
                    System.Windows.Point point = activeLayer.layerThread[i];
                    Ellipse ellipse = new Ellipse();
                    ellipse.Height = 5;
                    ellipse.Width = 5;
                    ellipse.Fill = new SolidColorBrush(Colors.Red);
                    Canvas.SetLeft(ellipse, point.X);
                    Canvas.SetTop(ellipse, point.Y);
                    CanvasMain.Children.Add(ellipse);

                    if (i == 0)
                    {
                        startPoint = point;
                    }
                    else
                    {
                        endPoint = point;
                        Line lineToAdd = buildLine(startPoint, endPoint);
                        CanvasMain.Children.Add(lineToAdd);
                        startPoint = endPoint;
                    }
                }
            }
        }

        private Line buildLine(System.Windows.Point startPoint, System.Windows.Point endPoint)
        {
            Line lineAdd = new Line();
            lineAdd.Fill = new SolidColorBrush(Colors.Red);
            lineAdd.Visibility = System.Windows.Visibility.Visible;
            lineAdd.StrokeThickness = 2;
            lineAdd.Stroke = System.Windows.Media.Brushes.Red;
            lineAdd.X1 = startPoint.X + ELLIPSE_SIZE / 2;
            lineAdd.Y1 = startPoint.Y + ELLIPSE_SIZE / 2;
            lineAdd.X2 = endPoint.X + ELLIPSE_SIZE / 2;
            lineAdd.Y2 = endPoint.Y + ELLIPSE_SIZE / 2;
            return lineAdd;
        }

        private void editLayerHeight(object sender, RoutedEventArgs e)
        {
            Button but = (Button)sender;
            string layerName = but.Tag?.ToString();

            string textBoxValue = ((TextBox)((StackPanel)((Button)sender).Parent).Children[0]).Text;
            if (textBoxValue == "")
            {
                ((TextBox)((StackPanel)((Button)sender).Parent).Children[0]).Text = storage.getLayerByName(layerName).heightLayer.ToString();
                return;
            }
            if (textBoxValue[0] == ',')
            {
                ((TextBox)((StackPanel)((Button)sender).Parent).Children[0]).Text = storage.getLayerByName(layerName).heightLayer.ToString();
                return;
            }
            storage.getLayerByName(layerName).heightLayer = float.Parse(textBoxValue);
        }

        private void deleteLayer(object sender, RoutedEventArgs e)
        {
            if (ItemsList.Count == 1)
            {
                MessageBox.Show("Это единственный слой. Создайте еще один, чтобы удалить текущий.");
                return;
            }

            Button but = (Button)sender;
            string layerName = but.Tag?.ToString();

            CustomItem selectedItem = (CustomItem)layerListBox.SelectedItem;
            string currentLabelValue = selectedItem.LabelContent;

            if (layerName.Equals(currentLabelValue))
            {
                int index = layerListBox.SelectedIndex;
                if (index == 0)
                {
                    activeLayer = storage.layers[layerListBox.SelectedIndex + 1];
                    layerListBox.SelectedIndex = index + 1;
                }
                else
                {
                    activeLayer = storage.layers[layerListBox.SelectedIndex - 1];
                    layerListBox.SelectedIndex = index - 1;
                }
                drawingState = DrawingStates.SET_END_POINT;
            }

            storage.removeLayerByName(layerName);
            ItemsList.Remove(getCustomItemEqualsLayer(layerName));

        }

        private void enableLayer_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            string layerName = box.Tag?.ToString();
            storage.getLayerByName(layerName).enable = true;
        }

        private void enableLayer_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            string layerName = box.Tag?.ToString();
            storage.getLayerByName(layerName).enable = false;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (var ch in e.Text)
            {
                if (!char.IsDigit(ch) && ch != ',')
                {
                    e.Handled = true;
                }
            }
        }

        private void moveLayerUp(object sender, RoutedEventArgs e)
        {
            Button but = (Button)sender;
            string layerName = but.Tag?.ToString();
            CustomItem item = getCustomItemEqualsLayer(layerName);
            int currentIndex = ItemsList.IndexOf(item);

            if (currentIndex != 0)
            {
                ItemsList.Move(currentIndex, currentIndex - 1);

                Layer currentLayer = storage.layers[currentIndex];
                Layer movedLayer = storage.layers[currentIndex - 1];
                storage.layers[currentIndex] = movedLayer;
                storage.layers[currentIndex - 1] = currentLayer;
            }
        }

        private void moveLayerDown(object sender, RoutedEventArgs e)
        {
            Button but = (Button)sender;
            string layerName = but.Tag?.ToString();
            CustomItem item = getCustomItemEqualsLayer(layerName);
            int currentIndex = ItemsList.IndexOf(item);

            if (currentIndex != ItemsList.Count - 1)
            {
                ItemsList.Move(currentIndex, currentIndex + 1);

                Layer currentLayer = storage.layers[currentIndex];
                Layer movedLayer = storage.layers[currentIndex + 1];
                storage.layers[currentIndex] = movedLayer;
                storage.layers[currentIndex + 1] = currentLayer;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            mw.Show();
            this.Close();
        }

        private double minScale = 1;

        private System.Windows.Point startPoint;
        private bool isDragging = false;
        private double translationFactor = 1.0;

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                var canvas = sender as Canvas;

                // Сохраняем текущий масштаб канваса и прокрутку
                var previousScale = ((MatrixTransform)canvas.RenderTransform).Matrix.M11;

                // Получаем позицию курсора относительно Canvas
                var mousePosition = e.GetPosition(canvas);

                // Вычисляем коэффициент масштабирования
                double scale = e.Delta > 0 ? 1.1 : 0.9;
                scale = previousScale * scale;

                if (scale >= minScale)
                {
                    Matrix matrix = new Matrix();
                    matrix.ScaleAtPrepend(scale, scale, mousePosition.X, mousePosition.Y);
                    canvas.RenderTransform = new MatrixTransform(matrix);
                }

                // Применяем масштабирование к канвасу


                // Предотвращаем дальнейшее распространение события
                e.Handled = true;
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                var canvas = sender as Canvas;
                startPoint = e.GetPosition(canvas);
                isDragging = true;
                canvas.CaptureMouse();
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && startPoint != null)
            {
                var canvas = sender as Canvas;
                var position = e.GetPosition(canvas);

                // Вычисляем разницу между текущей и начальной позицией мыши, умножая на фактор скорости
                double dx = (position.X - startPoint.X) * translationFactor;
                double dy = (position.Y - startPoint.Y) * translationFactor;

                // Получаем текущую матрицу трансформации канваса
                Matrix matrix = ((MatrixTransform)canvas.RenderTransform).Matrix;

                // Проверяем, не выходит ли канвас за пределы контейнера
                matrix.Translate(dx, dy);

                // Устанавливаем новую матрицу трансформации
                canvas.RenderTransform = new MatrixTransform(matrix);

                // Обновляем начальную позицию мыши
                startPoint = position;
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && isDragging)
            {
                var canvas = sender as Canvas;
                isDragging = false;
                canvas.ReleaseMouseCapture();
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "GCD files(*.gcd)|*.gcd",

            };

            if (openFileDialog.ShowDialog() == true)
            {
                string pathToPreset = openFileDialog.FileName;
                ProjectSettings.preset.loadPreset(openFileDialog.FileName);
                ProjectWindow pw = new ProjectWindow();
                pw.Show();
                this.Close();
            }
        }

    }
}
