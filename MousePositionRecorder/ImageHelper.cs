﻿using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MousePositionRecorder
{
    public class ImageHelper
    {
        public static void SaveCanvasAsImage(Grid grid, string filename, Brush backgroundBrush)
        {
            // 定义要保存的宽度和高度
            double dpi = 96d; // 分辨率
            int width = (int)grid!.ActualWidth;
            int height = (int)grid!.ActualHeight;

            string format = Path.GetExtension(filename).Replace(".", "");

            if (width == 0 || height == 0)
            {
                throw new InvalidOperationException("Canvas must have a defined size.");
            }

            // 创建RenderTargetBitmap来渲染Canvas
            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32);

            // 创建一个DrawingVisual
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                // 如果背景刷是 SolidColorBrush，则直接填充颜色
                if (backgroundBrush is SolidColorBrush solidColorBrush)
                {
                    dc.DrawRectangle(solidColorBrush, null, new Rect(0, 0, width, height));
                }
                // 如果背景刷是 ImageBrush，则绘制图像背景
                else if (backgroundBrush is ImageBrush imageBrush)
                {
                    dc.DrawRectangle(imageBrush, null, new Rect(0, 0, width, height));
                }

                // 使用 VisualBrush 绘制 Canvas 的内容
                VisualBrush visualBrush = new VisualBrush(grid);
                dc.DrawRectangle(visualBrush, null, new Rect(0, 0, width, height));
            }

            rtb.Render(visual);

            // 创建相应格式的 BitmapEncoder
            BitmapEncoder encoder = GetEncoder(format);

            if (encoder == null)
            {
                throw new InvalidOperationException($"Unsupported format: {format}");
            }

            encoder.Frames.Add(BitmapFrame.Create(rtb));

            // 将图片保存到指定路径
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                encoder.Save(fs);
            }
        }

        // 根据格式返回合适的BitmapEncoder
        public static BitmapEncoder GetEncoder(string format)
        {
            switch (format.ToLower())
            {
                case "png":
                    return new PngBitmapEncoder();
                case "jpeg":
                case "jpg":
                    return new JpegBitmapEncoder();
                case "bmp":
                    return new BmpBitmapEncoder();
                case "gif":
                    return new GifBitmapEncoder();
                case "tiff":
                    return new TiffBitmapEncoder();
                default:
                    return null;
            }
        }

    }
}
