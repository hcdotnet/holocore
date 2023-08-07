namespace Tomat.HoloCore.Framework.Platform;

public interface IGameHost {
    void InstallGame(Game game);

    void RunGame(Game game);
}

public static class GameHostExtensions {
    public static void InstallAndRunGame(this IGameHost host, Game game) {
        host.InstallGame(game);
        host.RunGame(game);
    }
}
