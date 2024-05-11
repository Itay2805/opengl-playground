
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Numerics;
using System.Runtime.InteropServices;
using Flecs.NET.Core;
using GLFW;
using MyGame.Engine;
using MyGame.Engine.Rendering;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Buffer = MyGame.Engine.Rendering.Buffer;
using Exception = System.Exception;
using GL = Silk.NET.OpenGL.GL;
using Image = MyGame.Engine.Rendering.Image;
using Monitor = GLFW.Monitor;
using Native = Flecs.NET.Bindings.Native;
using Shader = MyGame.Engine.Rendering.Shader;
using Texture = MyGame.Engine.Rendering.Texture;

namespace MyGame;

public static class Program
{
    public static void Main(string[] args)
    {
        Engine.Engine.Run();
    }
}