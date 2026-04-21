using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ticketmasterwpf.Controls
{
    public partial class ToastControl : UserControl
    {
        private int _toastGeneration = 0;

        public ToastControl()
        {
            InitializeComponent();
        }

        public async void ShowToast(string message, bool isSuccess, Action onCompleted = null)
        {
            _toastGeneration++;
            int currentGen = _toastGeneration;

            ErrorToast.BeginAnimation(UIElement.OpacityProperty, null);
            LeftTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, null);
            RightTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, null);

            if (isSuccess)
            {
                ErrorIconBack.Visibility = Visibility.Collapsed; ErrorIconText.Visibility = Visibility.Collapsed;
                SuccessIconBack.Visibility = Visibility.Visible; SuccessIconText.Visibility = Visibility.Visible;
                LeftTimerStroke.Stroke = RightTimerStroke.Stroke = Brushes.MediumSeaGreen;
                BgPathLeft.Stroke = BgPathRight.Stroke = new SolidColorBrush(Color.FromArgb(40, 60, 179, 113));
            }
            else
            {
                ErrorIconBack.Visibility = Visibility.Visible; ErrorIconText.Visibility = Visibility.Visible;
                SuccessIconBack.Visibility = Visibility.Collapsed; SuccessIconText.Visibility = Visibility.Collapsed;
                LeftTimerStroke.Stroke = RightTimerStroke.Stroke = Brushes.IndianRed;
                BgPathLeft.Stroke = BgPathRight.Stroke = new SolidColorBrush(Color.FromArgb(40, 205, 92, 92));
            }

            ErrorText.Text = message;
            ErrorToast.Opacity = 0;
            ErrorToast.Visibility = Visibility.Visible;
            ErrorToast.UpdateLayout();

            double w = ErrorToast.ActualWidth; double h = ErrorToast.ActualHeight;
            double halfW = w / 2; double r = 12;
            var inv = System.Globalization.CultureInfo.InvariantCulture;

            string leftData = string.Format(inv, "M {0:0.##},0 L {1:0.##},0 A {1:0.##},{1:0.##} 0 0 0 0,{1:0.##} L 0,{2:0.##} A {1:0.##},{1:0.##} 0 0 0 {1:0.##},{3:0.##} L {0:0.##},{3:0.##}", halfW, r, h - r, h);
            string rightData = string.Format(inv, "M {0:0.##},0 L {1:0.##},0 A {2:0.##},{2:0.##} 0 0 1 {3:0.##},{2:0.##} L {3:0.##},{4:0.##} A {2:0.##},{2:0.##} 0 0 1 {1:0.##},{5:0.##} L {0:0.##},{5:0.##}", halfW, w - r, r, w, h - r, h);

            LeftTimerStroke.Data = Geometry.Parse(leftData); RightTimerStroke.Data = Geometry.Parse(rightData);
            BgPathLeft.Data = LeftTimerStroke.Data; BgPathRight.Data = RightTimerStroke.Data;

            double pathLen = halfW + h + halfW + 10;
            LeftTimerStroke.StrokeDashArray = RightTimerStroke.StrokeDashArray = new DoubleCollection { pathLen, pathLen };
            LeftTimerStroke.StrokeDashOffset = RightTimerStroke.StrokeDashOffset = 0;

            ErrorToast.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0.2)));
            var vanishAnim = new DoubleAnimation(pathLen, TimeSpan.FromSeconds(5));
            LeftTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, vanishAnim);
            RightTimerStroke.BeginAnimation(Shape.StrokeDashOffsetProperty, vanishAnim);

            await Task.Delay(5000);
            if (_toastGeneration != currentGen) return;

            var fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.4));
            fadeOut.Completed += (s, e) =>
            {
                ErrorToast.Visibility = Visibility.Collapsed;
                onCompleted?.Invoke();
            };
            ErrorToast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }
}