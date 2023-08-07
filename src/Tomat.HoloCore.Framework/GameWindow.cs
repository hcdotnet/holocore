using System;
using Tomat.HoloCore.Framework.DependencyInjection;
using Tomat.HoloCore.Framework.Platform.Windowing;
using Veldrid;
using IServiceProvider = Tomat.HoloCore.Framework.DependencyInjection.IServiceProvider;

namespace Tomat.HoloCore.Framework;

public class GameWindow : IDisposable {
    public IWindow Window { get; }

    public GraphicsDevice GraphicsDevice { get; }

    public IServiceProvider ServiceProvider { get; } = new DefaultServiceProvider();

    public event Action<GameWindow>? OnInitialize;

    public event Action<GameWindow>? OnUpdate;

    public event Action<GameWindow>? OnExit;

    private bool isInitialized;

    public GameWindow(IWindow window, GraphicsDevice graphicsDevice) {
        Window = window;
        GraphicsDevice = graphicsDevice;
    }

    public virtual void Initialize() {
        OnInitialize?.Invoke(this);
    }

    public virtual void Update() {
        if (!isInitialized) {
            Initialize();
            isInitialized = true;
        }

        Window.PumpEvents();

        OnUpdate?.Invoke(this);
    }

    public void Dispose() {
        OnExit?.Invoke(this);

        GraphicsDevice.Dispose();
    }
}
