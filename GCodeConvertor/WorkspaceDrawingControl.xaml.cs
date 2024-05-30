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

using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using GCodeConvertor.WorkspaceInstruments;
using static System.Windows.Forms.LinkLabel;

namespace GCodeConvertor
{
    /// <summary>
    /// Логика взаимодействия для WorkspaceDrawingControl.xaml
    /// </summary>
    public partial class WorkspaceDrawingControl : UserControl
    {
        //?????????????????????????? - надо куда-то вынести, дублируется в инструменте рисования и здесь
        private static Color POINT_COLOR = Colors.Red;
        private const double ELLIPSE_SIZE = 5;
        private static Color LINE_COLOR = Colors.Red;
        private const double LINE_SIZE = 2;
        private static Color SELECTED_POINT_COLOR = Colors.BlueViolet;
        private static Color CONFLICT_LINE_COLOR = Colors.Gray;
        //??????????????????????????

        // Константы имён элементов
        private const string WORKSPACE_CANVAS_NAME = "WorkspaceCanvas";
        private const string CUSTOM_ELEMENT_TAG = "custom";
        
        // Элементы UI элементов рабочей области
        public Canvas workspaceCanvas { get; }
        
        // Списки иголок кружочков квадратов
        public List<Ellipse> ellipses { get; }
        public List<Rectangle> needles { get; } // список игл 

        public CustomLineStorage customLineStorage { get; set; }
        public double cellSize { get; set; }

        // Элементы логики рабочей области
        public Topology topology { get; }
        public Layer activeLayer { get; set; }
        public WorkspaceInstrument workspaceIntrument { get; set; }
        public bool areAnyConflictsHere { get; set; }

        public WorkspaceDrawingControl(Topology topology)
        {
            InitializeComponent();

            this.topology = topology;

            //Инициализация Canvas (рабочей области)
            workspaceCanvas = (Canvas)FindName(WORKSPACE_CANVAS_NAME);
            setupWorkspaceCanvasEvents();
            needles = new List<Rectangle>();
            ellipses = new List<Ellipse>();
            customLineStorage = new CustomLineStorage();
        }

        private void setupWorkspaceCanvasEvents()
        {
            workspaceCanvas.Loaded += Canvas_Loaded;

            // Назначаем обработку событий, за которые должны отвечать инструменты на реализацию WorkspaceInstrument
            workspaceCanvas.MouseLeftButtonDown += element_MouseLeftButtonDown;
            workspaceCanvas.MouseRightButtonDown += element_MouseRightButtonDown;
            workspaceCanvas.MouseRightButtonUp += element_MouseRightButtonUp;
            workspaceCanvas.MouseLeftButtonUp += element_MouseLeftButtonUp;
            workspaceCanvas.MouseMove += element_MouseMove;
            workspaceCanvas.MouseWheel += element_MouseWheel;
            this.KeyDown += element_KeyDown;
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
            customLineStorage.clear();
            ellipses.Clear();
            
            deleteCustomItems();
            this.activeLayer = activeLayer;
            initLayer();
        }

        public void repaint()
        {
            customLineStorage.clear();
            ellipses.Clear();
            deleteCustomItems();
            initLayer();
        }

        private void deleteCustomItems()
        {
            var canvasChildrens = workspaceCanvas.Children.Cast<FrameworkElement>().ToList();

            foreach (var child in canvasChildrens)
            {
                if(child.Tag is not null && child.Tag.Equals(CUSTOM_ELEMENT_TAG))
                    workspaceCanvas.Children.Remove(child);
            }
        }

        /// <summary>
        /// Интерфейс для выбора активного инструмента рисовалки
        /// </summary>
        /// <param name="workspaceInstrument">Объект, реализующий интерфейс WorkspaceInstrument</param>
        public void setActiveWorkspaceInstrument(WorkspaceInstrument workspaceInstrument)
        {
            this.workspaceIntrument = workspaceInstrument;
        }

        public string getCustomElementTag()
        {
            return CUSTOM_ELEMENT_TAG;
        }
        
        // Инициализации сетки поля и ячеек
        private void initWorkspace()
        {
            cellSize = (double)(workspaceCanvas.ActualWidth / topology.map.GetLength(0));

            for (int i = 0; i < topology.map.GetUpperBound(1) + 1; i++)
            {
                for (int j = 0; j < topology.map.GetUpperBound(0) + 1; j++)
                {
                    Rectangle cell = setupCell(topology.map[i, j]);

                    Canvas.SetTop(cell, cellSize * j);
                    Canvas.SetLeft(cell, cellSize * i);

                    workspaceCanvas.Children.Add(cell);
                }
            }
            //ItemsList.Add(new CustomItem(activeLayer.name, "12"));
            //layerListBox.SelectedIndex = 0;
        }

        private Rectangle setupCell(int cellType)
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

