using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 雪花ID
    /// </summary>
    public class Snowflake
    {
        /*
         使用一个 64 bit 的 long 型的数字作为全局唯一 id。在分布式系统中的应用十分广泛，且ID 引入了时间戳，基本上保持自增。
        格式：1bit保留 + 41bit时间戳 + 10bit机器 + 12bit序列号
        第一位不使用，主要是为了避免部分场景变成负数；
        41位时间戳，也就是2的41次方，毫秒为单位，足够保存69年。这里一般存储1970年以来的毫秒数，建议各个系统根据需要自定义这个开始日期；
        10位机器码，理论上可以表示1024台机器，也可以拆分几位表示机房几位表示机器。这里默认采用本机IPv4地址最后两段以及进程Id一起作为机器码，确保机房内部不同机器，
        以及相同机器上的不同进程，拥有不同的机器码；
        12位序列号，表示范围0~4095，一直递增，即使毫秒数加一，这里也不会归零，避免被恶意用户轻易猜测得到前后订单号；
         */

        #region 属性

        /// <summary>
        /// 随机数生成
        /// </summary>
        private static readonly RandomNumberGenerator Rnd = new RNGCryptoServiceProvider();

        /// <summary>
        /// 开始时间戳 首次使用前设置，否则无效，默认2018-3-15
        /// </summary>
        private static DateTime StartTimestamp { get; } = new DateTime(2018, 3, 15);

        /// <summary>
        /// 机器Id，取10位
        /// </summary>
        private static int WorkerId { get; set; }

        /// <summary>
        /// 当前序列
        /// </summary>
        private static int _sequence;

        /// <summary>
        /// 获取或者设置序列号，取12位
        /// </summary>
        public static int Sequence { get; set; }

        /// <summary>
        /// 距离开始时间的毫秒数
        /// </summary>
        private static long _msStart;

        private static Stopwatch _watch = Stopwatch.StartNew();

        /// <summary>
        /// 上次时间
        /// </summary>
        private static long _lastTime;

        #endregion

        #region 核心方法

        private static void Init()
        {
            // 初始化WorkerId，取5位实例加上5位进程，确保同一台机器的WorkerId不同
            if (WorkerId <= 0)
            {
                var nodeId = Next(1, 1024);
                var pid = Process.GetCurrentProcess().Id;
                var tid = Thread.CurrentThread.ManagedThreadId;
                WorkerId = (nodeId & 0x1F) << 5 | (pid ^ tid) & 0x1F;
            }

            // 记录此时距离起点的毫秒数以及开机嘀嗒数
            if (_watch == null)
            {
                _msStart = (long)(DateTime.UtcNow - StartTimestamp).TotalMilliseconds;
                _watch = Stopwatch.StartNew();
            }
        }

        /// <summary>
        /// 获取下一个Id
        /// </summary>
        /// <returns></returns>
        public static long NewId()
        {
            Init();

            // 此时计时器的嘀嗒数，加上起点毫秒数
            var ms = _watch.ElapsedMilliseconds + _msStart;
            var wid = WorkerId & 0x3FF;
            var seq = Interlocked.Increment(ref _sequence) & 0x0FFF;

            // 避免时间倒退
            if (ms < _lastTime) ms = _lastTime;

            // 相同毫秒内，如果序列号用尽，则可能超过4096，导致生成重复Id
            if (_lastTime == ms && seq == 0)
            {
                while (_lastTime == ms)
                    ms = _watch.ElapsedMilliseconds + _msStart;
            }

            _lastTime = ms;

            /*
                * 每个毫秒内_Sequence没有归零，主要是为了安全，避免被人猜测得到前后Id。
                * 而毫秒内的顺序，重要性不大。
                */

            return ms << 10 + 12 | (uint)(wid << 12) | (uint)seq;
        }

        /// <summary>
        /// 获取指定时间的Id，带上节点和序列号。可用于根据业务时间构造插入Id
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns></returns>
        public long NewId(DateTime time)
        {
            Init();

            var ms = (long)(time - StartTimestamp).TotalMilliseconds;
            var wid = WorkerId & 0x3FF;
            var seq = Interlocked.Increment(ref _sequence) & 0x0FFF;

            return ms << 10 + 12 | (uint)(wid << 12) | (uint)seq;
        }

        /// <summary>
        /// 时间转为Id，不带节点和序列号。可用于构建时间片段查询
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns></returns>
        public long GetId(DateTime time)
        {
            var t = (long)(time - StartTimestamp).TotalMilliseconds;
            return t << 10 + 12;
        }

        /// <summary>
        /// 尝试分析
        /// </summary>
        /// <param name="id"></param>
        /// <param name="time">时间</param>
        /// <param name="workerId">节点</param>
        /// <param name="sequence">序列号</param>
        /// <returns></returns>
        public bool TryParse(long id, out DateTime time, out int workerId, out int sequence)
        {
            time = StartTimestamp.AddMilliseconds(id >> 10 + 12);
            workerId = (int)(id >> 12 & 0x3FF);
            sequence = (int)(id & 0x0FFF);

            return true;
        }

        /// <summary>
        /// 返回一个指定范围内的随机数
        /// </summary>
        /// <remarks>调用平均耗时37.76ns，其中GC耗时77.56%</remarks>
        /// <param name="min">返回的随机数的下界（随机数可取该下界值）</param>
        /// <param name="max">返回的随机数的上界（随机数不能取该上界值）</param>
        /// <returns></returns>
        private static int Next(int min, int max)
        {
            if (max <= min)
                throw new ArgumentOutOfRangeException(nameof(max));
            var _buf = new byte[4];
            Rnd.GetBytes(_buf);
            var int32 = BitConverter.ToInt32(_buf, 0);
            if (min == int.MinValue && max == int.MaxValue)
                return int32;
            if (min == 0 && max == int.MaxValue)
                return Math.Abs(int32);
            return min == int.MinValue && max == 0
                ? -Math.Abs(int32)
                : (int)(((max - min) * (uint)int32 >> 32) + min);
        }

        #endregion
    }
}