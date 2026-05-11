// ScoreData.cs
using System;
using System.Collections.Generic;

[Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
    public DateTime date;
    public int rank;

    public ScoreEntry(string name, int scoreValue)
    {
        playerName = name;
        score = scoreValue;
        date = DateTime.Now;
    }
}

[Serializable]
public class LeaderboardData
{
    public List<ScoreEntry> scores = new List<ScoreEntry>();
    
    public void AddScore(ScoreEntry entry)
    {
        scores.Add(entry);
        SortScores();
        UpdateRanks();
    }
    
    private void SortScores()
    {
        scores.Sort((a, b) => b.score.CompareTo(a.score));
    }
    
    private void UpdateRanks()
    {
        for (int i = 0; i < scores.Count; i++)
        {
            scores[i].rank = i + 1;
        }
    }
    
    public List<ScoreEntry> GetTopScores(int count)
    {
        int topCount = Math.Min(count, scores.Count);
        return scores.GetRange(0, topCount);
    }
}