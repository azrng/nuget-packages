using System.Text;

namespace Azrng.Core.Extension
{
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// 根据条件拼接字符串
        /// </summary>
        /// <param name="builder">字符串</param>
        /// <param name="condition">条件</param>
        /// <param name="str">要添加的字符串</param>
        /// <returns></returns>
        public static StringBuilder AppendIF(this StringBuilder builder, bool condition, string str)
        {
            return condition ? builder.Append(str) : builder;
        }

        /// <summary>
        /// 根据条件拼接字符串（带换行）
        /// </summary>
        /// <param name="builder">字符串</param>
        /// <param name="condition">条件</param>
        /// <param name="str">要添加的字符串</param>
        /// <returns></returns>
        public static StringBuilder AppendLineIF(this StringBuilder builder, bool condition, string str)
        {
            return condition ? builder.AppendLine(str) : builder;
        }

        /// <summary>
        /// 根据条件追加格式化字符串
        /// </summary>
        /// <param name="builder">字符串</param>
        /// <param name="condition">条件</param>
        /// <param name="format">格式字符串</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static StringBuilder AppendFormatIF(this StringBuilder builder, bool condition, string format, params object[] args)
        {
            return condition ? builder.AppendFormat(format, args) : builder;
        }

        /// <summary>
        /// 追加字符串并换行（如果字符串不为空）
        /// </summary>
        /// <param name="builder">字符串</param>
        /// <param name="str">要添加的字符串</param>
        /// <returns></returns>
        public static StringBuilder AppendLineIfNotEmpty(this StringBuilder builder, string str)
        {
            return str.IsNullOrEmpty() ? builder : builder.AppendLine();
        }

        /// <summary>
        /// 追加字符串并换行（如果字符串不为空）
        /// </summary>
        /// <param name="builder">字符串</param>
        /// <param name="str">要添加的字符串</param>
        /// <returns></returns>
        public static StringBuilder AppendLineIfNotNullOrWhiteSpace(this StringBuilder builder, string str)
        {
            return str.IsNotNullOrWhiteSpace() ? builder : builder.AppendLine();
        }

        /// <summary>
        /// 追加字符串（如果字符串不为空）
        /// </summary>
        /// <param name="builder">字符串</param>
        /// <param name="str">要添加的字符串</param>
        /// <returns></returns>
        public static StringBuilder AppendIfNotEmpty(this StringBuilder builder, string str)
        {
            return str.IsNullOrEmpty() ? builder : builder.Append(str);
        }

        /// <summary>
        /// 追加字符串（如果字符串不为空）
        /// </summary>
        /// <param name="builder">字符串</param>
        /// <param name="str">要添加的字符串</param>
        /// <returns></returns>
        public static StringBuilder AppendIfNotNullOrWhiteSpace(this StringBuilder builder, string str)
        {
            return str.IsNullOrWhiteSpace() ? builder : builder.Append(str);
        }

        /// <summary>
        /// 移除末尾指定长度的字符
        /// </summary>
        /// <param name="builder">字符串</param>
        /// <param name="length">要移除的长度</param>
        /// <returns></returns>
        public static StringBuilder RemoveEnd(this StringBuilder builder, int length)
        {
            if (length > 0 && builder.Length >= length)
                builder.Remove(builder.Length - length, length);
            return builder;
        }

        /// <summary>
        /// 移除末尾的逗号（如果存在）
        /// </summary>
        /// <param name="builder">字符串</param>
        /// <returns></returns>
        public static StringBuilder RemoveEndComma(this StringBuilder builder)
        {
            return builder.RemoveEndWithChar(',');
        }

        /// <summary>
        /// 移除末尾的分号（如果存在）
        /// </summary>
        /// <param name="builder">字符串</param>
        /// <returns></returns>
        public static StringBuilder RemoveEndSemicolon(this StringBuilder builder)
        {
            return builder.RemoveEndWithChar(';');
        }

        /// <summary>
        /// 移除末尾的特定字符
        /// </summary>
        /// <param name="builder">字符串</param>
        /// <param name="ch">要移除的字符</param>
        /// <returns></returns>
        public static StringBuilder RemoveEndWithChar(this StringBuilder builder, char ch)
        {
            if (builder.Length > 0 && builder[^1] == ch)
                builder.Remove(builder.Length - 1, 1);
            return builder;
        }
    }
}