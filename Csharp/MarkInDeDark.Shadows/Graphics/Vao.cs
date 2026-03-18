using System;
using Microsoft.Xna.Framework.Graphics;

namespace MarkInDeDark.Shadows.Graphics
{
    internal abstract class Vao : IDisposable
    {
        protected Vao(VertexDeclaration vertexDeclaration, PrimitiveType primitiveTopology)
        {
            VertexDeclaration = vertexDeclaration;
            PrimitiveTopology = primitiveTopology;
        }

        public VertexBuffer VertexBuffer { get; protected set; }
        public IndexBuffer IndexBuffer { get; protected set; }
        public int PrimitiveCount { get; private set; }
        public VertexDeclaration VertexDeclaration { get; }
        public PrimitiveType PrimitiveTopology { get; }
        public bool HasIndices => IndexBuffer != null;
        public virtual int VertexCount => VertexBuffer.VertexCount;
        public virtual int IndexCount => IndexBuffer.IndexCount;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                VertexBuffer.Dispose();
                IndexBuffer?.Dispose();
            }
        }

        protected void CalculatePrimitiveCount()
        {
            PrimitiveCount = PrimitiveTopology switch
            {
                PrimitiveType.TriangleStrip => VertexCount - 2,
                PrimitiveType.TriangleList => HasIndices ? IndexCount / 3 : VertexCount / 3,
                _ => throw new NotImplementedException(),
            };
        }
    }

    internal static class VaoExtensions
    {
        public static void SetVertexArrayObject(this GraphicsDevice device, Vao vao)
        {
            device.SetVertexBuffer(vao.VertexBuffer);
            if (vao.HasIndices)
                device.Indices = vao.IndexBuffer;
        }
    }
}
