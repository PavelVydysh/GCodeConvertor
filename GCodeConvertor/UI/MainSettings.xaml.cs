﻿using System;
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

namespace GCodeConvertor.UI
{
    /// <summary>
    /// Логика взаимодействия для MainSettings.xaml
    /// </summary>
    public partial class MainSettings : Window
    {
        private OpenProjectForm openProjectForm;

        string startTheme;
        string currentTheme;

        public MainSettings(OpenProjectForm openProjectForm)
        {
            this.openProjectForm = openProjectForm;
            startTheme = Settings.Default.Theme;

            InitializeComponent();
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            saveSettings(currentTheme);
            startTheme = currentTheme;
            Close();
        }

        private void saveSettings(string themeName)
        {
            Settings.Default.Theme = themeName;
            Settings.Default.Save();
            ((App)Application.Current).ApplySavedTheme();
        }

        private void CancelSettings(object sender, RoutedEventArgs e)
        {
            ChangeCurrentTheme(startTheme);
        }

        private void ThemeComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            currentTheme = Settings.Default.Theme;
            ChangeCurrentTheme(currentTheme);
        }

        private void ChangeCurrentTheme(string themeName)
        {
            foreach (ComboBoxItem item in ThemeComboBox.Items)
            {
                if (item.Tag.ToString() == themeName)
                {
                    ThemeComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox.SelectedItem != null)
            {
                currentTheme = ((ComboBoxItem)ThemeComboBox.SelectedItem).Tag.ToString();
                saveSettings(currentTheme);
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
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

        private void Window_Closing(object sender, EventArgs e)
        {
            if (currentTheme != startTheme)
            {
                MessageWindow messageWindow = new MessageWindow("Присутствуют несохраненные изменения!", "Сохранить выбранные настройки?", "Сохранить", "Не сохранять");
                messageWindow.ShowDialog();
                if (messageWindow.resultMessageClick)
                {
                    saveSettings(currentTheme);
                }
                else
                {
                    saveSettings(startTheme);
                }
            }
        }
    }
}
