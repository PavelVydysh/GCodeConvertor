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
using System.Security.Policy;
using static System.Windows.Forms.LinkLabel;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using Point = System.Windows.Point;
using GCodeConvertor.WorkspaceInstruments;
using GCodeConvertor.ProjectForm.LayerElements;
using GCodeConvertor.Project3D;
using System.Windows.Forms.Design;
using GCodeConvertor.UI;
using GCodeConvertor.GScript;
using System.Windows.Controls.Primitives;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.IO;

namespace GCodeConvertor.ProjectForm
{
    /// <summary>
    /// Логика взаимодействия для ProjectWindow.xaml
    /// </summary>
    
    public partial class ProjectWindow : Window
    {
        private enum MenuState{
            OPEN,
            CLOSED
        }

        public WorkspaceDrawingControl wdc { get; set; }
        public OpenProjectForm openProjectForm;
        public ObservableCollection<InstrumentButtonInfo> workspaceInstruments { get; set; }
        private MenuState currentMenuState;
        public GScriptWindow GSWindow = null;
        public Project3dVisualizer project3DVisualizer = null;
        public string projectName { get; set; }

        List<Hotkey> hotkeys;
        HashSet<Key> pressedKeys;

        public ProjectWindow(OpenProjectForm openProjectForm)
        {
            this.openProjectForm = openProjectForm;

            InitializeComponent();

            wdc = new WorkspaceDrawingControl(ProjectSettings.preset.topology);
            projectName = ProjectSettings.preset.topology.name;

            DataContext = this;

            workspaceInstruments = new ObservableCollection<InstrumentButtonInfo>();

            IntrumentListBox.ItemsSource = workspaceInstruments;
            WorkspaceInstrument drawing = new DrawingWorkspaceInstrument(wdc);
            WorkspaceInstrument zooming = new ZoomingWorkspaceInstrument(wdc);
            WorkspaceInstrument moving = new MoveWorkspaceInstrument(wdc);
            InstrumentButtonInfo instrumentButtonInfoDrawing = new InstrumentButtonInfo("Кисть", drawing, "pack://application:,,,/Resources/brush_icon.png");
            InstrumentButtonInfo instrumentButtonInfoZooming = new InstrumentButtonInfo("Зум", zooming, "pack://application:,,,/Resources/zoom_icon.png");
            InstrumentButtonInfo instrumentButtonInfoMoving = new InstrumentButtonInfo("Движение", moving, "pack://application:,,,/Resources/move_icon.png");
            workspaceInstruments.Add(instrumentButtonInfoDrawing);
            workspaceInstruments.Add(instrumentButtonInfoZooming);
            workspaceInstruments.Add(instrumentButtonInfoMoving);

            IntrumentListBox.SelectedIndex = 0;

            WorkspaceContainer.Children.Add(wdc);

            LayerControl layerControl = new LayerControl(wdc);
            LayersContainer.Child = layerControl;
            LayersPopup.IsOpen = true;

            currentMenuState = MenuState.CLOSED;

            PreviewKeyUp += Window_KeyUp;
            PreviewKeyDown += Window_KeyDown;

            hotkeys = new List<Hotkey>();
            pressedKeys = new HashSet<Key>();

            List<Key> drawingHotkeys = new List<Key>();
            drawingHotkeys.Add(Key.LeftShift);
            drawingHotkeys.Add(Key.D);

            List<Key> zoomingHotkeys = new List<Key>();
            zoomingHotkeys.Add(Key.LeftShift);
            zoomingHotkeys.Add(Key.Z);

            List<Key> movingHotkeys = new List<Key>();
            movingHotkeys.Add(Key.LeftShift);
            movingHotkeys.Add(Key.M);

            hotkeys.Add(new Hotkey(drawingHotkeys, instrumentButtonInfoDrawing, this));
            hotkeys.Add(new Hotkey(zoomingHotkeys, instrumentButtonInfoZooming, this));
            hotkeys.Add(new Hotkey(movingHotkeys, instrumentButtonInfoMoving, this));
        }

        private void OpenScriptForm(object sender, RoutedEventArgs e)
        {
            GSWindow = new GScriptWindow(wdc, this);
            GSWindow.Show();
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

        private void ChooseInstrument(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                InstrumentButtonInfo instrument = e.AddedItems[0] as InstrumentButtonInfo;
                if (instrument != null)
                {
                    wdc.setActiveWorkspaceInstrument(instrument.workspaceInstrument);
                }
            }
        }

        public void setSelectedInstrument(int indexOfInstrument)
        {
            IntrumentListBox.SelectedIndex = indexOfInstrument;
        }

        private void ShowLayersPopup(object sender, RoutedEventArgs e)
        {
            SetupPopPlacement();

            if(LayersPopup.IsOpen && GCodePopup.IsOpen)
            {
                LayersPopup.IsOpen = false;
                GCodePopup.HorizontalOffset = WorkspaceContainer.ActualWidth - GCodePopup.Width;
                GCodePopup.VerticalOffset = WorkspaceContainer.ActualHeight - GCodePopup.Height;
            }else if(!LayersPopup.IsOpen && GCodePopup.IsOpen)
            {
                LayersPopup.IsOpen = true;
                SetupGcodePopPlacement();
            }
            else
            {
                LayersPopup.IsOpen = !LayersPopup.IsOpen;
            } 
        }

        private void OpenGcodePopup(object sender, RoutedEventArgs e)
        {
            SetupGcodePopPlacement();
            SetupPopPlacement();

            LayersPopup.IsOpen = LayersPopup.IsOpen || !GCodePopup.IsOpen;
            GCodePopup.IsOpen = !GCodePopup.IsOpen;
        }

