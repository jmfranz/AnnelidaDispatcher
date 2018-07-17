using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using AnnelidaDispatcher.ViewModel;

namespace AnnelidaDispatcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            MainViewViewModel model;
            if (e.Args.Length == 1)
                model = new MainViewViewModel(e.Args[0]);
            else
                model = new MainViewViewModel();

            var view = new MainWindow();
            view.DataContext = model;
            view.Show();
        }
    }
}
