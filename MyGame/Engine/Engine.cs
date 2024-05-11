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
        
        //
        // Import all the modules we need for flecs
        //
        World.Set<Native.EcsRest>(default);
        World.Import<Ecs.Monitor>();
        World.Import<Transform>();
        
        Gltf.Load("/home/tomato/checkouts/glTF-Sample-Models/2.0/Sponza/glTF/Sponza.gltf");

        var render = World.Query<EcsMesh, EcsTransform>();
        
        //
        // Create a simple camera
        //
        var camera = new PerspectiveCamera(1280f / 720f, MathF.PI / 180 * 45f, 100.0f, 0.1f)
        {
            View = Matrix4x4.CreateLookAt(
                new Vector3(3f, 1.7f, 0f), 
                new Vector3(0f, 1.7f, 0f), 
                Vector3.UnitY
            )
        };

        Renderer.SetLight(new Light
        {
            Color = new Vector3(150.0f),
            Position = new Vector3(3f, 0f, 0f)
        });
        
        // set some color
        GL.Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        
        //
        // The main game loop
        //
        while (!Glfw.WindowShouldClose(window))
        {
            // Progress the ECS properly
            World.Progress();

            // submit all the meshes for rendering 
            render.Each((ref EcsMesh mesh, ref EcsTransform transform) =>
            {
                Renderer.SubmitMesh(mesh.Mesh, transform.Value);
            });
            Renderer.Render(camera);
            
            // get events and present everything 
            Glfw.SwapBuffers(window);
            Glfw.PollEvents();
        }
        
        Glfw.DestroyWindow(window);
        Glfw.Terminate();
    }
    
}