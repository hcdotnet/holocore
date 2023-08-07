using Tomat.HoloCore.Framework.Windowing;
using Veldrid;

namespace Tomat.HoloCore.Framework.Platforms.SDL2;

public class Sdl2Window : IWindow {
    public int X {
        get => innerWindow.X;
        set => innerWindow.X = value;
    }

    public int Y {
        get => innerWindow.Y;
        set => innerWindow.Y = value;
    }

    public int Width {
        get => innerWindow.Width;
        set => innerWindow.Width = value;
    }

    public int Height {
        get => innerWindow.Height;
        set => innerWindow.Height = value;
    }

    public string Title {
        get => innerWindow.Title;
        set => innerWindow.Title = value;
    }

    public WindowState State {
        get => innerWindow.WindowState;
        set => innerWindow.WindowState = value;
    }

    public bool Exists => innerWindow.Exists;

    public void PumpEvents() {
        innerWindow.PumpEvents();
    }

    internal readonly Veldrid.Sdl2.Sdl2Window innerWindow;

    public Sdl2Window(Veldrid.Sdl2.Sdl2Window innerWindow) {
        this.innerWindow = innerWindow;
    }
}
