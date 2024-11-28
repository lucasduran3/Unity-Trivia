using Postgrest.Models;
using Postgrest.Attributes;
using Newtonsoft.Json;

[Table("ranking")]
public class Ranking : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }

    [JsonProperty("points")] // Coincide con el campo devuelto por la funci�n
    public int points { get; set; }

    [JsonProperty("trivia_id")] // Coincide con el campo devuelto por la funci�n
    public int trivia_id { get; set; }

    [JsonProperty("category")] // Campo adicional en la funci�n
    public string category
    {
        get; set;
    }

}