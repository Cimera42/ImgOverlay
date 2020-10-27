using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

            this.SizeChanged += Window_SizeChanged;

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

        public void ChangeOpacity(float opacity)
        {
            //displayImage.Opacity = opacity;
        }

        public void ChangeRotation(float angle)
        {
            // Create a transform to rotate the button
            RotateTransform myRotateTransform = new RotateTransform();

            // Set the rotation of the transform.
            myRotateTransform.Angle = angle;

            // Create a TransformGroup to contain the transforms
            // and add the transforms to it.
            TransformGroup myTransformGroup = new TransformGroup();
            myTransformGroup.Children.Add(myRotateTransform);

            //displayImage.RenderTransformOrigin = new Point(0.5, 0.5);
            // Associate the transforms to the button.
            //displayImage.RenderTransform = myTransformGroup;
        }

        private void SetImageSize()
        {
            //if (displayImage?.Source != null)
            //{
            //    // Set image size so that corners stay in the window when rotating
            //    var w = displayImage.Source.Width;
            //    var h = displayImage.Source.Height;
            //    var diag = Math.Sqrt(w * w + h * h);

            //    var scale = Math.Min(this.Width, this.Height) / diag;
            //    Container.MaxWidth = w * scale;
            //    Container.MaxHeight = h * scale;
            //}
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetImageSize();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
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

        private RotateTransform GetSafeRotateTransform(UIElement control)
        {
            var rotate = (control.RenderTransform as RotateTransform) ?? new RotateTransform(0);
            control.RenderTransform = rotate;
            return rotate;
        }

        public void RotateThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            var presenter = GetItemRoot(sender);

            var halfW = presenter.ActualWidth / 2;
            var halfH = presenter.ActualHeight / 2;

            _center = new Point(halfW, halfH);

            thumbAngle = Math.Atan(halfH / halfW) * (180 / Math.PI);
        }

        private void RotateThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var presenter = GetItemRoot(sender);
            var rotate = GetSafeRotateTransform(presenter);
            rotate.Angle = 0;

            var pos = Mouse.GetPosition(presenter);
            double angle = Math.Atan2((pos.Y - _center.Y), (pos.X - _center.X)) * (180 / Math.PI) + thumbAngle;
            rotate.Angle = angle;
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var presenter = GetItemRoot(sender);
            var rotate = GetSafeRotateTransform(presenter);
            var transformedChange = rotate.Transform(new Point(e.HorizontalChange, e.VerticalChange));

            var l = Canvas.GetLeft(presenter);
            var t = Canvas.GetTop(presenter);
            if (double.IsNaN(l)) l = 0;
            if (double.IsNaN(t)) t = 0;

            Canvas.SetLeft(presenter, l + transformedChange.X);
            Canvas.SetTop(presenter, t + transformedChange.Y);
        }
    }
}