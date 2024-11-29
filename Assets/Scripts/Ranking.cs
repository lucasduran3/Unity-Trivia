using Postgrest.Models;
using Postgrest.Attributes;

[Table("ranking")]
public class Ranking : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }

    [Column("points")]
    public int points { get; set; }

    [Column("trivia_id")]
    public int trivia_id { get; set; }

    [Column("category")]
    public string category
    {
        get; set;
    }

}