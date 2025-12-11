using Azrng.Core.Extension;
using NPOI.SS.UserModel;

namespace Azrng.Office.NPOI.Extensions
{
    public static class RowExtensions
    {
        /// <summary>
        /// 当前行是否是空行
        /// </summary>
        /// <param name="row"></param>
        /// <param name="maxCellIndex"></param>
        /// <returns></returns>
        public static bool IsEmptyOrNull(this IRow? row, int? startCellIndex = null, int? maxCellIndex = null)
        {
            if (row == null || row.FirstCellNum < 0) return true;

            var cellStartIndex = startCellIndex ?? row.FirstCellNum;
            var cellEndIndex = maxCellIndex ?? row.LastCellNum;

            for (int cellIndex = cellStartIndex; cellIndex <= cellEndIndex; cellIndex++)
            {
                if (row.GetCell(cellIndex).GetValue().IsNotNullOrWhiteSpace())
                    return false;
            }

            return true;
        }
    }
}