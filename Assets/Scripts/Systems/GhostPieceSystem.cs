using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

//[DisableAutoCreation]
public class GhostPieceSystem : JobComponentSystem
{
    NativeArray<Entity> prefabs_;
    EndSimulationEntityCommandBufferSystem initBufferSystem_;

    bool ran = false;

    //[BurstCompile]
    [RequireComponentTag(typeof(ActivePiece))]
    [ExcludeComponent(typeof(GhostPiece))]
    struct GhostPieceSystemJob : IJobForEachWithEntity<Piece, Translation>
    {
        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int index, ref Piece c0, ref Translation translation)
        {
            var e = commandBuffer.Instantiate(index, entity);
            commandBuffer.RemoveComponent(index, e, typeof(ActivePiece));
            commandBuffer.AddComponent(index, e, new GhostPiece());
            //commandBuffer.AddComponent(index, e, new SnapToHeightmap());
            commandBuffer.SetComponent(index, e, new Translation { Value = 0 });
        }
    }

    protected override void OnCreate()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        initBufferSystem_ = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        if(prefabs_.IsCreated)
            prefabs_.Dispose();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var proxy = GameObject.FindObjectOfType<GhostPieceSystemProxy>();

        if (proxy == null)
        {
            //Enabled = false;
            //return;
        }
        else
            prefabs_ = proxy.EntityPrefabs_;
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = inputDependencies;

        job = new GhostPieceSystemJob
        {
            commandBuffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent(),
        }.Schedule(this, job);

        initBufferSystem_.AddJobHandleForProducer(job);
        

 
        return job;
    }
}