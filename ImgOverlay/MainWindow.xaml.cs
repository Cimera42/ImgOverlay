using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImgOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ControlPanel cp = new ControlPanel();
        private ObservableCollection<BitmapImage> images = new ObservableCollection<BitmapImage>();

        public MainWindow()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            InitializeComponent();

            DataContext = images;
        }

        public void LoadImage(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                MessageBox.Show("Cannot open folders.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.IO.File.Exists(path))
            {
                MessageBox.Show("The selected image file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var img = new BitmapImage();
            try
            {
                img.BeginInit();
                img.UriSource = new Uri(path);
                img.EndInit();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error loading image. Perhaps its format is unsupported?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            images.Add(img);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cp.Owner = this;
            cp.Show();
            cp.Closed += (o, ev) =>
            {
                this.Close();
            };

            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
        }

        private Point _center;
        private double thumbAngle;

        private ContentPresenter GetItemRoot(object sender)
        {
            return (ContentPresenter)((Control)sender).TemplatedParent;
        }

        private RotateTransform GetSafeRotateTransform(Panel control)
        {
            RotateTransform rotate = (control.RenderTransform as RotateTransform) ?? new RotateTransform(0);
            control.RenderTransform = rotate;
            return rotate;
        }

        private ScaleTransform GetSafeScaleTransform(Panel control)
        {
            ScaleTransform scale = (control.RenderTransform as ScaleTransform) ?? new ScaleTransform(1, 1);
            control.RenderTransform = scale;
            return scale;
        }

        private Grid GetRotateContainer(object sender)
        {
            return (Grid) ((Thumb) sender).FindName("RotateContainer");
        }

        private Grid GetScaleContainer(object sender)
        {
            return (Grid)((Thumb)sender).FindName("ScaleContainer");
        }

        public void RotateThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            var container = GetRotateContainer(sender);

            var halfW = container.ActualWidth / 2;
            var halfH = container.ActualHeight / 2;

            _center = new Point(halfW, halfH);

            thumbAngle = Math.Atan(halfH / halfW) * (180 / Math.PI);
        }

        private void RotateThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var container = GetRotateContainer(sender);
            var rotate = GetSafeRotateTransform(container);
            rotate.Angle = 0;

            var pos = Mouse.GetPosition(container);
            double angle = Math.Atan2((pos.Y - _center.Y), (pos.X - _center.X)) * (180 / Math.PI) + thumbAngle;
            rotate.Angle = angle;
        }

        private void RotateThumb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var container = GetRotateContainer(sender);
            var rotate = GetSafeRotateTransform(container);
            rotate.Angle = 0;
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var presenter = GetItemRoot(sender);
            var container = GetRotateContainer(sender);
            var scaleContainer = GetScaleContainer(sender);
            var rotate = GetSafeRotateTransform(container);
            var scale = GetSafeScaleTransform(scaleContainer);
            var transformedChange = new Point(e.HorizontalChange, e.VerticalChange);
            transformedChange = scale.Transform(transformedChange);
            transformedChange = rotate.Transform(transformedChange);

            var l = Canvas.GetLeft(presenter);
            var t = Canvas.GetTop(presenter);
            if (double.IsNaN(l)) l = 0;
            if (double.IsNaN(t)) t = 0;

            Canvas.SetLeft(presenter, l + transformedChange.X);
            Canvas.SetTop(presenter, t + transformedChange.Y);
        }

        private void ScaleContainer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var container = (Grid)sender;
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                var scale = GetSafeScaleTransform(container);
                var sX = (container.ActualWidth + (e.Delta / 5)) / container.ActualWidth;
                scale.ScaleX *= sX;
                scale.ScaleY *= sX;
            }
            else
            {
                var imageThumb = (Thumb) container.FindName("DisplayImageThumb");
                var img = (Image) imageThumb.Template.FindName("DisplayImage", imageThumb);
                var o = img.Opacity + e.Delta / 2000.0;
                img.Opacity = Math.Max(Math.Min(o, 1), 0.1);
            }
        }

        public void ScaleThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ScaleThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? "Visible" : "Hidden";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString() == "Visible";
        }
    }
}