using UnityEngine;
using UnityEngine.UI;

public class SmashableObject : MonoBehaviour
{
    public GameObject smashEffect;

    private void Start()
    {
        if ( gameObject.name == "GR")
        {
            Invoke(nameof(SpawnParticles), 0.4f);
        }
    }

    public void SpawnParticles(float accel)
    {
        Vector3 offset = new Vector3(0, 0.5f, 0);
        GameObject s = Instantiate(smashEffect, transform.position + offset, Quaternion.identity);
        ParticleSystem ps = s.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        main.duration *= accel;
        main.startLifetime = new ParticleSystem.MinMaxCurve(main.startLifetime.constantMin * accel, main.startLifetime.constantMax * accel);
        main.startSpeed = new ParticleSystem.MinMaxCurve(main.startSpeed.constantMin * (1/accel), main.startSpeed.constantMax * (1/accel));

        ps.Play();
        
        // Destroy the particle system GameObject after the animation completes
        float duration = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(s, duration);
    }
}