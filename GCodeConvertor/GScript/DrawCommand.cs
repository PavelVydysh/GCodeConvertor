using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GCodeConvertor.GScript
{
    public class DrawCommand : AbstractCommand
    {
        public DrawCommand(string type, double size) : base(type, size)
        {

        }

        public override List<Point> execute(Point prevPoint, double steps, List<Point> points = null)
        {
            return updatePoints(points);
        }

        private List<Point> updatePoints(List<Point> pointsToUpdate) 
        {
            List<Point> readypoints = new List<Point>();

            for (int i = 0; i < pointsToUpdate.Count - 1; i++)
            {

                if (i == pointsToUpdate.Count - 1)
                {

                }

                double X = pointsToUpdate[i + 1].X - pointsToUpdate[i].X;
                double Y = pointsToUpdate[i + 1].Y - pointsToUpdate[i].Y;

                if (readypoints.Count == 0)
                {
                    Point p = new Point(pointsToUpdate[pointsToUpdate.Count - 1].X + X, pointsToUpdate[pointsToUpdate.Count - 1].Y + Y);
                    readypoints.Add(p);
                }
                else 
                {
                    Point p = new Point(readypoints[readypoints.Count - 1].X + X, readypoints[readypoints.Count - 1].Y + Y);
                    readypoints.Add(p);
                }
            }

            return readypoints;
        }
    }
}
