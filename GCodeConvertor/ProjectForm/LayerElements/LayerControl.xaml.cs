using GCodeConvertor.UI;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GCodeConvertor.ProjectForm.LayerElements
{
    /// <summary>
    /// Логика взаимодействия для LayerControl.xaml
    /// </summary>
    public partial class LayerControl : UserControl
    {

        WorkspaceDrawingControl workspaceDrawingControl;
        ObservableCollection<LayerItem> layerItems;

        private bool _isDragging;
        private object _draggedItem;

        public LayerControl(WorkspaceDrawingControl workspaceDrawingControl)
        {
            InitializeComponent();

            this.workspaceDrawingControl = workspaceDrawingControl;
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            initLayerItems();
        }

        private void initLayerItems()
        {

            if(layerListBox.SelectedItem == null)
            {
                layerItems = new ObservableCollection<LayerItem>();

                if (ProjectSettings.preset.layers.Count == 0)
                {
                    Layer layer = new Layer();
                    ProjectSettings.preset.addLayer(layer);
                    LayerItem layerItem = new LayerItem(layer);
                    layerItems.Add(layerItem);
                }
                else
                {
                    foreach (Layer layer in ProjectSettings.preset.layers)
                    {
                        LayerItem layerItem = new LayerItem(layer);
                        layerItems.Add(layerItem);
                    }
                }

                layerListBox.ItemsSource = layerItems;
                layerListBox.SelectedItem = layerItems.First();
            }
            
        }

        private void layerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LayerItem selectedLayerItem = layerListBox.SelectedItem as LayerItem;
            if (selectedLayerItem != null)
            {
                workspaceDrawingControl.setActiveLayer(selectedLayerItem.layer);
            }
        }

        private void deleteLayer(object sender, RoutedEventArgs e)
        {
            if (layerItems.Count == 1)
            {
                MessageWindow messageWindow = new MessageWindow("Невозможно удалить слой!", "Текущий слой является единственным.");
                messageWindow.ShowDialog();
                //MessageBox.Show("Это единственный слой. Создайте еще один, чтобы удалить текущий.");
                return;
            }

            LayerItem selectedLayerItem = layerListBox.SelectedItem as LayerItem;
            ProjectSettings.preset.removeLayer(selectedLayerItem.layer);
            //ProjectSettings.preset.removeLayer(selectedLayerItem.layer);
            layerItems.Remove(selectedLayerItem);
            layerListBox.SelectedItem = layerItems.First();
        }

        private void createLayer(object sender, RoutedEventArgs e)
        {
            LayerItem selectedLayerItem = layerListBox.SelectedItem as LayerItem;

            if (!selectedLayerItem.layer.isEnded())
            {
                MessageWindow messageWindow = new MessageWindow("Невозможно создать новый слой!", "Закончите текущий слой прежде, чем создать новый слой.");
                messageWindow.ShowDialog();
                //MessageBox.Show("Текущий слой не закончен");
                return;
            }

            Layer layer = new Layer();

            ProjectSettings.preset.insertLayer(0, layer);

            LayerItem layerItem = new LayerItem(layer);
            layerItems.Insert(0, layerItem);
            layerListBox.SelectedItem = layerItems.First();
        }

        private void moveLayerUp(object sender, RoutedEventArgs e)
        {
            LayerItem selectedLayerItem = layerListBox.SelectedItem as LayerItem;
            Layer selectedLayer = selectedLayerItem.layer;

            int indexOfCurrentLayer = layerItems.IndexOf(selectedLayerItem);
            int newIndexOfLayer = indexOfCurrentLayer - 1;

            if (indexOfCurrentLayer > 0)
            {
                layerItems.Remove(selectedLayerItem);
                layerItems.Insert(newIndexOfLayer, selectedLayerItem);

                ProjectSettings.preset.layers.Remove(selectedLayer);
                ProjectSettings.preset.layers.Insert(newIndexOfLayer, selectedLayer);

                layerListBox.SelectedItem = layerItems[newIndexOfLayer];
            }
        }

        private void moveLayerDown(object sender, RoutedEventArgs e)
        {
            LayerItem selectedLayerItem = layerListBox.SelectedItem as LayerItem;
            Layer selectedLayer = selectedLayerItem.layer;

            int indexOfCurrentLayer = layerItems.IndexOf(selectedLayerItem);
            int newIndexOfLayer = indexOfCurrentLayer + 1;

            if (indexOfCurrentLayer < layerItems.Count - 1)
            {
                layerItems.Remove(selectedLayerItem);
                layerItems.Insert(newIndexOfLayer, selectedLayerItem);

                ProjectSettings.preset.layers.Remove(selectedLayer);
                ProjectSettings.preset.layers.Insert(newIndexOfLayer, selectedLayer);

                layerListBox.SelectedItem = layerItems[newIndexOfLayer];
            }
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _draggedItem = (sender as ListBox).SelectedItem;
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedItem != null)
            {
                _isDragging = true;
                DragDrop.DoDragDrop(layerListBox, _draggedItem, DragDropEffects.Move);
            }
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _draggedItem = null;
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (!_isDragging || !e.Data.GetDataPresent(typeof(LayerItem)) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (!_isDragging || !e.Data.GetDataPresent(typeof(LayerItem)) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (_isDragging && e.Data.GetDataPresent(typeof(LayerItem)))
            {
                var droppedItem = (LayerItem)e.Data.GetData(typeof(LayerItem));
                var targetListBox = sender as ListBox;

                if (droppedItem != null && targetListBox != null)
                {
                    layerItems.Remove(droppedItem);
                    layerItems.Insert(targetListBox.SelectedIndex, droppedItem);
                }
            }
        }

    }
}
