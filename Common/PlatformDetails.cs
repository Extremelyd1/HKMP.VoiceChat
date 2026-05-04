using UnityEngine;

namespace HkmpVoiceChat.Common;

/// <summary>
/// Provides access to platform details.
/// </summary>
public static class PlatformDetails {
    /// <summary>
    /// Static boolean that indicates whether the platform is MacOS.
    /// </summary>
    public static bool IsMac => SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX;

    /// <summary>
    /// Static boolean that indicates whether the platform is Windows.
    /// </summary>
    public static bool IsWindows => SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows;
}