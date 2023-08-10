namespace Tomat.HoloCore.Framework.Platform.API.Windowing;

public interface IWindowProvider {
    IWindow CreateWindow(WindowCreationInfo creationInfo);
}
