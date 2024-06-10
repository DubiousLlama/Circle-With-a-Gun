using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Shooting : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;

    public int damage = 20;
    public float bulletForce = 20f;
    public float fireRate = 0.1f;
    float fireDelay = 0f;

    private Color bulletColor = new Color(0.8257952f, 0f, 1f);

    // Update is called once per frame
    void Update()
    {
        if (fireDelay > 0)
        {
            fireDelay -= Time.deltaTime;
        }

        // The charachter should shoot automatically while Fire1 is pressed
        if (Input.GetButton("Fire1"))
        {
            if (fireDelay <= 0)
            {
                Shoot();
                fireDelay = fireRate;
            }
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        BulletScript bs = bullet.GetComponent<BulletScript>();
        SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();

        sr.color = bulletColor;
        bs.damage = damage;

        rb.AddForce(firePoint.up*bulletForce, ForceMode2D.Impulse);
    }
}
