using System;
using System.Numerics;
using System.Text;
using Tomat.HoloCore.Framework;
using Tomat.HoloCore.Framework.DependencyInjection;
using Veldrid;
using Veldrid.SPIRV;

namespace Tomat.HoloCore.Game;

public class HoloCoreGameWindow : GameWindow {
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

    public HoloCoreGameWindow() { }

    public override void Initialize() {
        base.Initialize();

        var factory = GraphicsDevice.ResourceFactory;
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
        GraphicsDevice.UpdateBuffer(vertexBuffer, 0, quadVertices);

        var indexBuffer = factory.CreateBuffer(
            new BufferDescription(quad_count * sizeof(ushort), BufferUsage.IndexBuffer)
        );
        GraphicsDevice.UpdateBuffer(indexBuffer, 0, quadIndices);

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
            Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription,
        };
        var pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        var commandList = factory.CreateCommandList();

        ServiceProvider.Register(pipeline);
        ServiceProvider.Register(commandList);
        ServiceProvider.Register(new[] { vertexBuffer, indexBuffer });
        ServiceProvider.Register(shaders);
    }

    public override void Update() {
        base.Update();

        var commandList = ServiceProvider.ExpectService<CommandList>();
        var pipeline = ServiceProvider.ExpectService<Pipeline>();
        var deviceBuffers = ServiceProvider.ExpectService<DeviceBuffer[]>();
        var vertexBuffer = deviceBuffers[0];
        var indexBuffer = deviceBuffers[1];

        commandList.Begin();
        commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);

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
        GraphicsDevice.SubmitCommands(commandList);
        GraphicsDevice.SwapBuffers();
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (!disposing)
            return;

        var pipeline = ServiceProvider.ExpectService<Pipeline>();
        var commandList = ServiceProvider.ExpectService<CommandList>();
        var deviceBuffers = ServiceProvider.ExpectService<DeviceBuffer[]>();
        var vertexBuffer = deviceBuffers[0];
        var indexBuffer = deviceBuffers[1];
        var shaders = ServiceProvider.ExpectService<Shader[]>();

        pipeline.Dispose();
        foreach (var shader in shaders)
            shader.Dispose();
        commandList.Dispose();
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }
}
