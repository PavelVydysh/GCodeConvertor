using GCodeConvertor.ProjectForm;
using GCodeConvertor.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GCodeConvertor.GScript
{
    /// <summary>
    /// Логика взаимодействия для GScriptWindow.xaml
    /// </summary>
    public partial class GScriptWindow : Window
    {

        CommandReader commandReader;
        WorkspaceDrawingControl workspaceDrawingControl;
        DispatcherCommand dispatcherCommand;
        ProjectWindow projectWindow;

        public GScriptWindow(WorkspaceDrawingControl workspaceDrawingControl, ProjectWindow projectWindow)
        {
            InitializeComponent();

            this.projectWindow = projectWindow;
            this.workspaceDrawingControl = workspaceDrawingControl;
            commandReader = new CommandReader(this);
            dispatcherCommand = new DispatcherCommand(workspaceDrawingControl);
            UpdateLineNumbers();
            init();
        }

        private void init()
        {
            console.Text += ":::::GScript 1.0:::::\n";
            console.Text += DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "\n";
            console.Text += "Среда готова к исполнению.\n";
        }

        private void UpdateLineNumbers()
        {
            lineNumbersTextBox.Text = "";
            string[] lines = textBox.Text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lineNumbersTextBox.Text += (i + 1) + Environment.NewLine;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLineNumbers();
        }

        private void textScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                lineNumbersScrollViewer.ScrollToVerticalOffset(textScrollViewer.VerticalOffset);
            }
        }

        private void startScriptButton_Click(object sender, RoutedEventArgs e)
        {
            console.Text = "";
            console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Сборка скрипта. " + "\n";

            if (!workspaceDrawingControl.activeLayer.isStarted())
            {
                console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Не удалось собрать скрипт: ";
                console.Text += "Стартовая точка не определена\n";
                return;
            }
            else if (workspaceDrawingControl.conflictLines.Count > 0)
            {
                console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Не удалось собрать скрипт: ";
                console.Text += "Существуют нерешённые конфликты\n";
                return;
            }
            else if (workspaceDrawingControl.activeLayer.isEnded())
            {
                console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Не удалось собрать скрипт: ";
                console.Text += "Слой является законченым\n";
                return;
            }

            string[] commands = commandReader.readCommands();
            console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Сборка скрипта завершена успешно." + "\n";
            console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Предобработка скрипта начата." + "\n";
            List<System.Windows.Point> points = new List<System.Windows.Point>();
            
            try
            {
                points = dispatcherCommand.buildScript(commands, workspaceDrawingControl.activeLayer.thread[workspaceDrawingControl.activeLayer.thread.Count - 1]);
            }
            catch (Exception ex)
            {
                console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + ex.Message + "\n";
                return;
            }

            console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Предобработка скрипта завершена успешно." + "\n";

            console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Выполнение скрипта начато." + "\n";

            if (isPointsOnNeedle(points))
            {
                console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Ошибка времени выполнения:\n" + " Точка попадает на иглу." + "\n";
                return;
            }
            workspaceDrawingControl.addDrawingPointsToActiveLayer(points);

            console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Выполнение скрипта завершно успешно." + "\n";
            
        }

        private bool isPointsOnNeedle(List<System.Windows.Point> points)
        {
            foreach(System.Windows.Point point in points)
            {
                EllipseGeometry ellipseGeometry = new EllipseGeometry(
                    new System.Windows.Point(getDrawingValueByThreadValue(point.X), getDrawingValueByThreadValue(point.Y)), 
                    WorkspaceDrawingControl.ELLIPSE_SIZE, 
                    WorkspaceDrawingControl.ELLIPSE_SIZE);
                IntersectionDetail intersectionDetail;
                foreach (System.Windows.Shapes.Rectangle needle in workspaceDrawingControl.needles)
                {
                    RectangleGeometry rectGeo = new RectangleGeometry(new Rect(Canvas.GetLeft(needle), Canvas.GetTop(needle), needle.Width, needle.Height));
                    intersectionDetail = ellipseGeometry.FillContainsWithDetail(rectGeo);
                    if (intersectionDetail != IntersectionDetail.Empty)
                        return true;
                }
            }
            return false;
        }

        private double getDrawingValueByThreadValue(double threadValue)
        {
            return threadValue * workspaceDrawingControl.cellSize;
        }

        private void upSDutton_Click(object sender, RoutedEventArgs e)
        {   
            if (textBox.Text.Length != 0 && !textBox.Text[textBox.Text.Length - 1].Equals("\n"))
            {
                textBox.Text += "\n";
            }
            textBox.Text += "ВВЕРХ();";
            int startIndex = textBox.GetCharacterIndexFromLineIndex(textBox.LineCount - 1) + 6;
            textBox.Select(startIndex, 0);
            textBox.Focus();
        }

        private void leftSButton_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text.Length != 0 && !textBox.Text[textBox.Text.Length - 1].Equals("\n"))
            {
                textBox.Text += "\n";
            }
            textBox.Text += "ВЛЕВО();";
            int startIndex = textBox.GetCharacterIndexFromLineIndex(textBox.LineCount - 1) + 6;
            textBox.Select(startIndex, 0);
            textBox.Focus();
        }

        private void rightSButton_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text.Length != 0 && !textBox.Text[textBox.Text.Length - 1].Equals("\n"))
            {
                textBox.Text += "\n";
            }
            textBox.Text += "ВПРАВО();";
            int startIndex = textBox.GetCharacterIndexFromLineIndex(textBox.LineCount - 1) + 7;
            textBox.Select(startIndex, 0);
            textBox.Focus();
        }

        private void downSButton_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text.Length != 0 && !textBox.Text[textBox.Text.Length - 1].Equals("\n"))
            {
                textBox.Text += "\n";
            }
            textBox.Text += "ВНИЗ();";
            int startIndex = textBox.GetCharacterIndexFromLineIndex(textBox.LineCount - 1) + 5;
            textBox.Select(startIndex, 0);
            textBox.Focus();
        }

        private void setDotButton_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text.Length != 0 && !textBox.Text[textBox.Text.Length - 1].Equals("\n"))
            {
                textBox.Text += "\n";
            }
            textBox.Text += "ТОЧКА;";
        }

        private void startDrawButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text.Length != 0 && !textBox.Text[textBox.Text.Length - 1].Equals("\n"))
            {
                textBox.Text += "\n";
            }
            textBox.Text += "СТАРТ_РИСУНОК;";
        }

        private void drawButton_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text.Length != 0 && !textBox.Text[textBox.Text.Length - 1].Equals("\n"))
            {
                textBox.Text += "\n";
            }
            textBox.Text += "РИСУНОК;";
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            projectWindow.GSWindow = null;
            this.Close();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void HideWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
