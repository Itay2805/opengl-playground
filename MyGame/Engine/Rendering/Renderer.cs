using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace MyGame.Engine.Rendering;

public static class Renderer
{

    /// <summary>
    /// The meshes we need to draw, grouped by their material so we won't
    /// need to set the uniforms too much 
    /// </summary>
    private static Dictionary<Material, List<(Matrix4x4, MeshPrimitive)>> _meshes = new();

    /// <summary>
    /// The default material that we are using when rendering 
    /// </summary>
    public static readonly Material DefaultMaterial = new();
    
    /// <summary>
    /// The shader we are using for everything 
    /// </summary>
    private static Shader _shader = null!;

    /// <summary>
    /// The debug callback for opengl, keep a global reference to make
    /// sure the GC won't collect this delegate 
    /// </summary>
    private static readonly DebugProc GlDebugProc = (sourceInt, typeInt, id, severityInt, length, message, param) =>
    {
        var source = (DebugSource)sourceInt;
        var severity = (DebugSeverity)severityInt;
        var type = (DebugType)typeInt;
        
        var sourceStr = source switch
        {
            DebugSource.DebugSourceApi => "API",
            DebugSource.DebugSourceWindowSystem => "Window System",
            DebugSource.DebugSourceShaderCompiler => "Shader Compiler",
            DebugSource.DebugSourceThirdParty => "Third Party",
            DebugSource.DebugSourceApplication => "Application",
            DebugSource.DebugSourceOther => "Other",
            _ => source.ToString()
        };
        
        var severityStr = severity switch
        {
            DebugSeverity.DebugSeverityNotification => "Notification",
            DebugSeverity.DebugSeverityHigh => "High",
            DebugSeverity.DebugSeverityMedium => "Medium",
            DebugSeverity.DebugSeverityLow => "Low",
            _ => severity.ToString()
        };

        var typeStr = type switch
        {
            DebugType.DebugTypeError => "Error",
            DebugType.DebugTypeDeprecatedBehavior => "Deprecated Behavior",
            DebugType.DebugTypeUndefinedBehavior => "Undefined Behavior",
            DebugType.DebugTypePortability => "Portability",
            DebugType.DebugTypePerformance => "Performance",
            DebugType.DebugTypeOther => "Other",
            DebugType.DebugTypeMarker => "Marker",
            DebugType.DebugTypePushGroup => "Push Group",
            DebugType.DebugTypePopGroup => "Pop Group",
            _ => type.ToString()
        };

        var messageStr = Marshal.PtrToStringUTF8(message, length);
        var fullMsg = $"OpenGL[{sourceStr}/{typeStr}] ({severityStr}): {messageStr}";
        
        Console.WriteLine(fullMsg);

        if (type == DebugType.DebugTypeError)
        {
            throw new Exception(fullMsg);
        }
        
        // if (severity != DebugSeverity.DebugSeverityNotification)
        // {
        //     Console.WriteLine(new StackTrace().ToString());
        // }
    };
    
    /// <summary>
    /// Initialize the rendering subsystem
    /// </summary>
    public static void Init()
    {
        // Make sure we got the correct information
        Console.WriteLine($"Version: {GL.Gl.GetStringS(StringName.Version)}");
        Console.WriteLine($"Vendor/Renderer: {GL.Gl.GetStringS(StringName.Vendor)}/{GL.Gl.GetStringS(StringName.Renderer)}");

        // enable debugging if we have a debug context
        GL.Gl.GetInteger(GetPName.ContextFlags, out var flags);
        if ((flags & (int)ContextFlagMask.DebugBit) != 0)
        {
            GL.Gl.Enable(EnableCap.DebugOutput);
            GL.Gl.Enable(EnableCap.DebugOutputSynchronous);
            unsafe
            {
                GL.Gl.DebugMessageCallback(GlDebugProc, null);
                GL.Gl.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare, DebugSeverity.DontCare, 0, null, true);
            }
        }
        
        // set depth testing properly
        GL.Gl.Enable(EnableCap.DepthTest);
        GL.Gl.DepthFunc(DepthFunction.Lequal);
        
