using System.Numerics;

namespace MyGame.Engine.Component;

public record struct EcsMesh
{
    public Mesh Mesh { get; init; }
}

public record struct EcsCamera
{
    public Matrix4x4 Projection { get; init; }

    public EcsCamera(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
    {
        Projection = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);
    }
    
}

public record struct EcsLookAt
{
    public Vector3 Target { get; init; }
}

public record struct EcsLight
{
    public Vector3 Color { get; init; }
}

