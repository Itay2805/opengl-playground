using System.Numerics;

namespace MyGame.Engine.Component;

public record struct EcsTransform
{
    public Matrix4x4 Value { get; set; }
}

public record struct EcsPosition
{
    public Vector3 Value { get; set; }

    public EcsPosition(float x, float y, float z)
    {
        Value = new Vector3(x, y, z);
    }
    
}

public record struct EcsRotation
{
    public Vector3 Value { get; set; }
}

public record struct EcsScale
{
    public Vector3 Value { get; set; }
}
