using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPhysics : MonoBehaviour
{
    BulletScript bs;
    public GameObject hitEffect;

    void Start()
    {
        bs = GetComponentInParent<BulletScript>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (bs.wallBounce)
        {

            Rigidbody2D rb = GetComponent<Rigidbody2D>();

            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            rb.rotation = angle - 90f;


            bs.damage += bs.damage / 2;
            bs.wallBounce = false;
            return;
        }

        hitEffect.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        GameObject he = Instantiate(hitEffect, collision.contacts[0].point, Quaternion.identity);
        Destroy(he, 0.5f);
    }
}
