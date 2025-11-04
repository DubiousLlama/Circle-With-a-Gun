using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum ScoreLists
{
    Personal,
    Friends,
    Global
}

public class PopulateScores : MonoBehaviour
{

    // UI REFERENCES
    public GameObject content;
    public Roster roster;
    public GameObject scoreDisplayPrefab;

    public Button personalScoreButton;
    public Button friendsScoreButton;
    public Button globalScoreButton;


    // DUMMY DATA FOR TESTING UI
    public List<ScoreData> highScores = new List<ScoreData>()
    {
        new ScoreData(162375, "samjett", "BombsMcGee"),
        new ScoreData(161910, "samjett", "Kevin"),
        new ScoreData(87210, "Transcendent Pig", "MissMicro"),
        new ScoreData(64435, "samjett", "ElectricJeff"),
        new ScoreData(51995, "Transcendent Pig", "BombsMcGee"),
        new ScoreData(51995, "Transcendent Pig", "BombsMcGee"),
        new ScoreData(51995, "Transcendent Pig", "BombsMcGee"),
        new ScoreData(51995, "Transcendent Pig", "BombsMcGee"),
        new ScoreData(10, "John", "Kevin"),
    };

    // STEAMWORKS 



    // Start is called before the first frame update
    void Start()
    {
    }

    void CreateScoreUI()
    {
        // Clear any existing score objects in content
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        // Create UI for each score in the list
        for (int i = 0; i < highScores.Count; i++)
        {
            GameObject scoreObj = Instantiate(scoreDisplayPrefab, content.transform);
            highScores[i].scoreObject = scoreObj;
            UpdateScoreDisplay(scoreObj, highScores[i]);
            UpdateRank(scoreObj, i + 1);
        }

        Canvas.ForceUpdateCanvases();
    }

    public void UpdateScoreDisplay(GameObject scoreObject, ScoreData score)
    {
        Transform hL = scoreObject.transform.Find("HorizLayout");
        hL.GetChild(0).GetComponent<TextMeshProUGUI>().text = ""; // Rank (set later)
        scoreObject.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = score.score.ToString("N0");
        hL.GetChild(2).GetComponent<TextMeshProUGUI>().text = score.playerName;

        Character ch = roster.allCharacters.Find(c => c.prefName == score.characterUsed);
        if (ch != null)
        {
            hL.GetChild(1).GetComponent<Image>().sprite = ch.sprite;
        }
    }

    public void UpdateRank(GameObject scoreObject, int rank)
    {
        scoreObject.transform.Find("HorizLayout").GetChild(0).GetComponent<TextMeshProUGUI>().text = "#" + rank.ToString();
    }

}
