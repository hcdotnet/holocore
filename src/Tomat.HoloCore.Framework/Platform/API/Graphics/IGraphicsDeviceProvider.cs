using Tomat.HoloCore.Framework.Platform.API.Windowing;
using Veldrid;

namespace Tomat.HoloCore.Framework.Platform.API.Graphics;

public interface IGraphicsDeviceProvider {
    GraphicsDevice CreateGraphicsDevice(IWindow window, GraphicsDeviceOptions options, GraphicsBackend? preferredBackend = null);
}
