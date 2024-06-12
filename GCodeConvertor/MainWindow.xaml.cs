using GCodeConvertor;
using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Point = System.Windows.Point;

namespace GCodeConvertor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ICreateType createType;
        TopologyModel topologyModel;

        public MainWindow()
        {
            InitializeComponent();
            setCreateType(new CreateTypeA());
            topologyModel = new TopologyModel();
            this.DataContext = topologyModel;

            Shape shape = new Shape(new List<Point>() { new Point(0,0), new Point(100, 0), new Point(100, 100), new Point(0, 100), new Point(0, 0) });
            shape.saveShape();

            TensionLines tensionLines = new TensionLines(new List<List<Point>>() { new List<Point>() { new Point(20,0), new Point(20, 100) }, new List<Point>() { new Point(0, 20), new Point(100, 20) } });
            tensionLines.saveTensionLines();
        }

        private void CreateProjectA(object sender, RoutedEventArgs e)
        {
            TopologyByLineModel topologyByLine = new TopologyByLineModel();
            topologyByLine.NameProject = "test";
            topologyByLine.Step = 2;
            topologyByLine.TensionLines = "C:\\Users\\radev\\Documents\\TensionLines.xml";
            topologyByLine.Shape = "C:\\Users\\radev\\Documents\\shape.xml";
            topologyByLine.Accuracy = 1;
            topologyByLine.HeadIdentationX = 5;
            topologyByLine.HeadIdentationY = 5;
            topologyByLine.NozzleDiameter = 1;
            topologyByLine.PathProject = "C:\\Users\\radev\\Documents\\";

            GlobalPreset globalPreset = new GlobalPreset(topologyByLine);
            ProjectSettings.preset = globalPreset;

            ProjectWindow pw = new ProjectWindow();
            pw.Show();
            this.Close();
            //createType.create(topologyModel, this);
        }

        void setCreateType(ICreateType createType)
        {
            this.createType = createType;
        }

        private void OpenProject(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "GCD files(*.gcd)|*.gcd",
                
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string pathToPreset = openFileDialog.FileName;
                ProjectSettings.preset.loadPreset(openFileDialog.FileName);
                ProjectWindow pw = new ProjectWindow();
                pw.Show();
                this.Close();
            }
        }

        private void SetDefaultFolder(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                topologyModel.PathProject = dialog.SelectedPath;
            }
        }
    }

    interface ICreateType
    {
        void create(TopologyModel topologyModel, MainWindow window);

        //void setDefaultValues(TopologyModel topologyModel);
    }
    //Тест
    class CreateTypeA : ICreateType
    {
        public void create(TopologyModel topologyModel, MainWindow window)
        {
            if (topologyModel.Errors.Count == 0)
            {
                ProjectSettings.preset = new GlobalPreset(topologyModel);
                ProjectWindow pw = new ProjectWindow();
                pw.Show();
                window.Close();
            }
            else
            {
                //string errorMessage = topologyModel.Errors.Count;
                //foreach (string error in topologyModel.Errors.Values)
                //{
                //    errorMessage += error + "\n";
                //}

                MessageBox.Show("Причина ошибки: \n" + topologyModel.error + "\nСуммарное количество неверно заполненных полей - " + topologyModel.Errors.Count, "Ошибка!",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

    }

    class CreateTypeB : ICreateType
    {
        public void create(TopologyModel topologyModel, MainWindow window)
        {

        }

    }
}
