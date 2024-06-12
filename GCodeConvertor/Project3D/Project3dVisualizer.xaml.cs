using GCodeConvertor.ProjectForm;
using GCodeConvertor.UI;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static System.Windows.Forms.LinkLabel;

namespace GCodeConvertor.Project3D
{
    /// <summary>
    /// Логика взаимодействия для _3dVisualizer.xaml
    /// </summary>
    public partial class Project3dVisualizer : Window, INotifyPropertyChanged
    {
        private static double DEFAULT_SPEED = 50;

        private ProjectWindow projectWindow;

        public event PropertyChangedEventHandler? PropertyChanged;

        private string layerFactor;

        private double _speed;
        public double speed { get { return _speed; } set { if (_speed != value) { if (value > 0 && value < 500) { _speed = value; setupTimeline(); OnPropertyChanged(); } } } }
        private double _lineLength;
        public double lineLength { get { return _lineLength; } set { if (_lineLength != value) { _lineLength = value; OnPropertyChanged(); } } }
        private double _workTime;
        public double workTime { get { return _workTime;  } set { if (_workTime != value) { _workTime = value; OnPropertyChanged(); } } }

        private List<Point3D> rubberBandPoints;

        public Project3dVisualizer(ProjectWindow projectWindow, string layerFactor)
        {
            this.projectWindow = projectWindow;
            this.speed = DEFAULT_SPEED;
            this.layerFactor = layerFactor;
            rubberBandPoints = new List<Point3D>();

            InitializeComponent();

            DataContext = this;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Viewport3D_Loaded(object sender, RoutedEventArgs e)
        {
            bool loadResult = setupLinePoints();
            if (!loadResult)
            {
                MessageWindow messageWindow = new MessageWindow(
                    "Нет доступных слоев для визуализации!",
                    "Закончите хотя бы один слой, либо пометьте законченный слой активным.");
                messageWindow.ShowDialog();
                closeWingow();
            }
            else
            {
                setupLineLength();
                setupTimeline();
            }
            
        }

        private void setupTimeline()
        {
            if(TimelineSlider is not null)
            {
                double maxWorkTime = setupMaxWorkTime();
                TimelineSlider.Minimum = 0;
                TimelineSlider.Maximum = maxWorkTime;
                TimelineSlider.Value = maxWorkTime;
            }
        }

        private double setupMaxWorkTime()
        {
            return Math.Round(lineLength / speed, 2);
        }

        private void TimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            workTime = Math.Round(TimelineSlider.Value, 2);

            Viewport3D.Children.Clear();

            addAxises();

            Point3D previousPoint = rubberBandPoints[0];

            double currentMaxLength = workTime * speed;
            double currentLength = 0;

            for (int pointIndex = 1; pointIndex < rubberBandPoints.Count; pointIndex++)
            {
                LinesVisual3D line = new LinesVisual3D();
                line.Color = (Color)Application.Current.Resources["ForegroundColor"];
                line.Thickness = 5;

                if (currentMaxLength - currentLength >= DistanceBetweenPoints(previousPoint, rubberBandPoints[pointIndex]))
                {
                    currentLength += DistanceBetweenPoints(previousPoint, rubberBandPoints[pointIndex]);
                    line.Points.Add(previousPoint);
                    line.Points.Add(rubberBandPoints[pointIndex]);
                    Viewport3D.Children.Add(line);

                    previousPoint = rubberBandPoints[pointIndex];
                }
                else
                {
                    Point3D currentPoint = PointAtDistanceBetweenPoints(previousPoint, rubberBandPoints[pointIndex], currentMaxLength - currentLength);
                    line.Points.Add(previousPoint);
                    line.Points.Add(currentPoint);
                    Viewport3D.Children.Add(line);
                    break;
                }
            }
        }

