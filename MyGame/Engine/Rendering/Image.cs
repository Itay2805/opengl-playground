using Silk.NET.Core;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace MyGame.Engine.Rendering;

public class Image
{
    
    public int Width { get; }
    public int Height { get; }
    public byte[] Data { get; }

    public Image(string path)
    {
        // just load the image fully
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);
        Width = image.Width;
        Height = image.Height;
        Data = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(Data);
    }
    
}