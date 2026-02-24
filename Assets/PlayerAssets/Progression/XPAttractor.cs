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

    private ContactFilter2D xpContactFilter;
    private Collider2D[] colliderBuffer = new Collider2D[128];
    private Dictionary<int, XPPickup> xpPickupCache = new Dictionary<int, XPPickup>();

    void Start()
    {
        xpContactFilter.SetLayerMask(LayerMask.GetMask("XP"));
        xpContactFilter.useTriggers = true;
    }

    void FixedUpdate()
    {
        float attractionRadius = PlayerStats.instance.GetStatMod(StatTypes.AttractorRadius) * attractionStrength;

        int count = Physics2D.OverlapCircle(transform.position, attractionRadius, xpContactFilter, colliderBuffer);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = colliderBuffer[i];

            Vector2 direction = (transform.position - col.transform.position).normalized;
            float distance = Vector2.Distance(transform.position, col.transform.position);
            
            int instanceId = col.GetInstanceID();
            if (!xpPickupCache.TryGetValue(instanceId, out XPPickup xpPickup))
            {
                xpPickup = col.gameObject.GetComponent<XPPickup>();
                if (xpPickup != null)
                    xpPickupCache[instanceId] = xpPickup;
            }

            float orbScale = (xpPickup != null) ? col.transform.localScale.x / xpPickup.baseSize : 1f;
            float weight = Mathf.Max(1f, orbScale * weightFactor);
            
            float weightRadius = attractionRadius / weight;
            float effectiveRadius = Mathf.Clamp(weightRadius, attractionRadius * minRadiusMultiplier, attractionRadius);

            float baseAttractionSpeed = 1f / weight;
            float maxSpeed = 16f / weight;

            if (distance > effectiveRadius)
                maxSpeed *= 0.25f;
                baseAttractionSpeed *= 0.5f;

            float normalizedDistance = distance / effectiveRadius;
            
            float exponentialFactor = 1f - Mathf.Pow(normalizedDistance, 1.1f);
            float attractionSpeed = Mathf.Lerp(baseAttractionSpeed, maxSpeed, exponentialFactor);
            
            col.transform.position += (Vector3)(direction * attractionSpeed * Time.fixedDeltaTime);
        }
    }
}
