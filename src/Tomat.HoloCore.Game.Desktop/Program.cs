using System;
using System.Numerics;
using System.Text;
using Tomat.HoloCore.Framework.Platforms.SDL2;
using Tomat.HoloCore.Framework.Windowing;
using Veldrid;
using Veldrid.SPIRV;

namespace Tomat.HoloCore.Game.Desktop;

internal static class Program {
    public readonly struct VertexPositionColor {
        public const uint SIZE = 24;

        public Vector2 Position { get; }

        public RgbaFloat Color { get; }

        public VertexPositionColor(Vector2 position, RgbaFloat color) {
            Position = position;
            Color = color;
        }
    }

    private static readonly string vertex_code = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = Color;
}".Trim();

    private static readonly string fragment_code = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}".Trim();

    internal static void Main() {
        var windowProvider = new Sdl2WindowProvider();
        var graphicsDeviceProvider = new Sdl2GraphicsDeviceProvider();

        var windowInfo = new WindowCreationInfo {
            X = 100,
            Y = 100,
            Width = 960,
            Height =  540,
            Title = "Test",
        };
        var window = windowProvider.CreateWindow(windowInfo);

        var gdOptions = new GraphicsDeviceOptions {
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true,
        };
        var graphicsDevice = graphicsDeviceProvider.CreateGraphicsDevice(window, gdOptions);

        var (pipeline, commandList, vertexBuffer, indexBuffer, shaders) = CreateResources(graphicsDevice);

        while (window.Exists) {
            window.PumpEvents();
            Draw(graphicsDevice, commandList, pipeline, vertexBuffer, indexBuffer);
        }

        DisposeResources(graphicsDevice, pipeline, commandList, vertexBuffer, indexBuffer, shaders);
    }

    private static (Pipeline, CommandList, DeviceBuffer, DeviceBuffer, Shader[]) CreateResources(GraphicsDevice graphicsDevice) {
        var factory = graphicsDevice.ResourceFactory;
        const int quad_count = 4;
        var quadVertices = new VertexPositionColor[quad_count] {
            new VertexPositionColor(new Vector2(-0.75f,  0.75f), RgbaFloat.Red),
            new VertexPositionColor(new Vector2( 0.75f,  0.75f), RgbaFloat.Green),
            new VertexPositionColor(new Vector2(-0.75f, -0.75f), RgbaFloat.Blue),
            new VertexPositionColor(new Vector2( 0.75f, -0.75f), RgbaFloat.Yellow),
        };
        var quadIndices = new ushort[quad_count] { 0, 1, 2, 3 };

        var vertexBuffer = factory.CreateBuffer(
            new BufferDescription(quad_count * VertexPositionColor.SIZE, BufferUsage.VertexBuffer)
        );
        graphicsDevice.UpdateBuffer(vertexBuffer, 0, quadVertices);

        var indexBuffer = factory.CreateBuffer(
            new BufferDescription(quad_count * sizeof(ushort), BufferUsage.IndexBuffer)
        );
        graphicsDevice.UpdateBuffer(indexBuffer, 0, quadIndices);

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
        );

        var vertexShaderDesc = new ShaderDescription(
            ShaderStages.Vertex,
            Encoding.UTF8.GetBytes(vertex_code),
            "main"
        );
        var fragmentShaderDesc = new ShaderDescription(
            ShaderStages.Fragment,
            Encoding.UTF8.GetBytes(fragment_code),
            "main"
        );

        var shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

        var pipelineDescription = new GraphicsPipelineDescription {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new DepthStencilStateDescription {
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                DepthComparison = ComparisonKind.LessEqual,
            },
            RasterizerState = new RasterizerStateDescription {
                CullMode = FaceCullMode.Back,
                FillMode = PolygonFillMode.Solid,
                FrontFace = FrontFace.Clockwise,
                DepthClipEnabled = true,
                ScissorTestEnabled = false,
            },
            PrimitiveTopology = PrimitiveTopology.TriangleStrip,
            ResourceLayouts = Array.Empty<ResourceLayout>(),
            ShaderSet = new ShaderSetDescription(
                vertexLayouts: new[] { vertexLayout },
                shaders: shaders
            ),
            Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription,
        };
        var pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        var commandList = factory.CreateCommandList();

        return (pipeline, commandList, vertexBuffer, indexBuffer, shaders);
    }

    private static void Draw(GraphicsDevice graphicsDevice, CommandList commandList, Pipeline pipeline, DeviceBuffer vertexBuffer, DeviceBuffer indexBuffer) {
        commandList.Begin();
        commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);

        commandList.ClearColorTarget(0, RgbaFloat.Black);

        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
        commandList.SetPipeline(pipeline);
        commandList.DrawIndexed(
            indexCount: 4,
            instanceCount: 1,
            indexStart: 0,
            vertexOffset: 0,
            instanceStart: 0
        );

        commandList.End();
        graphicsDevice.SubmitCommands(commandList);
        graphicsDevice.SwapBuffers();
    }

    private static void DisposeResources(GraphicsDevice graphicsDevice, Pipeline pipeline, CommandList commandList, DeviceBuffer vertexBuffer, DeviceBuffer indexBuffer, Shader[] shaders) {
        pipeline.Dispose();
        foreach (var shader in shaders)
            shader.Dispose();
        commandList.Dispose();
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
        graphicsDevice.Dispose();
    }
}
