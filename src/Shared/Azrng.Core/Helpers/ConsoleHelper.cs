using System;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 控制台输出
    /// </summary>
    public static class ConsoleHelper
    {
        /// <summary>
        /// 打印错误信息
        /// </summary>
        /// <param name="str">待打印的字符串</param>
        /// <param name="color">想要打印的颜色</param>
        public static void WriteErrorLine(string str, ConsoleColor color = ConsoleColor.Red)
        {
            WriteColorLine(str, color);
        }

        /// <summary>
        /// 打印警告信息
        /// </summary>
        /// <param name="str">待打印的字符串</param>
        /// <param name="color">想要打印的颜色</param>
        public static void WriteWarningLine(string str, ConsoleColor color = ConsoleColor.Yellow)
        {
            WriteColorLine(str, color);
        }

        /// <summary>
        /// 打印正常信息
        /// </summary>
        /// <param name="str">待打印的字符串</param>
        /// <param name="color">想要打印的颜色</param>
        public static void WriteInfoLine(string str, ConsoleColor color = ConsoleColor.White)
        {
            WriteColorLine(str, color);
        }

        /// <summary>
        /// 打印成功的信息
        /// </summary>
        /// <param name="str">待打印的字符串</param>
        /// <param name="color">想要打印的颜色</param>
        public static void WriteSuccessLine(string str, ConsoleColor color = ConsoleColor.Green)
        {
            WriteColorLine(str, color);
        }

        public static string ReadLineWithPrompt(string prompt = "Press Enter to continue")
        {
            if (prompt != null)
            {
                WriteInfoLine(prompt);
            }

            return Console.ReadLine();
        }

        public static ConsoleKeyInfo ReadKeyWithPrompt(string prompt = "Press any key to continue")
        {
            if (prompt != null)
            {
                WriteInfoLine(prompt);
            }

            return Console.ReadKey();
        }

        /// <summary>
        ///  输出带颜色的内容
        /// </summary>
        /// <param name="str"></param>
        /// <param name="color"></param>
        private static void WriteColorLine(string str, ConsoleColor color)
        {
            var currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.ForegroundColor = currentForeColor;
        }
    }
}