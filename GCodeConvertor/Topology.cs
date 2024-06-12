using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace GCodeConvertor
{
    [Serializable]
    public class Topology
    {
        public string name { get; set; }
        public string path { get; set; }
        public float accuracy { get; set; }
        public int[,] map { get; set; }

        public Topology(TopologyModel model)
        {
            this.name = model.NameProject;
            this.path = model.PathProject;
            this.accuracy = model.Accuracy;
            this.map = generate(model);
            setupNeedles(model);
        }

        public Topology() { }

        public string getFullPath()
        {
            return System.IO.Path.Combine(path, name) + ".gcd";
        }

        private int[,] generate(TopologyModel model)
        {
            int mapWidth = (int)((model.PlatformW + model.HeadIdentationX * 2) / accuracy);
            int mapHeight = (int)((model.PlatformH + model.HeadIdentationY * 2) / accuracy);

            int[,] map = new int[mapWidth, mapHeight];

            map[0, 0] = 2;
            map[map.GetUpperBound(0), 0] = 2;
            map[0, map.GetUpperBound(1)] = 2;
            map[map.GetUpperBound(0), map.GetUpperBound(1)] = 2;

            for (int workspaceX = (int)(model.HeadIdentationX / accuracy);
                workspaceX < (mapWidth - (int)(model.HeadIdentationX / accuracy));
                workspaceX++)
            {
                for (int workspaceY = (int)(model.HeadIdentationY / accuracy);
                workspaceY < (mapHeight - (int)(model.HeadIdentationY / accuracy));
                workspaceY++)
                {
                    map[workspaceX, workspaceY] = 1; // 1 - идентиф. платформы
                }
            }
            return map;
        }

        private void setupNeedles(TopologyModel model)
        {
            for (int expectedNeedlePositionX = 0;
                expectedNeedlePositionX < (int)((model.PlatformW - model.StartNeedleOffsetX) / accuracy);
                expectedNeedlePositionX++)
            {
                for (int expectedNeedlePositionY = 0;
                    expectedNeedlePositionY < (int)((model.PlatformH - model.StartNeedleOffsetY) / accuracy);
                    expectedNeedlePositionY++)
                {

                    if (expectedNeedlePositionX % ((model.NeedleDiameter + model.StepNeedlesX) / accuracy) < model.NeedleDiameter / accuracy &&
                       expectedNeedlePositionY % ((model.NeedleDiameter + model.StepNeedlesY) / accuracy) < model.NeedleDiameter / accuracy &&
                       expectedNeedlePositionX + model.NeedleDiameter / accuracy - expectedNeedlePositionX % ((model.NeedleDiameter + model.StepNeedlesX) / accuracy) <= (int)((model.PlatformW - model.StartNeedleOffsetX) / accuracy) &&
                       expectedNeedlePositionY + model.NeedleDiameter / accuracy - expectedNeedlePositionY % ((model.NeedleDiameter + model.StepNeedlesY) / accuracy) <= (int)((model.PlatformH - model.StartNeedleOffsetY) / accuracy))
                    {
                        map[expectedNeedlePositionX + (int)(model.HeadIdentationX / accuracy + model.StartNeedleOffsetX / accuracy),
                            expectedNeedlePositionY + (int)(model.HeadIdentationY / accuracy + model.StartNeedleOffsetY / accuracy)] = 3;
                    }
                }
            }
        }

        public Topology(TopologyByLineModel model, List<Point> shape, List<List<Point>> tensionLines)
        {
            this.name = model.NameProject;
            this.path = model.PathProject;
            this.accuracy = model.Accuracy;

            var size = getSizeOfShape(shape);
            this.map = new int[(int)((size.height + model.HeadIdentationY * 2) / accuracy), (int)((size.width + model.HeadIdentationX * 2) / accuracy)];

            this.map[0, 0] = 2;
            this.map[this.map.GetUpperBound(0), 0] = 2;
            this.map[0, this.map.GetUpperBound(1)] = 2;
            this.map[this.map.GetUpperBound(0), this.map.GetUpperBound(1)] = 2;

            for (int i = 0; i < (int)(size.height / accuracy); i++)
            {
                for (int j = 0; j < (int)(size.width / accuracy); j++)
                {
                    this.map[i + (int)(model.HeadIdentationY / accuracy), j + (int)(model.HeadIdentationX / accuracy)] = 1;
                }
            }
        }

        private (double width, double height) getSizeOfShape(List<Point> points)
        {
            double minX = int.MaxValue;
            double maxX = 0;
            double minY = int.MaxValue;
            double maxY = 0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                minX = Math.Min(minX, points[i].X);
                maxX = Math.Max(maxX, points[i].X);
                minY = Math.Min(minY, points[i].Y);
                maxY = Math.Max(maxY, points[i].Y);
            }

            return (maxX - minX + 3, maxY - minY + 3);
        }

        public static (Topology topology, Layer layer) fillNozzlesAndLayer(TopologyByLineModel model)
        {
            Shape shape = new Shape();
            bool isLoaded = shape.loadPreset(model.PathShape);

            TensionLines tensionLines = new TensionLines();
            tensionLines.loadPreset(model.PathTensionLines);

            //Point[] shape = new Point[1]; // из файла
            //Point[][] tensionLines = {new Point[1], new Point[1], new Point[2]}; // из файла

            Topology topology = new Topology(model, shape.points, tensionLines.points);
            Layer layer = new Layer();
            //линии
            foreach (List<Point> line in tensionLines.points)
            {
                makeFigure(topology, layer, model, line);
            }

            //форма
            makeFigure(topology, layer, model, shape.points);

            //штриховка
            List<Point> hatchingPoints = GetHatchingPoints(shape.points, model.Step);
            makeFigure(topology, layer, model, hatchingPoints);

            return (topology, layer);
        }

        private static void makeFigure(Topology topology, Layer layer, TopologyByLineModel model, List<Point> shape)
        {
            List<Point> route = new List<Point>();

            Point[] startPoints = new Point[4];
            startPoints[0] = new Point(0, 0);
            startPoints[1] = new Point(topology.map.GetUpperBound(0), 0);
            startPoints[2] = new Point(0, topology.map.GetUpperBound(1));
            startPoints[3] = new Point(topology.map.GetUpperBound(0), topology.map.GetUpperBound(1));

            Point start = findNearestToStart(startPoints, shape[0]);

            route.Add(start);
            for (int i = 0; i < shape.Count; i++)
            {
                route.Add(new Point(shape[i].X + model.HeadIdentationX + 2, shape[i].Y + model.HeadIdentationY + 2));
            }
            Point pointForNozzle;
            for (int i = 1; i < route.Count; i++)
            {
                if (i == route.Count - 1)
                {
                    pointForNozzle = getPointForNozzle(route[i], getAngleBisector(route[i - 1], route[i], start), model.NozzleDiameter);
                }
                else
                {
                    pointForNozzle = getPointForNozzle(route[i], getAngleBisector(route[i - 1], route[i], route[i + 1]), model.NozzleDiameter);
                }
                for (int x = (int)(pointForNozzle.X - model.NozzleDiameter / 2); x < (int)(pointForNozzle.X + model.NozzleDiameter / 2); x++)
                {
                    for (int y = (int)(pointForNozzle.Y - model.NozzleDiameter / 2); y < (int)(pointForNozzle.Y + model.NozzleDiameter / 2); y++)
                    {
                        topology.map[x, y] = 3;
                    }
                }
            }
            route.Add(start);
            layer.thread.AddRange(route);
        }


        private static double getAngleBisector(Point a, Point b, Point c)
        {
            // Векторы, образующие угол
            var vectorAB = new Point(a.X - b.X, a.Y - b.Y);
            var vectorCB = new Point(c.X - b.X, c.Y - b.Y);

            // Углы векторов относительно оси X
            double angleAB = Math.Atan2(vectorAB.Y, vectorAB.X);
            double angleCB = Math.Atan2(vectorCB.Y, vectorCB.X);

            // Средний угол
            double bisectorAngle = (angleAB + angleCB) / 2;

            return bisectorAngle;
        }

        private static Point getPointForNozzle(Point point, double angle, float nozzleDiameter)
        {
            //вообще не уверен
            int nozzleHalf = (int)nozzleDiameter / 2;
            if (-Math.PI / 8 <= angle && angle < Math.PI / 8)
            {
                return new Point(point.X + nozzleHalf, point.Y);
            }
            if (Math.PI / 8 <= angle && angle < 3 * Math.PI / 8)
            {
                return new Point(point.X + nozzleHalf, point.Y + nozzleHalf);
            }
            if (3 * Math.PI / 8 <= angle && angle < 5 * Math.PI / 8)
            {
                return new Point(point.X, point.Y + nozzleHalf);
            }
            if (5 * Math.PI / 8 <= angle && angle < 7 * Math.PI / 8)
            {
                return new Point(point.X - nozzleHalf, point.Y + nozzleHalf);
            }
            if (-Math.PI / 8 <= angle && angle < -3 * Math.PI / 8)
            {
                return new Point(point.X + nozzleHalf, point.Y - nozzleHalf);
            }
            if (-3 * Math.PI / 8 <= angle && angle < -5 * Math.PI / 8)
            {
                return new Point(point.X, point.Y - nozzleHalf);
            }
            if (-5 * Math.PI / 8 <= angle && angle < -7 * Math.PI / 8)
            {
                return new Point(point.X - nozzleHalf, point.Y - nozzleHalf);
            }

            return new Point(point.X - nozzleHalf, point.Y);
        }

        private static Point findNearestToStart(Point[] points, Point target)
        {
            Point nearestPoint = points[0];
            double shortestDistance = GetDistance(points[0], target);

            foreach (var point in points)
            {
                double currentDistance = GetDistance(point, target);
                if (currentDistance < shortestDistance)
                {
                    shortestDistance = currentDistance;
                    nearestPoint = point;
                }
            }

            return nearestPoint;
        }

        private static double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        public static List<Point> GetHatchingPoints(List<Point> polygon, float step)
        {
            List<Point> hatchingPoints = new List<Point>();

            // Найти минимальное и максимальное значение X
            float minX = float.MaxValue;
            float maxX = float.MinValue;

            foreach (var point in polygon)
            {
                if (point.X < minX) minX = (float)point.X;
                if (point.X > maxX) maxX = (float)point.X;
            }

            // Пройтись по вертикальным линиям с заданным шагом
            for (float x = minX; x <= maxX; x += step)
            {
                List<Point> intersections = new List<Point>();

                // Найти пересечения текущей вертикальной линии с границами полигона
                for (int i = 0; i < polygon.Count; i++)
                {
                    Point p1 = polygon[i];
                    Point p2 = polygon[(i + 1) % polygon.Count];

                    if (IsIntersecting(p1, p2, x, out Point intersection))
                    {
                        intersections.Add(intersection);
                    }
                }

                // Сортировать точки пересечения по Y
                intersections.Sort((a, b) => a.Y.CompareTo(b.Y));

                // Добавить точки пересечения в результирующий список
                hatchingPoints.AddRange(intersections);
            }

            return hatchingPoints;
        }

        private static bool IsIntersecting(Point p1, Point p2, float x, out Point intersection)
        {
            intersection = new Point();

            if ((p1.X < x && p2.X >= x) || (p2.X < x && p1.X >= x))
            {
                float t = (float)(x - p1.X) / (float)(p2.X - p1.X);
                intersection.X = (int)x;
                intersection.Y = p1.Y + (int)(t * (p2.Y - p1.Y));
                return true;
            }

            return false;
        }
    }
}
