using System.Collections.Generic;

public class UserRankingData
{
    public int UserScrore {  get; set; }
    public int UserRanking { get; set; }
    public int GeneralPosition { get; set; }
    public int TriviaPosition { get; set; }
    public List<Ranking> TriviaTopScores {  get; set; }
    public List<Ranking> GeneralTopScores {  get; set; }
}
