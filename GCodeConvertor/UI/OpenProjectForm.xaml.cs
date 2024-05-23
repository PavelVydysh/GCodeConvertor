using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
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
    /// Логика взаимодействия для OpenProjectForm.xaml
    /// </summary>
    public partial class OpenProjectForm : Window
    {
        public ObservableCollection<ProjectItem> ProjectItems = new ObservableCollection<ProjectItem>();

        public OpenProjectForm()
        {
            InitializeComponent();

            this.MinWidth = 800;
            this.MinHeight = 600;

            ProjectItemListBox.ItemsSource = ProjectItems;

            DataContext = this;

            ProjectItems.Add(new ProjectItem { ProjectName = "project_one", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });
            ProjectItems.Add(new ProjectItem { ProjectName = "project_2", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });
            ProjectItems.Add(new ProjectItem { ProjectName = "project_3e", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });
            ProjectItems.Add(new ProjectItem { ProjectName = "534", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });
            ProjectItems.Add(new ProjectItem { ProjectName = "4", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });
            ProjectItems.Add(new ProjectItem { ProjectName = "project_one", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });

            ProjectItems.Add(new ProjectItem { ProjectName = "6", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });
            ProjectItems.Add(new ProjectItem { ProjectName = "project_one", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });
            ProjectItems.Add(new ProjectItem { ProjectName = "5", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });

            ProjectItems.Add(new ProjectItem { ProjectName = "124124", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });
            ProjectItems.Add(new ProjectItem { ProjectName = "345346", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });
            ProjectItems.Add(new ProjectItem { ProjectName = "project_one", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });

            ProjectItems.Add(new ProjectItem { ProjectName = "1", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });
            ProjectItems.Add(new ProjectItem { ProjectName = "12421412", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });
            ProjectItems.Add(new ProjectItem { ProjectName = "124", ProjectPath = "D:/fjkaks", ModifiedDate = "10.01.2024" });


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

        private void projectSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox?.Text.Length == 0)
            {
                projectSearchCue.Visibility = Visibility.Visible;
            }
            else
            {
                projectSearchCue.Visibility = Visibility.Collapsed;
            }
        }
    }

    public class ProjectItem
    {

        public string ProjectName { get; set; }
        public string ProjectPath { get; set; }
        public string ModifiedDate { get; set; }
       
    }
}
