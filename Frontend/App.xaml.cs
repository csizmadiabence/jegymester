using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ticketmasterwpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            EventManager.RegisterClassHandler(typeof(FrameworkElement),
                FrameworkElement.GotFocusEvent,
                new RoutedEventHandler((s, e) =>
                {
                    if (s is FrameworkElement fe) fe.FocusVisualStyle = null;
                }));
        }
    }
}
