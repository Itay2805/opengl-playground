using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using Flecs.NET.Core;

namespace MyGame.Engine.Rendering;

public class Buffer(string file, int byteLength = 0) : IDisposable
{

    public string? Name { get; init; } 
    
    public MemoryMappedFile Map { get; } = MemoryMappedFile.CreateFromFile(file, FileMode.Open, null, byteLength, MemoryMappedFileAccess.Read);

    public void Dispose()
    {
        Map.Dispose();
    }
    
}