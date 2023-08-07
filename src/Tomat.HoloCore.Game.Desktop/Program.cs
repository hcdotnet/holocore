using Tomat.HoloCore.Framework.Platform;

namespace Tomat.HoloCore.Game.Desktop;

internal static class Program {
    internal static void Main() {
        var game = new HoloCoreDesktopGame();
        var host = Host.CreatePlatformHost();
        host.InstallAndRunGame(game);
    }
}
