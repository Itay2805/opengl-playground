namespace MyGame.Engine.Rendering.Shaders;

public class MainShader : Shader
{
    
    
    public int UniformBaseColorFactor { get; }
    public int UniformMetallicFactor { get; }
    public int UniformRoughnessFactor { get; }
    public int UniformAlphaCutoff { get; set; }
    public int UniformNormalScale { get; set; }
    
    
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

    public MainShader() 
        : base("MainShader")
    {
        
        // now get all the locations we need
        UniformBaseColorFactor = GL.Gl.GetUniformLocation(Id, "u_baseColorFactor");
        UniformMetallicFactor = GL.Gl.GetUniformLocation(Id, "u_metallicFactor");
        UniformRoughnessFactor = GL.Gl.GetUniformLocation(Id, "u_roughnessFactor");
        UniformAlphaCutoff = GL.Gl.GetUniformLocation(Id, "u_alphaCutoff");
        UniformNormalScale = GL.Gl.GetUniformLocation(Id, "u_normalScale");

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
    
}