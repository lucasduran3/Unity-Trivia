using Postgrest.Models;
using Postgrest.Attributes;
using System.Data.SqlTypes;

public class User : BaseModel
{
    [Column("id"), PrimaryKey]
    public int id { get; set; }

    [Column("create_at")]
    public SqlDateTime create_at { get; set; }

    [Column("usage_time")]
    public float usage_time { get; set; }
}
