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
using static GCodeConvertor.ProjectWindow;

namespace GCodeConvertor.GScript
{
    /// <summary>
    /// Логика взаимодействия для GScriptWindow.xaml
    /// </summary>
    public partial class GScriptWindow : Window
    {

        CommandReader commandReader;
        ProjectWindow pWindow;
        DispatcherCommand dispatcherCommand;

        public GScriptWindow(ProjectWindow pWindow)
        {
            InitializeComponent();
            this.pWindow = pWindow;
            commandReader = new CommandReader(this);
            dispatcherCommand = new DispatcherCommand(pWindow.size);
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

            if (pWindow.drawingState == DrawingStates.SET_START_POINT)
            {
                console.Text += "Не удалось собрать скрипт: ";
                console.Text += "Стартовая точка не определена\n";
                return;
            }
            else if (pWindow.drawingState == DrawingStates.EDIT)
            {
                console.Text += "Не удалось собрать скрипт: ";
                console.Text += "Существуют нерешённые конфликты\n";
                return;
            }
            else if (pWindow.drawingState == DrawingStates.SET_END_POINT)
            {
                console.Text += "Не удалось собрать скрипт: ";
                console.Text += "Слой является законченым\n";
                return;
            }

            string[] commands = commandReader.readCommands();
            console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Сборка скрипта завершена успешно." + "\n";
            try
            {
                console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Предобработка скрипта начата." + "\n";
                List<System.Windows.Point> points = dispatcherCommand.buildScript(commands, pWindow.activeLayer.layerThread[pWindow.activeLayer.layerThread.Count - 1]);
                console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Предобработка скрипта завершена успешно." + "\n";

                console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Выполнение скрипта начато." + "\n";
                pWindow.appendScriptResult(points);
                console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]" + " Выполнение скрипта завершно успешно." + "\n";
            }
            catch (Exception ex)
            {
                console.Text += "[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]"  + ex.Message + "\n";
            }
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
    }
}
