using System.Numerics;
using Silk.NET.Maths;

namespace MyGame.Engine;

public abstract class Camera
{

    public abstract Matrix4x4 View { get; set; }
    public abstract Matrix4x4 Projection { get; set; }
    
}