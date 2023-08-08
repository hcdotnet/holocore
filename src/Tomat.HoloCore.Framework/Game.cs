using System;
using System.Collections.Generic;
using Tomat.HoloCore.Framework.DependencyInjection;
using Tomat.HoloCore.Framework.Platform;
using Tomat.HoloCore.Framework.Platform.Graphics;
using Tomat.HoloCore.Framework.Platform.Windowing;
using Veldrid;
using IServiceProvider = Tomat.HoloCore.Framework.DependencyInjection.IServiceProvider;

namespace Tomat.HoloCore.Framework;

public abstract class Game {
    private List<GameWindow> windows = new();
    private IGameHost? host;
    protected bool hasHostBeenSet;

    public virtual IGameHost? Host {
        get => host;

        set {
            if (hasHostBeenSet)
                throw new InvalidOperationException("Cannot reinstall game to a different host.");

            host = value;
            hasHostBeenSet = true;
        }
    }

    public IReadonlyServiceProvider? Dependencies { get; private set; }

    private IServiceProvider serviceProvider = null!;

    public virtual void Initialize() { }

    public virtual void Run() {
        // TODO: Use threads instead...

        while (true) {
            for (var i = 0; i < windows.Count; i++) {
                var window = windows[i];
                window.Update();

                if (!window.Window.Exists) {
                    window.Dispose();
                    windows.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    public virtual void RegisterDependencies(IReadonlyServiceProvider? parentServiceProvider) {
        Dependencies = CreateServiceProvider(parentServiceProvider);
    }

    protected virtual T CreateWindow<T>(WindowCreationInfo windowCreationInfo, GraphicsDeviceOptions graphicsDeviceOptions, GraphicsBackend? preferredBackend = null) where T : GameWindow, new() {
        if (Dependencies is null)
            throw new InvalidOperationException("Attempted to create window before dependencies were registered.");

        var window = serviceProvider.ExpectService<IWindowProvider>().CreateWindow(windowCreationInfo);
        var graphicsDevice = Dependencies.ExpectService<IGraphicsDeviceProvider>().CreateGraphicsDevice(window, graphicsDeviceOptions, preferredBackend);
        var gameWindow = new T {
            Window = window,
            GraphicsDevice = graphicsDevice,
        };

        windows.Add(gameWindow);
        return gameWindow;
    }

    protected virtual IReadonlyServiceProvider CreateServiceProvider(IReadonlyServiceProvider? parentServiceProvider) {
        return serviceProvider = new DefaultServiceProvider(parentServiceProvider);
    }
}
