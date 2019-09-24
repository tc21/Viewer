using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Viewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Pass e.Args to MainWindow
            MainWindow mainWindow = new MainWindow(e.Args);
            mainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Viewer.Properties.Settings.Default.Save();
        }
    }
}
