using System.Numerics;
using System.Runtime.CompilerServices;
using Flecs.NET.Core;
using MyGame.Engine.Component;
using MyGame.Engine.Rendering;

namespace MyGame.Engine.Systems;

public struct Graphics : IFlecsModule
{
    public void InitModule(ref World world)
    {
        // prepare all the meshes for rendering by actually
        // submitting them to the renderer
        world.Routine("SubmitMesh")
            .Kind(Ecs.PreStore)
            .With<EcsMesh>().In()
            .With<EcsTransform>().In()
            .Each((ref EcsMesh mesh, ref EcsTransform transform) =>
            {
                Renderer.SubmitMesh(mesh.Mesh, transform.Value);
            });

        // prepare all the lights, setting them in the
        // renderer properly
        world.Routine("SubmitLight")
            .Kind(Ecs.PreStore)
            .With<EcsLight>()
            .With<EcsTransform>()
            .Each((ref EcsLight light, ref EcsTransform transform) =>
            {
                Renderer.SubmitLight(transform.Value.Translation, light.Color);
            });
        
        // and now actually render it 
        // TODO: handle when there are multiple cameras in the scene
        world.Routine("Render")
            .Kind(Ecs.OnStore)
            .With<EcsCamera>().In()
            .With<EcsTransform>().In()
            .With<EcsLookAt>().In().Optional()
            .Each((ref EcsCamera camera, ref EcsTransform transform, ref EcsLookAt at) =>
            {
                Matrix4x4 view;
                if (!Unsafe.IsNullRef(ref at))
                {
                    view = Matrix4x4.CreateLookAt(
                        transform.Value.Translation,
                        at.Target, Vector3.UnitY);
                }
                else
                {
                    view = transform.Value;
                }
                Renderer.Render(camera.Projection, view);
            });
    }
}