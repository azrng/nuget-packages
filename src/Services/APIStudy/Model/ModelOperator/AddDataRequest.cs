using System.ComponentModel.DataAnnotations;

namespace APIStudy.Model.ModelOperator
{
    public class AddDataRequest
    {
        public string Name { get; set; }

        [MinValue] public int Id { get; set; }

        [MinValue] public long Long { get; set; }

        [MinValue] public decimal Decimal { get; set; }
    }
}