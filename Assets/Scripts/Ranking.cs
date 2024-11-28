using Postgrest.Models;
using Postgrest.Attributes;
using Newtonsoft.Json;

[Table("ranking")]
public class Ranking : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }

    [JsonProperty("points")] // Coincide con el campo devuelto por la función
    public int points { get; set; }

    [JsonProperty("trivia_id")] // Coincide con el campo devuelto por la función
    public int trivia_id { get; set; }

    [JsonProperty("category")] // Campo adicional en la función
    public string category
    {
        get; set;
    }

}