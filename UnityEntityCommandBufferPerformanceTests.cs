using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.PerformanceTesting;
using UnityEngine;

namespace Tests.Runtime.ECS
{
    public class UnityEntityCommandBufferPerformanceTests
    {
        EntityArchetype entityArchetype1;
        EntityArchetype entityArchetype2;
        EntityArchetype entityArchetype3;
        EntityManager entityManager;

        struct ColumnFloatUnity :  IComponentData
        {
            public float Value;
        }

        struct ColumnIntUnity :  IComponentData
        {
            public int Value;
        }

        struct ColumnBoolUnity :  IComponentData
        {
            public bool Value;
        }

        struct ColumnUIntUnity :  IComponentData
        {
            public uint Value;
        }

        struct ColumnLongUnity :  IComponentData
        {
            public long Value;
        }

        struct ColumnShortUnity :  IComponentData
        {
            public short Value;
        }

        [SetUp]
        public void Setup()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityArchetype1 = entityManager.CreateArchetype(typeof(ColumnFloatUnity));
            entityArchetype2 = entityManager.CreateArchetype(typeof(ColumnFloatUnity), typeof(ColumnIntUnity));
            entityArchetype3 = entityManager.CreateArchetype(typeof(ColumnFloatUnity), typeof(ColumnIntUnity),typeof(ColumnBoolUnity));
        }

        [TearDown]
        public void Teardown()
        {
        }

        [BurstCompile]
        struct UnityCreateEntryAABBJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            public EntityArchetype archetypeLine;
            public int preCount;

