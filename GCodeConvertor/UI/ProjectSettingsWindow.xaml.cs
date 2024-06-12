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

namespace GCodeConvertor.UI
{
    /// <summary>
    /// Логика взаимодействия для ProjectSettings.xaml
    /// </summary>
    public partial class ProjectSettingsWindow : Window
    {
        private string currentConflictResolveType;
        private string currentRubberType;

        public ProjectSettingsWindow()
        {
            InitializeComponent();
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            saveCurrentSettings();
        }

        private void saveCurrentSettings()
        {
            Settings.Default.ConflictResolving = currentConflictResolveType;
            Settings.Default.RubberBand = currentRubberType;
            Settings.Default.Save();
        }

        private void ChangeCurrentConflictTypeResolver(string conflictTypeResolver)
        {
            foreach (ComboBoxItem item in ConflictComboBox.Items)
            {
                if (item.Tag.ToString() == conflictTypeResolver)
                {
                    ConflictComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void ChangeCurrentRubberBandStatus(string rubberBandStatus)
        {
            foreach (ComboBoxItem item in StringComboBox.Items)
            {
                if (item.Tag.ToString() == rubberBandStatus)
                {
                    StringComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void StringComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StringComboBox.SelectedItem != null)
            {
                currentRubberType = ((ComboBoxItem)StringComboBox.SelectedItem).Tag.ToString();
            }
        }

        private void ConflictComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConflictComboBox.SelectedItem != null)
            {
                currentConflictResolveType = ((ComboBoxItem)ConflictComboBox.SelectedItem).Tag.ToString();
            }
        }

        private void ConflictComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            currentConflictResolveType = Settings.Default.ConflictResolving;
            ChangeCurrentConflictTypeResolver(currentConflictResolveType);
        }

        private void StringComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            currentRubberType = Settings.Default.RubberBand;
            ChangeCurrentRubberBandStatus(currentRubberType);
        }

        private void CancelSettings(object sender, RoutedEventArgs e)
        {
            ChangeCurrentConflictTypeResolver(Settings.Default.ConflictResolving);
            ChangeCurrentRubberBandStatus(Settings.Default.RubberBand);
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if(currentConflictResolveType != Settings.Default.ConflictResolving || currentRubberType != Settings.Default.RubberBand)
            {
                MessageWindow messageWindow = new MessageWindow("Присутствуют несохраненные изменения!", "Сохранить выбранные настройки?", "Сохранить", "Не сохранять");
                messageWindow.ShowDialog();
                if (messageWindow.resultMessageClick)
                {
                    saveCurrentSettings();
                }
            }
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

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
        }
    }
}
