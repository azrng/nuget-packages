using Azrng.Office.NPOI.Attributes;
using Azrng.Office.NPOI.Attributes.Styles;

namespace Azrng.Office.Npoi.Test.Export.Dtos
{
    public class User
    {
        [ColumnStyle(isBold: true)]
        [HeaderStyle(isBold:true)]
        public string Name { get; set; }

        [ColumnName("年龄")]
        public int Age { get; set; }

        [ColumnName("爱好")]
        public string Hobby { get; set; }

        [IgnoreColumn]
        public string Sex { get; set; }
    }
}