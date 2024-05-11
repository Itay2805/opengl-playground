using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Flecs.NET.Core;
using MyGame.Engine.Component;
using Silk.NET.OpenGL;

namespace MyGame.Engine.Rendering;

public record GltfBuffer
{
    [JsonRequired] public string Uri { get; set; }
    [JsonRequired] public int ByteLength { get; set; }
    public string? Name { get; set; } = null;
}

public record GltfBufferView
{
    [JsonRequired] public int Buffer { get; set; }
    public int ByteOffset { get; set; } = 0;
    [JsonRequired] public int ByteLength { get; set; }
    public uint ByteStride { get; } = 0;
    public string? Name { get; } = null;
}

public record GltfAccessor
{
    public int? BufferView { get; set; } = null;
    public int ByteOffset { get; set; } = 0;
    [JsonRequired] public VertexAttribType ComponentType { get; set; }
    public bool Normalized { get; set; } = false;
    [JsonRequired] public uint Count { get; set; }
    [JsonRequired, JsonConverter(typeof(JsonStringEnumConverter))] public BufferAccessorType Type { get; set; }
    public string? Name { get; set; } = null;
}

public record GltfMeshPrimitive
{
    [JsonRequired] public Dictionary<string, int> Attributes { get; set; }
    public int? Indices { get; set; } = null;
    public int? Material { get; set; } = null;
    public PrimitiveType Mode { get; set; } = PrimitiveType.Triangles;
}

public record GltfMesh
{
    [JsonRequired] public GltfMeshPrimitive[] Primitives { get; set; }
    public string? Name { get; set; } = null;
}

public record GltfTextureInfo
{
    [JsonRequired] public int Index { get; set; }
    public int TexCoord { get; set; } = 0;
}

public record GltfPbrMetallicRoughness
{
    public float[] BaseColorFactor { get; set; } = [1f, 1f, 1f, 1f];
    public GltfTextureInfo? BaseColorTexture { get; set; } = null;
    public float MetallicFactor { get; set; } = 1f;
    public float RoughnessFactor { get; set; } = 1f;
    public GltfTextureInfo? MetallicRoughnessTexture { get; set; } = null;
}

public record GltfNormalTextureInfo
{
    [JsonRequired] public int Index { get; set; }
    public int TexCoord { get; set; } = 0;
    public float Scale { get; set; } = 1f;
}

public record GltfMaterial
{
    public string? Name { get; set; } = null;
    public GltfPbrMetallicRoughness? PbrMetallicRoughness { get; set; } = null;
    public GltfNormalTextureInfo? NormalTexture { get; set; } = null;
    public GltfTextureInfo? EmissiveTexture { get; set; } = null;
    public float[] EmissiveFactor { get; set; } = [0f, 0f, 0f];
    [JsonConverter(typeof(JsonStringEnumConverter))] public AlphaMode AlphaMode { get; set; } = AlphaMode.Opaque;
    public float AlphaCutoff { get; set; } = 0.5f;
    public bool DoubleSided { get; set; } = false;
}

public record GltfImage
{
    public string? Uri { get; set; } = null;
    public string? MimeType { get; set; } = null;
    public int BufferView { get; set; } = -1;
    public string? Name { get; set; } = null;
}

public record GltfTexture
{
    public int? Sampler { get; set; } = null;
    public int? Source { get; set; } = null;
}

public record Node
{
    public int? Camera { get; set; }
    public int[]? Children { get; set; }
    public float[]? Matrix { get; set; }
    public int? Mesh { get; set; }
    public float[] Rotation { get; set; } = [0, 0, 0, 1];
    public float[] Scale { get; set; } = [1, 1, 1];
    public float[] Translation { get; set; } = [0, 0, 0];
    public string? Name { get; set; }
}

public record GltfJson
{
    public GltfImage[] Images { get; set; } = [];
    public TextureSampler[] Samplers { get; set; } = [];
    public GltfTexture[] Textures { get; set; } = [];
    public GltfBuffer[] Buffers { get; set; } = [];
    public GltfBufferView[] BufferViews { get; set; } = [];
    public GltfAccessor[] Accessors { get; set; } = [];
    public GltfMesh[] Meshes { get; set; } = [];
    public GltfMaterial[] Materials { get; set; } = [];
    public Node[] Nodes { get; set; } = [];
}

public class Gltf
{

