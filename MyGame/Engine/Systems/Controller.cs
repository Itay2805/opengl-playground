using System.Numerics;
using Flecs.NET.Core;
using GLFW;
using MyGame.Engine.Component;

namespace MyGame.Engine.Systems;

public struct Controller : IFlecsModule
{
    public void InitModule(ref World world)
    {
        world.Routine()
            .With<EcsPosition>().InOut()
            .With<EcsRotation>().InOut()
            .With<EcsCamera>().Filter().In()
            .Iter(it =>
            {
                ref var position = ref it.Field<EcsPosition>(1)[0];
                // ref var rotation = ref it.Field<EcsRotation>(2)[0];
                
                if (Glfw.GetKey(Engine.Window, Keys.W) == InputState.Press)
                {
                    position.Value = new Vector3(
                        position.Value.X + 1.2f * it.DeltaTime(),
                        position.Value.Y, position.Value.Z
                    );
                }
                
                if (Glfw.GetKey(Engine.Window, Keys.S) == InputState.Press)
                {
                    position.Value = new Vector3(
                        position.Value.X - 1.2f * it.DeltaTime(),
                        position.Value.Y, position.Value.Z
                    );
                }
            });
    }
}