using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class InitialPositionSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem initBufferSystem_;

    [BurstCompile]
    [RequireComponentTag(typeof(PieceTiles))]
    struct InitialPositionSystemJob : IJobForEachWithEntity<Piece>
    {
        [ReadOnly]
        public EntityCommandBuffer commandBuffer;

        [ReadOnly]
        public BufferFromEntity<PieceTiles> pieceTilesLookup;

        public void Execute(Entity entity, int index, ref Piece piece)
        {
            var tilesBuffer = pieceTilesLookup[entity];

        }
    }
    
    protected override void OnCreate()
    {
        initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new InitialPositionSystemJob
        {
            commandBuffer = initBufferSystem_.CreateCommandBuffer(),
            pieceTilesLookup = GetBufferFromEntity<PieceTiles>(false),
        }.Schedule(this, inputDependencies);

        initBufferSystem_.AddJobHandleForProducer(job);

        // Now that the job is set up, schedule it to be run. 
        return job;
    }
}