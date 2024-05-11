using System.Reflection;
using Silk.NET.OpenGL;

namespace MyGame.Engine.Rendering;

[Flags]
public enum ShaderAttributes
{
    BaseColorTexture = 1 << 0,
    NormalTexture = 1 << 1,
    MetallicRoughnessTexture = 1 << 2,
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

    public int UniformBaseColorFactor { get; }
    public int UniformMetallicFactor { get; }
    public int UniformRoughnessFactor { get; }
    
    
    public int UniformBaseColorTexture { get; }
    public int UniformNormalTexture { get; }
    public int UniformMetallicRoughnessTexture { get; }

    
    public int UniformAttributes { get; }
    public int UniformVertexAttributes { get; }

    public int UniformLightPosition { get; }
    public int UniformLightColor { get; }

    public int UniformViewPosition { get; }
    
    public int UniformModel { get; }
    public int UniformView { get; }
    public int UniformProjection { get; }
    
    public Shader()
    {
        var vertex = GetShader("MyGame.Engine.Rendering.Shader.vert");
        var fragment = GetShader("MyGame.Engine.Rendering.Shader.frag");

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
        
        // now get all the locations we need
        UniformBaseColorFactor = GL.Gl.GetUniformLocation(Id, "u_baseColorFactor");
        UniformMetallicFactor = GL.Gl.GetUniformLocation(Id, "u_metallicFactor");
        UniformRoughnessFactor = GL.Gl.GetUniformLocation(Id, "u_roughnessFactor");

        UniformBaseColorTexture = GL.Gl.GetUniformLocation(Id, "u_baseColorTexture");
        UniformNormalTexture = GL.Gl.GetUniformLocation(Id, "u_normalTexture");
        UniformMetallicRoughnessTexture = GL.Gl.GetUniformLocation(Id, "u_metallicRoughnessTexture");

        UniformAttributes = GL.Gl.GetUniformLocation(Id, "u_attributes");
        UniformVertexAttributes = GL.Gl.GetUniformLocation(Id, "u_vertexAttributes");

        UniformLightPosition = GL.Gl.GetUniformLocation(Id, "u_lightPosition");
        UniformLightColor = GL.Gl.GetUniformLocation(Id, "u_lightColor");
        
        UniformViewPosition = GL.Gl.GetUniformLocation(Id, "u_viewPosition");
        
        UniformModel = GL.Gl.GetUniformLocation(Id, "u_model");
        UniformView = GL.Gl.GetUniformLocation(Id, "u_view");
        UniformProjection = GL.Gl.GetUniformLocation(Id, "u_projection");
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