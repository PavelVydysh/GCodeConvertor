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
    /// Логика взаимодействия для TensionLinesPlacementPanel.xaml
    /// </summary>
    public partial class TensionLinesPlacementPanel : UserControl, ITopologable
    {
        private static string DEFAULT_NAME = "Линии напряжения";

        private string name;

        public TensionLinesPlacementPanel()
        {
            InitializeComponent();
            name = DEFAULT_NAME;
        }

        public string getFullPath()
        {
            throw new NotImplementedException();
        }

        public string getName()
        {
            return name;
        }

        public string getProjectFullPath()
        {
            throw new NotImplementedException();
        }

        public string getProjectName()
        {
            throw new NotImplementedException();
        }

        public bool isDataCorrect()
        {
            throw new NotImplementedException();
        }

        public void setTopology()
        {
            throw new NotImplementedException();
        }
    }
}
