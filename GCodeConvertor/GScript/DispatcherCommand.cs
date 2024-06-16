using GCodeConvertor.ProjectForm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GCodeConvertor.GScript
{
    public class DispatcherCommand
    {

        public WorkspaceDrawingControl pWindow { get; }

        private List<AbstractCommand> actualCommands;

        public DispatcherCommand(WorkspaceDrawingControl pWindow)
        {
            this.pWindow = pWindow;
            initializeCommands();
        }

        private void initializeCommands() 
        {
            actualCommands = new List<AbstractCommand>();

            actualCommands.Add(new UpCommand("ВВЕРХ", pWindow.cellSize));
            actualCommands.Add(new DownCommand("ВНИЗ", pWindow.cellSize));
            actualCommands.Add(new LeftCommand("ВЛЕВО", pWindow.cellSize));
            actualCommands.Add(new RightCommand("ВПРАВО", pWindow.cellSize));
            actualCommands.Add(new DotCommand("ТОЧКА", pWindow.cellSize));
            actualCommands.Add(new DrawCommand("СТАРТ_РИСУНОК", pWindow.cellSize));
            actualCommands.Add(new NoStartDrawCommand("РИСУНОК", pWindow.cellSize));
        }

        public List<Point> buildScript(string[] commands, Point lastPoint)
        {
            List<Point> tempPoints = new List<Point>();
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
                        List<Point> retPoints = executableCommand.execute(prevPoint, secondInteger * (ProjectSettings.preset.topology.accuracy));
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
                    if (points.Count == 0)
                    {
                        tempPoints.InsertRange(0, pWindow.activeLayer.thread);
                        //pWindow.addDrawingPointsToActiveLayer(points);
                    }
                    List<Point> retPoints = executableCommand.execute(prevPoint, 0, tempPoints);
                    foreach (Point p in retPoints)
                    {
                        points.Add(p);
                        tempPoints.Add(p);
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
