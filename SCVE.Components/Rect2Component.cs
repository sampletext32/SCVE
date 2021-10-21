﻿using SCVE.Core;
using SCVE.Core.App;
using SCVE.Core.Primitives;
using SCVE.Core.Rendering;

namespace SCVE.Components
{
    public class Rect2Component : Component
    {
        private readonly VertexArray _vertexArray;
        private readonly Program _program;
        private ColorRgba _colorRgba;

        public Rect2Component(ColorRgba colorRgba)
        {
            _colorRgba = colorRgba;
            _vertexArray = Application.Instance.RenderEntitiesCreator.CreateVertexArray();

            // var rectGeometry = GeometryGenerator.GenerateRect(Rect);
            var rectGeometry = GeometryGenerator.GeneratePositiveUnitSquare();

            var buffer = Application.Instance.RenderEntitiesCreator.CreateVertexBuffer(rectGeometry.Vertices);

            buffer.Layout = new VertexBufferLayout(new()
            {
                new(VertexBufferElementType.Float3, "a_Position")
            });
            _vertexArray.AddVertexBuffer(buffer);

            var indexBuffer = Application.Instance.RenderEntitiesCreator.CreateIndexBuffer(rectGeometry.Indices);

            _vertexArray.SetIndexBuffer(indexBuffer);

            _program = Application.Instance.ShaderProgramCache.LoadOrCache("FlatColor_MVP_Uniform");
        }

        public override void Render(IRenderer renderer)
        {
            _program.SetVector4("u_Color", _colorRgba.R, _colorRgba.G, _colorRgba.B, _colorRgba.A);
            
            _program.SetMatrix4("u_Model",
                ModelMatrix
            );
            _program.SetMatrix4("u_View",
                Application.Instance.ViewProjectionAccessor.ViewMatrix
            );
            _program.SetMatrix4("u_Proj",
                Application.Instance.ViewProjectionAccessor.ProjectionMatrix
            );
            _program.Bind();

            renderer.RenderSolid(_vertexArray);

            for (var i = 0; i < Children.Count; i++)
            {
                Children[i].Render(renderer);
            }
        }
    }
}