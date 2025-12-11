using System;
using System.Runtime.InteropServices;

namespace Common.Windows.Core;

/// <summary>
/// win32 API
/// </summary>
public class Win32ApiConfigHelper
{
    /// <summary>
    /// 获取mac地址使用用到
    /// </summary>
    /// <param name="ncb"></param>
    /// <returns></returns>
    [DllImport("NETAPI32.DLL")]
    public static extern char Netbios(ref Ncb ncb);

    /// <summary>
    /// 刷新注册表时候用到
    /// </summary>
    /// <param name="wEventId"></param>
    /// <param name="uFlags"></param>
    /// <param name="dwItem1"></param>
    /// <param name="dwItem2"></param>
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    public static extern void SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

    /// <summary>
    /// 网络检测时候用
    /// </summary>
    /// <param name="connectionDescription"></param>
    /// <param name="reservedValue"></param>
    /// <returns></returns>
    [DllImport("wininet.dll")]
    public static extern bool InternetGetConnectedState(out int connectionDescription, int reservedValue);

    internal enum Ncbconst
    {
        NCBNAMSZ = 16,      /* absolute length of a net name         */
        MAX_LANA = 254,      /* lana's in range 0 to MAX_LANA inclusive   */
        NCBENUM = 0x37,      /* NCB ENUMERATE LANA NUMBERS            */
        NRC_GOODRET = 0x00,      /* good return                              */
        NCBRESET = 0x32,      /* NCB RESET                        */
        NCBASTAT = 0x33,      /* NCB ADAPTER STATUS                  */
        NUM_NAMEBUF = 30,      /* Number of NAME's BUFFER               */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AdapterStatus
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] adapter_address;

        private readonly byte rev_major;
        private readonly byte reserved0;
        private readonly byte adapter_type;
        private readonly byte rev_minor;
        private readonly ushort duration;
        private readonly ushort frmr_recv;
        private readonly ushort frmr_xmit;
        private readonly ushort iframe_recv_err;
        private readonly ushort xmit_aborts;
        private readonly uint xmit_success;
        private readonly uint recv_success;
        private readonly ushort iframe_xmit_err;
        private readonly ushort recv_buff_unavail;
        private readonly ushort t1_timeouts;
        private readonly ushort ti_timeouts;
        private readonly uint reserved1;
        private readonly ushort free_ncbs;
        private readonly ushort max_cfg_ncbs;
        private readonly ushort max_ncbs;
        private readonly ushort xmit_buf_unavail;
        private readonly ushort max_dgram_size;
        private readonly ushort pending_sess;
        private readonly ushort max_cfg_sess;
        private readonly ushort max_sess;
        private readonly ushort max_sess_pkt_size;
        private readonly ushort name_count;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NameBuffer
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Ncbconst.NCBNAMSZ)]
        public byte[] name;

        public byte name_num;
        public byte name_flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Ncb
    {
        public byte ncb_command;
        public byte ncb_retcode;
        public byte ncb_lsn;
        public byte ncb_num;
        public IntPtr ncb_buffer;
        public ushort ncb_length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Ncbconst.NCBNAMSZ)]
        public byte[] ncb_callname;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Ncbconst.NCBNAMSZ)]
        public byte[] ncb_name;

        public byte ncb_rto;
        public byte ncb_sto;
        public IntPtr ncb_post;
        public byte ncb_lana_num;
        public byte ncb_cmd_cplt;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] ncb_reserve;

        public IntPtr ncb_event;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LanaEnum
    {
        public byte length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Ncbconst.MAX_LANA)]
        public byte[] lana;
    }

    [StructLayout(LayoutKind.Auto)]
    public struct Astat
    {
        public AdapterStatus adapt;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Ncbconst.NUM_NAMEBUF)]
        public NameBuffer[] NameBuff;
    }
}