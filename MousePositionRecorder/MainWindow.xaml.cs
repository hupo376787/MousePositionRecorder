using Hardcodet.Wpf.TaskbarNotification;
using MicaWPF.Controls;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfScreenHelper;

namespace MousePositionRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MicaWindow
    {
        Canvas? canvas;
        Point lastPoint;
        SolidColorBrush? brush;
        bool startRecording = false;
        bool saveLogFile = false;

        public MainWindow()
        {
            InitializeComponent();
            foreach (var screen in Screen.AllScreens)
            {

            }
            lastPoint = MousePositionHelper.GetMousePosition();
            InititCanvas();
            UpdateColorAsync();

            Task.Run(async () =>
            {
                await Record();
            });
        }

        private void InititCanvas()
        {
            // 创建一个新的Canvas
            canvas = new Canvas();
            canvas.Width = 2560;
            canvas.Height = 1600;
            canvas.Background = Brushes.Transparent;
            canvas.SnapsToDevicePixels = true;

            grid.Children.Add(canvas);
        }

        private async Task Record()
        {
            while (true)
            {
                if (startRecording)
                {
                    var pt = MousePositionHelper.GetMousePosition();
                    Debug.WriteLine($"{pt.X}, {pt.Y}");
                    if (pt != lastPoint)
                        DrawLine(pt);
                    else
                    {
                        var dt = DateTime.Now;
                        while (pt == lastPoint)
                        {
                            DrawCircle(pt, (DateTime.Now - dt).TotalSeconds / 10);
                            await Task.Delay(500);
                            pt = MousePositionHelper.GetMousePosition();
                        }
                    }
                    if (saveLogFile)
                        LogHelper.LogToFile("Log", $"{DateTime.Now:yyyy-MM-dd}", $"{DateTime.Now:yyyy-MM-dd HH:mm-ss-fff}, {pt.X}, {pt.Y}");
                    await Task.Delay(10);
                }
            }
        }

        private void DrawLine(Point point)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Line line = new Line()
                {
                    X1 = lastPoint.X,
                    Y1 = lastPoint.Y,
                    X2 = point.X,
                    Y2 = point.Y,
                    Stroke = brush,
                    StrokeThickness = 0.3
                };

                canvas!.Children.Add(line);
            });

            lastPoint = point;
        }

        private void DrawCircle(Point point, double radius)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Ellipse ellipse = new Ellipse()
                {
                    Width = radius,
                    Height = radius,
                    Fill = brush,
                };
                Canvas.SetLeft(ellipse, point.X - radius / 2);
                Canvas.SetTop(ellipse, point.Y - radius / 2);

                canvas!.Children.Add(ellipse);
            });
        }

        private async Task UpdateColorAsync()
        {
            while (true)
            {
                Color randomColor = ColorHelper.GetVibrantColor();
                brush = new SolidColorBrush(randomColor);

                await Task.Delay(30000);
            }
        }

        private void SaveLogFile_Checked(object sender, RoutedEventArgs e)
        {
            saveLogFile = true;
        }

        private void SaveLogFile_Unchecked(object sender, RoutedEventArgs e)
        {
            saveLogFile = false;
        }

        private void MicaWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            this.ShowInTaskbar = false;
            MyNotifyIcon.ShowBalloonTip("提示", "程序已最小化到系统托盘", BalloonIcon.Info);
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MyNotifyIcon.Visibility = Visibility.Hidden;
            Application.Current.Shutdown();
        }

        private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionButtonPath.Data == (Geometry)FindResource("StartGeometry"))
            {
                // 切换到 Stop 图标
                ActionButtonPath.Data = (Geometry)FindResource("StopGeometry");
                ActionButtonPath.Fill = new SolidColorBrush(Colors.Red);
                ActionButton.Tag = "Stop";

                startRecording = true;
                canvas!.Children.Clear();
                tbStatus.Text = "正在记录";
            }
            else
            {
                // 切换到 Start 图标
                ActionButtonPath.Data = (Geometry)FindResource("StartGeometry");
                ActionButtonPath.Fill = new SolidColorBrush(Colors.Green);
                ActionButton.Tag = "Start";

                startRecording = false;
                tbStatus.Text = "停止记录";
                Directory.CreateDirectory("Images");
                ImageHelper.SaveCanvasAsImage(canvas!, "Images/" + DateTime.Now.ToString("yyyy-MMMM-dd HH-mm-ss-fff") + ".png", (SolidColorBrush)grid.Background);

                MicaWPF.Dialogs.ContentDialog.ShowAsync(this, "图像已保存到App根目录Images文件夹", primaryButtonText: "确定");
            }
        }

        private void cbBackColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (grid == null) return;

            switch (cbBackColor.SelectedIndex)
            {
                case 0:
                    grid.Background = new SolidColorBrush(Colors.Transparent); break;
                case 1:
                    grid.Background = new SolidColorBrush(Colors.Black); break;
                case 2:
                    grid.Background = new SolidColorBrush(Colors.White); break;
            }
        }
    }
}