using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MousePositionRecorder
{
    public class MousePositionHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        // 导入 GetCursorPos 函数
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        // 获取全局鼠标位置
        public static Point GetMousePosition()
        {
            POINT point;
            if (GetCursorPos(out point))
            {
                // 返回一个 WPF Point 对象
                return new Point(point.X, point.Y);
            }
            return new Point(0, 0);
        }
    }
}
