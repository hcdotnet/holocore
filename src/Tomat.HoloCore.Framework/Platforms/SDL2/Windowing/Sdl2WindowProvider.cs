using System;
using Tomat.HoloCore.Framework.Platform.Windowing;
using Veldrid;
using Veldrid.Sdl2;

namespace Tomat.HoloCore.Framework.Platforms.SDL2.Windowing;

public class Sdl2WindowProvider : IWindowProvider {
    public IWindow CreateWindow(WindowCreationInfo creationInfo) {
        var flags = SDL_WindowFlags.OpenGL | SDL_WindowFlags.Resizable | GetWindowFlags(creationInfo.State);
        if (creationInfo.State != WindowState.Hidden)
            flags |= SDL_WindowFlags.Shown;

        return new Sdl2Window(
            new Veldrid.Sdl2.Sdl2Window(
                creationInfo.Title,
                creationInfo.X,
                creationInfo.Y,
                creationInfo.Width,
                creationInfo.Height,
                flags,
                false
            )
        );
    }

    private static SDL_WindowFlags GetWindowFlags(WindowState state) {
        return state switch {
            WindowState.Normal => 0,
            WindowState.FullScreen => SDL_WindowFlags.Fullscreen,
            WindowState.Maximized => SDL_WindowFlags.Maximized,
            WindowState.Minimized => SDL_WindowFlags.Minimized,
            WindowState.BorderlessFullScreen => SDL_WindowFlags.FullScreenDesktop,
            WindowState.Hidden => SDL_WindowFlags.Hidden,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
        };
    }
}
