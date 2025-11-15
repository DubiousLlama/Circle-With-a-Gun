using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HighScoresMenuController : MonoBehaviour
{
    public void Play()
    {
        GameManager.Instance.ShowCharacterSelectOnMenuLoad = true;
        // Load main menu scene
        SceneManager.LoadScene("MainMenu");
    }

    public void Menu()
    {
        // Load main menu scene
        GameManager.Instance.ShowCharacterSelectOnMenuLoad = false;
        SceneManager.LoadScene("MainMenu");
    }
}
