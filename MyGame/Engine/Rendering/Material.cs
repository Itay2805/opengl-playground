using System.Numerics;

namespace MyGame.Engine.Rendering;

public record PbrMetallicRoughness
{
    public Vector4 BaseColorFactor { get; init; } = Vector4.One;
    public Texture? BaseColorTexture { get; init; } = null;
    
    public float MetallicFactor { get; init; } = 1.0f;
    public float RoughnessFactor { get; init; } = 1.0f;
    public Texture? MetallicRoughnessTexture { get; init; } = null;

}

public enum AlphaMode
{
    Opaque,
    Mask,
    Blend
}

public record Material
{
    
    public string? Name { get; init; }
    public PbrMetallicRoughness PbrMetallicRoughness { get; init; } = new();
    
    public Texture? NormalTexture { get; init; }
    public float NormalScale { get; init; }
    public AlphaMode AlphaMode { get; init; } = AlphaMode.Opaque;
    public float AlphaCutoff { get; init; } = 0.5f;
    public bool DoubleSided { get; init; } = false;

}
