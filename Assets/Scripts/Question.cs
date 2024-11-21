using Postgrest.Models;
using Postgrest.Attributes;

public class question : BaseModel
{
    [Column("id"), PrimaryKey]
    public int Id { get; set; }

    [Column("question")]
    public string QuestionText { get; set; }

    [Column("answer1")]
    public string Answer1 { get; set; }

    [Column("answer2")]
    public string Answer2 { get; set; }

    [Column("answer3")]
    public string Answer3 { get; set; }

    [Column("correct_answer")]
    public string CorrectOption { get; set; }

    public int trivia_id { get; set; }

    public trivia trivia {get; set; }

}