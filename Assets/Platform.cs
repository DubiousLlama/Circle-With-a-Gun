using UnityEngine;

public class Platform
{
    private static PlatformType? hardcodedPlatform = null;

    // Enum for platform types
    public enum PlatformType
    {
        Mobile,
        Desktop
    }
    // Method to determine the platform
    public static PlatformType GetPlatform()
    {
        // Return the hardcoded platform if set
        if (hardcodedPlatform.HasValue)
        {
            return hardcodedPlatform.Value;
        }

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

            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.LinuxEditor:
                return PlatformType.Mobile; // Change this as needed

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
