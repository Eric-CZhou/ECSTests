using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.ParticleSystem.Chunks;
using Unity.PerformanceTesting;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Tests.Runtime.ECS.SparseTable
{
    public struct TestTag0 : Unity.Entities.IComponentData { }

    public struct TestTag1 : Unity.Entities.IComponentData { }

    public struct TestTag2 : Unity.Entities.IComponentData { }

    public struct TestTag3 : Unity.Entities.IComponentData { }

    public struct TestShared0 : Unity.Entities.ISharedComponentData
    {
        public float3 Value;
    }

    public struct TestShared1 : Unity.Entities.ISharedComponentData
    {
        public byte value0;
        public float2 value1;
        public float3 value2;
    }

    public struct TestShared2 : Unity.Entities.ISharedComponentData
    {
        public byte value0;
        public float2 value1;
        public float3 value2;
        public float4 value3;
        public float value4;
        public float4x4 value5;
    }
    
    public struct TestShared3 : Unity.Entities.ISharedComponentData
    {
        public int value;
    }
    
    public class UniteEntityQueryTest1S5D
    {
        TestShared0 s00, s01;
    
        //Unity
        World m_World;
        EntityManager manager;
        EntityArchetype ea;
        Entity tmp1;
        Entity tmp2;
        
        NativeArray<Entity> es1 = default;
        NativeArray<Entity> es2 = default;


        //Tuanjie
        static bool flag = false;
        

        [SetUp]
        public void Setup()
        {
            if (flag) return;
            flag = true;
            
            s00 = new TestShared0() { Value = new float3(1)};
            s01 = new TestShared0() { Value = new float3(2)};
            
            //Unity
            m_World = World.DefaultGameObjectInjectionWorld = new World("Test World");
            manager = m_World.EntityManager;
            var types = new ComponentType[6];
            types[0] = ComponentType.ReadOnly<TestTag0>();
            types[1] = ComponentType.ReadOnly<TestTag1>();
            types[2] = ComponentType.ReadWrite<TestShared0>();
            types[3] = ComponentType.ReadWrite<Position>();
            types[4] = ComponentType.ReadWrite<Rotation>();
            types[5] = ComponentType.ReadWrite<Velocity>();
            ea = manager.CreateArchetype(types);
            
        }

        [TearDown]
        public void Teardown()
        {
            if (!flag) return;
            flag = false;
            m_World.Dispose();
        }

        [Test, Performance]
        public unsafe void TestSharedPerformance1([Values(100, 1000, 10000, 100000)] int entCnt)
        {
            Measure.Method(
                    () =>
                    {
                        
                        manager.Instantiate(tmp1, es1);
                        
                    })
                .WarmupCount(10)
                .MeasurementCount(100)
                // Create and destroy the World (and EntityManager) each measurement, so we know it's not caching query data
                .SetUp(() =>
                {
                    tmp1 = manager.CreateEntity(ea);
                    tmp2 = manager.CreateEntity(ea);
                    manager.SetSharedComponent(tmp1, s00);
                    manager.SetSharedComponent(tmp2, s01);
                    
                    es1 = new NativeArray<Entity>(entCnt, Allocator.Temp);
                    
                })
                .CleanUp(() =>
                {
                    es1.Dispose();
                    manager.DestroyAndResetAllEntities();
                })
                .SampleGroup(new SampleGroup("Set Shared Component 1", SampleUnit.Millisecond))
                .Run();
        }
        
        

        [Test, Performance]
        public unsafe void TestQueryPerformance1([Values(100, 1000, 10000, 100000)] int entCnt)
        {
            NativeArray<ArchetypeChunk> ans = default;
            Measure.Method(
                    () =>
                    {
                        var query = new Unity.Entities.EntityQueryBuilder(Allocator.Temp).WithAll<TestTag0>().WithAny<TestTag1>().WithAll<TestShared0>().Build(manager);
                        query.SetSharedComponentFilter(s00);
                        ans = query.ToArchetypeChunkArray(Allocator.Temp);
                    })
                .WarmupCount(10)
                .MeasurementCount(100)

                // Create and destroy the World (and EntityManager) each measurement, so we know it's not caching query data
                .SetUp(() =>
                {
                    Setup();
                    es1 = new NativeArray<Entity>(entCnt / 2, Allocator.Temp);
                    es2 = new NativeArray<Entity>(entCnt / 2, Allocator.Temp);
                    tmp1 = manager.CreateEntity(ea);
                    tmp2 = manager.CreateEntity(ea);
                    manager.SetSharedComponent(tmp1, s00);
                    manager.SetSharedComponent(tmp2, s01);
                    manager.Instantiate(tmp1, es1);
                    manager.Instantiate(tmp2, es2);
                })
                .CleanUp(() =>
                {
                    int tot = 0;
                    for (int i = 0; i < ans.Length; i++)
                    {
                        tot += ans[i].Count;
                    }
                    Assert.AreEqual(entCnt / 2 + 1, tot );
                    es1.Dispose();
                    es2.Dispose();
                    Teardown();
                })
                .SampleGroup(new SampleGroup("CreateEntityQuery", SampleUnit.Millisecond))
                .Run();
        }
        
    }
    
    
    public class UniteEntityQueryTest2S5D
    {
        //Unity
        World m_World;
        EntityManager manager;
        EntityArchetype ea;
        Entity tmp1;
        Entity tmp2;
        Entity tmp3;
        Entity tmp4;
        
        NativeArray<Entity> es1 = default;
        NativeArray<Entity> es2 = default;
        NativeArray<Entity> es3 = default;
        NativeArray<Entity> es4 = default;

        //Tuanjie
        static bool flag = false;

        TestShared0 s00, s01;
        TestShared1 s10, s11;

        [SetUp]
        public void Setup()
        {
            if (flag) return;
            flag = true;
            
            s00 = new TestShared0() { Value = new float3(1)};
            s01 = new TestShared0() { Value = new float3(2)};
            s10 = new TestShared1() { value0 = 1, value1 = 1, value2 = 1 };
            s11 = new TestShared1() { value0 = 2, value1 = 2, value2 = 2 };

            //Unity
            m_World = World.DefaultGameObjectInjectionWorld = new World("Test World");
            manager = m_World.EntityManager;
            var types = new ComponentType[7];
            types[0] = ComponentType.ReadOnly<TestTag0>();
            types[1] = ComponentType.ReadOnly<TestTag1>();
            types[2] = ComponentType.ReadWrite<TestShared0>();
            types[3] = ComponentType.ReadWrite<Position>();
            types[4] = ComponentType.ReadWrite<Rotation>();
            types[5] = ComponentType.ReadWrite<Velocity>();
            types[6] = ComponentType.ReadWrite<TestShared1>();
            ea = manager.CreateArchetype(types);
            
        }

        [TearDown]
        public void Teardown()
        {
            if (!flag) return;
            flag = false;

            m_World.Dispose();
        }

        [Test, Performance]
        public unsafe void TestSharedPerformance1([Values(100, 1000, 10000, 100000)] int entCnt)
        {
            Measure.Method(
                    () =>
                    {
                        
                        manager.Instantiate(tmp1, es1);
                        
                    })
                .WarmupCount(10)
                .MeasurementCount(100)
                // Create and destroy the World (and EntityManager) each measurement, so we know it's not caching query data
                .SetUp(() =>
                {
                    tmp1 = manager.CreateEntity(ea);
                    tmp2 = manager.CreateEntity(ea);
                    tmp3 = manager.CreateEntity(ea);
                    tmp4 = manager.CreateEntity(ea);
                    manager.SetSharedComponent(tmp1, s00);
                    manager.SetSharedComponent(tmp1, s10);
                    manager.SetSharedComponent(tmp2, s00);
                    manager.SetSharedComponent(tmp2, s11);
                    manager.SetSharedComponent(tmp3, s01);
                    manager.SetSharedComponent(tmp3, s10);
                    manager.SetSharedComponent(tmp4, s01);
                    manager.SetSharedComponent(tmp4, s11);
                    
                    es1 = new NativeArray<Entity>(entCnt, Allocator.Temp);
                    
                })
                .CleanUp(() =>
                {
                    es1.Dispose();
                    manager.DestroyAndResetAllEntities();
                })
                .SampleGroup(new SampleGroup("Set Shared Component 1", SampleUnit.Millisecond))
                .Run();
        }
        

        [Test, Performance]
        public unsafe void TestQueryPerformance1([Values(100, 1000, 10000, 100000)] int entCnt)
        {
            NativeArray<ArchetypeChunk> ans = default;
            Measure.Method(
                    () =>
                    {
                        var query = new Unity.Entities.EntityQueryBuilder(Allocator.Temp).WithAll<TestTag0>().WithAny<TestTag1>().WithAll<TestShared0>().WithAll<TestShared1>().Build(manager);
                        query.SetSharedComponentFilter(s00,s11);
                        ans = query.ToArchetypeChunkArray(Allocator.Temp);
                    })
                .WarmupCount(10)
                .MeasurementCount(100)

                // Create and destroy the World (and EntityManager) each measurement, so we know it's not caching query data
                .SetUp(() =>
                {
                    Setup();
                    es1 = new NativeArray<Entity>(entCnt / 4, Allocator.Temp);
                    es2 = new NativeArray<Entity>(entCnt / 4, Allocator.Temp);
                    es3 = new NativeArray<Entity>(entCnt / 4, Allocator.Temp);
                    es4 = new NativeArray<Entity>(entCnt / 4, Allocator.Temp);
                    tmp1 = manager.CreateEntity(ea);
                    tmp2 = manager.CreateEntity(ea);
                    tmp3 = manager.CreateEntity(ea);
                    tmp4 = manager.CreateEntity(ea);
                    manager.SetSharedComponent(tmp1, s00);
                    manager.SetSharedComponent(tmp1, s10);
                    manager.SetSharedComponent(tmp2, s00);
                    manager.SetSharedComponent(tmp2, s11);
                    manager.SetSharedComponent(tmp3, s01);
                    manager.SetSharedComponent(tmp3, s10);
                    manager.SetSharedComponent(tmp4, s01);
                    manager.SetSharedComponent(tmp4, s11);
                    manager.Instantiate(tmp1, es1);
                    manager.Instantiate(tmp2, es2);
                    manager.Instantiate(tmp3, es3);
                    manager.Instantiate(tmp4, es4);
                })
                .CleanUp(() =>
                {
                    int tot = 0;
                    for (int i = 0; i < ans.Length; i++)
                    {
                        tot += ans[i].Count;
                    }
                    Assert.AreEqual(entCnt / 4 + 1, tot);
                    es1.Dispose();
                    es2.Dispose();
                    es3.Dispose();
                    es4.Dispose();
                    Teardown();
                })
                .SampleGroup(new SampleGroup("CreateEntityQuery", SampleUnit.Millisecond))
                .Run();
        }
    }
}
