using System;
using System.IO;
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
using Path = System.IO.Path;
using static GCodeConvertor.UI.ProjectsInfo;
using Microsoft.Win32;

namespace GCodeConvertor.UI
{
    /// <summary>
    /// Логика взаимодействия для TensionLinesPlacementPanel.xaml
    /// </summary>
    public partial class TensionLinesPlacementPanel : UserControl, ITopologable
    {
        private static string DEFAULT_NAME = "Линии напряжения";

        private string name;

        private TopologyByLineModel topologyByLineModel { get; set; }

        public TensionLinesPlacementPanel()
        {
            InitializeComponent();
            name = DEFAULT_NAME;
            topologyByLineModel = new TopologyByLineModel();
            DataContext = topologyByLineModel;
        }

        public string getName()
        {
            return name;
        }

        public string getProjectFullPath()
        {
            return Path.Combine(topologyByLineModel.PathProject, topologyByLineModel.NameProject) + ".gcd";
        }

        public string getProjectName()
        {
            return topologyByLineModel.NameProject;
        }

        public bool isDataCorrect()
        {
            return topologyByLineModel.Errors.Count == 0;
        }

        public void setTopology()
        {
            GlobalPreset globalPreset = new GlobalPreset(topologyByLineModel);
            ProjectSettings.preset = globalPreset;
        }

        private void ButtonDirectoryClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                topologyByLineModel.PathProject = dialog.SelectedPath;
            }
        }

        private void ButtonShapeClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML files(*.xml)|*.xml",

            };
            if (openFileDialog.ShowDialog() == true)
            {
                topologyByLineModel.PathShape = openFileDialog.FileName;
            }
        }

        private void ButtonTensionLinesClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML files(*.xml)|*.xml",

            };
            if (openFileDialog.ShowDialog() == true)
            {
                topologyByLineModel.PathTensionLines = openFileDialog.FileName;
            }
        }
    }
}
