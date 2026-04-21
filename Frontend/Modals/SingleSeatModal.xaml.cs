using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ticketmasterwpf.Modals
{
    public partial class SingleSeatModal : UserControl
    {
        public SingleSeatModal()
        {
            InitializeComponent();
        }

        // Modál bezárása animációval (ha van "CloseModal" erőforrásod a XAML-ben)
        public void CloseModal()
        {
            var sb = this.Resources["CloseModal"] as Storyboard;
            if (sb != null)
            {
                sb.Begin();
            }
            else
            {
                // Ha nincs animáció, csak simán tüntessük el
                this.Visibility = Visibility.Collapsed;
            }
        }

        // Az "Értem" vagy "OK" gombra kattintva
        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            CloseModal();
        }

        // Ha a sötétített háttérre kattintasz, akkor is záródjon be
        private void Background_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseModal();
        }
    }
}