namespace Tomat.HoloCore.Framework.Platform.Windowing;

public interface IWindowProvider {
    IWindow CreateWindow(WindowCreationInfo creationInfo);
}
