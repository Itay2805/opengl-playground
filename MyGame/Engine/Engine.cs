using System.Numerics;
using System.Runtime.InteropServices;
using Flecs.NET.Core;
using GLFW;
using MyGame.Engine.Component;
using MyGame.Engine.Rendering;
using MyGame.Engine.Systems;
using Exception = System.Exception;
using Native = Flecs.NET.Bindings.Native;

namespace MyGame.Engine;

public static class Engine
{

    /// <summary>
    /// The ECS world we are using for the engine
    /// </summary>
    public static World World { get; private set; }

    public static Window Window { get; private set; }
    
    /// <summary>
    /// Should debug rendering happen
    /// </summary>
    public static bool Debug { get; set; } = true;
    
    /// <summary>
    /// The callback for GLFW error reporting
    /// </summary>
    private static readonly ErrorCallback GlfwErrorCallback = (code, message) =>
    {
        Console.WriteLine(Marshal.PtrToStringUTF8(message));
    };
    
    /// <summary>
    /// Create a window nicely
    /// </summary>
    private static Window CreateWindow()
    {
        // setup a window with GLFW
        Glfw.SetErrorCallback(GlfwErrorCallback);
        if (!Glfw.Init())
        {
            throw new Exception("Failed to initialize GLFW");
        }
        
        Glfw.WindowHint(Hint.OpenglDebugContext, true);
        Glfw.WindowHint(Hint.ContextVersionMajor, 4);
        Glfw.WindowHint(Hint.ContextVersionMinor, 5);
        Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);

        var window = Glfw.CreateWindow(1280, 720, "Hello world", GLFW.Monitor.None, Window.None);
        if (window == Window.None)
        {
            Glfw.Terminate();
            throw new Exception("Failed to create a window");
        }
        
        // Setup the opengl context
        Glfw.MakeContextCurrent(window);
        GL.Gl = Silk.NET.OpenGL.GL.GetApi(Glfw.GetProcAddress);
        Glfw.SwapInterval(1);

        return window;
    }
    
    public static void Run()
    {
        // setup the world
        World = World.Create();
        
        //
        // Initialize the rendering
        //
        var window = CreateWindow();
        Window = window;
        
        // finally initialize the renderer
        Renderer.Init();

        //
        // add reflection for components
        //

        World.Component<EcsPosition>()
            .Member<float>("X")
            .Member<float>("Y")
            .Member<float>("Z");

        World.Component<EcsRotation>()
            .Member<float>("Roll (X)")
            .Member<float>("Pitch (Y)")
            .Member<float>("Yaw (Z)");
        
        World.Component<EcsScale>()
            .Member<float>("X")
            .Member<float>("Y")
            .Member<float>("Z");
        
        World.Component<EcsLight>()
            .Member<float>("R")
            .Member<float>("G")
            .Member<float>("B");

        World.Component<EcsLookAt>()
            .Member<EcsPosition>("Target");

        //
        // Import all the modules we need for flecs
        //
        World.Set<Native.EcsRest>(default);
        World.Import<Ecs.Monitor>();
        World.Import<Transform>();
        World.Import<Graphics>();
        World.Import<Controller>();

        //
        // Load something in 
        //
        Gltf.Load("/home/tomato/checkouts/glTF-Sample-Models/2.0/Avocado/glTF/Avocado.gltf");

        // simple camera
        var camera = World.Entity("Camera");
        camera.Set(new EcsPosition { Value = new Vector3(0.1f) });
        camera.Set(new EcsRotation());
        camera.Set(new EcsCamera(float.DegreesToRadians(45f), 1280f / 720f, 0.1f, 100f));
        camera.Set(new EcsLookAt { Target = new Vector3(0f, 0f, 0f) });
        
        // simple light
        var light = World.Entity("Light");
        light.Set(new EcsPosition(-3f, 0f, -0.5f));
        light.Set(new EcsLight{ Color = new Vector3(150f) });
        
        // set some color
        GL.Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        
        //
        // The main game loop
        //
        while (!Glfw.WindowShouldClose(window))
        {
            World.Progress();
            Glfw.SwapBuffers(window);
            Glfw.PollEvents();
        }
        
        Glfw.DestroyWindow(window);
        Glfw.Terminate();
    }
    
}