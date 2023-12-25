using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Drawing;
using Polenter.Serialization;
using System.Security.Policy;
using static System.Windows.Forms.LinkLabel;

namespace GCodeConvertor
{
    /// <summary>
    /// Логика взаимодействия для ProjectWindow.xaml
    /// </summary>

    public partial class ProjectWindow : Window
    {
        private enum DrawingStates{
            SET_START_POINT,
            DRAWING,
            SET_END_POINT
        }

        private List<System.Windows.Shapes.Rectangle> rectangles = new List<System.Windows.Shapes.Rectangle>();

        private DrawingStates drawingState;

        private int startPointX;
        private int startPointY;

        private double currentDotX;
        private double currentDotY;
        double size;
        private bool drawArrow = false;
        Line line;

        List<System.Windows.Point> layerPoints;

        public ProjectWindow()
        {
            InitializeComponent();
            DataContext = ProjectSettings.preset;
            line = new Line();
            drawingState = DrawingStates.SET_START_POINT;
            layerPoints = new List<System.Windows.Point>();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int docY = (int)Math.Floor(e.GetPosition(CanvasMain).Y / size);
            int docX = (int)Math.Floor(e.GetPosition(CanvasMain).X / size);

            int dotType = ProjectSettings.preset.topology.map[docY, docX];

            switch (drawingState)
            {
                case DrawingStates.SET_START_POINT:
                    if(dotType == 2)
                    {
                        startPointX = docX;
                        startPointY = docY;
                        draw(sender, e, dotType);
                        drawingState = DrawingStates.DRAWING;
                    }
                    break;
                case DrawingStates.DRAWING:
                    if (dotType != 3)
                    {
                        draw(sender, e, dotType);

                        if (docY == startPointY && docX == startPointX)
                        {
                            drawingState = DrawingStates.SET_END_POINT;
                        }
                    }
                    break;
                case DrawingStates.SET_END_POINT:
                    MessageBox.Show("Слой является законченным");
                    break;

            }

            //startPointFlag
        }

