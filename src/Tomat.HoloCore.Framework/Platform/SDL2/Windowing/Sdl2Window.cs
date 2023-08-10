using Tomat.HoloCore.Framework.Platform.API.Windowing;
using Veldrid;

namespace Tomat.HoloCore.Framework.Platform.SDL2.Windowing;

public class Sdl2Window : IWindow {
    public int X {
        get => InnerWindow.X;
        set => InnerWindow.X = value;
    }

    public int Y {
        get => InnerWindow.Y;
        set => InnerWindow.Y = value;
    }

    public int Width {
        get => InnerWindow.Width;
        set => InnerWindow.Width = value;
    }

    public int Height {
        get => InnerWindow.Height;
        set => InnerWindow.Height = value;
    }

    public string Title {
        get => InnerWindow.Title;
        set => InnerWindow.Title = value;
    }

    public WindowState State {
        get => InnerWindow.WindowState;
        set => InnerWindow.WindowState = value;
    }

    public bool Exists => InnerWindow.Exists;

    public void PumpEvents() {
        InnerWindow.PumpEvents();
    }

    internal Veldrid.Sdl2.Sdl2Window InnerWindow { get; }

    public Sdl2Window(Veldrid.Sdl2.Sdl2Window innerWindow) {
        this.InnerWindow = innerWindow;
    }
}
