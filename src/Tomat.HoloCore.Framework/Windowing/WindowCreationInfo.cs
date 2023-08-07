using Veldrid;

namespace Tomat.HoloCore.Framework.Windowing;

public record struct WindowCreationInfo(
    int X,
    int Y,
    int Width,
    int Height,
    string Title,
    WindowState State = WindowState.Normal
);
