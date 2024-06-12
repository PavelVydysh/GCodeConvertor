using GCodeConvertor.ProjectForm;
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

namespace GCodeConvertor.UI
{
    /// <summary>
    /// Логика взаимодействия для MeshThreadPlacementPanel.xaml
    /// </summary>
    public partial class MeshThreadPlacementPanel : UserControl, ITopologable
    {
        private static string DEFAULT_NAME = "Сетка";

        private string name;
        private TopologyModel topologyModel { get; set; }


        public MeshThreadPlacementPanel()
        {
            InitializeComponent();
            topologyModel = new TopologyModel();
            name = DEFAULT_NAME;
            DataContext = topologyModel;
        }

        public bool isDataCorrect()
        {
            return topologyModel.Errors.Count == 0;
        }

        public void setTopology()
        {
            ProjectSettings.preset = new GlobalPreset(topologyModel);
        }

        public string getName()
        {
            return name;
        }

        public string getProjectFullPath()
        {
            return Path.Combine(topologyModel.PathProject, topologyModel.NameProject) + ".gcd";
        }

        public string getProjectName()
        {
            return topologyModel.NameProject;
        }

        private void ButtonDirectoryClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                topologyModel.PathProject = dialog.SelectedPath;
            }
        }
    }
}