        // compile and use the shader 
        _shader = new Shader();
        GL.Gl.UseProgram(_shader.Id);
    }
    
    public static void SetLight(Light light)
    {
        var shader = _shader;
        GL.Gl.Uniform3(shader.UniformLightPosition, light.Position);
        GL.Gl.Uniform3(shader.UniformLightColor, light.Color);
    }

    public static void SubmitMesh(Mesh mesh, Matrix4x4 transform)
    {
        foreach (var primitive in mesh.Primitives)
        {
            if (!_meshes.TryGetValue(primitive.Material, out var list))
            {
                list = new List<(Matrix4x4, MeshPrimitive)>();
                _meshes[primitive.Material] = list;
            }
            list.Add((transform, primitive));
        }
    }
    
    /// <summary>
    /// Perform the full render cycle with the given camera
    /// </summary>
    public static void Render(Camera camera)
    {
        var shader = _shader;
        
        // clear everything 
        GL.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        // Update the camera in the shader 
        var projection = camera.Projection;
        var view = camera.View;
        unsafe
        {
            // TODO: only update the projection if changed from the last time 
            GL.Gl.UniformMatrix4(shader.UniformProjection, 1, false, (float*)&projection);
            GL.Gl.UniformMatrix4(shader.UniformView, 1, false, (float*)&view);
        }

        // camera position
        GL.Gl.Uniform3(shader.UniformViewPosition, view.M13, view.M23, view.M33);
        
        // Go over all the materials 
        var materialsToRemove = new List<Material>();
        foreach (var (material, list) in _meshes)
        {
            if (list.Count == 0)
            {
                materialsToRemove.Add(material);
                continue;
            }
            
            var pbr = material.PbrMetallicRoughness;
            
            //
            // Set all the base material parameters
            //
            GL.Gl.Uniform4(shader.UniformBaseColorFactor, pbr.BaseColorFactor);
            GL.Gl.Uniform1(shader.UniformMetallicFactor, pbr.MetallicFactor);
            GL.Gl.Uniform1(shader.UniformRoughnessFactor, pbr.RoughnessFactor);

            // 
            // Set all the textures of the material
            //
            var textureSlot = 0;
            ShaderAttributes attributes = 0;
            if (pbr.BaseColorTexture != null)
            {
                attributes |= ShaderAttributes.BaseColorTexture;
                GL.Gl.ActiveTexture(TextureUnit.Texture0 + textureSlot);
                GL.Gl.BindTexture(GLEnum.Texture2D, pbr.BaseColorTexture.Id);
                GL.Gl.Uniform1(shader.UniformBaseColorTexture, textureSlot);
                textureSlot += 1;
            }
            
            if (pbr.MetallicRoughnessTexture != null)
            {
                attributes |= ShaderAttributes.MetallicRoughnessTexture;
                GL.Gl.ActiveTexture(TextureUnit.Texture0 + textureSlot);
                GL.Gl.BindTexture(GLEnum.Texture2D, pbr.MetallicRoughnessTexture.Id);
                GL.Gl.Uniform1(shader.UniformMetallicRoughnessTexture, textureSlot);
                textureSlot += 1;
            }
            
            if (material.NormalTexture != null)
            {
                attributes |= ShaderAttributes.NormalTexture;
                GL.Gl.ActiveTexture(TextureUnit.Texture0 + textureSlot);
                GL.Gl.BindTexture(GLEnum.Texture2D, material.NormalTexture.Id);
                GL.Gl.Uniform1(shader.UniformNormalTexture, textureSlot);
                textureSlot += 1;
            }

            //
            // Update the attributes 
            //
            GL.Gl.Uniform1(shader.UniformAttributes, (int)attributes);
            
            // and now over all the meshes that use that material
            // and render them one by one 
            foreach (var (transform, primitive) in list)
            {
                //
                // Update the vertex attributes 
                //
                ShaderVertexAttributes vertexAttributes = 0;
                if (primitive.Tangent != null)
                {
                    vertexAttributes |= ShaderVertexAttributes.Tangents;
                }
                GL.Gl.Uniform1(shader.UniformVertexAttributes, (int)vertexAttributes);
                
                // set the transform of the mesh
                unsafe
                {
                    GL.Gl.UniformMatrix4(shader.UniformModel, 1, false, (float*)&transform);
                }
                
                // bind the vertex array that we are going to draw
                GL.Gl.BindVertexArray(primitive.Id);

                // actually draw it now
                var type = (DrawElementsType)primitive.Indices.ComponentType;
                unsafe
                {
                    GL.Gl.DrawElements(primitive.Mode, primitive.Indices.Count, type, null);
                }
            }
            
            // clear the list to save on space 
            list.Clear();
        }

        // remove materials that had no items in this iteration
        foreach (var material in materialsToRemove)
        {
            _meshes.Remove(material);
        }
    }
    
}