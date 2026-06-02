using System.ComponentModel.DataAnnotations.Schema;

namespace Azrng.NMaxCompute.Test.Model;

public class UserInfoDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Sex { get; set; }

    public int Age { get; set; }

    [Column("id_no")]
    public string IdNo { get; set; }

    public string CreateTime { get; set; }
}
