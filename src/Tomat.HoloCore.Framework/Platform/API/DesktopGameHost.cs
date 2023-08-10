using System;
using System.Runtime.InteropServices;
using Tomat.HoloCore.Framework.DependencyInjection;
using Tomat.HoloCore.Framework.Platform.API.Graphics;
using Tomat.HoloCore.Framework.Platform.API.Windowing;
using Tomat.HoloCore.Framework.Platform.Linux;
using Tomat.HoloCore.Framework.Platform.MacOS;
using Tomat.HoloCore.Framework.Platform.SDL2.Graphics;
using Tomat.HoloCore.Framework.Platform.SDL2.Windowing;
using Tomat.HoloCore.Framework.Platform.Windows;

namespace Tomat.HoloCore.Framework.Platform.API;

public abstract class DesktopGameHost : IGameHost {
    public virtual void InstallGame(Game game) {
        game.Host = this;

        var dependencies = new ServiceProvider();
        dependencies.Register<IGameHost>(this);
        dependencies.Register(game);

        // TODO: Currently we only support SDL2, do we have plans to ever
        // support anything else?
        dependencies.Register<IWindowProvider>(new Sdl2WindowProvider());
        dependencies.Register<IGraphicsDeviceProvider>(new Sdl2GraphicsDeviceProvider());

        game.RegisterDependencies(dependencies);
    }

    public virtual void RunGame(Game game) {
        if (game.Host != this)
            throw new InvalidOperationException("Cannot run a game that is not installed to this host.");

        game.Initialize();
        game.Run();
    }

    public static IGameHost CreatePlatformHost() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsGameHost();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new MacOsGameHost();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxGameHost();

        throw new PlatformNotSupportedException("Unsupported platform, cannot create a desktop game host.");
    }
}
