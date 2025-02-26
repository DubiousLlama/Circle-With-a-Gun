using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    float bonus = 0f;
    float moveBonusTime = 0f;
    public Rigidbody2D rb;
    public Camera cam;
    public Joystick movementJoystick;
    public Joystick directionJoystick;

    [HideInInspector]
    public Vector2 movement;
    Vector2 lookDir;

    ICollection<string> slowIDs = new List<string>();

    // Update is called once per frame
    void Update()
    {

        rb = GetComponent<Rigidbody2D>();

        if (movementJoystick.Horizontal != 0 || movementJoystick.Vertical != 0)
        {
            movement.x = movementJoystick.Horizontal;
            movement.y = movementJoystick.Vertical;
        }
        else
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
        }

        if (directionJoystick.Horizontal != 0 || directionJoystick.Vertical != 0)
        {
            lookDir.x = directionJoystick.Horizontal;
            lookDir.y = directionJoystick.Vertical;
        }
        else
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            lookDir = mousePos - rb.position;
        }
    }

    void FixedUpdate()
    {

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        rb.AddForce(movement.normalized * (moveSpeed + bonus));

        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg + 90f;

        rb.rotation = angle;


        if (bonus > 0)
        {
            bonus = bonus - (float)Math.Pow(3, moveBonusTime - 10f);
            moveBonusTime += 0.1f;
        }
        if (bonus <= 0)
        {
            bonus = 0;
            moveBonusTime = 0;
        }
    }

    public void Slow(float slowAmount, string slowID, float slowDuration)
    {
        if (slowIDs.Contains(slowID))
        {
            return;
        }
        else
        {
            slowIDs.Add(slowID);
            moveSpeed = moveSpeed * slowAmount;
            StartCoroutine(RemoveSlow(slowAmount, slowID, slowDuration));
        }
    }

    public void SetSpeedBonus(float amount)
    {
        bonus = amount;
    }

    IEnumerator RemoveSlow(float slowAmount, string slowID, float slowDuration)
    {
        yield return new WaitForSeconds(slowDuration);
        slowIDs.Remove(slowID);
        moveSpeed = moveSpeed * (1 / slowAmount);
    }
}
