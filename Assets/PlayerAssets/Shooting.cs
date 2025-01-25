using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class Shooting : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;

    public int damage = 20;
    public float bulletForce = 20f;
    public float fireRate = 1f;
    float fireDelay = 0f;

    public Color bulletColor = new Color(0.561111f, 0f, 1f, 1f);
    private AudioManager audioManager;
    PlayerStats playerStats;
    private bool shouldFire = false;

    private void Start()
    {
        audioManager = AudioManager.instance;
        playerStats = GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fireDelay > 0)
        {
            fireDelay -= Time.deltaTime;
        }

        if (Platform.IsDesktop()) {
            shouldFire = Input.GetButton("Fire1");
        }

        // The charachter should shoot automatically while Fire1 is pressed
        if (shouldFire)
        {
            if (fireDelay <= 0)
            {
                Shoot();
                fireDelay = fireRate;

                if (damage > 20)
                {
                    audioManager.PlaySfx("RetroLaser", 0.2f);
                }
                else
                {
                    audioManager.PlaySfx("Gun");
                }
                
            }
        }
    } 

    public void OnPointerDown()
    {
        shouldFire = true;
    }

    public void OnPointerUp()
    {
        shouldFire = false;
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        BulletScript bs = bullet.GetComponent<BulletScript>();
        SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();

        sr.color = bulletColor;
        bs.damage = (int)(damage * playerStats.Damage());

        rb.AddForce(firePoint.up*bulletForce, ForceMode2D.Impulse);
    }
}
