using Postgrest.Models;
using Postgrest.Attributes;
using System;

[Table("user")]
public class User : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }

    [Column("usage_time")]
    public float usage_time { get; set; }

    [Column("date")]
    public DateTime date { get; set; }
}
