using System.Diagnostics;
using System.IO;

namespace MousePositionRecorder
{
    public class LogHelper
    {
        private static readonly object lockObject = new(); // 用于线程同步的锁对象
        private static TextWriterTraceListener? myListener;
        private static string? currentLogFile; // 当前日志文件路径

        /// <summary>
        /// 实时写数据
        /// </summary>
        /// <param name="folder">文件存放目录</param>
        /// <param name="fileName">文件名，不包含路径</param>
        /// <param name="message">日志</param>
        public static void LogToFile(string folder, string fileName, string message)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string logFile = $"{folder}/{fileName}.log";

            try
            {
                lock (lockObject)
                {
                    if (myListener == null || myListener.Writer == null || logFile != currentLogFile)
                    {
                        // 如果之前的 myListener 已经存在，则先释放资源
                        myListener?.Flush();
                        myListener?.Close();

                        myListener = new TextWriterTraceListener(logFile, "myListener");
                        currentLogFile = fileName;
                    }

                    myListener.WriteLine(message);
                    myListener.Flush();
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public static void DisposeLogFileWriter()
        {
            lock (lockObject)
            {
                if (myListener != null)
                {
                    myListener.Flush(); // 先确保所有消息都被写入文件
                    myListener.Close(); // 关闭日志写入器
                    myListener.Dispose(); // 释放资源
                    myListener = null; // 将 myListener 置为 null，表示资源已释放
                }
            }
        }
    }
}
