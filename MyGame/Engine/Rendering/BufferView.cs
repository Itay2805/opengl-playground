using System.Buffers;
using System.IO.MemoryMappedFiles;
using Silk.NET.OpenGL;
using Buffer = MyGame.Engine.Rendering.Buffer;

namespace MyGame.Engine;

public class BufferView : IDisposable
{

    public uint Id { get; }
    public uint ByteStride { get; }
    
    public unsafe BufferView(Buffer buffer, int byteLength, int byteOffset=0, uint byteStride=0)
    {
        // setup the object
        ByteStride = byteStride;
        
        // create the buffer
        Id = GL.Gl.CreateBuffer();

        // get the memory mapped file, create an accessor of the
        // view from where we need it and set the data from it directly
        byte* ptr = null;
        var accessor = buffer.Map.CreateViewAccessor(byteOffset, byteLength, MemoryMappedFileAccess.Read);
        var data = new byte[byteLength];
        accessor.ReadArray(0, data, 0, byteLength);
        GL.Gl.NamedBufferData<byte>(Id, data, VertexBufferObjectUsage.StaticDraw);
    }
    
    public BufferView(Span<float> buffer, uint byteStride=0)
    {
        // setup the object
        ByteStride = byteStride;
        
        // create the buffer
        Id = GL.Gl.CreateBuffer();

        // get the memory mapped file, create an accessor of the
        // view from where we need it and set the data from it directly
        GL.Gl.NamedBufferData<float>(Id, buffer, VertexBufferObjectUsage.StaticDraw);
    }
    
    public BufferView(Span<ushort> buffer, uint byteStride=0)
    {
        // setup the object
        ByteStride = byteStride;
        
        // create the buffer
        Id = GL.Gl.CreateBuffer();

        // get the memory mapped file, create an accessor of the
        // view from where we need it and set the data from it directly
        GL.Gl.NamedBufferData<ushort>(Id, buffer, VertexBufferObjectUsage.StaticDraw);
    }

    public BufferView(Span<byte> buffer, uint byteStride=0)
    {
        // setup the object
        ByteStride = byteStride;
        
        // create the buffer
        Id = GL.Gl.CreateBuffer();

        // get the memory mapped file, create an accessor of the
        // view from where we need it and set the data from it directly
        GL.Gl.NamedBufferData<byte>(Id, buffer, VertexBufferObjectUsage.StaticDraw);
    }

    private void ReleaseUnmanagedResources()
    {
        GL.Gl.DeleteBuffer(Id);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~BufferView()
    {
        GL.Post(ReleaseUnmanagedResources);
    }
}