        private void draw(object sender, MouseButtonEventArgs e, int dotType)
        {
            if (drawArrow)
            {
                double startX = currentDotX;
                double startY = currentDotY;

                double endX = Canvas.GetLeft(sender as System.Windows.Shapes.Rectangle) + size / 2;
                double endY = Canvas.GetTop(sender as System.Windows.Shapes.Rectangle) + size / 2;

                bool isInsert = false; 

                LineGeometry lineGeometry = new LineGeometry(new System.Windows.Point(startX, startY), new System.Windows.Point(endX, endY));

                foreach(System.Windows.Shapes.Rectangle rectangle in rectangles)
                {
                    RectangleGeometry rectangleGeometry = new RectangleGeometry(new Rect(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle), rectangle.Width, rectangle.Height));
                    isInsert = lineGeometry.FillContainsWithDetail(rectangleGeometry) != IntersectionDetail.Empty;
                    if (isInsert)
                    {
                        break;
                    }
                }

                if (!isInsert)
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Height = 5;
                    ellipse.Width = 5;
                    ellipse.Fill = new SolidColorBrush(Colors.Red);
                    Canvas.SetLeft(ellipse, endX);
                    Canvas.SetTop(ellipse, endY);
                    ((sender as System.Windows.Shapes.Rectangle).Parent as Canvas).Children.Add(ellipse);

                    line.Fill = new SolidColorBrush(Colors.Red);
                    line.Visibility = System.Windows.Visibility.Visible;
                    line.StrokeThickness = 4;
                    line.Stroke = System.Windows.Media.Brushes.Red;
                    line.X1 = currentDotX;
                    line.Y1 = currentDotY;
                    line.X2 = endX;
                    line.Y2 = endY;
                    CanvasMain.Children.Add(line);
                    line = new Line();

                    currentDotX = endX;
                    currentDotY = endY;

                    layerPoints.Add(new System.Windows.Point((double)((int)Math.Floor((e.GetPosition(CanvasMain).X / size)) + 0.5),
                                                                    (double)((int)Math.Floor(e.GetPosition(CanvasMain).Y / size) + 0.5)));
                }
            }
            else
            {
                drawArrow = true;

                currentDotX = Canvas.GetLeft(sender as System.Windows.Shapes.Rectangle) + size / 2;
                currentDotY = Canvas.GetTop(sender as System.Windows.Shapes.Rectangle) + size / 2;

                Ellipse ellipse = new Ellipse();
                ellipse.Height = 5;
                ellipse.Width = 5;
                ellipse.Fill = new SolidColorBrush(Colors.Red);
                Canvas.SetLeft(ellipse, currentDotX);
                Canvas.SetTop(ellipse, currentDotY);
                ((sender as System.Windows.Shapes.Rectangle).Parent as Canvas).Children.Add(ellipse);

                layerPoints.Add(new System.Windows.Point((double)((int)Math.Floor((e.GetPosition(CanvasMain).X / size)) + 0.5),
                                                                (double)((int)Math.Floor(e.GetPosition(CanvasMain).Y / size) + 0.5)));
            }
        }

        private void checkLineForDots()
        {

        }

        private void openMenu(object sender, RoutedEventArgs e)
        {

            DoubleAnimation anim = new DoubleAnimation();
            if (RightMenu.Width > 0)
            {
                anim.From = 250;
                anim.To = 0;
                anim.Duration = TimeSpan.FromSeconds(0.2);
                RightMenu.BeginAnimation(WidthProperty, anim);
                return;
            }

            anim.From = 0;
            anim.To = 250;
            anim.Duration = TimeSpan.FromSeconds(0.2);
            RightMenu.BeginAnimation(WidthProperty, anim);
        }

        private void Canvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (drawArrow)
            {
                line.Fill = new SolidColorBrush(Colors.Red);
                line.Visibility = System.Windows.Visibility.Visible;
                line.StrokeThickness = 4;
                line.Stroke = System.Windows.Media.Brushes.Red;
                line.X1 = currentDotX;
                line.Y1 = currentDotY;
                line.X2 = e.GetPosition(CanvasMain).X;
                line.Y2 = e.GetPosition(CanvasMain).Y;
            }
        }

        private void Canvas_(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            ProjectSettings.preset.savePreset();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Layer layer = new Layer();
            layer.layerThread = layerPoints;
            
            if ((bool)!manyCheck.IsChecked)
            {
                GCodeGenerator.generate(new List<Layer> { layer });
            }
            else
            {
                List<Layer> layers = new List<Layer>();
                for (int i = 0; i < int.Parse(layerZ_Count.Text); i++)
                {
                    Layer currentLayer = new Layer();
                    currentLayer.layerThread = layerPoints;

                    currentLayer.heightLayer = int.Parse(layerZ.Text) * i;

                    layers.Add(currentLayer);
                }
                GCodeGenerator.generate(layers);
            }
        }

        bool AreLinesIntersecting(System.Windows.Point l1p1, 
            System.Windows.Point l1p2, 
            System.Windows.Point l2p1, 
            System.Windows.Point l2p2) 
        { 
            double q1 = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y); 
            double q2 = (l1p2.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p2.X - l2p1.X) * (l2p2.Y - l2p1.Y);
            double q3 = (l2p1.Y - l1p1.Y) * (l1p2.X - l1p1.X) - (l2p1.X - l1p1.X) * (l1p2.Y - l1p1.Y); 
            double q4 = (l2p2.Y - l1p1.Y) * (l1p2.X - l1p1.X) - (l2p2.X - l1p1.X) * (l1p2.Y - l1p1.Y); 
            
            return (q1 * q2 < 0) && (q3 * q4 < 0); 
        }

        private void manyCheck_Checked(object sender, RoutedEventArgs e)
        {
            layerZ.IsEnabled = layerZ_Count.IsEnabled = true;
        }
        private void manyCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            layerZ.IsEnabled = layerZ_Count.IsEnabled = false;
        }
        private void CanvasMain_Loaded(object sender, RoutedEventArgs e)
        {
            size = (double)(CanvasMain.ActualWidth / ProjectSettings.preset.topology.map.GetLength(0));
            MessageBox.Show(ProjectSettings.preset.topology.map.Length.ToString());

            for (int i = 0; i < ProjectSettings.preset.topology.map.GetUpperBound(1) + 1; i++)
            {
                for (int j = 0; j < ProjectSettings.preset.topology.map.GetUpperBound(0) + 1; j++)
                {
                    System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                    if (ProjectSettings.preset.topology.map[j, i] == 1)
                    {
                        rectangle.Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFBE98")) ;
                    }
                    else if (ProjectSettings.preset.topology.map[j, i] == 2)
                    {
                        rectangle.Fill = new SolidColorBrush(Colors.Green);
                    }
                    else if (ProjectSettings.preset.topology.map[j, i] == 3)
                    {
                        rectangle.Fill = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        rectangle.Fill = new SolidColorBrush(Colors.White);
                    }
                    rectangle.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
                    rectangle.Height = size;
                    rectangle.Width = size;
                    rectangle.Stroke = new SolidColorBrush(Colors.Black);
                    rectangle.StrokeThickness = 1;

                    if(ProjectSettings.preset.topology.map[j, i] == 3)
                    {
                        rectangles.Add(rectangle);
                    }

                    Canvas.SetTop(rectangle, size * j);
                    Canvas.SetLeft(rectangle, size * i);
                    CanvasMain.Children.Add(rectangle);
                }
            }
        }
    }
}