        private void initLayer()
        {
            int pointLastIndex = activeLayer.thread.Count - 1;

            Ellipse oldEllipse = null;
            Ellipse currentEllipse = null;
            Line currentLine = null;

            areAnyConflictsHere = false;

            for (int pointIndex = 0; pointIndex <= pointLastIndex; pointIndex++) 
            {
                int previousPointIndex = pointIndex - 1;

                bool isConflict = false;

                if (previousPointIndex >= 0)
                {
                    isConflict |= isLineCrossTheNeedles(
                        new Point(getDrawingValueByThreadValue(activeLayer.thread[previousPointIndex].X),
                                  getDrawingValueByThreadValue(activeLayer.thread[previousPointIndex].Y)),
                        new Point(getDrawingValueByThreadValue(activeLayer.thread[pointIndex].X),
                                  getDrawingValueByThreadValue(activeLayer.thread[pointIndex].Y))
                        );
                }

                if (isConflict)
                {
                    currentLine.Stroke = new SolidColorBrush(CONFLICT_LINE_COLOR);
                }

                areAnyConflictsHere |= isConflict;

                oldEllipse = currentEllipse;
                currentEllipse = drawPoint(
                    getDrawingValueByThreadValue(activeLayer.thread[pointIndex].X), 
                    getDrawingValueByThreadValue(activeLayer.thread[pointIndex].Y),
                    activeLayer.isDotSelected(activeLayer.thread[pointIndex]));
                
                if (ellipses.Count >= 2)
                {
                    customLineStorage.addLine(new CustomLine(currentLine, oldEllipse, currentEllipse));
                }

                if (pointIndex != pointLastIndex)
                {
                    currentLine = drawLine(
                        getDrawingValueByThreadValue(activeLayer.thread[pointIndex].X),
                        getDrawingValueByThreadValue(activeLayer.thread[pointIndex].Y),
                        getDrawingValueByThreadValue(activeLayer.thread[pointIndex + 1].X),
                        getDrawingValueByThreadValue(activeLayer.thread[pointIndex + 1].Y));
                }
            }
        }

        private Line drawLine(double previousDrawingX, double previousDrawingY, double currentDrawingX, double currentDrawingY)
        {
            Line drawingLine = setupLine();
            drawingLine.X1 = previousDrawingX;
            drawingLine.Y1 = previousDrawingY;
            drawingLine.X2 = currentDrawingX;
            drawingLine.Y2 = currentDrawingY;
            workspaceCanvas.Children.Add(drawingLine);
            return drawingLine;
        }

        private Ellipse drawPoint(double currentDrawingX, double currentDrawingY, bool isSelected)
        {
            Ellipse drawingPoint = setupEllipse(isSelected);
            ellipses.Add(drawingPoint);
            //ellipse.MouseRightButtonDown += Ellipse_MouseRightDown;
            Canvas.SetLeft(drawingPoint, currentDrawingX - ELLIPSE_SIZE / 2);
            Canvas.SetTop(drawingPoint, currentDrawingY - ELLIPSE_SIZE / 2);
            //layerEllipses.Add(ellipse);
            drawingPoint.MouseLeftButtonDown += element_MouseLeftButtonDown;
            drawingPoint.MouseRightButtonDown += element_MouseRightButtonDown;
            drawingPoint.MouseMove += element_MouseMove;
            workspaceCanvas.Children.Add(drawingPoint);
            return drawingPoint;
        }

        private Line setupLine()
        {
            Line line = new Line();
            line.Tag = getCustomElementTag();
            line.Fill = new SolidColorBrush(LINE_COLOR);
            line.Visibility = Visibility.Visible;
            line.StrokeThickness = LINE_SIZE;
            line.Stroke = new SolidColorBrush(LINE_COLOR);
            line.MouseRightButtonDown += element_MouseRightButtonDown;

            return line;
        }

        private Ellipse setupEllipse(bool isSelected)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Tag = getCustomElementTag();
            ellipse.Height = ELLIPSE_SIZE;
            ellipse.Width = ELLIPSE_SIZE;
            ellipse.Fill = new SolidColorBrush(POINT_COLOR);
            if(isSelected)
            {
                ellipse.Fill = new SolidColorBrush(SELECTED_POINT_COLOR);

            }
            return ellipse;
        }

        private bool isLineCrossTheNeedles(Point previousPoint, Point currentPoint)
        {
            LineGeometry lineGeometry = new LineGeometry(previousPoint, currentPoint);
            foreach (Rectangle needle in needles)
            {
                RectangleGeometry rectangleGeometry = new RectangleGeometry(
                    new Rect(Canvas.GetLeft(needle), Canvas.GetTop(needle), needle.Width, needle.Height));
                if (lineGeometry.FillContainsWithDetail(rectangleGeometry) != IntersectionDetail.Empty)
                {
                    return true;
                }
            }
            return false;
        }


        private double getDrawingValueByThreadValue(double threadValue)
        {
            return threadValue * cellSize;
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            initWorkspace();
        }

        // Отлавливание Events и перенаправление на вызов выбранного инструмента 

        // При наличии нескольких ссылок 2+, то рекомендуется выставлять e.Hadlded = false, 
        // т.к. вышестоящий элемент в иерархии

        public void element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            executeInstrument(EventType.LeftMouseButtonUp, sender, e);
        }
        public void element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            executeInstrument(EventType.LeftMouseButtonDown, sender, e);
            e.Handled = false;
        }
        public void element_MouseWheel(object sender, MouseEventArgs e)
        {
            executeInstrument(EventType.MouseWheel, sender, e);
        }
        public void element_MouseMove(object sender, MouseEventArgs e)
        {
            executeInstrument(EventType.MouseMove, sender, e);
        }
        public void element_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            executeInstrument(EventType.RightMouseButtonDown, sender, e);
        }
        public void element_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            executeInstrument(EventType.RightMouseButtonUp, sender, e);
        }
        public void element_KeyDown(object sender, KeyEventArgs e)
        {
            executeInstrument(EventType.KeyDown, sender, e);
        }

    }
}
