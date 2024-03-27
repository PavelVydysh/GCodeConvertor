using GCodeConvertor.WorkspaceInstruments;
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

namespace GCodeConvertor
{
    /// <summary>
    /// Логика взаимодействия для instrumentItem.xaml
    /// </summary>
    public partial class InstrumentItem : UserControl
    {
        public string elementName { get;}

        private WorkspaceInstrument workspaceInstrument { get; }

        private WorkspaceDrawingControl workspaceDrawingControl { get; }

        public InstrumentItem(
            string elementName, 
            WorkspaceInstrument workspaceInstrument, 
            WorkspaceDrawingControl workspaceDrawingControl)
        {
            InitializeComponent();

            DataContext = this;

            this.elementName = elementName;
            this.workspaceInstrument = workspaceInstrument;
            this.workspaceDrawingControl = workspaceDrawingControl;

            this.MouseLeftButtonDown += selectInstrument;
        }

        private void selectInstrument(object sender, MouseButtonEventArgs e)
        {
            workspaceDrawingControl.workspaceIntrument = workspaceInstrument;
        }

    }
}
