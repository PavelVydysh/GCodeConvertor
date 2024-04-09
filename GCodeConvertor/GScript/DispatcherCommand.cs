﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GCodeConvertor.GScript
{
    public class DispatcherCommand
    {

        public ProjectWindow pWindow { get; }

        private List<AbstractCommand> actualCommands;

        public DispatcherCommand(ProjectWindow pWindow)
        {
            this.pWindow = pWindow;
            initializeCommands();
        }

        private void initializeCommands() 
        {
            actualCommands = new List<AbstractCommand>();

            actualCommands.Add(new UpCommand("ВВЕРХ", pWindow.size));
            actualCommands.Add(new DownCommand("ВНИЗ", pWindow.size));
            actualCommands.Add(new LeftCommand("ВЛЕВО", pWindow.size));
            actualCommands.Add(new RightCommand("ВПРАВО", pWindow.size));
            actualCommands.Add(new DotCommand("ТОЧКА", pWindow.size));
            actualCommands.Add(new DrawCommand("СТАРТ_РИСУНОК", pWindow.size));
            actualCommands.Add(new NoStartDrawCommand("РИСУНОК", pWindow.size));
        }

        public List<Point> buildScript(string[] commands, Point lastPoint)
        {
            List<Point> points = new List<Point>();
            Point prevPoint = lastPoint;
            Point currentPoint;
            AbstractCommand executableCommand;

            foreach (string command in commands)
            {
                if (command.Equals(""))
                {
                    continue;
                }

                string[] values = splitCommand(command);
                if (values.Length <= 0)
                {
                    throw new Exception(" Ошибка при предобработке:\nСинтаксическая ошибка: " + command);
                }

                executableCommand = defineCommand(values[0]);
                if (executableCommand == null)
                {
                    throw new Exception(" Ошибка при предобработке:\nНеизвестная команда: " + command);
                }

                if (values.Length > 1)
                {
                    int secondInteger;
                    bool isInt = int.TryParse(values[1], out secondInteger);

                    if (isInt)
                    {
                        List<Point> retPoints = executableCommand.execute(prevPoint, secondInteger);
                        currentPoint = retPoints[retPoints.Count - 1];

                    }
                    else
                    {
                        throw new Exception(" Ошибка при предобработке:\nНеверный аргумент: " + command);
                    }
                }
                else if (executableCommand is DotCommand)
                {
                    List<Point> retPoints = executableCommand.execute(prevPoint, 0);
                    currentPoint = retPoints[retPoints.Count - 1];
                }
                else if (executableCommand is DrawCommand || executableCommand is NoStartDrawCommand)
                {
                    if (points.Count != 0)
                    {
                        pWindow.appendScriptResult(points, true);
                    }
                    List<Point> retPoints = executableCommand.execute(prevPoint, 0, pWindow.activeLayer.layerThread);
                    foreach (Point p in retPoints)
                    {
                        points.Add(p);
                    }
                    currentPoint = retPoints[retPoints.Count - 1];
                }

                if (currentPoint == prevPoint)
                {
                    points.Add(currentPoint);
                }
                prevPoint = currentPoint;
            }

            return points;
        }

        private string[] splitCommand(string command)
        {
            return command.Replace(")", "").Split("(");
        }

        private AbstractCommand defineCommand(string str_command) 
        {
            foreach (AbstractCommand command in actualCommands)
            {
                if (command.type.Equals(str_command.ToUpper()))
                {
                    return command;
                }
            }
            return null;
        }
    }
}
