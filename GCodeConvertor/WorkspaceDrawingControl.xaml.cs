using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Rectangle = System.Windows.Shapes.Rectangle;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using GCodeConvertor.WorkspaceInstruments;

namespace GCodeConvertor
{
    /// <summary>
    /// Логика взаимодействия для WorkspaceDrawingControl.xaml
    /// </summary>
    public partial class WorkspaceDrawingControl : UserControl
    {

        // Константы имён элементов
        private const string WORKSPACE_CANVAS_NAME = "WorkspaceCanvas";
        
        // Элементы UI элемента рабочей области
        private Canvas workspaceCanvas;
        public List<Rectangle> needles { get; } // список игл 

        // Элементы логики рабочей области
        public Topology topology { get; }
        public Layer activeLayer { get; set; }
        public WorkspaceInstrument workspaceIntrument { get; set; }

        public WorkspaceDrawingControl(Topology topology)
        {
            InitializeComponent();

            this.topology = topology;

            //Инициализация Canvas (рабочей области)
            workspaceCanvas = (Canvas)FindName(WORKSPACE_CANVAS_NAME);
            setupWorkspaceCanvasEvents();
            needles = new List<Rectangle>();

        }

        private void setupWorkspaceCanvasEvents()
        {
            workspaceCanvas.Loaded += Canvas_Loaded;

            // Назначаем обработку событий, за которые должны отвечать инструменты на реализацию WorkspaceInstrument
            workspaceCanvas.MouseLeftButtonDown += element_MouseLeftButtonDown;
            workspaceCanvas.MouseLeftButtonUp += element_MouseLeftButtonUp;
            workspaceCanvas.MouseMove += element_MouseMove;
            workspaceCanvas.MouseWheel += element_MouseWheel;
        }
        private void executeInstrument(EventType eventType, object sender, EventArgs e)
        {
            if (workspaceIntrument is not null)
            {
                workspaceIntrument.execute(eventType, sender, e);
            }
        }

        /// <summary>
        /// Интерфейс для выбора активного слоя "рисовалки"
        /// </summary>
        /// <param name="activeLayer">Слой, с которым должна работать "рисовалка"</param>
        public void setActiveLayer(Layer activeLayer)
        {
            this.activeLayer = activeLayer;
        }

        /// <summary>
        /// Интерфейс для выбора активного инструмента рисовалки
        /// </summary>
        /// <param name="workspaceInstrument">Объект, реализующий интерфейс WorkspaceInstrument</param>
        public void setActiveWorkspaceInstrument(WorkspaceInstrument workspaceInstrument)
        {
            this.workspaceIntrument = workspaceInstrument;
        }
        
        // Инициализации сетки поля и ячеек
        private void initWorkspace()
        {
            double size = (double)(workspaceCanvas.ActualWidth / topology.map.GetLength(0));

            for (int i = 0; i < topology.map.GetUpperBound(1) + 1; i++)
            {
                for (int j = 0; j < topology.map.GetUpperBound(0) + 1; j++)
                {
                    Rectangle cell = setupCell(topology.map[i, j], size);

                    Canvas.SetTop(cell, size * j);
                    Canvas.SetLeft(cell, size * i);

                    workspaceCanvas.Children.Add(cell);
                }
            }
            //ItemsList.Add(new CustomItem(activeLayer.name, "12"));
            //layerListBox.SelectedIndex = 0;
        }

        private Rectangle setupCell(int cellType, double cellSize)
        {
            Rectangle cell = new Rectangle();

            cell.Height = cellSize;
            cell.Width = cellSize;
            cell.StrokeThickness = 1;

            if (cellType == 1)
            {
                cell.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBE98"));
            }
            else if (cellType == 2)
            {
                cell.Fill = new SolidColorBrush(Colors.Green);
            }
            else if (cellType == 3)
            {
                cell.Fill = new SolidColorBrush(Colors.Black);
                needles.Add(cell);
            }
            else
            {
                cell.Fill = new SolidColorBrush(Colors.White);
            }

            cell.MouseLeftButtonDown += element_MouseLeftButtonDown;

            return cell;
        }


        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            initWorkspace();
        }

        // Отлавливание Events и перенаправление на вызов выбранного инструмента 

        // При наличии нескольких ссылок 2+, то рекомендуется выставлять e.Hadlded = false, 
        // т.к. вышестоящий элемент в иерархии
        private void element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            executeInstrument(EventType.LeftMouseButtonUp, sender, e);
        }
        private void element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            executeInstrument(EventType.LeftMouseButtonDown, sender, e);
            e.Handled = false;
        }
        private void element_MouseWheel(object sender, MouseEventArgs e)
        {
            executeInstrument(EventType.MouseWheel, sender, e);
        }
        private void element_MouseMove(object sender, MouseEventArgs e)
        {
            executeInstrument(EventType.MouseMove, sender, e);
        }

    }
}
