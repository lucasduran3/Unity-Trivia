using Postgrest.Models;
using Postgrest.Attributes;

public class usuarios : BaseModel
{
    [Column("id"), PrimaryKey]
    public int id { get; set; }

    [Column("username")]
    public string username { get; set; }

    [Column("age")]
    public int age { get; set; }

    [Column("password")]
    public string password { get; set; }
}
