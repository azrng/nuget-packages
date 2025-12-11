namespace Azrng.DbOperateHelper
{
    /// <summary>
    /// 分页类
    /// </summary>
    public class PageInfo
    {
        private long _pageSize;

        //
        // 摘要:
        //     数据数量
        public long Count { get; set; }

        //
        // 摘要:
        //     分页索引
        public long Index { get; set; }

        //
        // 摘要:
        //     分页尺寸
        public long PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if (value <= 0)
                {
                    _pageSize = 1L;
                }
                else
                {
                    _pageSize = value;
                }
            }
        }

        //
        // 摘要:
        //     获取总页数
        public long PageCount
        {
            get
            {
                long num = Count % PageSize;
                long num2 = Count / PageSize;
                if (num > 0)
                {
                    num2++;
                }

                return num2;
            }
        }

        //
        // 摘要:
        //     分页类
        public PageInfo()
            : this(0L, 15L)
        {
        }

        //
        // 摘要:
        //     分页类
        //
        // 参数:
        //   index:
        //     分页索引
        //
        //   pagesize:
        //     分页尺寸(给0或小于0就会默认15)
        public PageInfo(long index, long pagesize)
        {
            Index = index;
            PageSize = pagesize;
        }

        //
        // 摘要:
        //     显示分页页数集合,例如:第一组1,2,3,4,5,第二组6,7,8,9,10
        //
        // 参数:
        //   showpagecount:
        //     一组分多少页
        public long[] GetGroupPage(long showpagecount)
        {
            long pageCount = PageCount;
            List<long> list = new List<long>();
            long num = Index / showpagecount;
            long num2 = showpagecount * num;
            for (long num3 = num2; num3 < num2 + showpagecount && num3 < pageCount; num3++)
            {
                list.Add(num3);
            }

            return list.ToArray();
        }

        //
        // 摘要:
        //     显示上一个分组最后一页,如果无法显示返回-1
        //
        // 参数:
        //   showpagecount:
        //     每组分页数量
        public long GetGroupPageIndexWithPre(long showpagecount)
        {
            long[] groupPage = GetGroupPage(showpagecount);
            if (groupPage.Length == 0)
            {
                return -1L;
            }

            long num = groupPage[0];
            num--;
            if (num < 0)
            {
                return -1L;
            }

            return num;
        }

        //
        // 摘要:
        //     显示下一个分组的第一页,如果无法显示返回-1
        //
        // 参数:
        //   showpagecount:
        //     每组分页数量
        public long GetGroupPageIndexWithNext(long showpagecount)
        {
            long[] groupPage = GetGroupPage(showpagecount);
            long num = groupPage.Length;
            if (num == 0L)
            {
                return -1L;
            }

            long num2 = groupPage[num - 1];
            num2++;
            long index = Index;
            Index = num2;
            groupPage = GetGroupPage(showpagecount);
            Index = index;
            int num3 = groupPage.Length;
            if (num3 == 0)
            {
                return -1L;
            }

            if (num2 > groupPage[num3 - 1])
            {
                return -1L;
            }

            return groupPage[0];
        }
    }
}
