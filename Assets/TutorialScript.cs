using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TutorialState
{
    Move,
    Shoot,
    Secondary,
    Completed,
    Transitioning
}

public class TutorialScript : MonoBehaviour
{
    GameObject player;
    public TutorialState currentState = TutorialState.Move;
    GlitchPopupController glitchController;
    TextMeshProUGUI textObject;

    public GameObject enemyPrefab;
    GameObject testDummy;

    public GameObject secondaryPrefab;
    GameObject secondaryDummy;

    public GameObject spawner;

    float textShowTime = 3.2f;
    

    // Start is called before the first frame update
    void Start()
    {
        bool doTutorial = (PlayerPrefs.GetInt("doTutorial", 1) == 1);

        if (!doTutorial)
        {
            this.transform.gameObject.SetActive(false);
            return;
        }

        spawner.GetComponent<EnemyTracker>().enableSpawning = false;

        player = GameObject.Find("PC");
        glitchController = gameObject.GetComponentInChildren<GlitchPopupController>();
        textObject = gameObject.GetComponentInChildren<TextMeshProUGUI>();

        // Setup the tutorial
        textObject.text = "Move with WASD.";
        Invoke(nameof(Reveal), 1f);
    }

    // Update is called once per frame
    void Update()
    {
        // State switching
        switch (currentState)
        {
            case TutorialState.Transitioning:
                break;
            case TutorialState.Move:
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) {
                    currentState = TutorialState.Transitioning;
                    Conceal();
                    HideText();
                    Invoke(nameof(ShootStage), 1f);
                }
                break;
            case TutorialState.Shoot:
                if (Input.GetButtonDown("Fire1"))
                {
                    currentState = TutorialState.Transitioning;
                    Conceal();
                    HideText();
                    Invoke(nameof(SecondaryStage), 1f);
                }
                break;
            case TutorialState.Secondary:
                if (Input.GetButtonDown("Fire2"))
                {
                    currentState = TutorialState.Completed;
                    Conceal();
                    HideText();
                    PlayerPrefs.SetInt("doTutorial", 0);

                    spawner.GetComponent<EnemyTracker>().enableSpawning = true;
                }
                break;
        }
    }

    private void ShootStage()
    {
        // Instantiate an enemy
        testDummy = null;
        for (int i = 0; i < 50; i++)
        {
            testDummy = spawner.GetComponent<SpawnScript>().SpawnEnemy(enemyPrefab, 0, true);
            if (testDummy != null) { break; }
        }
        if (testDummy == null) { Debug.LogError("Failed to spawn test dummy"); return; }
        testDummy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;

        GetComponent<RectTransform>().sizeDelta = new Vector2(1000, GetComponent<RectTransform>().sizeDelta.y);
        textObject.text = "Corners. Imperfection.";
        Reveal();

        Invoke(nameof(ShowText), 0.5f);
        Invoke(nameof(Conceal), textShowTime);
        Invoke(nameof(HideText), textShowTime - 0.5f);
        Invoke(nameof(ShootStageII), textShowTime + 0.5f);
    }

    private void ShootStageII()
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(750, GetComponent<RectTransform>().sizeDelta.y);
        textObject.text = "Left click?";
        Reveal();
        Invoke(nameof(ShowText), 0.5f);

        currentState = TutorialState.Shoot;
    }

    private void SecondaryStage()
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(1250, GetComponent<RectTransform>().sizeDelta.y);
        textObject.text = "Move over items to pick them up.";
        Reveal();
        Invoke(nameof(ShowText), 0.5f);

        ItemSpawner itemSpawner = spawner.GetComponentInChildren<ItemSpawner>();

        bool spawned = false;
        for (int i = 0; i < 50; i++)
        {
            Vector2 playerPos = player.GetComponent<Transform>().position;

            spawned = itemSpawner.SpawnItem(secondaryPrefab, playerPos + Random.insideUnitCircle * 7f, 0);
            if (spawned) { break; }

            if (i == 49) Debug.LogWarning("Failed to spawn secondary tutorial item!");
        }

        Invoke(nameof(Conceal), textShowTime);
        Invoke(nameof(HideText), textShowTime - 0.5f);
        Invoke(nameof(SecondaryStageII), textShowTime + 0.5f);
    }

    private void SecondaryStageII()
    {
        // Set the width of the Rect Transform of this game object to 1000
        GetComponent<RectTransform>().sizeDelta = new Vector2(1500, GetComponent<RectTransform>().sizeDelta.y);

        textObject.text = "Right click while still to use your secondary.";
        Reveal();
        Invoke(nameof(ShowText), 0.5f);

        testDummy = null;
        for (int i = 0; i < 50; i++)
        {
            testDummy = spawner.GetComponent<SpawnScript>().SpawnEnemy(enemyPrefab, 0, true);
            if (testDummy != null) { break; }
        }
        if (testDummy == null) { Debug.LogError("Failed to spawn test dummy"); return; }
        testDummy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;

        currentState = TutorialState.Secondary;
    }

    private void Reveal()
    {
        glitchController.TriggerShow();
        Invoke(nameof(ShowText), 0.5f);
    }

    private void Conceal()
    {
        HideText();
        glitchController.TriggerHide();
    }

    private void ShowText()
    {
        textObject.enabled = true;
    }

    private void HideText()
    {
        textObject.enabled = false;
    }
}