    private static Mesh[] ParseMeshes(GltfJson gltf, string basePath)
    {
        var images = new Image[gltf.Images.Length];
        var textures = new Texture[gltf.Textures.Length];
        var buffers = new Buffer[gltf.Buffers.Length];
        var bufferViews = new BufferView[gltf.BufferViews.Length];
        var accessors = new BufferAccessor[gltf.Accessors.Length];
        var meshes = new Mesh[gltf.Meshes.Length];
        var materials = new Material[gltf.Materials.Length];

        // get the images
        for (var i = 0; i < images.Length; i++)
        {
            var image = gltf.Images[i];
            images[i] = new Image(Path.Join(basePath, image.Uri!));
        }
        
        // get the textures
        for (var i = 0; i < textures.Length; i++)
        {
            var texture = gltf.Textures[i];

            TextureSampler? sampler = null;
            if (texture.Sampler != null)
            {
                sampler = gltf.Samplers[texture.Sampler.Value];
            }
            
            textures[i] = new Texture(images[texture.Source!.Value], sampler);
        }
        
        // get the buffers
        for (var i = 0; i < buffers.Length; i++)
        {
            var buffer = gltf.Buffers[i];
            buffers[i] = new Buffer(Path.Join(basePath, buffer.Uri), buffer.ByteLength)
            {
                Name = buffer.Name
            };
        }

        // get the views
        for (var i = 0; i < bufferViews.Length; i++)
        {
            var bufferView = gltf.BufferViews[i];

            bufferViews[i] = new BufferView(
                buffers[bufferView.Buffer], 
                bufferView.ByteLength, bufferView.ByteOffset,  
                bufferView.ByteStride
            );
        }
        
        // get all the accessors 
        for (var i = 0; i < accessors.Length; i++)
        {
            var accessor = gltf.Accessors[i];
            accessors[i] = new BufferAccessor(
                bufferViews[accessor.BufferView!.Value], 
                accessor.Type, accessor.ComponentType, 
                accessor.Count, accessor.ByteOffset, 
                accessor.Normalized
            );
        }
        
        // get all the meshes
        for (var i = 0; i < materials.Length; i++)
        {
            var material = gltf.Materials[i];

            PbrMetallicRoughness pbr;
            if (material.PbrMetallicRoughness != null)
            {
                var pmr = material.PbrMetallicRoughness;
                
                Texture? baseColorTexture = null;
                if (pmr.BaseColorTexture != null)
                {
                    if (pmr.BaseColorTexture.TexCoord != 0) 
                        throw new NotSupportedException("Multiple TexCoords are not supported currently");
                    baseColorTexture = textures[pmr.BaseColorTexture.Index];
                }
                
                Texture? metallicRoughnessTexture = null;
                if (pmr.MetallicRoughnessTexture != null)
                {
                    if (pmr.MetallicRoughnessTexture.TexCoord != 0) 
                        throw new NotSupportedException("Multiple TexCoords are not supported currently");
                    metallicRoughnessTexture = textures[pmr.MetallicRoughnessTexture.Index];
                }
                
                pbr = new PbrMetallicRoughness
                {
                    MetallicFactor = pmr.MetallicFactor,
                    RoughnessFactor = pmr.RoughnessFactor,
                    BaseColorTexture = baseColorTexture,
                    BaseColorFactor = new Vector4(pmr.BaseColorFactor),
                    MetallicRoughnessTexture = metallicRoughnessTexture
                };
            }
            else
            {
                pbr = new PbrMetallicRoughness();
            }
            
            Texture? normal = null;
            var normalScale = 1.0f;
            if (material.NormalTexture != null)
            {
                if (material.NormalTexture.TexCoord != 0) 
                    throw new NotSupportedException("Multiple TexCoords are not supported currently");
                normal = textures[material.NormalTexture.Index];
                normalScale = material.NormalTexture.Scale;
            }
            
            materials[i] = new Material
            {
                PbrMetallicRoughness = pbr,
                NormalTexture = normal,
                NormalScale = normalScale,
                AlphaMode = material.AlphaMode,
                AlphaCutoff = material.AlphaCutoff,
                DoubleSided = material.DoubleSided,
                Name = material.Name
            };
        }
        
        // and now get all the meshes
        for (var i = 0; i < meshes.Length; i++)
        {
            var mesh = gltf.Meshes[i];
            
            // create all the primitives
            var primitives = new MeshPrimitive[mesh.Primitives.Length];
            for (var j = 0; j < primitives.Length; j++)
            {
                var primitive = mesh.Primitives[j];

                var indices = accessors[primitive.Indices!.Value];

                // required attributes
                var position = accessors[primitive.Attributes["POSITION"]];
                var normal = accessors[primitive.Attributes["NORMAL"]];
                
                BufferAccessor? texcoord = null;
                if (primitive.Attributes.TryGetValue("TEXCOORD_0", out var texCoordI))
                {
                    texcoord = accessors[texCoordI];
                }
                
                BufferAccessor? tangent = null;
                if (primitive.Attributes.TryGetValue("TANGENT", out var tangentI))
                {
                    tangent = accessors[tangentI];
                }
                
                Material? material = null;
                if (primitive.Material != null)
                {
                    material = materials[primitive.Material.Value];
                }

                primitives[j] = new MeshPrimitive(
                    indices, 
                    position, 
                    normal, 
                    texcoord,
                    tangent,
                    primitive.Mode,
                    material
                );
            }

            // and finally we can create the mesh
            meshes[i] = new Mesh
            {
                Primitives = primitives,
                Name = mesh.Name
            };
        }

        return meshes;
    }
    
