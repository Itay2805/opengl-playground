using Silk.NET.OpenGL;

namespace MyGame.Engine.Rendering;

public enum MeshPrimitiveLocation : uint
{
    Position = 0,
    Normal = 1,
    Tangent = 2,
    TexCoord = 3
}

[Flags]
public enum MeshPrimitiveAttributes : byte
{
    BaseColorTexture = 1 << 0,
    NormalTexture = 1 << 2,
}


public class MeshPrimitive : IDisposable
{

    /// <summary>
    /// The OpenGL id of the mesh primitive
    /// </summary>
    public uint Id { get; }
    
    /// <summary>
    /// The drawing mode of the mesh primitive
    /// </summary>
    public PrimitiveType Mode { get; }
    
    public BufferAccessor Indices { get; }

    public BufferAccessor Position { get; }
    public BufferAccessor Normal { get; }
    public BufferAccessor? TexCoord { get; }
    public BufferAccessor? Tangent { get; }


    public Material Material { get; }
    
    public MeshPrimitive(
        // buffers
        BufferAccessor indices,
        BufferAccessor position,
        BufferAccessor normal,
        BufferAccessor? texcoord=null,
        BufferAccessor? tangent=null,
        
        // other information about the mesh primitive
        PrimitiveType mode=PrimitiveType.Triangles,
        Material? material=null
    )
    {
        Id = GL.Gl.CreateVertexArray();

        // save stuff
        Mode = mode;

        // use a default material if we are missing one 
        material ??= Renderer.DefaultMaterial;
        Material = material;

        //
        // Prepare the indices
        // as far as I know byte offset can't be non-zero in this,
        // so we make sure it is zero in the view that we get 
        //
        Indices = indices;
        GL.Gl.VertexArrayElementBuffer(Id, indices.BufferView.Id);

        //
        // for easier setup add all the attributes
        // into this array
        //
        List<(BufferAccessor, uint)> attributes = new();
        
        // we always have a position
        Position = position; 
        attributes.Add((position, (uint)MeshPrimitiveLocation.Position));

        // we always have a normal
        Normal = normal;
        attributes.Add((normal, (uint)MeshPrimitiveLocation.Normal));

        // if we have tangents pass them in here
        if (tangent != null)
        {
            Tangent = tangent;
            attributes.Add((tangent, (uint)MeshPrimitiveLocation.Tangent));
        }

        // add texture coords if we have a texture
        // TODO: support multiple sets of coords
        if (
            material.PbrMetallicRoughness.BaseColorTexture != null ||
            material.NormalTexture != null
        )
        {
            // TexCoord is required if we have a texture in general
            TexCoord = texcoord ?? throw new ArgumentNullException(nameof(texcoord));
            
            // add the tex coord
            attributes.Add((texcoord, (uint)MeshPrimitiveLocation.TexCoord));
        }
        
        //
        // Figure all the different bindings we need
        // to access the code
        //
        Dictionary<(uint, int), uint> bindings = new();
        foreach (var (attribute, i) in attributes)
        {
            // enable the attribute since we have it 
            GL.Gl.EnableVertexArrayAttrib(Id, i);
            
            // add the attrib and set the format
            GL.Gl.VertexArrayAttribFormat(
                Id, i, 
                attribute.GetTypeCount(), attribute.ComponentType, 
                attribute.Normalized, 0
            );

            // figure the buffer we need to bind, if we don't
            // have it bound yet then bind it right now 
            var key = (attribute.BufferView.Id, attribute.ByteOffset);
            if (!bindings.TryGetValue(key, out var binding))
            {
                binding = (uint)bindings.Count;
                bindings[key] = binding;
                GL.Gl.VertexArrayVertexBuffer(
                    Id, binding, 
                    attribute.BufferView.Id, 
                    attribute.ByteOffset, attribute.ByteStride
                );
            }
            GL.Gl.VertexArrayAttribBinding(Id, i, binding);
        }
    }

    private void ReleaseUnmanagedResources()
    {
        GL.Gl.DeleteVertexArray(Id);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~MeshPrimitive()
    {
        GL.Post(ReleaseUnmanagedResources);
    }
}