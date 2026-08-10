using System;
using UnityEngine;

/// <summary>
/// PlayerPrefs-backed crosshair / aim-line opacity shared by the pause menu and gameplay visuals.
/// </summary>
public static class CrosshairOpacity
{
    public const string PrefsKey = "crosshairOpacity";
    public const float Default = 0.5f;

    public static event Action<float> Changed;

    public static float Get() => PlayerPrefs.GetFloat(PrefsKey, Default);

    public static void Set(float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PrefsKey, value);
        Changed?.Invoke(value);
    }
}
