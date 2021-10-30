﻿using System.IO;
using SCVE.Core;
using SCVE.Core.App;
using SCVE.Core.Primitives;
using SCVE.Core.Rendering;

namespace SCVE.Components.Legacy
{
    public class RectWithOrthoComponent : Component
    {
        private readonly VertexArray _vertexArray;
        private readonly ShaderProgram _shaderProgram;
        private ColorRgba _colorRgba;
        private ScveMatrix4X4 _ortho;

        public RectWithOrthoComponent(Rect rect, ColorRgba colorRgba)
        {
            _ortho = ScveMatrix4X4.CreateOrthographicOffCenter(-5, 5, -5, 5, -1, 1);
            _colorRgba = colorRgba;
            _vertexArray = Application.Instance.RenderEntitiesCreator.CreateVertexArray();

            var rectGeometry = GeometryGenerator.GenerateRect(rect);

            var buffer = Application.Instance.RenderEntitiesCreator.CreateVertexBuffer(rectGeometry.Vertices, BufferUsage.Static);

            buffer.Layout = new VertexBufferLayout(new()
            {
                new(VertexBufferElementType.Float3, "a_Position")
            });
            _vertexArray.AddVertexBuffer(buffer);

            var indexBuffer = Application.Instance.RenderEntitiesCreator.CreateIndexBuffer(rectGeometry.Indices, BufferUsage.Static);

            _vertexArray.SetIndexBuffer(indexBuffer);

            string vertexSrc = @"
            #version 330 core
			
            layout(location = 0) in vec3 a_Position;

            uniform mat4 u_Ortho;
            out vec3 v_Position;

            void main()
            {
                v_Position = a_Position;
                gl_Position = u_Ortho * vec4(a_Position, 1.0);	
            }
            ";

            string fragmentSrc = @"
            #version 330 core
			
            layout(location = 0) out vec4 color;

            in vec3 v_Position;

            uniform vec4 u_Color;

            void main()
            {
                color = u_Color;
            }
            ";

            if (true)
            {
                using var vertexShader = Application.Instance.RenderEntitiesCreator.CreateShader(vertexSrc, ScveShaderType.Vertex);
                using var fragmentShader = Application.Instance.RenderEntitiesCreator.CreateShader(fragmentSrc, ScveShaderType.Fragment);

                vertexShader.Compile();
                fragmentShader.Compile();

                _shaderProgram = Application.Instance.RenderEntitiesCreator.CreateProgram();

                _shaderProgram.AttachShader(vertexShader);
                _shaderProgram.AttachShader(fragmentShader);
                _shaderProgram.Link();

                _shaderProgram.DetachShader(vertexShader);
                _shaderProgram.DetachShader(fragmentShader);

                var binary = _shaderProgram.GetBinary();

                File.WriteAllBytes("assets/program_binary.bin", binary.Data);
            }
            else
            {
                _shaderProgram = Application.Instance.RenderEntitiesCreator.CreateProgram(File.ReadAllBytes("assets/program_binary.bin"), 0);
            }
        }

        public override void Render(IRenderer renderer)
        {
            _shaderProgram.SetVector4("u_Color", _colorRgba.R, _colorRgba.G, _colorRgba.B, _colorRgba.A);
            _shaderProgram.SetMatrix4("u_Ortho", _ortho);

            _shaderProgram.Bind();
            renderer.RenderWireframe(_vertexArray, _shaderProgram);

            for (var i = 0; i < Children.Count; i++)
            {
                Children[i].Render(renderer);
            }
        }
    }
}