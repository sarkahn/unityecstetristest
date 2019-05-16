using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//[UpdateInGroup(typeof(TRSToLocalToParentSystem))]
//[UpdateBefore(typeof(TRSToLocalToParentSystem))]
[UpdateBefore(typeof(InitBoardSystem))]
public class SnapToGridSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem initCommandBufferSystem_;
    
    [RequireComponentTag(typeof(SnapToGrid))]
    struct SnapToGridSystemJob : IJobForEachWithEntity<Piece, Translation>
    {
        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;
        
        public void Execute(Entity entity, int index, [ReadOnly] ref Piece piece, ref Translation translation)
        {
            commandBuffer.RemoveComponent(index, entity, typeof(SnapToGrid));

            var p = translation.Value;
            float3 offset = new float3(piece.snapOffset, piece.snapOffset, 0);
            p = math.floor(p) + new float3(0.5f, 0.5f, 0) - offset;
            translation.Value = p;
        }
    }

    protected override void OnCreate()
    {
        initCommandBufferSystem_ = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        //Debug.Log("Snapping to grid");
        var job = new SnapToGridSystemJob
        {
            commandBuffer = initCommandBufferSystem_.CreateCommandBuffer().ToConcurrent(),
        }.Schedule(this, inputDependencies);

        initCommandBufferSystem_.AddJobHandleForProducer(job);

        return job;
    }
}