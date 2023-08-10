using Tomat.HoloCore.Framework.Platform;
using Tomat.HoloCore.Framework.Platform.API;

namespace Tomat.HoloCore.Game.Desktop;

internal static class Program {
    internal static void Main() {
        var game = new HoloCoreDesktopGame();
        var host = DesktopGameHost.CreatePlatformHost();
        host.InstallAndRunGame(game);
    }
}
