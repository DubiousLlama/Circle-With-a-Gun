using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLauncherUpgrade : MonoBehaviour
{
    [Header("Upgrade Settings")]
    public float launchInterval = 7.0f;

    [Header("Missile Settings")]
    public float blastRadius = 2.0f;
    public int damage = 200;
    public float moveSpeed = 7.0f;

    [Header("Prefabs")]
    public GameObject missileProjectilePrefab;

    Transform firePoint;
    WaitForSeconds intervalDelay;

    public bool firing = true;

    void Start()
    {
        intervalDelay = new(launchInterval);
        firePoint = transform.parent.GetChild(0);
        StartCoroutine(LaunchRoutine());
    }

    IEnumerator LaunchRoutine()
    {
        while (firing)
        {
            yield return intervalDelay;
            Fire();
        }
    }

    void Fire()
    {
        Vector3 aimDirection = firePoint.up;

        // Create the bomb projectile
        GameObject missile = Instantiate(missileProjectilePrefab, firePoint.position, firePoint.rotation);
        MissileLogic ml = missile.GetComponent<MissileLogic>();
        ml.InitalizeMissile(dmg: damage, radius: blastRadius, move: moveSpeed, homing: true);
    }

    void OnDestroy()
    {
        firing = false;
        StopAllCoroutines();
    }
}
