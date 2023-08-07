namespace Tomat.HoloCore.Framework.Windowing;

public interface IWindowProvider {
    IWindow CreateWindow(WindowCreationInfo creationInfo);
}
