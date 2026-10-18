using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ArtsTech.HugePageCollections;

public static partial class HugePages
{
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("windows")]
    public static bool IsHugePageAllow()
    {
        if (OperatingSystem.IsLinux())
            return IsHugePageAllowLinux();
        return false;
    }
}