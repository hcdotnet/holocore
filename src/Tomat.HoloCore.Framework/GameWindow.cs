using System;
using Tomat.HoloCore.Framework.DependencyInjection;
using Tomat.HoloCore.Framework.Platform.API.Windowing;
using Veldrid;
using IServiceProvider = Tomat.HoloCore.Framework.DependencyInjection.IServiceProvider;

namespace Tomat.HoloCore.Framework;

public class GameWindow : IDisposable {
    public IWindow Window { get; init; } = null!;

    public GraphicsDevice GraphicsDevice { get; init; } = null!;

    public IServiceProvider ServiceProvider { get; } = new ServiceProvider();

    public event Action<GameWindow>? OnInitialize;

    public event Action<GameWindow>? OnUpdate;

    public event Action<GameWindow>? OnExit;

    private bool isInitialized;

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

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            OnExit?.Invoke(this);

            GraphicsDevice.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
