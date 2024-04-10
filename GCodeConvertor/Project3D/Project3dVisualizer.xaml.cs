using HelixToolkit.Wpf;
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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static System.Windows.Forms.LinkLabel;

namespace GCodeConvertor.Project3D
{
    /// <summary>
    /// Логика взаимодействия для _3dVisualizer.xaml
    /// </summary>
    public partial class Project3dVisualizer : Window
    {
        public Project3dVisualizer()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            List<Layer> layers = ProjectSettings.preset.layers;

            double height = 0;

            List<Point3D> points = new List<Point3D>();

            foreach (Layer layer in layers)
            {
                height += layer.height;

                for (int pointIndex = 0; pointIndex <= layer.thread.Count - 1; pointIndex++)
                {
                    points.Add(new Point3D(layer.thread[pointIndex].X, layer.thread[pointIndex].Y, height));
                }
            }

            int lastPointIndex = points.Count - 1;

            for (int pointIndex = 0; pointIndex <= lastPointIndex; pointIndex++)
            {
                if (pointIndex != lastPointIndex)
                {
                    LinesVisual3D line = new LinesVisual3D();
                    line.Color = Colors.Yellow;
                    line.Thickness = 5;
                    line.Points.Add(points[pointIndex]);
                    line.Points.Add(points[pointIndex + 1]);

                    Viewport3D.Children.Add(line);
                }

                SphereVisual3D point = new SphereVisual3D();
                point.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Purple));
                point.Radius = 0.5;
                point.Center = points[pointIndex];

                Viewport3D.Children.Add(point);
            }
        }

    }
}
