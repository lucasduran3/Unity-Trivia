using Postgrest.Models;
using Postgrest.Attributes;
using System;

[Table("time")]
public class TimeModel : BaseModel
{
    [PrimaryKey("id", false)]
    public string id { get; set; }

    [Column("date")]
    public DateTime date { get; set; }

    [Column("usage_time")]
    public float usage_time { get; set; }

    [Column("user_id")]
    public string user_id { get; set; }
}