        private void addAxises()
        {
            LinesVisual3D lineBlue = new LinesVisual3D();
            lineBlue.Color = Colors.Blue;
            lineBlue.Thickness = 2;
            lineBlue.Points.Add(new Point3D(0, 0, 0));
            lineBlue.Points.Add(new Point3D(0, 0, 50));
            Viewport3D.Children.Add(lineBlue);

            LinesVisual3D lineGreen = new LinesVisual3D();
            lineGreen.Color = Colors.Green;
            lineGreen.Thickness = 2;
            lineGreen.Points.Add(new Point3D(0, 0, 0));
            lineGreen.Points.Add(new Point3D(0, 50, 0));
            Viewport3D.Children.Add(lineGreen);

            LinesVisual3D lineRed = new LinesVisual3D();
            lineRed.Color = Colors.Red;
            lineRed.Thickness = 2;
            lineRed.Points.Add(new Point3D(0, 0, 0));
            lineRed.Points.Add(new Point3D(50, 0, 0));
            Viewport3D.Children.Add(lineRed);
        }

        public double DistanceBetweenPoints(Point3D point1, Point3D point2)
        {
            double deltaX = point2.X - point1.X;
            double deltaY = point2.Y - point1.Y;
            double deltaZ = point2.Z - point1.Z;

            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }

        public Point3D PointAtDistanceBetweenPoints(Point3D point1, Point3D point2, double distance)
        {
            double directionX = point2.X - point1.X;
            double directionY = point2.Y - point1.Y;
            double directionZ = point2.Z - point1.Z;

            double length = Math.Sqrt(directionX * directionX + directionY * directionY + directionZ * directionZ);

            double unitX = directionX / length;
            double unitY = directionY / length;
            double unitZ = directionZ / length;

            double scaledX = unitX * distance;
            double scaledY = unitY * distance;
            double scaledZ = unitZ * distance;

            double newX = point1.X + scaledX;
            double newY = point1.Y + scaledY;
            double newZ = point1.Z + scaledZ;

            return new Point3D(newX, newY, newZ);
        }

        private bool setupLinePoints()
        {
            double height = 0;

            List<Point3D> points = new List<Point3D>();

            List<Layer> layers = getGoodLayers();

            if(layers == null || layers.Count == 0) {
                return false;
            }

            foreach (Layer layer in layers)
            {
                height += layer.height;

                List<Point> drawingPoints = new List<Point>();

                foreach (Point point in layer.thread)
                {
                    drawingPoints.Add(new Point(getDrawingValueByThreadValue(point.X), getDrawingValueByThreadValue(point.Y)));
                }

                List<Point> rubberDrawingPoints = projectWindow.wdc.getRubberBandPath(drawingPoints.ToArray());

                foreach (Point point in rubberDrawingPoints)
                {
                    points.Add(new Point3D(
                        getThreadValueByTopologyValue((int)Math.Floor(point.X / projectWindow.wdc.cellSize)),
                        getThreadValueByTopologyValue((int)Math.Floor(point.Y / projectWindow.wdc.cellSize)),
                        height));
                }
            }
            rubberBandPoints = points;
            return true;
        }

        private double getDrawingValueByThreadValue(double threadValue)
        {
            return threadValue / ProjectSettings.preset.topology.accuracy * projectWindow.wdc.cellSize;
        }

        private double getThreadValueByTopologyValue(double topologyValue)
        {
            return topologyValue * ProjectSettings.preset.topology.accuracy + ProjectSettings.preset.topology.accuracy / 2;
        }

        private void setupLineLength()
        {
            double length = 0;
            Point3D previousPoint = rubberBandPoints[0];
            for (int pointIndex = 1; pointIndex < rubberBandPoints.Count; pointIndex++)
            {
                length += DistanceBetweenPoints(previousPoint, rubberBandPoints[pointIndex]);
                previousPoint = rubberBandPoints[pointIndex];
            }
            lineLength = Math.Round(length, 2);
        }

        private List<Layer> getGoodLayers()
        {
            List<Layer> goodLayers = new List<Layer>();

            ProjectSettings.preset.layers.Reverse();

            foreach (Layer layer in ProjectSettings.preset.layers)
            {
                if(layer.isEnable && layer.isEnded())
                {
                    goodLayers.Add(layer);
                }
            }

            ProjectSettings.preset.layers.Reverse();

            List<Layer> tempList = new List<Layer>();
            for (int i = 0; i < int.Parse(layerFactor); i++)
            {
                tempList.AddRange(goodLayers);
            }
            goodLayers = tempList;

            return goodLayers;
        }

        private void closeWingow()
        {
            this.Close();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            projectWindow.project3DVisualizer = null;
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

        private void Window_StateChanged(object sender, EventArgs e)
        {
            
        }
        
    }
}
