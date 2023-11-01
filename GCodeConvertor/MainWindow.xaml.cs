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

namespace GCodeConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ICreateType createType;
        TopologyModel topologyModel;

        public MainWindow()
        {
            InitializeComponent();
            setCreateType(new CreateTypeA());
            topologyModel = new TopologyModel();
            this.DataContext = topologyModel;
        }

        private void CreateProjectA(object sender, RoutedEventArgs e)
        {
            createType.create(topologyModel, this);
        }

        void setCreateType(ICreateType createType)
        {
            this.createType = createType;
        }
    }

    interface ICreateType
    {
        void create(TopologyModel topologyModel, MainWindow window);
    }

    class CreateTypeA : ICreateType
    {
        public void create(TopologyModel topologyModel, MainWindow window)
        {
            if (String.IsNullOrEmpty(topologyModel.error))
            {
                ProjectWindow pw = new ProjectWindow();
                pw.Show();
                window.Close();
            }
            else
            {
                MessageBox.Show("В настройках проекта указаны не все поля, либо допущены ошибки в оформлении полей.",
                                "[Code 0] Не удалось создать проект",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }

    class CreateTypeB : ICreateType
    {
        public void create(TopologyModel topologyModel, MainWindow window)
        {

        }
    }
}
