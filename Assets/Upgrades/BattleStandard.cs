using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// When you pick up a powerup, spawn this object at the player's position
// While the player is within range of the battle standard, heal them over time and boost their attack speed
public class BattleStandard : MonoBehaviour
{
    public GameObject battleStandardPrefab;
    public float duration = 10f;

    [Tooltip("Should be a multiple of 5.")]
    public float healRate = 300f;
    public float attackBoost = 1.2f;

    private void Start()
    {
        PowerUpTrigger.PowerUpPickedUp += (e) => OnPowerUpPickedUp(e.position);
    }

    void OnPowerUpPickedUp(Vector3 pos)
    {
        GameObject battleStandard = Instantiate(battleStandardPrefab, pos, Quaternion.identity);
        BannerEffect be = battleStandard.GetComponentInChildren<BannerEffect>();
        be.ActivateBannerEffect(duration, healRate, attackBoost);
        battleStandard.transform.position = pos;
    }
}
