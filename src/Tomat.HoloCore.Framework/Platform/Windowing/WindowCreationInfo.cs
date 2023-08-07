using Veldrid;

namespace Tomat.HoloCore.Framework.Platform.Windowing;

public record struct WindowCreationInfo(
    int X,
    int Y,
    int Width,
    int Height,
    string Title,
    WindowState State = WindowState.Normal
);
