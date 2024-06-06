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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GCodeConvertor.UI
{
    /// <summary>
    /// Логика взаимодействия для MeshThreadPlacementPanel.xaml
    /// </summary>
    public partial class MeshThreadPlacementPanel : UserControl, ITopologable
    {
        TopologyModel topologyModel;

        public MeshThreadPlacementPanel()
        {
            InitializeComponent();
            topologyModel = new TopologyModel();
            DataContext = topologyModel;
        }

        public bool isDataCorrect()
        {
            return topologyModel.Errors.Count == 0;
        }

        public void setTopology()
        {
            throw new NotImplementedException();
        }
    }
}
