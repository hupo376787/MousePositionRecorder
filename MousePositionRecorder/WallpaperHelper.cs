using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace MousePositionRecorder
{
    internal class WallpaperHelper
    {
        // 定义常量用于获取桌面壁纸
        private const int SPI_GETDESKWALLPAPER = 0x0073;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

        public static string GetWallpaperPath()
        {
            StringBuilder wallpaperPath = new StringBuilder(260);
            SystemParametersInfo(SPI_GETDESKWALLPAPER, wallpaperPath.Capacity, wallpaperPath, 0);
            return wallpaperPath.ToString();
        }

        public static ImageBrush ConvertBitmapToImageBrush(Bitmap bitmap)
        {
            // 将 Bitmap 转换为 MemoryStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // 将 Bitmap 以 PNG 格式保存到内存流中
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                // 创建一个新的 BitmapImage
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                memoryStream.Seek(0, SeekOrigin.Begin); // 重置流的位置
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                // 创建一个 ImageBrush 并使用 BitmapImage 作为图像源
                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = bitmapImage;
                imageBrush.Stretch = Stretch.UniformToFill;

                return imageBrush;
            }
        }
    }
}
