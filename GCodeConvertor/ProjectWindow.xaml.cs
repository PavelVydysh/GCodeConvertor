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

namespace GCodeConvertor
{
    /// <summary>
    /// Логика взаимодействия для ProjectWindow.xaml
    /// </summary>
    public partial class ProjectWindow : Window
    {
        public ProjectWindow()
        {
            InitializeComponent();
            DataContext = ProjectSettings.preset;
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
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                CanvasMain.Children.Add(
                    new Line
                    {
                        X1 = e.GetPosition(CanvasMain).X,
                        Y1 = e.GetPosition(CanvasMain).Y,
                        X2 = e.GetPosition(CanvasMain).X + 1,
                        Y2 = e.GetPosition(CanvasMain).Y + 1,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round,
                        StrokeThickness = 1,
                        Stroke = System.Windows.Media.Brushes.Black
                    });
            }
        }

        private void Canvas_(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //int[,] array = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
            //Topology topology = new Topology();
            //topology.name = "Test";
            //topology.accuracy = 1;
            //topology.map = array;

            //Layer layer = new Layer();
            //layer.layerThread = new List<System.Drawing.Point> { new System.Drawing.Point(2, 3), new System.Drawing.Point(12, 18), new System.Drawing.Point(20, 30) };
            //layer.heightLayer = 12;

            //Layer layer2 = new Layer();
            //layer2.layerThread = new List<System.Drawing.Point> { new System.Drawing.Point(5, 10), new System.Drawing.Point(10, 58), new System.Drawing.Point(64, 3) };
            //layer2.heightLayer = 24;

            //GlobalPreset globalPreset = new GlobalPreset(new List<Layer> { layer, layer2 }, topology);

            //globalPreset.savePreset();

            

            ProjectSettings.preset.savePreset();


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Layer layer = new Layer();
            layer.layerThread = new List<System.Drawing.Point> { new System.Drawing.Point(2, 3), new System.Drawing.Point(12, 18), new System.Drawing.Point(20, 30) };
            layer.heightLayer = 12;

            Layer layer2 = new Layer();
            layer2.layerThread = new List<System.Drawing.Point> { new System.Drawing.Point(5, 10), new System.Drawing.Point(10, 58), new System.Drawing.Point(64, 3) };
            layer2.heightLayer = 24;

            GCodeGenerator.generate(new List<Layer> { layer, layer2 });
        }
    }
}
