using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class UpdateChildTilesSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem initBufferSystem_;

    //[BurstCompile]
    [RequireComponentTag(typeof(Child))]
    //[ExcludeComponent(typeof(PieceTiles))]
    struct UpdateChildTilesSystemJob : IJobForEachWithEntity<Piece>
    {
        [ReadOnly]
        public EntityCommandBuffer commandBuffer;
        [ReadOnly]
        public BufferFromEntity<Child> childLookup;

        public void Execute(Entity entity, int index, [ReadOnly] ref Piece piece )
        {
            //var tilesBuffer = commandBuffer.AddBuffer<PieceTiles>(entity);
            var childBuffer = childLookup[entity];
            for( int i = 0; i < childBuffer.Length; ++i )
            {
                var child = childBuffer[i];
                //tilesBuffer.Add(child.Value);
            }
        }
    }

    protected override void OnCreate()
    {
        initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new UpdateChildTilesSystemJob
        {
            commandBuffer = initBufferSystem_.CreateCommandBuffer(),
            childLookup = GetBufferFromEntity<Child>(true),
            //ltpLookup = GetComponentDataFromEntity<LocalToParent>(true),
        }.Schedule(this, inputDependencies);
        
        initBufferSystem_.AddJobHandleForProducer(job);
        
        // Now that the job is set up, schedule it to be run. 
        return job;
    }
}