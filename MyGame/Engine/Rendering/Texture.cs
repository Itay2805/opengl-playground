using Silk.NET.OpenGL;

namespace MyGame.Engine.Rendering;

public record TextureSampler
{
    public TextureMagFilter? MagFilter { get; init; } = null;
    public TextureMinFilter? MinFilter { get; init; } = null;
    public TextureWrapMode WrapS { get; init; } = TextureWrapMode.Repeat;
    public TextureWrapMode WrapT { get; init; } = TextureWrapMode.Repeat;
}

public class Texture : IDisposable
{

    private static readonly TextureSampler DefaultSampler = new();
    
    public uint Id { get; }

    public Texture(Image image, TextureSampler? sampler = null)
    {
        sampler ??= DefaultSampler;
        
        // create and load it 
        Id = GL.Gl.CreateTexture(TextureTarget.Texture2D);

        // calculate the amount of mipmaps to use, only do so if we 
        // have a mip map based min filter
        uint mipMapLevels = 1;
        if (sampler.MinFilter is TextureMinFilter.NearestMipmapNearest or TextureMinFilter.LinearMipmapNearest or TextureMinFilter.NearestMipmapLinear or TextureMinFilter.LinearMipmapLinear)
        {
            mipMapLevels = 1 + (uint)Math.Floor(Math.Log2(Math.Max(image.Height, image.Width)));
        }
        
        // Allocate the textuer and its mipmaps 
        GL.Gl.TextureStorage2D(Id, mipMapLevels, SizedInternalFormat.Rgba8, (uint)image.Width, (uint)image.Height);
        
        // upload the full texture
        GL.Gl.TextureSubImage2D<byte>(Id, 0, 0, 0, 
            (uint)image.Width, (uint)image.Height, 
            PixelFormat.Rgba, PixelType.UnsignedByte, 
            image.Data);
        
        if (mipMapLevels > 1)
        {
            GL.Gl.GenerateTextureMipmap(Id);
        }
        
        // make sure we have a sampler
        GL.Gl.TextureParameter(Id, TextureParameterName.TextureWrapS, (int)sampler.WrapS);
        GL.Gl.TextureParameter(Id, TextureParameterName.TextureWrapT, (int)sampler.WrapT);
        if (sampler.MinFilter != null) GL.Gl.TextureParameter(Id, TextureParameterName.TextureMinFilter, (int)sampler.MinFilter);
        if (sampler.MagFilter != null) GL.Gl.TextureParameter(Id, TextureParameterName.TextureMagFilter, (int)sampler.MagFilter);
    }

    private void ReleaseUnmanagedResources()
    {
        GL.Gl.DeleteTexture(Id);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Texture()
    {
        GL.Post(ReleaseUnmanagedResources);
    }
}