using Azrng.Database.DynamicSqlBuilder.SqlOperation;

namespace Azrng.Database.DynamicSqlBuilder.Model
{
    /// <summary>
    /// 排序字段的信息
    /// </summary>
    public class SortFieldDto
    {
        public SortFieldDto(string field, bool asc = true)
        {
            Field = field;
            Asc = asc;
        }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 是否正序
        /// </summary>
        public bool Asc { get; private set; }

        /// <summary>
        /// 排序字符串,根据Asc生成
        /// </summary>
        public string OrderStr => Asc ? SqlConstant.OrderByASC : SqlConstant.OrderByDESC;
    }
}
