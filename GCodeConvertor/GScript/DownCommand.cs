﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GCodeConvertor.GScript
{
    public class DownCommand : AbstractCommand
    {
        public DownCommand(string type, double size) : base(type, size) 
        {
            
        }

        public override List<Point> execute(Point prevPoint, double steps, List<Point> points = null)
        {
            List<Point> pointsRet = new List<Point>();
            pointsRet.Add(new Point(prevPoint.X, prevPoint.Y + steps));
            return pointsRet;
        }
    }
}
