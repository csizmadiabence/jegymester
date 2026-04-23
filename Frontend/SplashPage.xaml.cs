using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ticketmasterwpf.Services;

namespace ticketmasterwpf
{
    public partial class SplashPage : Page
    {
        public SplashPage()
        {
            InitializeComponent();
        }

        //ANIMÁCIÓ
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string fullText = "ticketmaster";
            LogoPanel.Children.Clear();
            SpectrumCanvas.Children.Clear();

            FontFamily customFont = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/Assets/#Calistoga");

            LinearGradientBrush premiumBrush = new LinearGradientBrush();
            premiumBrush.StartPoint = new Point(0, 0);
            premiumBrush.EndPoint = new Point(1, 1);
            premiumBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#A0A5B5"), 0.0));
            premiumBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFFF"), 0.5));
            premiumBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#606575"), 1.0));

            Brush[] lineColors = {
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A5B5")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D99058")),
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#20232A")),
                Brushes.White,
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#404555"))
            };

            DropShadowEffect subtleGlow = new DropShadowEffect
            {
                Color = (Color)ColorConverter.ConvertFromString("#A0A5B5"),
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0.6
            };

            Random rnd = new Random();
            List<TextBlock> letters = new List<TextBlock>();
            List<Rectangle> spectrumLines = new List<Rectangle>();

            int lineCount = 90;
            double totalWidth = 0;
            List<double> widths = new List<double>();

            for (int i = 0; i < lineCount; i++)
            {
                double w = rnd.NextDouble() * 2.5 + 0.5;
                widths.Add(w);
                totalWidth += w;
            }

            double currentX = -totalWidth / 2;

            for (int i = 0; i < lineCount; i++)
            {
                Rectangle line = new Rectangle
                {
                    Width = widths[i],
                    Height = rnd.Next(50, 70),
                    Fill = lineColors[rnd.Next(lineColors.Length)],
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };

                Canvas.SetLeft(line, currentX);
                Canvas.SetTop(line, -line.Height / 2);
                SpectrumCanvas.Children.Add(line);
                currentX += widths[i];

                TransformGroup lineTg = new TransformGroup();
                lineTg.Children.Add(new ScaleTransform(1, 1));
                lineTg.Children.Add(new TranslateTransform(0, 0));
                line.RenderTransform = lineTg;

                spectrumLines.Add(line);
            }

            foreach (char letter in fullText)
            {
                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(new ScaleTransform(0.5, 0.5));
                transformGroup.Children.Add(new TranslateTransform(0, 0));

                TextBlock letterBlock = new TextBlock
                {
                    Text = letter.ToString(),
                    Foreground = premiumBrush,
                    FontSize = 55,
                    FontFamily = customFont,
                    Margin = new Thickness(0, 0, -6, 0),
                    Opacity = 0,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = transformGroup,
                    Effect = subtleGlow
                };

                LogoPanel.Children.Add(letterBlock);
                letters.Add(letterBlock);
            }

            await Task.Delay(500);

            IEasingFunction easeOut = new CubicEase { EasingMode = EasingMode.EaseOut };
            IEasingFunction easeIn = new CubicEase { EasingMode = EasingMode.EaseIn };
            IEasingFunction easeInOut = new CubicEase { EasingMode = EasingMode.EaseInOut };

            for (int i = 0; i < letters.Count; i++)
            {
                var tb = letters[i];
                var scale = (ScaleTransform)((TransformGroup)tb.RenderTransform).Children[0];

                tb.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400)) { EasingFunction = easeOut });
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(400)) { EasingFunction = easeOut });
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(400)) { EasingFunction = easeOut });

                await Task.Delay(30);
            }

            await Task.Delay(1500);

            double offsetToCenter = (LogoPanel.ActualWidth / 2) - (letters[0].ActualWidth / 2);
            var tTrans = (TranslateTransform)((TransformGroup)letters[0].RenderTransform).Children[1];
            tTrans.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, offsetToCenter, TimeSpan.FromMilliseconds(500)) { EasingFunction = easeIn });

            for (int i = letters.Count - 1; i > 0; i--)
            {
                var tb = letters[i];
                var tg = (TransformGroup)tb.RenderTransform;
                var sc = (ScaleTransform)tg.Children[0];
                var tr = (TranslateTransform)tg.Children[1];

                tb.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300)) { EasingFunction = easeIn });
                sc.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300)) { EasingFunction = easeIn });
                sc.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300)) { EasingFunction = easeIn });
                tr.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, -30, TimeSpan.FromMilliseconds(300)) { EasingFunction = easeIn });

                await Task.Delay(20);
            }

            await Task.Delay(600);

            var tScale = (ScaleTransform)((TransformGroup)letters[0].RenderTransform).Children[0];

            TimeSpan inhaleDuration = TimeSpan.FromMilliseconds(200);
            tScale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1, 0.8, inhaleDuration) { EasingFunction = easeOut });
            tScale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1, 0.8, inhaleDuration) { EasingFunction = easeOut });

            await Task.Delay(250);

            TimeSpan zoomDuration = TimeSpan.FromMilliseconds(400);
            tScale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.8, 15, zoomDuration) { EasingFunction = easeIn });
            tScale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.8, 15, zoomDuration) { EasingFunction = easeIn });

            await Task.Delay(400);

            letters[0].Visibility = Visibility.Hidden;
            SpectrumCanvas.Opacity = 1;

            for (int i = 0; i < spectrumLines.Count; i++)
            {
                var line = spectrumLines[i];
                var lScale = (ScaleTransform)((TransformGroup)line.RenderTransform).Children[0];
                var lTrans = (TranslateTransform)((TransformGroup)line.RenderTransform).Children[1];

                double moveDistance = (i < lineCount / 2) ? rnd.Next(-2500, -800) : rnd.Next(800, 2500);
                TimeSpan lineDuration = TimeSpan.FromMilliseconds(rnd.Next(500, 900));

                DoubleAnimation stretchY = new DoubleAnimation(15, rnd.Next(150, 400), lineDuration) { EasingFunction = easeOut };
                DoubleAnimation thickX = new DoubleAnimation(3, rnd.Next(15, 50), lineDuration) { EasingFunction = easeOut };
                DoubleAnimation flyX = new DoubleAnimation(0, moveDistance, lineDuration) { EasingFunction = easeOut };

                TimeSpan fadeBegin = TimeSpan.FromMilliseconds(lineDuration.TotalMilliseconds * 0.3);
                TimeSpan fadeDuration = TimeSpan.FromMilliseconds(lineDuration.TotalMilliseconds * 0.7);

                DoubleAnimation fadeOut = new DoubleAnimation(1, 0, fadeDuration)
                {
                    BeginTime = fadeBegin,
                    EasingFunction = easeInOut
                };

                lScale.BeginAnimation(ScaleTransform.ScaleYProperty, stretchY);
                lScale.BeginAnimation(ScaleTransform.ScaleXProperty, thickX);
                lTrans.BeginAnimation(TranslateTransform.XProperty, flyX);
                line.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }

            await Task.WhenAll(
                DataService.InitializationTask,
                Task.Delay(500)
            );

            if (NavigationService != null)
            {
                NavigationService.Navigate(new HomePage());
            }
        }
    }
}