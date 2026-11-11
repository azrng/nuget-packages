using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 雪花ID
    /// </summary>
    public static class Snowflake
    {
        /*
         使用一个 64 bit 的 long 型的数字作为全局唯一 id。在分布式系统中的应用十分广泛，且ID 引入了时间戳，基本上保持自增。
        格式：1bit保留 + 41bit时间戳 + 10bit机器 + 12bit序列号
        第一位不使用，主要是为了避免部分场景变成负数；
        41位时间戳，也就是2的41次方，毫秒为单位，足够保存69年。这里一般存储1970年以来的毫秒数，建议各个系统根据需要自定义这个开始日期；
        10位机器码，理论上可以表示1024台机器。默认自动派生：高5位取本机标识（非回环IPv4地址，无则机器名）的稳定哈希，
        低5位取进程Id，保证机房内不同机器、相同机器上的不同进程派生出不同的机器码；
        分布式部署建议通过 ConfigureWorkerId 显式配置，彻底避免自动派生的哈希碰撞。
        12位序列号，表示范围0~4095，一直递增，即使毫秒数加一，这里也不会归零，避免被恶意用户轻易猜测得到前后订单号；
         */

        #region 属性

        private const int WorkerIdBits = 10;
        private const int SequenceBits = 12;
        private const int WorkerIdShift = SequenceBits;
        private const int TimestampShift = WorkerIdBits + SequenceBits;
        private const int WorkerIdMask = (1 << WorkerIdBits) - 1;
        private const int SequenceMask = (1 << SequenceBits) - 1;
        private const long MaxTimestamp = (1L << 41) - 1;
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// 开始时间戳 首次使用前设置，否则无效，默认2018-3-15
        /// </summary>
        private static DateTime StartTimestamp { get; } = new DateTime(2018, 3, 15, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 当前机器Id，范围为0~1023。首次生成Id时未配置则自动生成。
        /// </summary>
        public static int WorkerId
        {
            get
            {
                Init();
                return _workerId!.Value;
            }
        }

        private static int? _workerId;

        private static bool _hasGeneratedId;

        /// <summary>
        /// 当前序列
        /// </summary>
        private static int _sequence;

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

        /// <summary>
        /// 配置机器Id。应在首次生成Id前调用。
        /// </summary>
        /// <param name="workerId">机器Id，范围为0~1023。</param>
        /// <exception cref="ArgumentOutOfRangeException">机器Id超出范围。</exception>
        /// <exception cref="InvalidOperationException">已经使用其他机器Id生成过Id。</exception>
        public static void ConfigureWorkerId(int workerId)
        {
            if (workerId < 0 || workerId > WorkerIdMask)
                throw new ArgumentOutOfRangeException(nameof(workerId), "机器Id必须在0到1023之间。");

            lock (SyncRoot)
            {
                if (_hasGeneratedId && _workerId != workerId)
                {
                    throw new InvalidOperationException(
                        "雪花Id已经生成，不能切换机器Id。请在首次生成Id前完成配置。");
                }

                _workerId = workerId;
            }
        }

        private static void Init()
        {
            lock (SyncRoot)
            {
                // 初始化WorkerId：未显式配置时按机器标识+进程Id派生，避免多机/多进程默认值撞号产生重复雪花Id
                if (!_workerId.HasValue)
                {
                    _workerId = DeriveDefaultWorkerId();
                }

                // 记录此时距离起点的毫秒数以及开机嘀嗒数
                if (_msStart <= 0 || _watch == null)
                {
                    _msStart = (long)(DateTime.UtcNow - StartTimestamp).TotalMilliseconds;
                    _watch = Stopwatch.StartNew();
                }
            }
        }

        /// <summary>
        /// 获取下一个Id
        /// </summary>
        /// <returns></returns>
        public static long NewId()
        {
            Init();

            lock (SyncRoot)
            {
                var ms = _watch.ElapsedMilliseconds + _msStart;
                var wid = WorkerId & WorkerIdMask;
                var seq = ++_sequence & SequenceMask;

                // 避免时间倒退
                if (ms < _lastTime) ms = _lastTime;

                // 相同毫秒内，如果序列号用尽，则等待到下一毫秒，避免生成重复Id
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

                var id = CreateId(ms, wid, seq);
                _hasGeneratedId = true;
                return id;
            }
        }

        /// <summary>
        /// 获取指定时间的Id，带上节点和序列号。可用于根据业务时间构造插入Id
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns></returns>
        public static long NewId(DateTime time)
        {
            Init();

            var utcTime = ToUtc(time);
            if (utcTime < StartTimestamp)
                throw new ArgumentOutOfRangeException(nameof(time), "时间不能早于雪花ID起始时间。");

            var ms = (long)(utcTime - StartTimestamp).TotalMilliseconds;
            if (ms > MaxTimestamp)
                throw new ArgumentOutOfRangeException(nameof(time), "时间超出雪花ID时间戳可表示范围。");

            lock (SyncRoot)
            {
                var wid = WorkerId & WorkerIdMask;
                var seq = ++_sequence & SequenceMask;

                var id = CreateId(ms, wid, seq);
                _hasGeneratedId = true;
                return id;
            }
        }

        /// <summary>
        /// 尝试分析
        /// </summary>
        /// <param name="id"></param>
        /// <param name="time">时间</param>
        /// <param name="workerId">节点</param>
        /// <param name="sequence">序列号</param>
        /// <returns></returns>
        public static bool TryParse(long id, out DateTime time, out int workerId, out int sequence)
        {
            time = default;
            workerId = default;
            sequence = default;

            if (id <= 0)
                return false;

            var milliseconds = id >> TimestampShift;
            if (milliseconds <= 0 || milliseconds > MaxTimestamp)
                return false;

            time = StartTimestamp.AddMilliseconds(milliseconds);
            workerId = (int)((id >> WorkerIdShift) & WorkerIdMask);
            sequence = (int)(id & SequenceMask);

            return true;
        }

        private static DateTime ToUtc(DateTime time)
        {
            return time.Kind switch
            {
                DateTimeKind.Local => time.ToUniversalTime(),
                _ => DateTime.SpecifyKind(time, DateTimeKind.Utc)
            };
        }

        private static long CreateId(long milliseconds, int workerId, int sequence)
        {
            return (milliseconds << TimestampShift)
                   | ((long)(workerId & WorkerIdMask) << WorkerIdShift)
                   | (long)(sequence & SequenceMask);
        }

        /// <summary>
        /// 默认 WorkerId 派生：高5位取机器标识的稳定哈希，低5位取进程Id。
        /// </summary>
        internal static int DeriveDefaultWorkerId()
        {
            var machinePart = GetMachineIdentityHash() & 0x1F;
            var processPart = Process.GetCurrentProcess().Id & 0x1F;
            return machinePart << 5 | processPart;
        }

        /// <summary>
        /// 机器标识哈希：优先取非回环 IPv4 地址，DNS 不可用或无 IPv4 时退回机器名。
        /// </summary>
        private static int GetMachineIdentityHash()
        {
            try
            {
                var ipv4 = Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a))
                    .Select(a => a.ToString())
                    .OrderBy(address => address, StringComparer.Ordinal)
                    .FirstOrDefault();
                if (ipv4 != null)
                {
                    return StableHash(ipv4);
                }
            }
            catch (SocketException)
            {
                // DNS 解析不可用时退回机器名哈希
            }

            return StableHash(Environment.MachineName);
        }

        /// <summary>
        /// FNV-1a 稳定哈希：string.GetHashCode 跨进程随机化，不能用于跨进程一致的身份标识。
        /// </summary>
        internal static int StableHash(string value)
        {
            unchecked
            {
                var hash = 2166136261;
                foreach (var c in value)
                {
                    hash ^= c;
                    hash *= 16777619;
                }

                return (int)hash;
            }
        }

        #endregion
    }
}
