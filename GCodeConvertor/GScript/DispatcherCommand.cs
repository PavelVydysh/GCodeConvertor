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

        public double size { get; }

        private List<AbstractCommand> actualCommands;

        public DispatcherCommand(double size)
        {
            this.size = size;
            initializeCommands();
        }

        private void initializeCommands() 
        {
            actualCommands = new List<AbstractCommand>();

            actualCommands.Add(new UpCommand("ВВЕРХ", size));
            actualCommands.Add(new DownCommand("ВНИЗ", size));
            actualCommands.Add(new LeftCommand("ВЛЕВО", size));
            actualCommands.Add(new RightCommand("ВПРАВО", size));
            actualCommands.Add(new DotCommand("ТОЧКА", size));
        }

        public List<Point> buildScript(string[] commands, Point lastPoint)
        {
            List<Point> points = new List<Point>();
            Point prevPoint = lastPoint;
            Point currentPoint;

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

                AbstractCommand executableCommand = defineCommand(values[0]);
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
                        currentPoint = executableCommand.execute(prevPoint, secondInteger);
                        
                    }
                    else
                    {
                        throw new Exception(" Ошибка при предобработке:\nНеверный аргумент: " + command);
                    }
                }
                else
                {
                    currentPoint = executableCommand.execute(prevPoint, 0);
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
