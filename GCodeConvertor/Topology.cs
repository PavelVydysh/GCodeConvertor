using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace GCodeConvertor
{
    [Serializable]
    public class Topology
    {
        public string name { get; set; }
        public string path { get; set; }
        public float accuracy { get; set; }
        public int[,] map { get; set; }

        public Topology() { }

        public Topology(TopologyModel model)
        {
            this.name = model.NameProject;
            this.path = model.PathProject;
            this.accuracy = model.Accuracy;
            int[,] map = new int[(int)((model.PlatformH + model.HeadIdentationY * 2) / accuracy), (int)((model.PlatformW + model.HeadIdentationX * 2) / accuracy)];


            map[0, 0] = 2;
            map[map.GetUpperBound(0), 0] = 2;
            map[0, map.GetUpperBound(1)] = 2;
            map[map.GetUpperBound(0), map.GetUpperBound(1)] = 2;

            for (int i = 0; i < (int)(model.PlatformH / accuracy); i++)
            {
                for (int j = 0; j < (int)(model.PlatformW / accuracy); j++)
                {
                    map[i + (int)(model.HeadIdentationY / accuracy), j + (int)(model.HeadIdentationX / accuracy)] = 1;
                }
            }

            int needlesDots = (int)(2 / accuracy);

            for (int j = (int)(model.StartNeedleOffsetY / accuracy) + (int)(model.HeadIdentationY / accuracy); j < map.GetUpperBound(0) + 1 - (int)(model.StartNeedleOffsetY / accuracy) - (int)(model.HeadIdentationY / accuracy); j += 1 + (int)(model.StepNeedlesY / accuracy))
            {
                for (int j1 = 0; j1 < needlesDots; j1++)
                {

                    for (int i = (int)(model.StartNeedleOffsetX / accuracy) + (int)(model.HeadIdentationX / accuracy); i < map.GetUpperBound(1) + 1 - (int)(model.StartNeedleOffsetX / accuracy) - (int)(model.HeadIdentationX / accuracy); i += 1 + (int)(model.StepNeedlesX / accuracy))
                    {
                        for (int i1 = 0; i1 < needlesDots; i1++)
                        {
                            map[i, j] = 3;
                            i++;
                        }
                        i--;
                    }
                    j++;
                }
                j--;
            }

            this.map = map;
        }

        public Topology(string name, string path, float accuracy,
            int PlatformH, int PlatformW,
            int HeadIdentationX, int HeadIdentationY,
            int StartNeedleOffsetX, int StartNeedleOffsetY, int StepNeedlesX, int StepNeedlesY)
        {
            this.name = name;
            this.path = path;
            this.accuracy = accuracy;
            int[,] map = new int[(int)((PlatformH + HeadIdentationY * 2) / accuracy), (int)((PlatformW + HeadIdentationX * 2) / accuracy)];


            map[0, 0] = 2;
            map[map.GetUpperBound(0), 0] = 2;
            map[0, map.GetUpperBound(1)] = 2;
            map[map.GetUpperBound(0), map.GetUpperBound(1)] = 2;

            for (int i = 0; i < (int)(PlatformH / accuracy); i++)
            {
                for (int j = 0; j < (int)(PlatformW / accuracy); j++)
                {
                    map[i + (int)(HeadIdentationY / accuracy), j + (int)(HeadIdentationX / accuracy)] = 1;
                }
            }

            int needlesDots = (int)(2 / accuracy);

            for (int j = (int)(StartNeedleOffsetY / accuracy) + (int)(HeadIdentationY / accuracy); j < map.GetUpperBound(0) + 1 - (int)(StartNeedleOffsetY / accuracy) - (int)(HeadIdentationY / accuracy); j += 1 + (int)(StepNeedlesY / accuracy))
            {
                for (int j1 = 0; j1 < needlesDots; j1++)
                {

                    for (int i = (int)(StartNeedleOffsetX / accuracy) + (int)(HeadIdentationX / accuracy); i < map.GetUpperBound(1) + 1 - (int)(StartNeedleOffsetX / accuracy) - (int)(HeadIdentationX / accuracy); i += 1 + (int)(StepNeedlesX / accuracy))
                    {
                        for (int i1 = 0; i1 < needlesDots; i1++)
                        {
                            map[i, j] = 3;
                            i++;
                        }
                        i--;
                    }
                    j++;
                }
                j--;
            }

            this.map = map;

        }

        public Topology(TopologyByLineModel model, Point[] shape, Point[] tensionLines) 
        {
            this.name = model.NameProject;
            this.path = model.PathProject;
            this.accuracy = model.Accuracy;

            var size = getSizeOfShape(shape);
            int[,] map = new int[(int)((size.height + model.HeadIdentationY * 2) / accuracy), (int)((size.width + model.HeadIdentationX * 2) / accuracy)];

            map[0, 0] = 2;
            map[map.GetUpperBound(0), 0] = 2;
            map[0, map.GetUpperBound(1)] = 2;
            map[map.GetUpperBound(0), map.GetUpperBound(1)] = 2;

            for (int i = 0; i < (int)(size.height / accuracy); i++)
            {
                for (int j = 0; j < (int)(size.width / accuracy); j++)
                {
                    map[i + (int)(model.HeadIdentationY / accuracy), j + (int)(model.HeadIdentationX / accuracy)] = 1;
                }
            }
        }

        private (double width, double height) getSizeOfShape(Point[] points)
        {
            double minX = 0;
            double maxX = int.MaxValue;
            double minY = 0;
            double maxY = int.MaxValue;

            for(int i = 0; i < points.Length - 1; i++)
            {
                minX = Math.Min(minX, points[i].X);
                maxX = Math.Max(maxX, points[i].X);
                minY = Math.Min(minY, points[i].Y);
                maxY = Math.Max(maxY, points[i].Y);
            }

            return (maxX - minX, maxY - minY);
        }

        public static (Topology topology, Layer layer) fillNozzlesAndLayer(TopologyByLineModel model)
        {
            Point[] shape = new Point[1]; // из файла
            Point[] tensionLines = new Point[1]; // из файла

            Topology topology = new Topology(model, shape, tensionLines);
            Layer layer = new Layer();

            makeFigure(topology, layer, model, shape);

            return (topology, layer);
        }

        private static void makeFigure(Topology topology, Layer layer, TopologyByLineModel model, Point[] shape)
        {
            List<Point> route = new List<Point>();

            Point[] startPoints = new Point[4];
            startPoints[0] = new Point(0, 0);
            startPoints[1] = new Point(topology.map.GetUpperBound(0), 0);
            startPoints[2] = new Point(0, topology.map.GetUpperBound(1));
            startPoints[3] = new Point(topology.map.GetUpperBound(0), topology.map.GetUpperBound(1));

            Point start = findNearestToStart(startPoints, shape[0]);

            route.Add(start);
            for (int i = 0; i < shape.Length; i++) 
            {
                route.Add(shape[i]);

            }
            route.Add(start);
            layer.layerThread = route;
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
    }
}
