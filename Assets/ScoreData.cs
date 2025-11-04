using UnityEngine;

public class ScoreData
{
    public int score;
    public string playerName;
    public string characterUsed;
    public GameObject scoreObject;

    public ScoreData(int score, string playerName, string characterUsed)
    {
        this.score = score;
        this.playerName = playerName;
        this.characterUsed = characterUsed;
        this.scoreObject = null;
    }
}