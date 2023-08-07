using System;
using System.Collections.Generic;
using Tomat.HoloCore.Framework.DependencyInjection;
using Tomat.HoloCore.Framework.Platform.Graphics;
using Tomat.HoloCore.Framework.Platform.Windowing;
using Tomat.HoloCore.Framework.Platforms.SDL2.Graphics;
using Tomat.HoloCore.Framework.Platforms.SDL2.Windowing;

namespace Tomat.HoloCore.Framework.Platform;

public abstract class DesktopGameHost : IGameHost {
    public virtual void InstallGame(Game game) {
        game.Host = this;

        var dependencies = new DefaultServiceProvider();
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
}