    private static void ParseJson(string json, string basePath)
    {
        var gltf = JsonSerializer.Deserialize<GltfJson>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        
        // parse all the meshes
        var meshes = ParseMeshes(gltf, basePath);
        
        // initialize all the nodes since they are self referencing 
        // might as well start creating them 
        var entities = new Entity[gltf.Nodes.Length];
        for (var i = 0; i < entities.Length; i++)
        {
            // create and set the name 
            var node = gltf.Nodes[i];
            var entity = node.Name != null ? Engine.World.Entity(node.Name) : Engine.World.Entity();
            entities[i] = entity;

            // get the position, rotation and scale, either from
            // a matrix, or from the components
            Matrix4x4 mat;
            if (node.Matrix != null)
            {
                mat = new Matrix4x4(
                    node.Matrix[0],  node.Matrix[1],  node.Matrix[2],  node.Matrix[3],
                    node.Matrix[4],  node.Matrix[5],  node.Matrix[6],  node.Matrix[7],
                    node.Matrix[8],  node.Matrix[9],  node.Matrix[10], node.Matrix[11],
                    node.Matrix[12], node.Matrix[13], node.Matrix[14], node.Matrix[15]
                );
            }
            else
            {
                mat = Matrix4x4.CreateTranslation(new Vector3(node.Translation));
                mat *= Matrix4x4.CreateFromQuaternion(new Quaternion(new Vector3(node.Rotation[0], node.Rotation[1], node.Rotation[2]), node.Rotation[3]));
                mat *= Matrix4x4.CreateScale(new Vector3(node.Scale));
            }

            // decompose it so we can actually setup everything properly
            Matrix4x4.Decompose(mat, out var scale, out var q, out var translation);

            // finish to decompose the quant 
            float yaw = 0.0f, pitch = 0.0f, roll = 0.0f;

            // Calculate roll (x-axis rotation)
            var sinrCosp = 2 * (q.W * q.X + q.Y * q.Z);
            var cosrCosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            roll = MathF.Atan2(sinrCosp, cosrCosp);

            // Calculate pitch (y-axis rotation)
            var sinp = 2 * (q.W * q.Y - q.Z * q.X);
            pitch = MathF.Abs(sinp) >= 1 ? MathF.CopySign(MathF.PI / 2, sinp) : MathF.Asin(sinp);

            // Calculate yaw (z-axis rotation)
            var sinyCosp = 2 * (q.W * q.Z + q.X * q.Y);
            var cosyCosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            yaw = MathF.Atan2(sinyCosp, cosyCosp);
            
            // set it 
            entity.Set(new EcsPosition{ Value = translation });
            entity.Set(new EcsRotation{ Value = new Vector3(roll, pitch, yaw) });
            entity.Set(new EcsScale{ Value = scale });
            
            // apply a mesh if needed
            if (node.Mesh != null)
            {
                entity.Set(new EcsMesh { Mesh = meshes[node.Mesh.Value] });
            }
        }
        
        // and now connect all the children
        for (var i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            var node = gltf.Nodes[i];
            if (node.Children == null)
                continue;
            
            foreach (var child in node.Children)
            {
                entity.ChildOf(entities[child]);
            }
        }
    }
    
    // TODO: load a scene instead
    public static void Load(string gltf)
    {
        ParseJson(File.ReadAllText(gltf), Path.GetDirectoryName(gltf));
    }
    
}