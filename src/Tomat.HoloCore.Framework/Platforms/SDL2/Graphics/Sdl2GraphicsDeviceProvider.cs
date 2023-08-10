using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Tomat.HoloCore.Framework.Platform.Graphics;
using Tomat.HoloCore.Framework.Platform.Windowing;
using Veldrid;
using Veldrid.OpenGL;
using Veldrid.Sdl2;
using static Veldrid.Sdl2.Sdl2Native;
using Sdl2Window = Tomat.HoloCore.Framework.Platforms.SDL2.Windowing.Sdl2Window;

namespace Tomat.HoloCore.Framework.Platforms.SDL2.Graphics;

public class Sdl2GraphicsDeviceProvider : IGraphicsDeviceProvider {
    private static readonly (int, int)[] gl_test_versions = { (4, 6), (4, 3), (4, 0), (3, 3), (3, 0) };
    private static readonly (int, int)[] gles_test_versions = { (3, 2), (3, 0) };
    private static readonly object gl_version_lock = new();
    private static (int major, int minor)? glVersion;
    private static (int major, int minor)? glesVersion;

    public GraphicsDevice CreateGraphicsDevice(IWindow window, GraphicsDeviceOptions options, GraphicsBackend? preferredBackend = null) {
        if (window is not Sdl2Window sdl2Window)
            throw new ArgumentException("Window must be an SDL2 window.", nameof(window));

        var innerWindow = sdl2Window.innerWindow;

        preferredBackend ??= GetPlatformDefaultBackend();

        switch (preferredBackend.Value) {
            case GraphicsBackend.Direct3D11:
                return CreateD3D11GraphicsDevice(options, innerWindow);

            case GraphicsBackend.Vulkan:
                return CreateVulkanGraphicsDevice(options, innerWindow);

            case GraphicsBackend.OpenGL:
            case GraphicsBackend.OpenGLES:
                return CreateOpenGlGraphicsDevice(options, innerWindow, preferredBackend.Value);

            case GraphicsBackend.Metal:
                return CreateMetalGraphicsDevice(options, innerWindow);

            default:
                throw new ArgumentOutOfRangeException(nameof(preferredBackend), preferredBackend, null);
        }
    }

