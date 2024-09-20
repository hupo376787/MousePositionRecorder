using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MousePositionRecorder
{
    public class ColorHelper
    {
        private static Random _random = new Random();
        public static Color GetVibrantColor()
        {
            // 随机色相在 [0, 360) 范围内
            double hue = _random.NextDouble() * 360;
            double saturation = 1.0; // 100% 饱和度
            double lightness = 0.5;  // 50% 亮度

            // 将 HSL 转换为 RGB
            return HslToRgb(hue, saturation, lightness);
        }

        private static Color HslToRgb(double hue, double saturation, double lightness)
        {
            double r, g, b;

            if (saturation == 0)
            {
                r = g = b = lightness; // 灰色
            }
            else
            {
                double q = lightness < 0.5 ? lightness * (1 + saturation) : lightness + saturation - (lightness * saturation);
                double p = 2 * lightness - q;

                r = HueToRgb(p, q, hue / 360.0 + 1.0 / 3.0);
                g = HueToRgb(p, q, hue / 360.0);
                b = HueToRgb(p, q, hue / 360.0 - 1.0 / 3.0);
            }

            return Color.FromArgb(255, (byte)(Clamp(r) * 255), (byte)(Clamp(g) * 255), (byte)(Clamp(b) * 255));
        }


        private static double Clamp(double value)
        {
            return Math.Max(0, Math.Min(1, value)); // 确保值在 0 到 1 之间
        }


        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }
    }
}
