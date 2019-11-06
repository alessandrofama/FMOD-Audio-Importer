using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FMODAudioImporter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Create the startup window
            MainWindow wnd = new MainWindow();
            // Do stuff here, e.g. to the window
            wnd.Title = "FMOD Audio Importer";
            // Show the window
            wnd.Show();
        }

    }
}
