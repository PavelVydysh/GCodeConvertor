using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using GCodeConvertor.UI;

namespace GCodeConvertor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ApplySavedTheme();
        }

        public void ApplySavedTheme()
        {
            string savedTheme = Settings.Default.Theme;
            Uri themeUri;

            if (savedTheme == "Dark")
            {
                themeUri = new Uri("pack://application:,,,/UI/Themes/DarkTheme.xaml");
            }
            else
            {
                themeUri = new Uri("pack://application:,,,/UI/Themes/LightTheme.xaml");
            }
            ApplyTheme(themeUri);
        }

        private void ApplyTheme(Uri themeUri)
        {
            Resources.MergedDictionaries.Clear();

            ResourceDictionary theme = new ResourceDictionary();
            theme.Source = themeUri;

            Resources.MergedDictionaries.Add(theme);
        }

    }
}
