using Tomat.HoloCore.Framework.Platform.Windowing;
using Veldrid;

namespace Tomat.HoloCore.Framework.Platform.Graphics;

public interface IGraphicsDeviceProvider {
    GraphicsDevice CreateGraphicsDevice(IWindow window, GraphicsDeviceOptions options, GraphicsBackend? preferredBackend = null);
}
