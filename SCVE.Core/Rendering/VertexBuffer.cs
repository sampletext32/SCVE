﻿using System;

namespace SCVE.Core.Rendering
{
    /// <summary>
    /// Representation of an array in Graphics Engine (aka OpenGL)
    /// </summary>
    public abstract class VertexBuffer : IRenderEntity, IDisposable
    {
        public int Id { get; protected set; }
        
        public VertexBufferLayout Layout { get; set; }
        
        public abstract int GetSize();

        public abstract void Bind();

        public abstract void Unbind();

        public abstract void Dispose();
    }
}