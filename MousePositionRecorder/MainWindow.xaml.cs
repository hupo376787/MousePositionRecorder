using Hardcodet.Wpf.TaskbarNotification;
using MicaWPF.Controls;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        bool grayMode = false;

        public MainWindow()
        {
            InitializeComponent();
            foreach (var screen in Screen.AllScreens)
            {

            }

            lastPoint = MousePositionHelper.GetMousePosition();
            InititCanvas();

            Task.Run(async () =>
            {
                while (true)
                {
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        UpdateColorAsync();
                    });

                    //10-30s随机变色
                    await Task.Delay(new Random().Next(10, 30) * 1000);
                }
            });
            Task.Run(async () =>
            {
                await Record();
            });
            Task.Run(async () =>
            {
                while (true)
                {
                    RemoveSameCenterCircle();
                    await Task.Delay(3000);
                }
            });
        }

        private void InititCanvas()
        {
            // 创建一个新的Canvas
            canvas = new Canvas();
            canvas.Width = Screen.AllScreens.First().WpfBounds.Width;
            canvas.Height = Screen.AllScreens.First().WpfBounds.Height;
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
                            //内圆
                            DrawCircle(pt, (DateTime.Now - dt).TotalSeconds / 1.5, true);
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

        private void DrawCircle(Point point, double radius, bool fill = true)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Ellipse ellipse = new Ellipse()
                {
                    Width = radius,
                    Height = radius,
                    Fill = fill ? brush : null,
                    Stroke = brush,
                    StrokeThickness = 1,
                    Tag = new Tuple<Point, DateTime>(point, DateTime.Now)
                };
                Canvas.SetLeft(ellipse, point.X - radius / 2);
                Canvas.SetTop(ellipse, point.Y - radius / 2);

                canvas!.Children.Add(ellipse);
            });
        }

        private void RemoveSameCenterCircle()
        {
            // 获取 Canvas 中所有的 Ellipse
            var allEllipses = canvas!.Children
                .OfType<Ellipse>()
                .ToList();

            // 查找具有相同 Point 的 Ellipse 分组
            var groupedByPoint = allEllipses
                .Where(e => e.Tag is Tuple<Point, DateTime>) // 确保 Tag 是 Tuple<Point, DateTime>
                .GroupBy(e => ((Tuple<Point, DateTime>)e.Tag).Item1) // 根据 Point 分组
                .Where(g => g.Count() > 1) // 仅保留具有相同 Point 的组（即多个 Ellipse）
                .ToList();

            // 对每个分组，按照时间排序并移除较早的
            foreach (var group in groupedByPoint)
            {
                // 按照 DateTime 排序，保留最新的 Ellipse
                var ellipsesSortedByTime = group
                    .OrderByDescending(e => ((Tuple<Point, DateTime>)e.Tag).Item2)
                    .ToList();

                // 移除时间较早的 Ellipse，保留最新的
                foreach (var ellipse in ellipsesSortedByTime.Skip(1)) // Skip(1) 保留最新的，移除其余的
                {
                    canvas.Children.Remove(ellipse);
                }
            }
        }

        private void UpdateColorAsync()
        {
            if (grayMode)
            {
                switch (cbBackColor.SelectedIndex)
                {
                    case 0:
                    case 2:
                        brush = new SolidColorBrush(Colors.Black); break;
                    case 1:
                        brush = new SolidColorBrush(Colors.White); break;
                }
            }
            else
            {
                Color randomColor = ColorHelper.GetVibrantColor();
                brush = new SolidColorBrush(randomColor);
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

        DateTime dt1 = DateTime.Now;
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
                dt1 = DateTime.Now;
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

                DateTime dt2 = DateTime.Now;
                TimeSpan ts = dt2 - dt1;
                var str = ts.TotalSeconds <= 60 ? $"{(int)ts.TotalSeconds} seconds" :
                    ts.TotalMinutes <= 60 ? $"{(int)ts.TotalMinutes} minutes" :
                    $"{ts.TotalHours:#0.00} hours";
                var desc = $"(from {dt1:HH}-{dt1:mm} to {dt2:HH}-{dt2:mm})";
                ImageHelper.SaveCanvasAsImage(grid, $"Images/Graph - {str} {desc}.png", grid.Background);

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
                case 3:
                    {
                        //ImageBrush brush = new ImageBrush();
                        //brush.ImageSource = new BitmapImage(new Uri(WallpaperHelper.GetWallpaperPath(), UriKind.Absolute));
                        var dd = CaptureDesktop();
                        ImageBrush brush = WallpaperHelper.ConvertBitmapToImageBrush(dd!);
                        brush.Stretch = Stretch.UniformToFill;

                        grid.Background = brush;
                        break;
                    }
            }
        }
        private System.Drawing.Bitmap? CaptureDesktop()
        {
            try
            {
                // 获取所有屏幕的宽度和高度
                int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                int screenHeight = (int)SystemParameters.PrimaryScreenHeight;

                // 创建一个 Bitmap 来保存截图
                System.Drawing.Bitmap screenshot = new System.Drawing.Bitmap(screenWidth, screenHeight);

                var state = this.WindowState;
                this.WindowState = WindowState.Minimized;
                Thread.Sleep(500);
                // 使用 Graphics 从屏幕中复制
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));
                }
                //Thread.Sleep(500);
                this.WindowState = state;

                return screenshot;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法截取桌面截图：{ex.Message}");
                return null;
            }
        }
        private async void GrayMode_Checked(object sender, RoutedEventArgs e)
        {
            await GrayModeChanged(true);
        }

        private async void GrayMode_Unchecked(object sender, RoutedEventArgs e)
        {
            await GrayModeChanged(false);
        }

        private async Task GrayModeChanged(bool check)
        {
            grayMode = check;
            var res = await MicaWPF.Dialogs.ContentDialog.ShowAsync(this, "灰色模式将重置画板，继续吗", primaryButtonText: "是", secondaryButtonText: "否");
            if (res == MicaWPF.Core.Enums.ContentDialogResult.PrimaryButton)
            {
                canvas!.Children.Clear();
            }
            else
            {
                cbGrayMode.IsChecked = false;
            }

            UpdateColorAsync();

        }
    }
}