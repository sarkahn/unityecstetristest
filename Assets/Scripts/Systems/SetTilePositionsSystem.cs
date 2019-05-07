using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

// TODO: We should store tile positions (PieceTiles) as float3s, and only apply centeroffset 
// when checking the positions against the board, rather than storing them as ints and
// applying the offset when setting positions?

public class SetTilePositionsSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem initBufferSystem_;

    //[BurstCompile]
    [RequireComponentTag(typeof(Child))]
    struct SetTilePositionsSystemJob : IJobForEachWithEntity<Piece>
    {
        [ReadOnly]
        public EntityCommandBuffer commandBuffer;
        [ReadOnly]
        public BufferFromEntity<Child> childBufferLookup;
        [ReadOnly]
        public BufferFromEntity<PieceTiles> tilesBufferLookup;

        public void Execute(Entity entity, int index, [ReadOnly] ref Piece piece)
        {
            var childBuffer = childBufferLookup[entity];
            var tilesBuffer = tilesBufferLookup[entity];

            for( int i = 0; i < childBuffer.Length; ++i )
            {
                var child = childBuffer[i].Value;
                float3 pos = new float3(tilesBuffer[i].tile, 0);
                pos -= piece.centerOffset;
                commandBuffer.SetComponent(child, new Translation { Value = pos });
            }
        }
    }

    protected override void OnCreate()
    {
        initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new SetTilePositionsSystemJob
        {
            commandBuffer = initBufferSystem_.CreateCommandBuffer(),
            childBufferLookup = GetBufferFromEntity<Child>(true),
            tilesBufferLookup = GetBufferFromEntity<PieceTiles>(true),

        }.Schedule(this, inputDependencies);

        initBufferSystem_.AddJobHandleForProducer(job);

        // Now that the job is set up, schedule it to be run. 
        return job;
    }
}