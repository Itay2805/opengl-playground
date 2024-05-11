using System.Numerics;
using Silk.NET.Maths;

namespace MyGame.Engine;

public class PerspectiveCamera : Camera
{
    public override Matrix4x4 View { get; set; }
    public override Matrix4x4 Projection { get; set; }

    public PerspectiveCamera(float aspectRatio, float yfov, float zfar, float znear)
    {
        // create the proper projection
        Projection = Matrix4x4.CreatePerspectiveFieldOfView(yfov, aspectRatio, znear, zfar);
        
        // and set a default view 
        View = Matrix4x4.Identity;
    }
    
}