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

    public IServiceProvider? Dependencies { get; private set; }

    protected virtual List<GameWindow> Windows { get; set; } = new();

    private IServiceProvider serviceProvider = null!;

    public virtual void Initialize() { }

    public virtual void Run() {
        // TODO: Use threads instead...

        while (Windows.Count > 0) {
            for (var i = 0; i < Windows.Count; i++) {
                var window = Windows[i];
                window.Update();

                if (!window.Window.Exists) {
                    window.Dispose();
                    Windows.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    public virtual void RegisterDependencies(IServiceProvider? parentServiceProvider) {
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

        Windows.Add(gameWindow);
        return gameWindow;
    }

    protected virtual IServiceProvider CreateServiceProvider(IServiceProvider? parentServiceProvider) {
        return serviceProvider = new ServiceProvider(parentServiceProvider);
    }
}
