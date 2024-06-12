using GCodeConvertor.ProjectForm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace GCodeConvertor.UI
{
    /// <summary>
    /// Логика взаимодействия для CreateProjectForm.xaml
    /// </summary>
    public partial class CreateProjectForm : Window
    {
        private OpenProjectForm openProjectForm;

        public ObservableCollection<ProjectTypeItem> projectTypeItems;

        public CreateProjectForm(OpenProjectForm openProjectForm)
        {
            this.openProjectForm = openProjectForm;
            InitializeComponent();
            projectTypeItems = new ObservableCollection<ProjectTypeItem>();
            ProjectTypeListBox.ItemsSource = projectTypeItems;

            ITopologable meshTopologable = new MeshThreadPlacementPanel();
            projectTypeItems.Add(new ProjectTypeItem(meshTopologable));
            ITopologable linesTopologable = new TensionLinesPlacementPanel();
            projectTypeItems.Add(new ProjectTypeItem(linesTopologable));

            ProjectTypeListBox.SelectedIndex = 0;
        }

        private void projectTypeSearch(object sender, TextChangedEventArgs e)
        {
            if (projectTypeSearchBlock.Text.Length == 0)
            {
                projectTypeSearchCue.Visibility = Visibility.Visible;
            }
            else
            {
                projectTypeSearchCue.Visibility = Visibility.Collapsed;
            }
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ProjectTypeListBox.ItemsSource);
            if (projectTypeSearchBlock.Text.Length == 0)
            {
                view.Filter = null;
            }
            view.Filter = item =>
            {
                if (item is ProjectTypeItem currentItem)
                {
                    return currentItem.topologable.getName().Contains(projectTypeSearchBlock.Text, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            };
            view.Refresh();
        }

        private void ChooseProjectType(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ProjectTypeItem projectType = e.AddedItems[0] as ProjectTypeItem;
                if (projectType != null)
                {
                    Border element = (Border)this.FindName("projecTypeContainer");
                    element.Child = (FrameworkElement)projectType.topologable;
                }
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            openProjectForm.Close();
            this.Close();
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

        private void CancelCreating(object sender, RoutedEventArgs e)
        {
            openProjectForm.setVisible();
            this.Close();
        }

        private void CreateProject(object sender, RoutedEventArgs e)
        {
            Border element = (Border)this.FindName("projecTypeContainer");
            ITopologable topologable = (ITopologable)element.Child;
            if (topologable.isDataCorrect())
            {
                topologable.setTopology();
                ProjectSettings.preset.savePreset();
                openProjectForm.projectInfo.addProjectInfo(new ProjectsInfo.ProjectsInfoItem(topologable.getProjectName(), topologable.getProjectFullPath()));
                ProjectWindow pw = new ProjectWindow(openProjectForm);
                pw.Show();
                this.Close();
            }
        }
    }

    public class ProjectTypeItem
    {
        public string name { get; set; }
        public ITopologable topologable { get; set; }

        public ProjectTypeItem(ITopologable topologable)
        {
            this.topologable = topologable;
            name = topologable.getName();
        }

    }

}
