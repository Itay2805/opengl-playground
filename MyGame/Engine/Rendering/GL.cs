using System.Collections.Concurrent;

namespace MyGame.Engine;

public static class GL
{

    public static Silk.NET.OpenGL.GL Gl;

    private static readonly Queue<Action> TaskQueue = new();

    public static void Post(Action action)
    {
        lock (TaskQueue)
        {
            TaskQueue.Enqueue(action);
        }
    }

    public static void Flush()
    {
        
        lock (TaskQueue)
        {
            while (TaskQueue.Count > 0)
            {
                TaskQueue.Dequeue()();
            }
        }
    }

}