using Silk.NET.OpenGL;

namespace MyGame.Engine;

public enum BufferAccessorType
{
    Scalar,
    Vec2,
    Vec3,
    Vec4,
    Mat2,
    Mat3,
    Mat4
}

public record BufferAccessor(
    BufferView BufferView,
    BufferAccessorType Type,
    VertexAttribType ComponentType,
    uint Count,
    int ByteOffset = 0,
    bool Normalized = false
)
{

    public uint ByteStride => (uint)(BufferView.ByteStride == 0 ? (GetComponentSize() * GetTypeCount()) : BufferView.ByteStride);

    private uint GetComponentSize()
    {
        return ComponentType switch
        {
            VertexAttribType.Byte => 1,
            VertexAttribType.UnsignedByte => 1,
            VertexAttribType.Short => 2,
            VertexAttribType.UnsignedShort => 2,
            VertexAttribType.Int => 4,
            VertexAttribType.UnsignedInt => 4,
            VertexAttribType.Float => 4,
            VertexAttribType.Double => 8,
            VertexAttribType.HalfFloat => 2,
            _ => 0
        };
    }

    public int GetTypeCount()
    {
        return Type switch
        {
            BufferAccessorType.Scalar => 1,
            BufferAccessorType.Vec2 => 2,
            BufferAccessorType.Vec3 => 3,
            BufferAccessorType.Vec4 => 4,
            BufferAccessorType.Mat2 => 2 * 2,
            BufferAccessorType.Mat3 => 3 * 3,
            BufferAccessorType.Mat4 => 4 * 4,
            _ => 0
        };
    }
    
}