        private void SetupGcodePopPlacement()
        {
            GCodePopup.PlacementTarget = WorkspaceContainer;
            GCodePopup.Placement = PlacementMode.Relative;
            GCodePopup.HorizontalOffset = WorkspaceContainer.ActualWidth - LayersPopup.Width - GCodePopup.Width;
            GCodePopup.VerticalOffset = WorkspaceContainer.ActualHeight - GCodePopup.Height;
        }

        private void SetupPopPlacement()
        {
            LayersPopup.PlacementTarget = WorkspaceContainer;
            LayersPopup.Placement = PlacementMode.Relative;
            LayersPopup.HorizontalOffset = WorkspaceContainer.ActualWidth - LayersPopup.Width;
            LayersPopup.VerticalOffset = WorkspaceContainer.ActualHeight - LayersPopup.Height;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            foreach (Hotkey hotkey in hotkeys)
            {
                hotkey.selectInstrument(pressedKeys.ToList());
            }

            pressedKeys.Remove(e.Key);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            pressedKeys.Add(e.Key);
        }

        private void WorkspaceContainerLoaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            LayersPopup.IsOpen = false;
            GCodePopup.IsOpen = false;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            //SetupPopPlacement();
            //LayersPopup.IsOpen = true;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            LayersPopup.IsOpen = false;
            GCodePopup.IsOpen = false;
        }

        private void ShowFileMenuPopUp(object sender, RoutedEventArgs e)
        {
            SetupFilePopPlacement();
            FilePopup.IsOpen = true;
        }

        private void SetupFilePopPlacement()
        {
            FilePopup.PlacementTarget = WorkspaceContainer;
            FilePopup.Placement = PlacementMode.Relative;
            FilePopup.HorizontalOffset = 5;
            FilePopup.VerticalOffset = 0;
        }

        private void OpenProjectSettings(object sender, RoutedEventArgs e)
        {
            ProjectSettingsWindow projectSettingsWindow = new ProjectSettingsWindow(this);
            projectSettingsWindow.ShowDialog();
        }

        private void CreateGCode(object sender, RoutedEventArgs e)
        {
            bool isAllEnded = true;

            foreach (Layer layer in ProjectSettings.preset.layers)
            {
                if (layer.isEnable)
                    isAllEnded &= layer.isEnded();
            }

            if (isAllEnded)
            {
                ProjectSettings.preset.layers.Reverse();
                List<Layer> layersToGenerate = new List<Layer>();
                foreach (Layer layer in ProjectSettings.preset.layers)
                {
                    if (layer.isEnable)
                        layersToGenerate.Add(layer);
                }

                List<Layer> tempList = new List<Layer>();
                for (int i = 0; i < int.Parse(layerFactor.Text); i++)
                {
                    tempList.AddRange(layersToGenerate);
                }
                layersToGenerate = tempList;

                if(layersToGenerate.Count == 0)
                {
                    MessageWindow messageWindow = new MessageWindow("Невозможно сгенерировать G-код!", "Не найдено активных слоев для формирования G-код.");
                    messageWindow.ShowDialog();
                    return;
                }

                GCodeGenerator.generate(layersToGenerate);
                ProjectSettings.preset.layers.Reverse();
            }
            else
            {
                MessageWindow messageWindow = new MessageWindow("Невозможно сгенерировать G-код!", "Закончите все активные слои для генерации G-код.");
                messageWindow.ShowDialog();
            }
        }

        private void CreateNewProject(object sender, RoutedEventArgs e)
        {
            this.Close();
            CreateProjectForm createProjectForm = new CreateProjectForm(openProjectForm);
            createProjectForm.Show();
        }

        private void OpenProjectExplorer(object sender, RoutedEventArgs e)
        {
            string folderPath = ProjectSettings.preset.topology.path; 

            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                MessageWindow messageWindow = new MessageWindow("Невозможно открыть путь в проводнике!", "Путь не найден или указан неверный путь.");
                messageWindow.ShowDialog();
            }
        }

        private void SaveProject(object sender, RoutedEventArgs e)
        {
            ProjectSettings.preset.savePreset();
        }

        private void CloseProject(object sender, RoutedEventArgs e)
        { 
            this.Close();
            openProjectForm.setVisible();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(GSWindow is not null)
            {
                GSWindow.Close();
            }
            if (project3DVisualizer is not null)
            {
                project3DVisualizer.Close();
            }
            checkSaving();
        }

        private void checkSaving()
        {
            if (!ProjectSettings.preset.checkIsActualSaved())
            {
                MessageWindow messageWindow = new MessageWindow("Присутствуют несохраненные изменения!", "Сохранить текущие изменения?", "Сохранить", "Не сохранять");
                messageWindow.ShowDialog();
                if (messageWindow.resultMessageClick)
                {
                    ProjectSettings.preset.savePreset();
                }
            }
        }

        private void FactorChanged(object sender, TextChangedEventArgs e)
        {
            int factorValue;
            if (int.TryParse(layerFactor.Text, out factorValue))
            {
                if (factorValue > 0)
                {
                    return;
                }
            }
            layerFactor.Text = "1";
        }

        private void Open3DWindow(object sender, RoutedEventArgs e)
        {
            project3DVisualizer = new Project3dVisualizer(this, layerFactor.Text);
            project3DVisualizer.ShowDialog();
        }
    }

    public class InstrumentButtonInfo
    {
        public string name { get; set; }
        public WorkspaceInstrument workspaceInstrument { get; set; }
        public string pathToIcon { get; set; }

        public InstrumentButtonInfo(string name, WorkspaceInstrument workspaceInstrument, string pathToIcon)
        {
            this.name = name;
            this.workspaceInstrument = workspaceInstrument;
            this.pathToIcon = pathToIcon;
        }
    }

}
