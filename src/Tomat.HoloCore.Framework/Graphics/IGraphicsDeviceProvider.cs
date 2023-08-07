using Tomat.HoloCore.Framework.Windowing;
using Veldrid;

namespace Tomat.HoloCore.Framework.Graphics;

public interface IGraphicsDeviceProvider {
    GraphicsDevice CreateGraphicsDevice(IWindow window, GraphicsDeviceOptions options, GraphicsBackend? preferredBackend = null);
}