    private static GraphicsBackend GetPlatformDefaultBackend() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GraphicsBackend.Direct3D11;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal) ? GraphicsBackend.Metal : GraphicsBackend.OpenGL;

        return GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan) ? GraphicsBackend.Vulkan : GraphicsBackend.OpenGL;
    }

    private static GraphicsDevice CreateD3D11GraphicsDevice(GraphicsDeviceOptions options, Veldrid.Sdl2.Sdl2Window window) {
        var source = GetSwapchainSource(window);
        var description = new SwapchainDescription(
            source,
            (uint)window.Width,
            (uint)window.Height,
            options.SwapchainDepthFormat,
            options.SyncToVerticalBlank,
            options.SwapchainSrgbFormat
        );

        return GraphicsDevice.CreateD3D11(options, description);
    }

    private static GraphicsDevice CreateVulkanGraphicsDevice(GraphicsDeviceOptions options, Veldrid.Sdl2.Sdl2Window window, bool colorSrgb = false) {
        var swapchain = GetSwapchainSource(window);
        var description = new SwapchainDescription(
            swapchain,
            (uint)window.Width,
            (uint)window.Height,
            options.SwapchainDepthFormat,
            options.SyncToVerticalBlank,
            colorSrgb
        );

        return GraphicsDevice.CreateVulkan(options, description);
    }

    private static GraphicsDevice CreateMetalGraphicsDevice(GraphicsDeviceOptions options, Veldrid.Sdl2.Sdl2Window window, bool? colorSrgb = null) {
        colorSrgb ??= options.SwapchainSrgbFormat;

        var source = GetSwapchainSource(window);
        var description = new SwapchainDescription(
            source,
            (uint)window.Width,
            (uint)window.Height,
            options.SwapchainDepthFormat,
            options.SyncToVerticalBlank,
            colorSrgb.Value
        );

        return GraphicsDevice.CreateMetal(options, description);
    }

    private static unsafe GraphicsDevice CreateOpenGlGraphicsDevice(GraphicsDeviceOptions options, Veldrid.Sdl2.Sdl2Window window, GraphicsBackend backend) {
        SDL_ClearError();

        SDL_SysWMinfo wmInfo;
        SDL_GetVersion(&wmInfo.version);
        SDL_GetWMWindowInfo(window.SdlWindowHandle, &wmInfo);

        SetSdlGlContextAttributes(options, backend);

        var contextHandle = SDL_GL_CreateContext(window.SdlWindowHandle);
        var error = PtrToString(SDL_GetError());

        if (!string.IsNullOrEmpty(error)) {
            throw new InvalidOperationException(
                "Unable to create OpenGL context:\n"
              + $"{error}\n"
              + "This may indicate that the system does not support the requested OpenGL profile, version, or Swapchain format."
            );
        }

        int actualDepthSize;
        int actualStencilSize;
        SDL_GL_GetAttribute(SDL_GLAttribute.DepthSize, &actualDepthSize);
        SDL_GL_GetAttribute(SDL_GLAttribute.StencilSize, &actualStencilSize);

        SDL_GL_SetSwapInterval(options.SyncToVerticalBlank ? 1 : 0);

        var platformInfo = new OpenGLPlatformInfo(
            contextHandle,
            SDL_GL_GetProcAddress,
            context => SDL_GL_MakeCurrent(window.SdlWindowHandle, context),
            SDL_GL_GetCurrentContext,
            () => SDL_GL_MakeCurrent(new SDL_Window(nint.Zero), nint.Zero),
            SDL_GL_DeleteContext,
            () => SDL_GL_SwapWindow(window.SdlWindowHandle),
            sync => SDL_GL_SetSwapInterval(sync ? 1 : 0)
        );

        return GraphicsDevice.CreateOpenGL(options, platformInfo, (uint)window.Width, (uint)window.Height);
    }

    private static void SetSdlGlContextAttributes(GraphicsDeviceOptions options, GraphicsBackend backend) {
        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        var contextFlags = options.Debug ? SDL_GLContextFlag.Debug | SDL_GLContextFlag.ForwardCompatible : SDL_GLContextFlag.ForwardCompatible;

        SDL_GL_SetAttribute(SDL_GLAttribute.ContextFlags, (int)contextFlags);

        GetMaxGlVersion(backend == GraphicsBackend.OpenGLES, out var major, out var minor);

        if (backend == GraphicsBackend.OpenGL)
            SDL_GL_SetAttribute(SDL_GLAttribute.ContextProfileMask, (int)SDL_GLProfile.Core);
        else
            SDL_GL_SetAttribute(SDL_GLAttribute.ContextProfileMask, (int)SDL_GLProfile.ES);

        SDL_GL_SetAttribute(SDL_GLAttribute.ContextMajorVersion, major);
        SDL_GL_SetAttribute(SDL_GLAttribute.ContextMinorVersion, minor);

        var depthBits = 0;
        var stencilBits = 0;

        if (options.SwapchainDepthFormat.HasValue) {
            switch (options.SwapchainDepthFormat) {
                case PixelFormat.R16_UNorm:
                    depthBits = 16;
                    break;

                case PixelFormat.D24_UNorm_S8_UInt:
                    depthBits = 24;
                    stencilBits = 8;
                    break;

                case PixelFormat.R32_Float:
                    depthBits = 32;
                    break;

                case PixelFormat.D32_Float_S8_UInt:
                    depthBits = 32;
                    stencilBits = 8;
                    break;

                default:
                    throw new PlatformNotSupportedException("Invalid depth format: " + options.SwapchainDepthFormat);
            }
        }

        SDL_GL_SetAttribute(SDL_GLAttribute.DepthSize, depthBits);
        SDL_GL_SetAttribute(SDL_GLAttribute.StencilSize, stencilBits);

        SDL_GL_SetAttribute(SDL_GLAttribute.FramebufferSrgbCapable, options.SwapchainSrgbFormat ? 1 : 0);
    }

    private static unsafe SwapchainSource GetSwapchainSource(Veldrid.Sdl2.Sdl2Window window) {
        SDL_SysWMinfo wmInfo;
        SDL_GetVersion(&wmInfo.version);
        SDL_GetWMWindowInfo(window.SdlWindowHandle, &wmInfo);

        switch (wmInfo.subsystem) {
            case SysWMType.Windows: {
                var info = Unsafe.Read<Win32WindowInfo>(&wmInfo.info);
                return SwapchainSource.CreateWin32(info.Sdl2Window, info.hinstance);
            }

            case SysWMType.X11: {
                var info = Unsafe.Read<X11WindowInfo>(&wmInfo.info);
                return SwapchainSource.CreateXlib(info.display, info.Sdl2Window);
            }

            case SysWMType.Wayland: {
                var info = Unsafe.Read<WaylandWindowInfo>(&wmInfo.info);
                return SwapchainSource.CreateWayland(info.display, info.surface);
            }

            case SysWMType.Cocoa: {
                var info = Unsafe.Read<CocoaWindowInfo>(&wmInfo.info);
                return SwapchainSource.CreateNSWindow(info.Window);
            }

            default:
                throw new PlatformNotSupportedException("Cannot create a SwapchainSource for the subsystem type: " + wmInfo.subsystem);
        }
    }

    private static void GetMaxGlVersion(bool es, out int major, out int minor) {
        lock (gl_version_lock) {
            if (es && glesVersion.HasValue) {
                major = glesVersion.Value.major;
                minor = glesVersion.Value.minor;
            }
            else if (!es && glVersion.HasValue) {
                major = glVersion.Value.major;
                minor = glVersion.Value.minor;
            }
            else {
                TestMaxGlVersion(es, out major, out minor);

                if (es)
                    glesVersion = (major, minor);
                else
                    glVersion = (major, minor);
            }
        }
    }

    private static void TestMaxGlVersion(bool es, out int major, out int minor) {
        var testVersions = es ? gles_test_versions : gl_test_versions;

        major = 0;
        minor = 0;

        foreach (var (testMajor, testMinor) in testVersions) {
            if (!TestIndividualGlVersion(es, testMinor, testMinor))
                continue;

            major = testMajor;
            minor = testMinor;
        }
    }

    private static unsafe bool TestIndividualGlVersion(bool es, int major, int minor) {
        var profileMask = es ? SDL_GLProfile.ES : SDL_GLProfile.Core;

        SDL_GL_SetAttribute(SDL_GLAttribute.ContextProfileMask, (int)profileMask);
        SDL_GL_SetAttribute(SDL_GLAttribute.ContextMajorVersion, major);
        SDL_GL_SetAttribute(SDL_GLAttribute.ContextMinorVersion, minor);

        var window = SDL_CreateWindow(string.Empty, 0, 0, 1, 1, SDL_WindowFlags.Hidden | SDL_WindowFlags.OpenGL);
        var error = PtrToString(SDL_GetError());

        if (window.NativePointer == nint.Zero || !string.IsNullOrEmpty(error)) {
            SDL_ClearError();
            // TODO: Unable to create version {major}.{minor} {profileMask}
            // context.
            return false;
        }

        var context = SDL_GL_CreateContext(window);
        error = PtrToString(SDL_GetError());

        if (!string.IsNullOrEmpty(error)) {
            SDL_ClearError();
            // TODO: Unable to create version {major}.{minor} {profileMask}
            // context.
            SDL_DestroyWindow(window);
            return false;
        }

        SDL_GL_DeleteContext(context);
        SDL_DestroyWindow(window);
        return true;
    }

    private static unsafe string PtrToString(byte* pStr) {
        var len = 0;
        while (pStr[len] != 0)
            len++;
        return Encoding.UTF8.GetString(pStr, len);
    }
}
