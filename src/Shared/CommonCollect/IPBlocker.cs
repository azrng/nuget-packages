using Newtonsoft.Json;
using System.Net;

namespace CommonCollect
{
    public static class IPBlocker
    {
        static List<IPBaseData> ipBlackListWith32Prefix = new List<IPBaseData>();
        static List<IPBaseData> ipBlackList = new List<IPBaseData>();
        const string ipBlackListFile = "IPBlackList.txt";

        static IPBlocker()
        {
            LoadBlackListToList();
        }

        public static void LoadBlackListToList()
        {
            string strJsonData = File.ReadAllText(ipBlackListFile);
            ipBlackList = JsonConvert.DeserializeObject<List<IPBaseData>>(strJsonData);
            ipBlackListWith32Prefix = ipBlackList.Where(o => o.PreFix == 32).ToList();
            ipBlackList = ipBlackList.Where(o => o.PreFix != 32).ToList();
        }

        public static bool IsIPInBlackList(string strAccessIP)
        {
            if (ipBlackListWith32Prefix.Any(o => o.IP.Equals(strAccessIP)))
                return true;
            uint ipToCheck = IpToUInt(strAccessIP);
            if (ipBlackList.Any(o => IsIpInRange(ipToCheck, o.MinimumUintIP, o.MaximumUintIP)))
                return true;
            return false;
        }

        static uint IpToUInt(string ipAddress)
        {
            var bytes = IPAddress.Parse(ipAddress).GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        static bool IsIpInRange(uint ipToCheck, uint startIp, uint endIp)
        {
            return startIp <= ipToCheck && ipToCheck <= endIp;
        }
    }

    public class IPBaseData
    {
        public string IP { get; set; }
        public int PreFix { get; set; }
        public uint MinimumUintIP { get; set; }
        public uint MaximumUintIP { get; set; }
    }
}