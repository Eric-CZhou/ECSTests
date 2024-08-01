using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using Unity.ParticleSystem.Chunks;
using Unity.PerformanceTesting;
using Unity.Entities;
using UnityEngine;

namespace Tests.Runtime.ECS.SparseTable
{
    public unsafe class UniteStructralChangeCreateTest
    {
        //Unity
        World m_World;
        EntityArchetype ea;
        
        struct TestPosition : Unity.Entities.IComponentData
        {
            public float3 Value;
        }

        struct NewbornTag : Unity.Entities.IComponentData
        {
            
        }
        
        struct TrailTag : Unity.Entities.IComponentData
        {
            
        }
        
        struct LightTag : Unity.Entities.IComponentData
        {
            
        }
        
        struct AdultTag : Unity.Entities.IComponentData
        {
            
        }

        const int totNumber = 100000;

        [SetUp]
        public void Setup()
        { 
            //Unity
            m_World = World.DefaultGameObjectInjectionWorld = new World("Test World");
        }
        
        [TearDown]
        public void Teardown()
        {
            
            m_World.Dispose();
        }
        
        
        [Test, Performance]
        public void MeasureCreateEntriesPerformanceFive1([Values(100, 1000, 10000, 100000)] int size)
        {
            ea = m_World.EntityManager.CreateArchetype(
                ComponentType.ReadWrite<TestPosition>(),
                ComponentType.ReadWrite<Velocity>(),
                ComponentType.ReadWrite<Rotation>(),
                ComponentType.ReadOnly<Spawner>(),
                ComponentType.ReadOnly<LightTag>());
            Measure.Method(() =>
                {
                    m_World.EntityManager.CreateEntity(ea, size);
                })
                .CleanUp(() =>
                {
                    m_World.EntityManager.DestroyAndResetAllEntities();
                })
                .WarmupCount(10)
                .MeasurementCount(100)
                .Run();
            
        }
        
        
        [Test, Performance]
        public void MeasureCreateEntriesPerformanceTen1([Values(100, 1000, 10000, 100000)] int size)
        {
            ea = m_World.EntityManager.CreateArchetype(
                ComponentType.ReadWrite<TestPosition>(),
                ComponentType.ReadWrite<Velocity>(), 
                ComponentType.ReadOnly<NewbornTag>(),
                ComponentType.ReadOnly<TrailTag>(),
                ComponentType.ReadOnly<LightTag>(),
                ComponentType.ReadOnly<AdultTag>(),
                ComponentType.ReadWrite<Position>(),
                ComponentType.ReadWrite<Rotation>(),
                ComponentType.ReadWrite<Time>(),
                ComponentType.ReadWrite<Index>());
            Measure.Method(() =>
                {
                    m_World.EntityManager.CreateEntity(ea, size);
                })
                .CleanUp(() =>
                {
                    m_World.EntityManager.DestroyAndResetAllEntities();
                })
                .WarmupCount(10)
                .MeasurementCount(100)
                .Run();
            
        }
        
        
        
        
    }
}
