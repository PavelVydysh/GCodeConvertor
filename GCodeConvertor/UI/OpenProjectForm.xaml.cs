﻿using GCodeConvertor.ProjectForm;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static GCodeConvertor.UI.ProjectsInfo;

namespace GCodeConvertor.UI
{
    /// <summary>
    /// Логика взаимодействия для OpenProjectForm.xaml
    /// </summary>
    public partial class OpenProjectForm : Window
    {
        public ObservableCollection<ProjectItem> projectItems;

        public ProjectsInfo projectInfo { get; set; }

        public OpenProjectForm()
        {
            InitializeComponent();
            setupFormProperties();

            loadProjectsInfo();
            applySorting();
        }

        private void loadProjectsInfo()
        {
            projectItems.Clear();
            projectInfo = ProjectLoader.loadProjectsInfo();
            foreach (ProjectsInfoItem projectInfo in projectInfo.ProjectsInfos)
            {
                bool projectExists = checkIfProjectExists(projectInfo.PathToProject);

                projectItems.Add(new ProjectItem
                {
                    ProjectName = projectInfo.ProjectName,
                    ProjectPath = projectInfo.PathToProject,
                    ModifiedDate = projectExists ? getModifiedFileDate(projectInfo.PathToProject) : "",
                    IsAccessable = projectExists
                });
            }
        }

        public void setVisible()
        {
            this.Visibility = Visibility.Visible;
            loadProjectsInfo();
        }

        private bool checkIfProjectExists(string pathToProject)
        {
            return File.Exists(pathToProject);
        }

        private string getModifiedFileDate(string pathToProject)
        {
            FileInfo projectFileInfo = new FileInfo(pathToProject);
            return projectFileInfo.LastAccessTime.ToShortDateString() + " " + projectFileInfo.LastAccessTime.ToShortTimeString();
        }

        private void setupFormProperties()
        {
            this.MinWidth = 800;
            this.MinHeight = 600;

            projectItems = new ObservableCollection<ProjectItem>();
            ProjectItemListBox.ItemsSource = projectItems;
            DataContext = this;
        }

        private void applySorting()
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ProjectItemListBox.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("ModifiedDate", ListSortDirection.Descending));
        }

        private void projectSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (projectSearch.Text.Length == 0)
            {
                projectSearchCue.Visibility = Visibility.Visible;
            }
            else
            {
                projectSearchCue.Visibility = Visibility.Collapsed;
            }
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ProjectItemListBox.ItemsSource);
            if (projectSearch.Text.Length == 0)
            {
                view.Filter = null;
            }
            view.Filter = item =>
            {
                if (item is ProjectItem currentItem)
                {
                    return currentItem.ProjectName.Contains(projectSearch.Text, StringComparison.OrdinalIgnoreCase)
                        || currentItem.ProjectPath.Contains(projectSearch.Text, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            };
            view.Refresh();
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

        private void OpenProject(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "GCD files(*.gcd)|*.gcd",

            };

            if (openFileDialog.ShowDialog() == true)
            {
                string pathToPreset = openFileDialog.FileName;
                projectInfo.addProjectInfo(new ProjectsInfoItem(Path.GetFileNameWithoutExtension(pathToPreset), pathToPreset));
                ProjectLoader.saveProjectsInfo(projectInfo);
                loadProjectsInfo();
                openProject(pathToPreset);
            }
        }

        private void CreateNewProject(object sender, RoutedEventArgs e)
        {
            CreateProjectForm createProjectForm = new CreateProjectForm(this);
            createProjectForm.Show();
            this.Visibility = Visibility.Collapsed;
        }

        private void openProject(string pathToProject)
        {
            ProjectSettings.preset.loadPreset(pathToProject);
            ProjectWindow pw = new ProjectWindow(this);
            Visibility = Visibility.Collapsed;
            pw.Show();
        }

        private void OpenTrackableProject(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ProjectItem trackableProject = e.AddedItems[0] as ProjectItem;
                if (trackableProject != null && trackableProject.IsAccessable)
                {
                    openProject(trackableProject.ProjectPath);
                }
            }
            ProjectItemListBox.SelectedItem = null;
        }

        private void OpenSettingsWindow(object sender, RoutedEventArgs e)
        {
            MainSettings mainSettings = new MainSettings(this);
            mainSettings.ShowDialog();
        }
    }

    public class ProjectItem
    {
        public string ProjectName { get; set; }
        public string ProjectPath { get; set; }
        public string ModifiedDate { get; set; }
        public bool IsAccessable { get; set; }

    }
}
