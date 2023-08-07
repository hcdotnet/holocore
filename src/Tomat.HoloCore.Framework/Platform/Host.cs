using System;
using System.Runtime.InteropServices;
using Tomat.HoloCore.Framework.Platforms.Linux;
using Tomat.HoloCore.Framework.Platforms.MacOS;
using Tomat.HoloCore.Framework.Platforms.Windows;

namespace Tomat.HoloCore.Framework.Platform;

public static class Host {
    public static IGameHost CreatePlatformHost() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsGameHost();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new MacOsGameHost();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxGameHost();

        throw new PlatformNotSupportedException("Unsupported platform, cannot create a game host.");
    }
}
