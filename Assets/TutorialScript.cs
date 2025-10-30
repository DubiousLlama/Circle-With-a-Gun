using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class TutorialScript : MonoBehaviour
{

    GlitchPopupController glitchController;
    GameObject player;
    TextMeshProUGUI textObject;
    EnemyTracker spawner;

    public float perCharLength = 10;
    public float baseLength = 50;

    public float revealTime = 0.7f;
    public float hideTime = 0.7f;
    public float hideDelay = 0.15f;

    public GameObject enemyPrefab;

    private GameObject testDummy = null;

    void Start()
    {
        player = GameObject.Find("PC");
        glitchController = gameObject.GetComponentInChildren<GlitchPopupController>();
        textObject = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        spawner = GameObject.Find("Spawner").GetComponent<EnemyTracker>();
        spawner.GetComponent<EnemyTracker>().enableSpawning = false;

        StartCoroutine(RunTutorial());
    }

    private IEnumerator RunTutorial()
    {
        yield return StartCoroutine(MoveSection());
        yield return StartCoroutine(SpawnSection());
        yield return StartCoroutine(ShootSection());
        yield return StartCoroutine(SecondarySection());

        PlayerPrefs.SetInt("doTutorial", 0);
        spawner.GetComponent<EnemyTracker>().enableSpawning = false;
    }


    private IEnumerator MoveSection()
    {
        textObject.text = Gamepad.all.Count > 0 ? "Right joystick to look, left joystick to move." : "WASD to move.";
        yield return StartCoroutine(ShowBox());

        // Wait for player to move
        yield return new WaitUntil(() => player.GetComponent<Rigidbody2D>().velocity.magnitude > 0.1f);
        yield return StartCoroutine(HideBox());
    }

    private IEnumerator SpawnSection()
    {
        textObject.text = "Corners. Imperfection.";
        yield return StartCoroutine(ShowBox());

        SpawnTestDummy();

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(HideBox());
    }

    private IEnumerator ShootSection()
    {
        if (testDummy == null) yield break;
        textObject.text = Gamepad.all.Count > 0 ? "Right trigger?" : "Left click?";
        yield return StartCoroutine(ShowBox());

        // Wait for player to shoot dummy
        bool primaryUsed = false;
        Weapon.OnWeaponUsed += (args) =>
        {
            if (args.weaponType == WeaponType.Primary)
            {
                primaryUsed = true;
            }
        };
        yield return new WaitUntil(() => primaryUsed);
        if (testDummy != null) { testDummy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None; }
        yield return StartCoroutine(HideBox());
    }

    private IEnumerator SecondarySection()
    {
        SpawnTestDummy();
        if (testDummy == null) yield break;
        textObject.text = Gamepad.all.Count > 0 ? "Right trigger while still for a powerful special attack." : "Right click while still for a powerful special attack.";
        yield return StartCoroutine(ShowBox());

        // Wait for player to shoot dummy with secondary
        bool secondaryUsed = false;
        Weapon.OnWeaponUsed += (args) =>
        {
           if (args.weaponType == WeaponType.Secondary)
            {
                secondaryUsed = true;
            }
        };
        yield return new WaitUntil(() => secondaryUsed);
        if (testDummy != null) { testDummy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None; }
        yield return StartCoroutine(HideBox());
    }

    private void AdjustSize()
    {
        float newWidth = baseLength + (textObject.text.Length * perCharLength);
        if (newWidth > 1600) { Debug.LogWarning("Tutorial text too long, clipping width"); newWidth = 1600; }
        GetComponent<RectTransform>().sizeDelta = new Vector2(newWidth, GetComponent<RectTransform>().sizeDelta.y);
    }

    private void SpawnTestDummy()
    {
        // Spawn test dummy
        for (int i = 0; i < 50; i++)
        {
            testDummy = spawner.GetComponent<SpawnScript>().SpawnEnemy(enemyPrefab, 0, true);
            if (testDummy != null) { break; }
        }
        if (testDummy == null) { Debug.LogError("Failed to spawn test dummy"); }
        testDummy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
    }

    private IEnumerator ShowBox()
    {
        AdjustSize();
        glitchController.TriggerShow();
        yield return new WaitForSeconds(revealTime);
        textObject.enabled = true;
    }

    private IEnumerator HideBox()
    {
        yield return new WaitForSeconds(hideDelay);
        textObject.enabled = false;
        glitchController.TriggerHide();
        yield return new WaitForSeconds(hideTime);
    }

}
