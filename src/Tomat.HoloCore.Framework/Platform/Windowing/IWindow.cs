using Veldrid;

namespace Tomat.HoloCore.Framework.Platform.Windowing;

public interface IWindow {
    int X { get; set; }

    int Y { get; set; }

    int Width { get; set; }

    int Height { get; set; }

    string Title { get; set; }

    WindowState State { get; set; }

    bool Exists { get; }

    void PumpEvents();
}
