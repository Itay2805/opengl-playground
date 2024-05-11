using System.Numerics;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Flecs.NET.Utilities;
using MyGame.Engine.Component;

namespace MyGame.Engine.Systems;

public struct Transform : IFlecsModule
{

    public void InitModule(ref World world)
    {
        world.Module<Transform>();
        
        // A system that adds the transform component to anything that has a 
        // position or rotation or scale 
        world.Routine("AddTransform")
            .Kind(Ecs.PostLoad)
            .Write<EcsTransform>()
            .Without<EcsTransform>()
            .With<EcsPosition>().Self().Up().Or()
            .With<EcsRotation>().Self().Up().Or()
            .With<EcsScale>().Self().Up()
            .Iter((it) =>
            {
                for (var i = 0; i < it.Count(); i++)
                {
                    it.Entity(i).Set(new EcsTransform { Value = Matrix4x4.Identity });
                }
            });

        world.Routine("ApplyTransform")
            .Kind(Ecs.OnValidate)
            .With<EcsTransform>().Out()
            .With<EcsTransform>().In().Optional().Parent().Cascade()
            .With<EcsPosition>().In()
            .With<EcsRotation>().In().Optional()
            .With<EcsScale>().In().Optional()
            .Instanced()
            .Iter(it =>
            {
                // Skip it if the table has not changed
                if (!it.Changed())
                {
                    it.Skip();
                    return;
                }
                
                var m = it.Field<EcsTransform>(1);
                var mParent = it.Field<EcsTransform>(2);
                var p = it.Field<EcsPosition>(3);
                var r = it.Field<EcsRotation>(4);
                var s = it.Field<EcsScale>(5);

                if (mParent.IsNull)
                {
                    if (it.IsSelf(3))
                    {
                        for (var i = 0; i < it.Count(); i++)
                        {
                            m[i].Value = Matrix4x4.CreateTranslation(p[i].Value);
                        }
                    }
                    else
                    {
                        for (var i = 0; i < it.Count(); i++)
                        {
                            m[i].Value = Matrix4x4.CreateTranslation(p[0].Value);
                        }
                    }
                }
                else
                {
                    if (it.IsSelf(3))
                    {
                        for (var i = 0; i < it.Count(); i++)
                        {
                            m[i].Value = mParent[0].Value * Matrix4x4.CreateTranslation(p[i].Value);
                        }
                    }
                    else
                    {
                        for (var i = 0; i < it.Count(); i++)
                        {
                            m[i].Value = mParent[0].Value * Matrix4x4.CreateTranslation(p[0].Value);
                        }
                    }
                }
                
                if (!r.IsNull)
                {
                    if (it.IsSelf(4))
                    {
                        for (var i = 0; i < it.Count(); i++)
                        {
                            m[i].Value *= Matrix4x4.CreateRotationX(r[i].Value.X);
                            m[i].Value *= Matrix4x4.CreateRotationY(r[i].Value.Y);
                            m[i].Value *= Matrix4x4.CreateRotationZ(r[i].Value.Z);
                        }
                    }
                    else
                    {
                        for (var i = 0; i < it.Count(); i++)
                        {
                            m[i].Value *= Matrix4x4.CreateRotationX(r[0].Value.X);
                            m[i].Value *= Matrix4x4.CreateRotationY(r[0].Value.Y);
                            m[i].Value *= Matrix4x4.CreateRotationZ(r[0].Value.Z);
                        }
                    }
                }
                
                if (!s.IsNull)
                {
                    for (var i = 0; i < it.Count(); i++)
                    {
                        m[i].Value *= Matrix4x4.CreateScale(s[i].Value);
                    }
                }
            });
    }
}