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
        List<Layer> layers;

        ObservableCollection<LayerItem> layerItems;

        public LayerControl(List<Layer> layers, WorkspaceDrawingControl workspaceDrawingControl)
        {
            InitializeComponent();

            this.workspaceDrawingControl = workspaceDrawingControl;
            this.layers = layers;
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            initLayerItems();
        }

        private void initLayerItems()
        {
            layerItems = new ObservableCollection<LayerItem>();

            if (layers.Count == 0)
            {
                Layer layer = new Layer();
                ProjectSettings.preset.addLayer(layer);
                LayerItem layerItem = new LayerItem(layer);
                layerItems.Add(layerItem);
            }
            else
            {
                foreach (Layer layer in layers)
                {
                    LayerItem layerItem = new LayerItem(layer);
                    layerItems.Add(layerItem);
                }
            }

            layerListBox.ItemsSource = layerItems;
            layerListBox.SelectedItem = layerItems.First();
        }

        private void layerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LayerItem selectedLayerItem = layerListBox.SelectedItem as LayerItem;
            if (selectedLayerItem != null)
            {
                workspaceDrawingControl.setActiveLayer(selectedLayerItem.layer);
            }
        }

        private void moveLayerUp(object sender, RoutedEventArgs e)
        {
            

        }

        private void deleteLayer(object sender, RoutedEventArgs e)
        {
            if(layerItems.Count == 1)
            {
                MessageBox.Show("Это единственный слой. Создайте еще один, чтобы удалить текущий.");
                return;
            }

            LayerItem selectedLayerItem = layerListBox.SelectedItem as LayerItem;
            ProjectSettings.preset.removeLayer(selectedLayerItem.layer);
            layerItems.Remove(selectedLayerItem);
            layerListBox.SelectedItem = layerItems.First();
        }

        private void createLayer(object sender, RoutedEventArgs e)
        {
            LayerItem selectedLayerItem = layerListBox.SelectedItem as LayerItem;

            if (!selectedLayerItem.layer.isEnded())
            {
                MessageBox.Show("Текущий слой не закончен");
                return;
            }

            Layer layer = new Layer();
            ProjectSettings.preset.addLayer(layer);
            LayerItem layerItem = new LayerItem(layer);
            layerItems.Insert(0, layerItem);
            layerListBox.SelectedItem = layerItems.First();
        }

        private void moveLayerDown(object sender, RoutedEventArgs e)
        {

        }
        
    }
}
