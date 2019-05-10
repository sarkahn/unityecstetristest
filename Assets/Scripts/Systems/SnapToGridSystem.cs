using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class SnapToGridSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem initBufferSystem_;

    //[BurstCompile]
    [RequireComponentTag(typeof(SnapToGrid))]
    struct SnapToGridSystemJob : IJobForEachWithEntity<Translation, Piece>
    {
        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int index, ref Translation translation, [ReadOnly] ref Piece piece)
        {
            float3 snapOffset = new float3(.5f + piece.snapOffset, 0);
            translation.Value = math.floor(translation.Value) + snapOffset;
            commandBuffer.RemoveComponent(index, entity, typeof(SnapToGrid));
        }
    }

    protected override void OnCreate()
    {
        initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var jobHandle = new SnapToGridSystemJob()
        {
            commandBuffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDependencies);

        initBufferSystem_.AddJobHandleForProducer(jobHandle);

        // Now that the job is set up, schedule it to be run. 
        return jobHandle;
    }
}