            public void Execute(int index)
            {
                ecb.CreateEntity(preCount + index, archetypeLine);
            }
        }

        [BurstCompile]
        struct UnityCreateEntryABABJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            public EntityArchetype archetypeLine1;
            public EntityArchetype archetypeLine2;

            public void Execute(int index)
            {
                ecb.CreateEntity(index, archetypeLine1);
                ecb.CreateEntity(index, archetypeLine2);
            }
        }

        [BurstCompile]
        struct UnityRemoveEntryJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            public NativeArray<Entity> array;

            public void Execute(int index)
            {
                ecb.DestroyEntity(index, array[index]);
            }
        }

        [Test, Performance]
        public void TestCreateEntry()
        {
            Measure.Method(() =>
            {
                var unityEcb = new EntityCommandBuffer(Allocator.Persistent);
                var unityEcb2 = new EntityCommandBuffer(Allocator.Persistent);

                var count1 = 10000;
                for (int i = 0; i < count1; i++)
                {
                    unityEcb.CreateEntity(entityArchetype1);
                }

                var count2 = 10000;
                for (int i = 0; i < count2; i++)
                {
                    unityEcb.CreateEntity(entityArchetype2);
                }

                new UnityCreateEntryAABBJob()
                {
                    archetypeLine = entityArchetype1,
                    ecb = unityEcb2.AsParallelWriter(),
                    preCount = 0
                }.Schedule(count1, 1).Complete();
                new UnityCreateEntryAABBJob()
                {
                    archetypeLine = entityArchetype2,
                    ecb = unityEcb2.AsParallelWriter(),
                    preCount = count1
                }.Schedule(count2, 1).Complete();

                using (Measure.Scope("UnityCreateEntityAABB"))
                {
                    unityEcb.Playback(entityManager);
                }

                using (Measure.Scope("UnityCreateEntityAABBWithSortKey"))
                {
                    unityEcb2.Playback(entityManager);
                }

                EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<ColumnFloatUnity>().Build(entityManager);
                entityManager.DestroyEntity(query);
                unityEcb2.Dispose();
                unityEcb.Dispose();

                unityEcb = new EntityCommandBuffer(Allocator.Persistent);
                unityEcb2 = new EntityCommandBuffer(Allocator.Persistent);

                for (int i = 0; i < count1 + count2; i++)
                {
                    unityEcb.CreateEntity(entityArchetype1);
                    unityEcb.CreateEntity(entityArchetype2);
                }

                new UnityCreateEntryABABJob()
                {
                    archetypeLine1 = entityArchetype1,
                    archetypeLine2 = entityArchetype2,
                    ecb = unityEcb2.AsParallelWriter()
                }.Schedule(count1 + count2, 1).Complete();

                using (Measure.Scope("UnityCreateEntityABAB"))
                {
                    unityEcb.Playback(entityManager);
                }

                using (Measure.Scope("UnityCreateEntityABABWithSortKey"))
                {
                    unityEcb2.Playback(entityManager);
                }

                query = new EntityQueryBuilder(Allocator.Temp).WithAll<ColumnFloatUnity>().Build(entityManager);
                entityManager.DestroyEntity(query);
                unityEcb2.Dispose();
                unityEcb.Dispose();
            })
            .SampleGroup("CreateGroup")
            .WarmupCount(10)
            .MeasurementCount(1000)
            .Run();
        }

        [Test, Performance]
        public void TestRemoveEntry1Archetype()
        {
            Measure.Method(() =>
            {
                var unityEcb = new EntityCommandBuffer(Allocator.Persistent);
                var unityEcb2 = new EntityCommandBuffer(Allocator.Persistent);
                var count = 10000;

                var entityArray1 = new NativeArray<Entity>(count, Allocator.Persistent);
                var entityArray2 = new NativeArray<Entity>(count, Allocator.Persistent);

                for (int i = 0; i < count; i++)
                {
                    entityArray1[i] =  entityManager.CreateEntity(entityArchetype1);
                    entityArray2[i] =  entityManager.CreateEntity(entityArchetype1);
                }

                for (int i = 0; i < count; i++)
                {
                    unityEcb.DestroyEntity(entityArray1[i]);
                }

                new UnityRemoveEntryJob()
                {
                    ecb = unityEcb2.AsParallelWriter(),
                    array = entityArray2
                }.Schedule(count, 1).Complete();

                using (Measure.Scope("UnityRemoveEntity_After"))
                {
                    unityEcb.Playback(entityManager);
                }

                using (Measure.Scope("UnityRemoveEntityWithSortKey_After"))
                {
                    unityEcb2.Playback(entityManager);
                }

                var query = new EntityQueryBuilder(Allocator.Temp).WithAll<ColumnFloatUnity>().Build(entityManager);
                entityManager.DestroyEntity(query);

                entityArray2.Dispose();
                entityArray1.Dispose();
                unityEcb.Dispose();
                unityEcb2.Dispose();
            })
            .SampleGroup("RemoveGroup")
            .WarmupCount(10)
            .MeasurementCount(1000)
            .Run();
        }

        [Test, Performance]
        public void TestRemoveEntry2Archetype()
        {
            Measure.Method(() =>
            {
                var unityEcb = new EntityCommandBuffer(Allocator.Persistent);
                var unityEcb2 = new EntityCommandBuffer(Allocator.Persistent);
                var count = 10000;

                var entityArray1 = new NativeArray<Entity>(count + count, Allocator.Persistent);
                var entityArray2 = new NativeArray<Entity>(count + count, Allocator.Persistent);

                for (int i = 0; i < count; i++)
                {
                    entityArray1[i] =  entityManager.CreateEntity(entityArchetype1);
                    entityArray2[i] =  entityManager.CreateEntity(entityArchetype1);
                }

                for (int i = count; i < count + count; i++)
                {
                    entityArray1[i] =  entityManager.CreateEntity(entityArchetype2);
                    entityArray2[i] =  entityManager.CreateEntity(entityArchetype2);
                }

                for (int i = 0; i < count + count; i++)
                {
                    unityEcb.DestroyEntity(entityArray1[i]);
                }

                new UnityRemoveEntryJob()
                {
                    ecb = unityEcb2.AsParallelWriter(),
                    array = entityArray2
                }.Schedule(count + count, 1).Complete();

                using (Measure.Scope("UnityRemoveEntity_After"))
                {
                    unityEcb.Playback(entityManager);
                }

                using (Measure.Scope("UnityRemoveEntityWithSortKey_After"))
                {
                    unityEcb2.Playback(entityManager);
                }

                var query = new EntityQueryBuilder(Allocator.Temp).WithAll<ColumnFloatUnity>().Build(entityManager);
                entityManager.DestroyEntity(query);

                entityArray2.Dispose();
                entityArray1.Dispose();
                unityEcb.Dispose();
                unityEcb2.Dispose();
            })
            .SampleGroup("RemoveGroup")
            .WarmupCount(10)
            .MeasurementCount(1000)
            .Run();
        }

        [Test, Performance]
        public void TestRemoveEntry3Archetype()
        {
            Measure.Method(() =>
            {
                var unityEcb = new EntityCommandBuffer(Allocator.Persistent);
                var unityEcb2 = new EntityCommandBuffer(Allocator.Persistent);
                var count = 10000;
                var allCount = count * 3;

                var entityArray1 = new NativeArray<Entity>(allCount, Allocator.Persistent);
                var entityArray2 = new NativeArray<Entity>(allCount, Allocator.Persistent);

                for (int i = 0; i < count; i++)
                {
                    entityArray1[i] =  entityManager.CreateEntity(entityArchetype1);
                    entityArray2[i] =  entityManager.CreateEntity(entityArchetype1);
                }

                for (int i = count; i < count + count; i++)
                {
                    entityArray1[i] =  entityManager.CreateEntity(entityArchetype2);
                    entityArray2[i] =  entityManager.CreateEntity(entityArchetype2);
                }

                for (int i = count + count; i < allCount; i++)
                {
                    entityArray1[i] =  entityManager.CreateEntity(entityArchetype3);
                    entityArray2[i] =  entityManager.CreateEntity(entityArchetype3);
                }

                for (int i = 0; i < allCount; i++)
                {
                    unityEcb.DestroyEntity(entityArray1[i]);
                }

                new UnityRemoveEntryJob()
                {
                    ecb = unityEcb2.AsParallelWriter(),
                    array = entityArray2
                }.Schedule(allCount, 1).Complete();

                using (Measure.Scope("UnityRemoveEntity_After"))
                {
                    unityEcb.Playback(entityManager);
                }

                using (Measure.Scope("UnityRemoveEntityWithSortKey_After"))
                {
                    unityEcb2.Playback(entityManager);
                }

                var query = new EntityQueryBuilder(Allocator.Temp).WithAll<ColumnFloatUnity>().Build(entityManager);
                entityManager.DestroyEntity(query);

                entityArray2.Dispose();
                entityArray1.Dispose();
                unityEcb.Dispose();
                unityEcb2.Dispose();
            })
            .SampleGroup("RemoveGroup")
            .WarmupCount(10)
            .MeasurementCount(1000)
            .Run();
        }
    }
}
