using System;
using System.Buffers;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ArtsTech.HugePageCollections;

partial class HugePages
{
    [SupportedOSPlatform("linux")]
    private static bool IsHugePageAllowLinux()
    {
        const string hugePageSettingsPath = "/proc/sys/vm/nr_hugepages";
        if (!File.Exists(hugePageSettingsPath))
            return false;
        
        if (!int.TryParse(File.ReadAllText(hugePageSettingsPath), out int totalPages))
            return false;
        
        return totalPages > 1;
    }
    
    
}