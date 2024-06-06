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

        private ITopologable topologable;

        public CreateProjectForm(OpenProjectForm openProjectForm)
        {
            this.openProjectForm = openProjectForm;
            InitializeComponent();
            projectTypeItems = new ObservableCollection<ProjectTypeItem>();
            ProjectTypeListBox.ItemsSource = projectTypeItems;
            DataContext = this;

            projectTypeItems.Add(new ProjectTypeItem("Тип 1"));
            projectTypeItems.Add(new ProjectTypeItem("Тип 2"));

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
                    return currentItem.ProjectType.Contains(projectTypeSearchBlock.Text, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            };
            view.Refresh();
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
            openProjectForm.Visibility = Visibility.Visible;
            this.Close();
        }
    }

    public class ProjectTypeItem
    {
        public string ProjectType { get; set; }

        public ProjectTypeItem(string projectType)
        {
            ProjectType = projectType;
        }

    }

}
