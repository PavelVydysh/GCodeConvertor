using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GCodeConvertor.UI
{
    /// <summary>
    /// Логика взаимодействия для MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public string title { get; set; }
        public string body { get; set; }
        public string okButtonText { get; set; }
        public string cancelButtonText { get; set; }
        public bool resultMessageClick { get; set; }


        public MessageWindow(string title, string body)
        {
            InitializeComponent();

            this.title = title;
            this.body = body;
            this.okButtonText = "Ок";

            CancelButton.Visibility = Visibility.Collapsed;

            DataContext = this;
        }

        public MessageWindow(string title, string body, string okButtonText, string cancelButtonText)
        {
            InitializeComponent();

            this.title = title;
            this.body = body;
            this.cancelButtonText = cancelButtonText;
            this.okButtonText = okButtonText;

            DataContext = this;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            resultMessageClick = false;
            Close();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            resultMessageClick = true;
            Close();
        }
    }
}
