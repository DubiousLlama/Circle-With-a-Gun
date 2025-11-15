using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPAttractor : MonoBehaviour
{
    public float attractionStrength = 3f;
    [Header("Weight Settings")]
    [Tooltip("How much the orb's scale affects its weight (higher = more weight effect)")]
    public float weightFactor = 1f;
    [Tooltip("Minimum effective radius multiplier for heavy orbs")]
    public float minRadiusMultiplier = 0.4f;

    // Update is called once per frame
    void FixedUpdate()
    {
        float attractionRadius = PlayerStats.instance.GetStatMod(StatTypes.AttractorRadius) * attractionStrength;

        // Get all objects within the attraction radius on the 'XP' layer
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attractionRadius, LayerMask.GetMask("XP"));

        foreach (Collider2D col in colliders)
        {
            // Calculate direction towards the player
            Vector2 direction = (transform.position - col.transform.position).normalized;
            
            // Calculate distance to determine attraction strength
            float distance = Vector2.Distance(transform.position, col.transform.position);
            
            // Get the "weight" of the orb based on its scale
            float orbScale = col.transform.localScale.x / col.gameObject.GetComponent<XPPickup>().baseSize;
            float weight = Mathf.Max(1f, orbScale * weightFactor);
            
            // Calculate effective attraction radius (smaller for heavier orbs)
            float weightRadius = attractionRadius / weight;
            float effectiveRadius = Mathf.Clamp(weightRadius, attractionRadius * minRadiusMultiplier, attractionRadius);

            // Calculate attraction speed with exponential increase for closeness
            float baseAttractionSpeed = 1f / weight; // Heavier orbs move slower
            float maxSpeed = 16f / weight; // Max speed also reduced by weight

            // If outside the effective radius, decrease attraction speed significantly
            if (distance > effectiveRadius)
                maxSpeed *= 0.25f;
                baseAttractionSpeed *= 0.5f;

            // Normalize distance (0 = at player, 1 = at edge of effective radius)
            float normalizedDistance = distance / effectiveRadius;
            
            // Use exponential curve: closer objects get exponentially faster
            float exponentialFactor = 1f - Mathf.Pow(normalizedDistance, 1.1f);
            float attractionSpeed = Mathf.Lerp(baseAttractionSpeed, maxSpeed, exponentialFactor);
            
            // Move the XP object towards the player
            col.transform.position += (Vector3)(direction * attractionSpeed * Time.fixedDeltaTime);
        }
    }
}
