using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnMainMenu : MonoBehaviour
{
    public static event Action OnReturnToMainMenu;

    public void invokeMenuReturn()
    {
        OnReturnToMainMenu?.Invoke();
    }

    public void returnMainMenu()
    {
        OnReturnToMainMenu?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        Time.timeScale = 1f;
    }
}
