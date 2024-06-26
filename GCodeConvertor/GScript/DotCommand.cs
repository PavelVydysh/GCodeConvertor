﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GCodeConvertor.GScript
{
    public class DotCommand : AbstractCommand
    {
        public DotCommand(string type, double size) : base(type, size)
        {

        }

        public override List<Point> execute(Point prevPoint, double steps, List<Point> points = null)
        {
            List<Point> pointsRet = new List<Point>();
            pointsRet.Add(prevPoint);
            return pointsRet;
        }
    }
}
