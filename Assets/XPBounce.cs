using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPBounce : MonoBehaviour
{
    [Header("Bounce Settings")]
    [Tooltip("Base scale of the object")]
    public Vector3 baseScale = Vector3.one;
    
    [Tooltip("How much larger the object gets (multiplier)")]
    [Range(0.01f, 1f)]
    public float bounceAmount = 0.1f;
    
    [Tooltip("Speed of the bounce animation")]
    [Range(0.5f, 10f)]
    public float bounceSpeed = 2f;
    
    private Vector3 targetScale;
    private float timeOffset;

    // Start is called before the first frame update
    void Start()
    {
        // Store the initial scale as the base scale if not set
        if (baseScale == Vector3.one && transform.localScale != Vector3.one)
        {
            baseScale = transform.localScale;
        }
        
        // Add a random time offset so multiple XP orbs don't bounce in sync
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the bounce using a sine wave for smooth animation
        float bounceValue = Mathf.Sin((Time.time + timeOffset) * bounceSpeed);
        
        // Convert the sine wave (-1 to 1) to a scale multiplier (1 to 1+bounceAmount)
        float scaleMultiplier = 1f + (bounceValue * bounceAmount);
        
        // Apply the scale
        targetScale = baseScale * scaleMultiplier;
        transform.localScale = targetScale;
    }
}
