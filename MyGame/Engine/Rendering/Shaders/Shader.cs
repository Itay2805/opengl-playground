using System.Reflection;
using Silk.NET.OpenGL;

namespace MyGame.Engine.Rendering;

[Flags]
public enum ShaderAttributes
{
    BaseColorTexture = 1 << 0,
    NormalTexture = 1 << 1,
    MetallicRoughnessTexture = 1 << 2,
    AlphaMask = 1 << 3
}

[Flags]
public enum ShaderVertexAttributes
{
    Tangents = 1 << 0
}

public class Shader : IDisposable
{
    
    /// <summary>
    /// Compiles the shader and returns it 
    /// </summary>
    private static uint CompileShader(string shaderSource, ShaderType type)
    {
        var shader = GL.Gl.CreateShader(type);
        GL.Gl.ShaderSource(shader, shaderSource);
        GL.Gl.CompileShader(shader);
        if (GL.Gl.GetShader(shader, ShaderParameterName.CompileStatus) != 1)
        {
            GL.Gl.GetShaderInfoLog(shader, out var infoLog);
            GL.Gl.DeleteShader(shader);
            throw new Exception(infoLog);
        }
        return shader;
    }

    private static string GetShader(string name)
    {
        using var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    
    public uint Id { get; }

    public Shader(string name)
    {
        var vertex = GetShader($"MyGame.Engine.Rendering.Shaders.{name}.vert");
        var fragment = GetShader($"MyGame.Engine.Rendering.Shaders.{name}.frag");

        // compile and link the shaders that we generated
        var vertexShader = CompileShader(vertex, ShaderType.VertexShader);
        try
        {
            var fragmentShader = CompileShader(fragment, ShaderType.FragmentShader);
            try
            {
                // link it properly
                Id = GL.Gl.CreateProgram();
                GL.Gl.AttachShader(Id, vertexShader);
                GL.Gl.AttachShader(Id, fragmentShader);
                GL.Gl.LinkProgram(Id);
                
                if (GL.Gl.GetProgram(Id, ProgramPropertyARB.LinkStatus) != 1)
                {
                    GL.Gl.GetProgramInfoLog(Id, out var infoLog);
                    throw new Exception(infoLog);
                }
            }
            finally
            {
                GL.Gl.DeleteShader(fragmentShader);
            }
        }
        finally
        {
            // need to cleanup in here
            GL.Gl.DeleteShader(vertexShader);
        }
    }
    
    private void ReleaseUnmanagedResources()
    {
        GL.Gl.DeleteProgram(Id);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Shader()
    {
        GL.Post(ReleaseUnmanagedResources);
    }
}