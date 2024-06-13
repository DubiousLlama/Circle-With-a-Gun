using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Camera cam;

    Vector2 movement;
    Vector2 mousePos;

    ICollection<string> slowIDs = new List<string>();

    PlayerStats playerStats;

    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        playerStats = GetComponent<PlayerStats>();

        rb = GetComponent<Rigidbody2D>();



    }

    void FixedUpdate() {

        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        rb.AddForce(movement.normalized * moveSpeed * playerStats.Speed());

        Vector2 LookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(LookDir.y, LookDir.x) * Mathf.Rad2Deg + 90f;

        rb.rotation = angle;
    }

    public void Slow(float slowAmount, string slowID, float slowDuration)
    {
        if (slowIDs.Contains(slowID))
        {
            return;
        } else
        {
            slowIDs.Add(slowID);
            moveSpeed = moveSpeed * slowAmount;
            StartCoroutine(RemoveSlow(slowAmount, slowID, slowDuration));
        } 
    }

    IEnumerator RemoveSlow(float slowAmount, string slowID, float slowDuration)
    {
        yield return new WaitForSeconds(slowDuration);
        slowIDs.Remove(slowID);
        moveSpeed = moveSpeed * (1 / slowAmount);
    }
}
