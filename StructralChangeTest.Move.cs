using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.ParticleSystem.Chunks;
using Unity.PerformanceTesting;
using UnityEngine;


namespace Tests.Runtime.ECS.SparseTable
{
    public struct Spawner : Unity.Entities.IComponentData
    {
        public Entity Prefab;
        public float NextSpawnTime;
        public float SpawnRate;
    }

    public struct Index : Unity.Entities.IComponentData
    {
        byte value;
    }

    public struct Position : Unity.Entities.IComponentData
    {
        public float3 value;
    }

    public struct Velocity : Unity.Entities.IComponentData
    {
        float3 value;
    }

    public struct Rotation : Unity.Entities.IComponentData
    {
        float3 value;
    }

    public struct Time : Unity.Entities.IComponentData
    {
        float value;
    }

    public struct Matrix : Unity.Entities.IComponentData
    {
        float4x4 matrix;
    }

    public struct ComponentTestAdd : Unity.Entities.IComponentData
    {
        int value;
    }

    public class UniteStructralChangeMoveTest
    {
        
        
        //Unity
        World m_World;
        EntityManager manager;
        EntityArchetype ea0;
        EntityArchetype ea1;
        
        

        [SetUp]
        public void SetUp()
        {
            
            //Unity
            m_World = World.DefaultGameObjectInjectionWorld = new World("Test World");
            manager = m_World.EntityManager;
            var types = new NativeList<ComponentType> (10, Allocator.Temp)
            {
                ComponentType.ReadWrite<Spawner>(),
                ComponentType.ReadWrite<Position>(),
                ComponentType.ReadWrite<Rotation>(),
                ComponentType.ReadWrite<Time>(),
                ComponentType.ReadWrite<Velocity>(),
                ComponentType.ReadWrite<Matrix>(),
                ComponentType.ReadWrite<Index>(),
            };
            ea0 = manager.CreateArchetype(types.AsArray());
            types.Add(ComponentType.ReadWrite<ComponentTestAdd>());
            ea1 = manager.CreateArchetype(types.AsArray());
            
        }

        [TearDown]
        public void TearDown()
        {
            m_World.Dispose();
        }

        
        [Test, Performance]
        public unsafe void TestAddPerformance1([Values(1000, 10000, 100000)] int number)
        {
            
            EntityQuery q = default;
            
            Measure.Method(() =>
                {
                    manager.AddComponent(q, ComponentType.ReadWrite<ComponentTestAdd>());
                })
                .SetUp(() =>
                {
                    manager.CreateEntity(ea0, number, Allocator.TempJob);
                    q = new EntityQueryBuilder(Allocator.TempJob).WithAll<Spawner>().Build(manager);
                })
                .CleanUp(() =>
                {
                    manager.DestroyAndResetAllEntities();
                })
                .WarmupCount(100)
                .MeasurementCount(100)
                .Run();
        }
        
        
        [Test, Performance]
        public unsafe void TestRemovePerformance1([Values(1000, 10000, 100000)] int number)
        {

            EntityQuery q = default;
            Measure.Method(() =>
                {
                    manager.RemoveComponent(q, ComponentType.ReadWrite<ComponentTestAdd>());
                })
                .SetUp(() =>
                {
                    manager.CreateEntity(ea1, number, Allocator.TempJob);
                    q = new EntityQueryBuilder(Allocator.TempJob).WithAll<Spawner>().Build(manager);
                })
                .CleanUp(() =>
                {
                    manager.DestroyAndResetAllEntities();
                })
                .WarmupCount(100)
                .MeasurementCount(100)
                .Run();
        }
        
    }
}
