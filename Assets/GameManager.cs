using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool ShowCharacterSelectOnMenuLoad { get; set; } = false;
    public bool NewHighScore { get; set; } = false;

    public int NewHighScoreValue { get; set; } = 0;
    public string NewHighScoreCharacter { get; set; } = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}