using System.Data;

namespace Azrng.DbOperateHelper
{
    /// <summary>
    /// 数据读取器扩展
    /// </summary>
    public static class DbDataReaderEx
    {
        public static T GetValue<T>(this IDataReader dbDataReader, int ordinal)
        {
            object value = dbDataReader.GetValue(ordinal);
            if (value == DBNull.Value)
                return default(T);

            if (value is T)
            {
                //return (T)Convert.ChangeType(value, typeof(T));
                T typedValue = (T)value;
                return typedValue;
            }
            else if (value is IConvertible)
            {
                IConvertible convertibleValue = (IConvertible)value;
                return (T)convertibleValue.ToType(typeof(T), null);
            }
            else
            {
                return default(T);
            }
        }

        public static T GetFieldValue<T>(this IDataReader reader, int columnIndex)
        {
            object value = reader.GetValue(columnIndex);
            if (value == DBNull.Value)
            {
                return default(T);
            }
            else
            {
                T typedValue = (T)value;
                return typedValue;
            }
        }

        //
        // 摘要:
        //     获取sbyte
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        //     列索引
        public static sbyte GetSByteEx(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return 0;
            }

            return dbDataReader.GetValue<sbyte>(ordinal);
        }

        //
        // 摘要:
        //     获取ushort
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        //     列索引
        public static ushort GetUInt16Ex(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return 0;
            }

            return dbDataReader.GetFieldValue<ushort>(ordinal);
        }

        //
        // 摘要:
        //     获取uint
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        //     列索引
        public static uint GetUInt32Ex(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return 0u;
            }

            return dbDataReader.GetFieldValue<uint>(ordinal);
        }

        //
        // 摘要:
        //     获取ulong
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        //     列索引
        public static ulong GetUInt64Ex(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return 0uL;
            }

            return dbDataReader.GetFieldValue<ulong>(ordinal);
        }

        //
        // 摘要:
        //     转换bool
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        public static bool GetBooleanEx(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return false;
            }
            return dbDataReader.GetBoolean(ordinal);
        }

        //
        // 摘要:
        //     转换byte
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        public static byte GetByteEx(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return 0;
            }

            return dbDataReader.GetByte(ordinal);
        }

        //
        // 摘要:
        //     转换char
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        public static char GetCharEx(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return '0';
            }

            return dbDataReader.GetChar(ordinal);
        }

        //
        // 摘要:
        //     转换datetime
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        public static DateTime GetDateTimeEx(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return DateTime.MinValue;
            }

            return dbDataReader.GetDateTime(ordinal);
        }

        //
        // 摘要:
        //     获取timespan
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        public static TimeSpan GetTimeSpanEx(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return TimeSpan.Zero;
            }

            return dbDataReader.GetFieldValue<TimeSpan>(ordinal);
        }

        //
        // 摘要:
        //     转换decimal
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        public static decimal GetDecimalEx(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return 0m;
            }

            return dbDataReader.GetDecimal(ordinal);
        }

        //
        // 摘要:
        //     转换double
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        public static double GetDoubleEx(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return 0.0;
            }

            return dbDataReader.GetDouble(ordinal);
        }

        //
        // 摘要:
        //     转换short
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        public static short GetInt16Ex(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return 0;
            }

            return dbDataReader.GetInt16(ordinal);
        }

        //
        // 摘要:
        //     转换int
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        public static int GetInt32Ex(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return 0;
            }

            return dbDataReader.GetInt32(ordinal);
        }
        public static string GetStringEx(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return string.Empty;
            }

            return dbDataReader.GetString(ordinal);
        }
        public static byte[] GetBytesEx(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return null;
            }

            const int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            long bytesRead;
            long startIndex = 0;
            using (MemoryStream stream = new MemoryStream())
            {
                while ((bytesRead = dbDataReader.GetBytes(ordinal, startIndex, buffer, 0, bufferSize)) > 0)
                {
                    stream.Write(buffer, 0, (int)bytesRead);
                    startIndex += bufferSize;
                }
                return stream.ToArray();
            }
        }

        //
        // 摘要:
        //     转换long
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        public static long GetInt64Ex(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return 0L;
            }

            return dbDataReader.GetInt64(ordinal);
        }

        //
        // 摘要:
        //     转换float
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        public static float GetFloatEx(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return 0f;
            }

            return dbDataReader.GetFloat(ordinal);
        }

        //
        // 摘要:
        //     获取guid
        //
        // 参数:
        //   dbDataReader:
        //
        //   ordinal:
        public static Guid GetGuidEx(this IDataReader dbDataReader, int ordinal)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                return Guid.Empty;
            }

            return dbDataReader.GetGuid(ordinal);
        }
    }
}
