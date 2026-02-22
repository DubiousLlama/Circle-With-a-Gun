using UnityEditor;
using UnityEngine;

public class Platform
{
    private static PlatformType? hardcodedPlatform = null;

    // Enum for platform types
    public enum PlatformType
    {
        Mobile,
        Desktop,
        Unset
    }

# pragma warning disable CS0414
    private static PlatformType platformCache = PlatformType.Unset;

    // Method to determine the platform
    public static PlatformType GetPlatform()
    {
        // Return the hardcoded platform if set
        if (hardcodedPlatform.HasValue)
        {
            return hardcodedPlatform.Value;
        }

        #if UNITY_EDITOR
            if (platformCache == PlatformType.Unset)
            {
                EditorConfig config = AssetDatabase.LoadAssetAtPath<EditorConfig>("Assets/EditorConfig.asset");
                platformCache = config.platform;
            }
            return platformCache;
        #endif

        // Disable unreachable code warnings
        #pragma warning disable CS0162
        // Determine platform based on Unity's runtime platform
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
                return PlatformType.Mobile;

            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.WebGLPlayer:
                return PlatformType.Desktop;

            default:
                Debug.LogWarning("Unknown platform. Defaulting to Desktop.");
                return PlatformType.Desktop;
        }
    }

    // Convenience methods
    public static bool IsMobile()
    {
        return GetPlatform() == PlatformType.Mobile;
    }

    public static bool IsDesktop()
    {
        return GetPlatform() == PlatformType.Desktop;
    }
}
