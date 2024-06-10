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

namespace GCodeConvertor.ProjectForm.LayerElements
{
    /// <summary>
    /// Логика взаимодействия для LayerItem.xaml
    /// </summary>
    public partial class LayerItem : UserControl
    {
        public Layer layer { get; set; }

        public LayerItem(Layer layer)
        {
            InitializeComponent();
            DataContext = this;

            this.layer = layer;
        }

        private void enableLayer_Checked(object sender, RoutedEventArgs e)
        {
            layer.isEnable = true;
        }

        private void enableLayer_Unchecked(object sender, RoutedEventArgs e)
        {
            layer.isEnable = false;
        }

        private void heightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            double heightValue;
            if (double.TryParse(heightTextBox.Text, out heightValue))
            {
                if(heightValue > 0)
                {
                    return;
                }
            }
            heightTextBox.Text = Layer.DEFAULT_HEIGHT.ToString();
        }

        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(nameTextBox.Text != string.Empty)
            {
                return;
            }
            nameTextBox.Text = Layer.DEFAULT_NAME.ToString();
        }
    }
}
