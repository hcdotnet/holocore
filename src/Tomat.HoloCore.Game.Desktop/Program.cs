using Tomat.HoloCore.Framework.Platform;

namespace Tomat.HoloCore.Game.Desktop;

internal static class Program {
    internal static void Main() {
        var game = new HoloCoreDesktopGame();
        var host = DesktopGameHost.CreatePlatformHost();
        host.InstallAndRunGame(game);
    }
}
